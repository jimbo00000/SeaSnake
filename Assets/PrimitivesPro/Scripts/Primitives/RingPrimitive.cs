// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Diagnostics;
using UnityEngine;

namespace PrimitivesPro.Primitives
{
    /// <summary>
    /// class for creating Ring primitive
    /// </summary>
    public class RingPrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for Ring
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="radius0">radius0 of ring</param>
        /// <param name="radius1">radius1 of ring</param>
        /// <param name="segments">number of segments</param>
        public static float GenerateGeometry(Mesh mesh, float radius0, float radius1, int segments)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            radius0 = Mathf.Clamp(radius0, 0, 100);
            radius1 = Mathf.Clamp(radius1, 0, 100);
            segments = Mathf.Clamp(segments, 3, 100);

            mesh.Clear();

            var verticesNum = segments * 2;
            var trianglesNum = segments * 2;

            if (verticesNum > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[verticesNum];
            var normals = new Vector3[verticesNum];
            var uvs = new Vector2[verticesNum];
            var triangles = new int[trianglesNum*3];

            // generate vertices
            var vertIndex = 0;
            for (int i = 0; i < segments; i++)
            {
                var angle = (float) i/segments * Mathf.PI * 2.0f;
                var v0 = new Vector3(Mathf.Sin(angle), 0.0f, Mathf.Cos(angle));

                var uvRatio = 0.5f * (radius0 / radius1);

                var uvV = new Vector2(v0.x * 0.5f, v0.z * .5f);
                var uvVInner = new Vector2(v0.x * uvRatio, v0.z * uvRatio);
                var uvCenter = new Vector2(0.5f, 0.5f);

                vertices[vertIndex + 0] = new Vector3(v0.x*radius0, 0.0f, v0.z*radius0);
                normals[vertIndex + 0] = new Vector3(0, 1, 0);
                uvs[vertIndex + 0] = uvCenter + uvVInner;

                vertices[vertIndex + 1] = new Vector3(v0.x * radius1, 0.0f, v0.z * radius1);
                normals[vertIndex + 1] = new Vector3(0, 1, 0);
                uvs[vertIndex + 1] = uvCenter + uvV;

                vertIndex += 2;
            }

            // generate triangles
            var triVert = 0;
            var triIdx = 0;
            for (int i = 0; i < segments; i++)
            {
                triangles[triIdx + 0] = triVert + 0;
                triangles[triIdx + 1] = triVert + 1;
                triangles[triIdx + 2] = triVert + 3;

                triangles[triIdx + 3] = triVert + 2;
                triangles[triIdx + 4] = triVert + 0;
                triangles[triIdx + 5] = triVert + 3;

                if (i == segments - 1)
                {
                    triangles[triIdx + 2] = 1;
                    triangles[triIdx + 3] = 0;
                    triangles[triIdx + 5] = 1;
                }

                triVert += 2;
                triIdx += 6;
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
    }
}
