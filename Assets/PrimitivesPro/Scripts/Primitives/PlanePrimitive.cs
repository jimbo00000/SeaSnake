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
    /// class for creating Plane primitive
    /// </summary>
    public class PlanePrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for plane
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="width">width of plane</param>
        /// <param name="length">length of plane</param>
        /// <param name="widthSegments">number of triangle segments in width</param>
        /// <param name="lengthSegments">number of triangle segments in length</param>
        public static float GenerateGeometry(Mesh mesh, float width, float length, int widthSegments, int lengthSegments)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            width = Mathf.Clamp(width, 0, 100);
            length = Mathf.Clamp(length, 0, 100);
            widthSegments = Mathf.Clamp(widthSegments, 1, 100);
            lengthSegments = Mathf.Clamp(lengthSegments, 1, 100);

            mesh.Clear();

            int hCount2 = widthSegments + 1;
            int vCount2 = lengthSegments + 1;
            int numTriangles = widthSegments * lengthSegments * 6;
            int numVertices = hCount2 * vCount2;

            if (numVertices > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[numVertices];
            var uvs = new Vector2[numVertices];
            var triangles = new int[numTriangles];

            int index = 0;
            float uvFactorX = 1.0f / widthSegments;
            float uvFactorY = 1.0f / lengthSegments;
            float scaleX = width / widthSegments;
            float scaleY = length / lengthSegments;

            for (float y = 0.0f; y < vCount2; y++)
            {
                for (float x = 0.0f; x < hCount2; x++)
                {
                    vertices[index] = new Vector3(x * scaleX - width / 2f, 0.0f, y * scaleY - length / 2f);
                    uvs[index++] = new Vector2(x * uvFactorX, y * uvFactorY);
                }
            }

            var triIndex = 0;
            for (int y = 0; y < lengthSegments; y++)
            {
                for (int x = 0; x < widthSegments; x++)
                {
                    triangles[triIndex + 0] = (y * hCount2) + x;
                    triangles[triIndex + 1] = ((y + 1) * hCount2) + x;
                    triangles[triIndex + 2] = (y * hCount2) + x + 1;

                    triangles[triIndex + 3] = ((y + 1) * hCount2) + x;
                    triangles[triIndex + 4] = ((y + 1) * hCount2) + x + 1;
                    triangles[triIndex + 5] = (y * hCount2) + x + 1;
                    triIndex += 6;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            MeshUtils.CalculateTangents(mesh);
            mesh.RecalculateBounds();
            mesh.Optimize();

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
    }
}
