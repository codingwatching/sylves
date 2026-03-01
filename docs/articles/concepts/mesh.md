# Meshes

A [polygon mesh](https://en.wikipedia.org/wiki/Polygon_mesh) is a data structure commonly used in computer graphics. Roughly speaking they are a collection of vertices, edges and faces.

Sylves mostly deals with meshes using the [MeshData](xref:Sylves.MeshData) class which stores a mesh in arrays. MeshData is used with various grids:

* [MeshGrid](../grids/meshgrid.md)
* [MeshPrismGrid](../grids/meshprismgrid.md)
* [PeriodicPlanarMeshGrid](../grids/periodicplanarmeshgrid.md)

There's also [MeshUtils.ToMesh](xref:Sylves.MeshUtils.ToMesh(Sylves.IGrid)) to convert from an arbitrary finite grid back to Meshdata.

## MeshData format

[MeshData](xref:Sylves.MeshData) stores mesh geometry in arrays, similar to Unity’s `Mesh` but usable outside Unity and with support for arbitrary n-gons.

**Vertex data**

* **vertices** — `Vector3[]`: 3D positions. All faces reference these by index.
* **uv** — `Vector2[]` (optional): texture coordinates, one per vertex. May be null.
* **normals** — `Vector3[]` (optional): vertex normals. May be null; can be filled with [RecalculateNormals](xref:Sylves.MeshData.RecalculateNormals).
* **tangents** — `Vector4[]` (optional): vertex tangents. May be null.

When present, `uv`, `normals` and `tangents` have the same length as `vertices`.

**Indexes**

The mesh data is organized into submeshes, and each submesh into various faces.

For each submesh:

* **indices[i]** — `int[]`: vertex indices that define the faces of submesh `i`.
* **topologies[i]** — [MeshTopology](xref:Sylves.MeshTopology): how those indices are grouped into faces.

The indices index into the vertex data, allowing several faces to share a vertex.

Topology determines how each submesh's indices array is interpreted.

* [`Triangles`](xref:Sylves.MeshTopology.Triangles) — each set of 3 indices is a face
* [`Quads`](xref:Sylves.MeshTopology.Quads) — each set of 4 indices is a face.
* [`NGon`](xref:Sylves.MeshTopology.NGon) — faces are variable length, with the final index indicated by bitwise inversion. I.e. you could declare a triangle followed by a square with array `new int[] {0,1,~2,3,4,5,~6}`.

## Deformation

The [grid deformation functionality](shape.md#deformation) can be used from any mesh with [`Sylves.DeformationUtils.GetDeformation`](xref:Sylves.DeformationUtils.GetDeformation(Sylves.MeshData,System.Single,System.Single,System.Boolean,System.Int32,System.Single,System.Int32,System.Boolean)).



