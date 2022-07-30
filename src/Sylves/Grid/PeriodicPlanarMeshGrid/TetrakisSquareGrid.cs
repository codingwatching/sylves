﻿using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public class TetrakisSquareGrid : PeriodicPlanarMeshGrid
    {
        public TetrakisSquareGrid():base(TetrakisSquareGridMeshData(), new Vector2(1, 0), new Vector2(0, 1))
        {

        }
        private static MeshData TetrakisSquareGridMeshData()
        {
            var meshData = new MeshData();
            meshData.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 0),
            };
            meshData.indices = new[]{new []
            {
                0, 1, 2, 3,
            } };
            meshData.subMeshCount = 1;
            meshData.topologies = new[] { MeshTopology.Quads };
            return ConwayOperators.Kis(meshData);
        }

    }
}