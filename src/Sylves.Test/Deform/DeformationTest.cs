using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

using static Sylves.Test.TestUtils;

namespace Sylves.Test
{
    [TestFixture]
    public class DeformationTest
    {
        private const double Epsilon = 1e-5;
        private const double TangentWPrecision = 1e-6;

        private static readonly Vector3 SamplePoint = new Vector3(1.2f, -0.5f, 4.3f);
        private static readonly Vector3 SampleNormal = new Vector3(0.3f, 0.4f, 0.5f).normalized;
        private static readonly Vector4 SampleTangent = new Vector4(0.6f, -0.2f, 0.7f, -1);

        private static readonly Matrix4x4 CompositionTranslation = Matrix4x4.Translate(new Vector3(3, -2, 1));
        private static readonly Matrix4x4 BaseTransform = Matrix4x4.TRS(
            new Vector3(2.5f, -1.0f, 3.25f),
            Quaternion.Euler(20, 35, -10),
            new Vector3(1.2f, 0.8f, 1.5f));
        private static readonly Matrix4x4 WindingScale = Matrix4x4.Scale(new Vector3(2, 3, 4));
        private static readonly Matrix4x4 AssociativityA = Matrix4x4.TRS(new Vector3(1, 2, 3), Quaternion.Euler(10, 20, 30), new Vector3(1.1f, 0.9f, 1.3f));
        private static readonly Matrix4x4 AssociativityB = Matrix4x4.TRS(new Vector3(-2, 1, 0), Quaternion.Euler(-15, 5, 40), new Vector3(0.7f, 1.4f, 1.0f));
        private static readonly Matrix4x4 AssociativityC = Matrix4x4.TRS(new Vector3(0.5f, -1.5f, 2), Quaternion.Euler(0, 60, -25), new Vector3(1.2f, 1.2f, 0.8f));
        private static readonly Vector3 AssociativityPoint = new Vector3(4.2f, -3.3f, 1.7f);

        private static Deformation MakeAffineDeformation(Matrix4x4 m, bool invertWinding = false)
        {
            return new Deformation(
                p => m.MultiplyPoint3x4(p),
                (Vector3 p, out Matrix4x4 j) => j = m,
                invertWinding);
        }

        private static readonly Deformation ShearPlusTranslate = CompositionTranslation * new Deformation(
            p => new Vector3(p.x + 2 * p.y, p.y, p.z),
            (Vector3 p, out Matrix4x4 j) => j = VectorUtils.ToMatrix(
                new Vector3(1, 0, 0),
                new Vector3(2, 1, 0),
                new Vector3(0, 0, 1),
                new Vector3(p.x + 2 * p.y, p.y, p.z)),
            false);

        private static readonly Deformation ScaleThenShear = new Deformation(
            p => new Vector3(2 * p.x, p.y + p.z, p.z),
            (Vector3 p, out Matrix4x4 j) => j = VectorUtils.ToMatrix(
                new Vector3(2, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1),
                new Vector3(2 * p.x, p.y + p.z, p.z)),
            false);

