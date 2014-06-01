// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Diagnostics;
using UnityEngine;

namespace PrimitivesPro.Primitives
{
    /// <summary>
    /// class for creating SuperEllipsoid primitive
    /// generation algorithm is based on: 
    /// http://paulbourke.net/geometry/roundcube/ and http://paulbourke.net/geometry/superellipse/
    /// 
    /// for possible combinations of parameters n1 and n2 with picture reference see:
    /// http://en.wikipedia.org/wiki/Superellipsoid
    /// </summary>
    public class SuperEllipsoidPrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for SuperEllipsoid
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="width">width of roundedCube</param>
        /// <param name="height">height of roundedCube</param>
        /// <param name="length">length of roundedCube</param>
        /// <param name="segments">number of segments</param>
        /// <param name="n2">second parameter of superellipsoid</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        /// <param name="n1">second parameter of superellipsoid</param>
        public static float GenerateGeometry(Mesh mesh, float width, float height, float length, int segments, float n1, float n2, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            width = Mathf.Clamp(width, 0, 100);
            length = Mathf.Clamp(length, 0, 100);
            height = Mathf.Clamp(height, 0, 100);
            segments = Mathf.Clamp(segments, 1, 100);
            n1 = Mathf.Clamp(n1, 0.01f, 5.0f);
            n2 = Mathf.Clamp(n2, 0.01f, 5.0f);

            mesh.Clear();

            // to fix spherical uv generation use only segments % 3
            segments = segments*4 - 1;

            segments += 5;

            var numVertices = (segments + 1)*(segments/2 + 1);
            var numTriangles = segments*(segments / 2) * 6;

            if (normalsType == NormalsType.Face)
            {
                numVertices = numTriangles;
            }

            if (numVertices > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[numVertices];
            var uvs = new Vector2[numVertices];
            var triangles = new int[numTriangles];

            var pivotOffset = Vector3.zero;
            switch (pivotPosition)
            {
                case PivotPosition.Botttom: pivotOffset = new Vector3(0.0f, height, 0.0f);
                    break;
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -height, 0.0f);
                    break;
            }

            var vertIndex = 0;

            for (int j = 0; j <= segments / 2; j++)
            {
                for (int i = 0; i <= segments; i++)
                {
                    var index = j * (segments + 1) + i;
                    var theta = i * 2.0f * Mathf.PI / segments;
                    var phi = -0.5f * Mathf.PI + Mathf.PI * j / (segments / 2.0f);

                    // make unit sphere, power determines roundness
                    vertices[index].x = RPower(Mathf.Cos(phi), n1) * RPower(Mathf.Cos(theta), n2) * width;
                    vertices[index].z = RPower(Mathf.Cos(phi), n1) * RPower(Mathf.Sin(theta), n2) * length;
                    vertices[index].y = RPower(Mathf.Sin(phi), n1) * height;

                    // compute uv spherical mapping
                    uvs[index].x = Mathf.Atan2(vertices[index].z, vertices[index].x) / (2.0f * Mathf.PI);

                    if (uvs[index].x < 0)
                    {
                        uvs[index].x = 1 + uvs[index].x;
                    }

                    uvs[index].y = 0.5f + Mathf.Atan2(vertices[index].y, Mathf.Sqrt(vertices[index].x * vertices[index].x + vertices[index].z * vertices[index].z)) / Mathf.PI;

                    // fix seams
                    if (j == 0)
                    {
                        vertices[index].x = 0;
                        vertices[index].y = -height;
                        vertices[index].z = 0;
                        uvs[index].y = 0.0f;
                        uvs[index].x = 0;
                    }

                    if (j == segments / 2)
                    {
                        vertices[index].x = 0;
                        vertices[index].y = height;
                        vertices[index].z = 0;
                        uvs[index].y = 1.0f;
                        uvs[index].x = uvs[(j-1) * (segments + 1) + i].x;
                    }

                    if (i == segments)
                    {
                        vertices[index].x = vertices[j * (segments + 1)].x;
                        vertices[index].z = vertices[j * (segments + 1)].z;
                        uvs[index].x = 1.0f;
                    }

                    vertices[index] += pivotOffset;
                 
                    if (vertIndex < index)
                    {
                        vertIndex = index;
                    }
                }
            }

            // fix uv seam
            for (int i=0; i<=segments; i++)
            {
                var indexNext = (segments + 1) + i;
                uvs[i].x = uvs[indexNext].x;
            }

            var triIndex = 0;
            for (int j=0;j<segments/2;j++)
            {
                for (int i = 0; i < segments; i++)
                {
                    var i1 = j*(segments + 1) + i;
                    var i2 = j*(segments + 1) + (i + 1);
                    var i3 = (j + 1)*(segments + 1) + (i + 1);
                    var i4 = (j + 1)*(segments + 1) + i;

                    triangles[triIndex + 0] = i3;
                    triangles[triIndex + 1] = i2;
                    triangles[triIndex + 2] = i1;
                    triangles[triIndex + 3] = i4;
                    triangles[triIndex + 4] = i3;
                    triangles[triIndex + 5] = i1;

                    triIndex += 6;
                }
            }

            if (normalsType == NormalsType.Face)
            {
                MeshUtils.DuplicateSharedVertices(ref vertices, ref uvs, triangles, -1);
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            if (normalsType == NormalsType.Vertex)
            {
                Vector3[] normals = null;
                MeshUtils.ComputeVertexNormals(vertices, triangles, out normals);

                // fix normals seam
                for (int j = 0; j < segments / 2; j++)
                {
                    var index0 = j * (segments + 1);
                    var index = j * (segments + 1) + segments;
                    normals[index] = normals[index0];
                }

                mesh.normals = normals;
            }
            else
            {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
            MeshUtils.CalculateTangents(mesh);
            mesh.Optimize();

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }

        static float RPower(float v, float n)
        {
            if (v >= 0)
            {
                return Mathf.Pow(v, n);
            }

            return -Mathf.Pow(-v, n);
        }
    }
}
