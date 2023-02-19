﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    internal class UnrelaxedTownscaperGrid : BasePlanarLazyGrid
    {
        private readonly int n;
        private readonly float weldTolerance;
        private readonly HexGrid chunkGrid;

        public UnrelaxedTownscaperGrid(int n, float weldTolerance) : base()
        {
            this.n = n;
            this.weldTolerance = weldTolerance;
            chunkGrid = new HexGrid(n);

            // Work out the dimensions of the chunk grid, needed for PlanarLazyGrid
            var strideX = ToVector2(chunkGrid.GetCellCenter(ChunkToCell(new Vector2Int(1, 0))));
            var strideY = ToVector2(chunkGrid.GetCellCenter(ChunkToCell(new Vector2Int(0, 1))));

            var polygon = chunkGrid.GetPolygon(ChunkToCell(new Vector2Int())).Select(ToVector2);
            var aabbBottomLeft = polygon.Aggregate(Vector2.Min);
            var aabbTopRight = polygon.Aggregate(Vector2.Max);
            var aabbSize = aabbTopRight - aabbBottomLeft;

            base.Setup(strideX, strideY, aabbBottomLeft, aabbSize);
        }


        private static Cell ChunkToCell(Vector2Int chunk) => new Cell(chunk.x, chunk.y, -chunk.x - chunk.y);
        private static Vector2 ToVector2(Vector3 v) => new Vector2(v.x, v.y);

        public override IGrid Unbounded => throw new NotImplementedException();

        public override IGrid BoundBy(IBound bound)
        {
            throw new NotImplementedException();
        }

        protected override MeshData GetMeshData(Vector2Int chunk)
        {
            var offset = chunkGrid.GetCellCenter(ChunkToCell(chunk));

            // Make a triangle grid that fills the chunk
            var triangleGrid = new TriangleGrid(0.5f, TriangleOrientation.FlatSides, bound: TriangleBound.Hexagon(n));
            var meshData = Matrix4x4.Translate(offset) * triangleGrid.ToMeshData();

            // Randomly pair the triangles of that grid
            var seed = chunk.x * 1000 + chunk.y;
            var random = new Random(seed);
            meshData = meshData.RandomPairing(random.NextDouble);

            // Split into quads
            meshData = ConwayOperators.Ortho(meshData);

            // Weld vertices
            meshData = meshData.Weld(weldTolerance);

            return meshData;
        }
    }

    /// <summary>
    /// A grid closely modelled after the grid used in Townscaper.
    /// See the corresponding tutorial.
    /// </summary>
    public class TownscaperGrid : RelaxModifier
    {
        const float weldTolerance = 1e-2f;

        public TownscaperGrid(int n):base(new UnrelaxedTownscaperGrid(n, weldTolerance), n, weldTolerance: weldTolerance)
        {

        }
    }
}