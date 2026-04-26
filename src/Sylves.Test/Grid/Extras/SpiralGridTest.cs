using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

using static Sylves.Test.TestUtils;

namespace Sylves.Test.Grid.Extras
{
    [TestFixture]
    public class SpiralGridTest
    {
        private const double Epsilon = 1e-5;

        private SpiralGrid GetSpiralGrid()
        {
            var periodic = new PeriodicPlanarMeshGrid(
                new SquareGrid(1, new SquareBound(0, 0, 1, 1)).ToMeshData(),
                new Vector2(1, 0),
                new Vector2(0, 1));
            return new SpiralGrid(periodic, 6, 6);
        }

        [Test]
        public void TestFindCell()
        {
            var spiral = GetSpiralGrid();
            GridTest.FindCell(spiral, new Cell(0, 2, 3));
        }

        [Test]
        public void TransformAndUntransformAreInversesAlongWithJacobians()
        {
            var spiral = GetSpiralGrid();

            var source = new Vector3(0.4f, -1.7f, 2.3f);

            var transformed = spiral.Transform(source);
            var roundTrip = spiral.Untransform(transformed);
            AssertAreEqual(source, roundTrip, Epsilon);

            spiral.GetTransformJacobi(source, out var jacobiForward);
            var jacobiInverse = spiral.UntransformJacobi(transformed);

            var expectedIdentity = Matrix4x4.identity;
            AssertAreEqual(expectedIdentity, jacobiInverse * jacobiForward, Epsilon);
            AssertAreEqual(expectedIdentity, jacobiForward * jacobiInverse, Epsilon);
        }
    }
}