        [Test]
        public void TestCompositionOperator()
        {
            var composed = ShearPlusTranslate * ScaleThenShear;

            var pSeq = ShearPlusTranslate.DeformPoint(ScaleThenShear.DeformPoint(SamplePoint));
            var pCmp = composed.DeformPoint(SamplePoint);
            AssertAreEqual(pSeq, pCmp, Epsilon);

            var nSeq = ShearPlusTranslate.DeformNormal(ScaleThenShear.DeformPoint(SamplePoint), ScaleThenShear.DeformNormal(SamplePoint, SampleNormal));
            var nCmp = composed.DeformNormal(SamplePoint, SampleNormal);
            AssertAreEqual(nSeq, nCmp, Epsilon);

            var tSeq = ShearPlusTranslate.DeformTangent(ScaleThenShear.DeformPoint(SamplePoint), ScaleThenShear.DeformTangent(SamplePoint, SampleTangent));
            var tCmp = composed.DeformTangent(SamplePoint, SampleTangent);
            AssertAreEqual(new Vector3(tSeq.x, tSeq.y, tSeq.z), new Vector3(tCmp.x, tCmp.y, tCmp.z), Epsilon);
            Assert.AreEqual(tSeq.w, tCmp.w, TangentWPrecision);

            ShearPlusTranslate.GetJacobi(ScaleThenShear.DeformPoint(SamplePoint), out var j1);
            ScaleThenShear.GetJacobi(SamplePoint, out var j2);
            var expectedJacobi = VectorUtils.ToMatrix(
                j1.MultiplyVector(j2.MultiplyVector(Vector3.right)),
                j1.MultiplyVector(j2.MultiplyVector(Vector3.up)),
                j1.MultiplyVector(j2.MultiplyVector(Vector3.forward)),
                pSeq);
            composed.GetJacobi(SamplePoint, out var actualJacobi);
            AssertAreEqual(expectedJacobi, actualJacobi, Epsilon);
        }

        [Test]
        public void TestCompositionWithIdentity()
        {
            var d = MakeAffineDeformation(BaseTransform);
            var p = new Vector3(-3.1f, 0.25f, 8.0f);
            var n = new Vector3(0.2f, 0.6f, 0.7f).normalized;
            var t = new Vector4(0.5f, 0.1f, -0.8f, 1);

            var left = Deformation.Identity * d;
            var right = d * Deformation.Identity;

            AssertAreEqual(d.DeformPoint(p), left.DeformPoint(p), Epsilon);
            AssertAreEqual(d.DeformPoint(p), right.DeformPoint(p), Epsilon);

            AssertAreEqual(d.DeformNormal(p, n), left.DeformNormal(p, n), Epsilon);
            AssertAreEqual(d.DeformNormal(p, n), right.DeformNormal(p, n), Epsilon);

            var dTangent = d.DeformTangent(p, t);
            var leftTangent = left.DeformTangent(p, t);
            var rightTangent = right.DeformTangent(p, t);
            AssertAreEqual(new Vector3(dTangent.x, dTangent.y, dTangent.z), new Vector3(leftTangent.x, leftTangent.y, leftTangent.z), Epsilon);
            AssertAreEqual(new Vector3(dTangent.x, dTangent.y, dTangent.z), new Vector3(rightTangent.x, rightTangent.y, rightTangent.z), Epsilon);
            Assert.AreEqual(dTangent.w, leftTangent.w, TangentWPrecision);
            Assert.AreEqual(dTangent.w, rightTangent.w, TangentWPrecision);
        }

        [Test]
        public void TestCompositionInvertWindingXor()
        {
            var dFalse = MakeAffineDeformation(WindingScale, false);
            var dTrue = MakeAffineDeformation(WindingScale, true);

            Assert.IsFalse((dFalse * dFalse).InvertWinding);
            Assert.IsTrue((dFalse * dTrue).InvertWinding);
            Assert.IsTrue((dTrue * dFalse).InvertWinding);
            Assert.IsFalse((dTrue * dTrue).InvertWinding);
        }

        [Test]
        public void TestCompositionAssociativityForPointAndJacobi()
        {
            var a = MakeAffineDeformation(AssociativityA);
            var b = MakeAffineDeformation(AssociativityB);
            var c = MakeAffineDeformation(AssociativityC);

            var left = (a * b) * c;
            var right = a * (b * c);

            AssertAreEqual(left.DeformPoint(AssociativityPoint), right.DeformPoint(AssociativityPoint), Epsilon);

            left.GetJacobi(AssociativityPoint, out var leftJ);
            right.GetJacobi(AssociativityPoint, out var rightJ);
            AssertAreEqual(leftJ, rightJ, Epsilon);
        }
    }
}
