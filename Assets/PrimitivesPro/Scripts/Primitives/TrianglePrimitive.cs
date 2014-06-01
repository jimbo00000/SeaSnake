// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PrimitivesPro.Primitives
{
    /// <summary>
    /// class for creating Triangle primitive
    /// </summary>
    public class TrianglePrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for Triangle
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="side">length of side</param>
        /// <param name="subdivision">subdivision of the triangle</param>
        public static float GenerateGeometry(Mesh mesh, float side, int subdivision)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            side = Mathf.Clamp(side, 0.01f, 100);
            subdivision = Mathf.Clamp(subdivision, 0, 7);

            mesh.Clear();

            // generate central point
            var trianglesNum = GetTriCount(subdivision);
            var verticesNum = GetVertCount(subdivision);

            var triangles = new int[trianglesNum * 3];
            var trianglesTmp = new int[trianglesNum * 3];
            var vertices = new Vector3[verticesNum];
            var normals = new Vector3[verticesNum];
            var uvs = new Vector2[verticesNum];

            var a = Vector3.zero;
            var v0 = new Vector3(1, 0, 0);
            var b = a + v0*side;

            // rotate vector by 60 degrees in equilateral triangle
            var vec0 = Quaternion.AngleAxis(-60, Vector3.up) * v0;

            var c = a + vec0*side;

            var centroid = (a + b + c)/3.0f;

            vertices[0] = a - centroid;
            vertices[1] = b - centroid;
            vertices[2] = c - centroid;

            normals[0] = new Vector3(0, 1, 0);
            normals[1] = new Vector3(0, 1, 0);
            normals[2] = new Vector3(0, 1, 0);

            triangles[0] = 2;
            triangles[1] = 1;
            triangles[2] = 0;

            uvs[0] = new Vector2(0.0f, 0.0f);
            uvs[1] = new Vector2(1.0f, 0.0f);
            uvs[2] = new Vector2(0.5f, Mathf.Sin(Mathf.Deg2Rad*60.0f));

            // triangle subdivision
            var indexLookup = new Dictionary<int, int>();

            var vertIndex = GetVertCount(0);

            for (int i = 0; i < subdivision; i++)
            {
                var newTriIdx = 0;
                var triCount = GetTriCount(i) * 3;

                for (int triIdx = 0; triIdx < triCount; triIdx += 3)
                {
                    // get triangle
                    var v1 = triangles[triIdx + 0];
                    var v2 = triangles[triIdx + 1];
                    var v3 = triangles[triIdx + 2];

                    // split each edge in half
                    var va = AddMidPoint(vertices, uvs, normals, vertIndex++, v1, v2, indexLookup);
                    var vb = AddMidPoint(vertices, uvs, normals, vertIndex++, v2, v3, indexLookup);
                    var vc = AddMidPoint(vertices, uvs, normals, vertIndex++, v3, v1, indexLookup);

                    // create 4 new triangles
                    trianglesTmp[newTriIdx + 0] = v1;
                    trianglesTmp[newTriIdx + 1] = va;
                    trianglesTmp[newTriIdx + 2] = vc;

                    trianglesTmp[newTriIdx + 3] = v2;
                    trianglesTmp[newTriIdx + 4] = vb;
                    trianglesTmp[newTriIdx + 5] = va;

                    trianglesTmp[newTriIdx + 6] = v3;
                    trianglesTmp[newTriIdx + 7] = vc;
                    trianglesTmp[newTriIdx + 8] = vb;

                    trianglesTmp[newTriIdx + 9] = va;
                    trianglesTmp[newTriIdx + 10] = vb;
                    trianglesTmp[newTriIdx + 11] = vc;

                    newTriIdx += 12;
                }

                var swapTmp = trianglesTmp;
                trianglesTmp = triangles;
                triangles = swapTmp;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            MeshUtils.CalculateTangents(mesh);
            mesh.Optimize();

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// create new vertex by spliting triangle edge
        /// </summary>
        /// <param name="vertices">array of vertices</param>
        /// <param name="normals">array of normals</param>
        /// <param name="vertIndex">current vertex index</param>
        /// <param name="id0">index of first edge point</param>
        /// <param name="id1">index of second edge point</param>
        /// <param name="indexLookup">lookup cache for already computed vertices</param>
        /// <param name="uvs">array of uvs</param>
        /// <returns>returns index of new point</returns>
        static int AddMidPoint(Vector3[] vertices, Vector2[] uvs, Vector3[] normals, int vertIndex, int id0, int id1, Dictionary<int, int> indexLookup)
        {
            int key = 0;

            // for shared vertices make a lookup for used index
            key = id0 < id1 ? (id0 << 16) + id1 : (id1 << 16) + id0;

            int result;
            if (indexLookup.TryGetValue(key, out result))
            {
                return result;
            }

            // make unit vector in direction to split edge
            vertices[vertIndex] = (vertices[id0] + vertices[id1])*0.5f;
            normals[vertIndex] = Vector3.up;
            uvs[vertIndex] = (uvs[id0] + uvs[id1])*0.5f;

            // add index to lookup
            indexLookup.Add(key, vertIndex);

            return vertIndex;
        }

        static int GetVertCount(int subdivision)
        {
            return GetTriCount(subdivision)  * 3;
        }

        static int GetTriCount(int subdivision)
        {
            return (int)Mathf.Pow(4, subdivision);
        }
    }
}
