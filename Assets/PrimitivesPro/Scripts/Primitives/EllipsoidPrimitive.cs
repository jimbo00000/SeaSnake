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
    /// class for creating Ellipsoid primitive
    /// </summary>
    public class EllipsoidPrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for Ellipsoid
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="width">width of ellipsoid</param>
        /// <param name="height">height of ellipsoid</param>
        /// <param name="length">length of ellipsoid</param>
        /// <param name="segments">number of segments</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float width, float height, float length, int segments, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            width = Mathf.Clamp(width, 0, 100);
            height = Mathf.Clamp(height, 0, 100);
            length = Mathf.Clamp(length, 0, 100);
            segments = Mathf.Clamp(segments, 4, 100);

            mesh.Clear();

            int rings = segments-1;
            int sectors = segments;

            float R = 1 / (float)(rings - 1);
            float S = 1 / (float)(sectors - 1);

            int verticesNum = rings * sectors;
            var trianglesNum = (rings - 1)*(sectors - 1)*6;

            if (normalsType == NormalsType.Face)
            {
                verticesNum = trianglesNum;
            }

            var pivotOffset = Vector3.zero;
            switch (pivotPosition)
            {
                case PivotPosition.Botttom: pivotOffset = new Vector3(0.0f, height, 0.0f);
                    break;
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -height, 0.0f);
                    break;
            }

            if (verticesNum > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[verticesNum];
            var normals = new Vector3[verticesNum];
            var uvs = new Vector2[verticesNum];
            var triangles = new int[trianglesNum];

            var vertIndex = 0;
            var triIndex = 0;

            for (int r = 0; r < rings; r++)
            {
                var y = Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * r * R);
                var sinR = Mathf.Sin(Mathf.PI*r*R);

                for (int s = 0; s < sectors; s++)
                {
                    float x = Mathf.Sin(2 * Mathf.PI * s * S) * sinR;
                    float z = Mathf.Cos(2 * Mathf.PI * s * S) * sinR;

                    vertices[vertIndex + 0] = new Vector3(x*width, y*height, z*length) + pivotOffset;
                    normals[vertIndex + 0] = new Vector3(x, y, z);
                    uvs[vertIndex + 0] = new Vector2(1.0f - (s * S), 1.0f - (r * R));

                    if (r < rings-1 && s < sectors - 1)
                    {
                        triangles[triIndex + 0] = (r + 1) * sectors + (s);
                        triangles[triIndex + 1] = r * sectors + (s + 1);
                        triangles[triIndex + 2] = r * sectors + s;

                        triangles[triIndex + 3] = (r + 1) * sectors + (s + 1);
                        triangles[triIndex + 4] = r * sectors + (s + 1);
                        triangles[triIndex + 5] = (r + 1) * sectors + (s);

                        triIndex += 6;
                    }

                    vertIndex += 1;
                }
            }

            // duplicate triangles in face case
            if (normalsType == NormalsType.Face)
            {
                MeshUtils.DuplicateSharedVertices(ref vertices, ref uvs, triangles, -1);
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            // generate normals by unity in face case
            if (normalsType == NormalsType.Face)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                mesh.normals = normals;
            }

            mesh.RecalculateBounds();
            MeshUtils.CalculateTangents(mesh);
            mesh.Optimize();

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
    }
}
