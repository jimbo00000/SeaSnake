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
    /// class for creating Ellipse primitive
    /// </summary>
    public class EllipsePrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for Ellipse
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="radius0">radius0 of ellipse</param>
        /// <param name="radius1">radius1 of ellipse</param>
        /// <param name="segments">number of segments</param>
        public static float GenerateGeometry(Mesh mesh, float radius0, float radius1, int segments)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            radius0 = Mathf.Clamp(radius0, 0, 100);
            radius1 = Mathf.Clamp(radius1, 0, 100);
            segments = Mathf.Clamp(segments, 3, 100);

            mesh.Clear();

            var verticesNum = 1 + segments;
            var trianglesNum = segments;

            if (verticesNum > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[verticesNum];
            var normals = new Vector3[verticesNum];
            var uvs = new Vector2[verticesNum];
            var triangles = new int[trianglesNum*3];

            // generate outer points
            for (int i = 0; i < segments; i++)
            {
                var angle = (float) i/segments * Mathf.PI * 2.0f;
                var v0 = new Vector3(Mathf.Sin(angle), 0.0f, Mathf.Cos(angle));

                vertices[i] = new Vector3(v0.x*radius0, 0.0f, v0.z*radius1);
                normals[i] = new Vector3(0, 1, 0);

                var radiusRatio = radius0/radius1;

                uvs[i] = new Vector2(0.5f + v0.x*0.5f*radiusRatio, 0.5f + v0.z*0.5f*1/radiusRatio);
            }

            // generate central point
            vertices[segments] = new Vector3(0, 0, 0);
            normals[segments] = new Vector3(0, 1, 0);
            uvs[segments] = new Vector2(0.5f, 0.5f);

            // generate triangles
            var triVert = 0;
            var triIdx = 0;
            for (int i = 0; i < segments; i++)
            {
                triangles[triIdx + 0] = triVert + 0;
                triangles[triIdx + 1] = triVert + 1;
                triangles[triIdx + 2] = segments;

                if (i == segments-1)
                {
                    triangles[triIdx + 1] = 0;
                }

                triVert += 1;
                triIdx += 3;
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
