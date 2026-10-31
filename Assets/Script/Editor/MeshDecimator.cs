using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Simple grid-based vertex-clustering mesh decimator.
/// Merges vertices that fall into the same grid cell, rebuilds the
/// triangle list, and saves a new Mesh asset alongside the source.
/// Preserves skin weights, normals, UVs and colors by keeping the
/// first vertex found in each cell. Rough but safe for stylized models.
/// </summary>
public static class MeshDecimator
{
    /// <summary>
    /// Decimate a mesh asset to approximately the given grid cell size.
    /// Smaller cell = finer = closer to original. Returns the new mesh.
    /// </summary>
    public static Mesh Decimate(Mesh src, float cellSize)
    {
        var verts = src.vertices;
        var tris = src.triangles;
        var normals = src.normals;
        var uv = src.uv;
        var boneWeights = src.boneWeights;
        var colors32 = src.colors32;

        var map = new int[verts.Length];          // old index -> new index
        var cellToNew = new Dictionary<Vector3Int, int>();
        var newVerts = new List<Vector3>();
        var newNormals = new List<Vector3>();
        var newUv = new List<Vector2>();
        var newWeights = new List<BoneWeight>();
        var newColors = new List<Color32>();

        for (int i = 0; i < verts.Length; i++)
        {
            var cell = Vector3Int.RoundToInt(verts[i] / cellSize);
            if (cellToNew.TryGetValue(cell, out int ni))
            {
                map[i] = ni;
                continue;
            }
            ni = newVerts.Count;
            cellToNew[cell] = ni;
            map[i] = ni;
            newVerts.Add(verts[i]);
            if (normals != null && normals.Length == verts.Length) newNormals.Add(normals[i]);
            if (uv != null && uv.Length == verts.Length) newUv.Add(uv[i]);
            if (boneWeights != null && boneWeights.Length == verts.Length) newWeights.Add(boneWeights[i]);
            if (colors32 != null && colors32.Length == verts.Length) newColors.Add(colors32[i]);
        }

        var newTris = new List<int>();
        for (int t = 0; t < tris.Length; t += 3)
        {
            int a = map[tris[t]], b = map[tris[t + 1]], c = map[tris[t + 2]];
            if (a == b || b == c || a == c) continue; // degenerate
            newTris.Add(a); newTris.Add(b); newTris.Add(c);
        }

        var dst = new Mesh();
        dst.name = src.name + "_dec";
        dst.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        dst.vertices = newVerts.ToArray();
        if (newNormals.Count == newVerts.Count) dst.normals = newNormals.ToArray();
        if (newUv.Count == newVerts.Count) dst.uv = newUv.ToArray();
        if (newWeights.Count == newVerts.Count) dst.boneWeights = newWeights.ToArray();
        if (newColors.Count == newVerts.Count) dst.colors32 = newColors.ToArray();
        dst.bindposes = src.bindposes;
        dst.triangles = newTris.ToArray();
        dst.RecalculateBounds();
        return dst;
    }
}
