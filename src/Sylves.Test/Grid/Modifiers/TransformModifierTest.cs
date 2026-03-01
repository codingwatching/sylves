using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    internal class TransformModifierTest
    {


        private static readonly object[] FindCellCases =
        {
            new object[] { "identity",  Matrix4x4.identity },
            new object[] { "translate", Matrix4x4.Translate(Vector3.right * 100) },
            new object[] { "rotate",    Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0)) },
            new object[] { "scale",     Matrix4x4.Scale(Vector3.one * 2) },
            new object[] { "combined",  Matrix4x4.TRS(Vector3.right * 100, Quaternion.Euler(0, 90, 0), Vector3.one * 2) },
        };

        [Test]
        [TestCaseSource(nameof(FindCellCases))]
        public void TestFindCell(string _, Matrix4x4 transform)
        {
            var g = new TransformModifier(new SquareGrid(1), transform);
            GridTest.FindCell(g, new Cell(5, 5));
        }

        [Test]
        public void TestCombineTransforms()
        {
            Matrix4x4 tf1 = Matrix4x4.Scale(Vector3.one * 2);
            Matrix4x4 tf2 = Matrix4x4.Translate(Vector3.right * 5);
            IGrid g = new HexGrid(1);
            g = g.Transformed(tf1).Transformed(tf2);

            Assert.IsTrue(g is TransformModifier);
            Assert.IsTrue((g as TransformModifier).Underlying is HexGrid);
            var combinedTf = tf2 * tf1;
            Assert.AreEqual(new Vector3(5, 0, 0), g.GetCellCenter(new Cell()));
        }
    }
}
