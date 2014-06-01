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
    /// class for creating Torus primitive
    /// </summary>
    public class TorusPrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for Torus
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="torusRadius">radius of torus</param>
        /// <param name="coneRadius">radius of cone</param>
        /// <param name="torusSegments">number of triangle of torus</param>
        /// <param name="coneSegments">number of triangle of torus cone</param>
        /// <param name="slice">slice</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float torusRadius, float coneRadius, int torusSegments, int coneSegments, float slice, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            torusRadius = Mathf.Clamp(torusRadius, 0, 100);
            coneRadius = Mathf.Clamp(coneRadius, 0, 100);
            torusSegments = Mathf.Clamp(torusSegments, 3, 250);
            coneSegments = Mathf.Clamp(coneSegments, 3, 100);
            slice = Mathf.Clamp(slice, 0.0f, 360.0f);

            mesh.Clear();

            // normalize slice
            slice = slice / 360.0f;
            int sidesSlice = (int)(torusSegments * (1.0f - slice));
//            float pipeAngle = (1.0f - slice) * 2.0f * Mathf.PI;

            if (sidesSlice == 0)
            {
                sidesSlice = 1;
            }

            int numTriangles = 2 * (coneSegments) * (sidesSlice);
            int numVertices = 0;

            if (normalsType == NormalsType.Vertex)
            {
                numVertices = (sidesSlice + 1)*(coneSegments + 1);
            }
            else
            {
                numVertices = numTriangles*3;
            }

            int numTrianglesCaps = 0;
            int numVerticesCaps = 0;

            if (sidesSlice < torusSegments)
            {
                numTrianglesCaps = (coneSegments + 1)*2;
                numVerticesCaps = (coneSegments + 2)*2;
            }

            if (numVertices > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[numVertices + numVerticesCaps];
            var normals = new Vector3[numVertices + numVerticesCaps];
            var uvs = new Vector2[numVertices + numVerticesCaps];
            var triangles = new int[numTriangles * 3 + numTrianglesCaps * 3];

            var theta = 0.0f;
            float step = 2.0f*Mathf.PI/torusSegments;
            var p = new Vector3();
            var pNext = new Vector3();
            var vertIndex = 0;
            var triIndex = 0;
            var triCapIndex = numTriangles*3;
            var vertCapIndex = numVertices+1;
            var lastTri = vertices.Length-1;

            var pivotOffset = Vector3.zero;
            switch (pivotPosition)
            {
                case PivotPosition.Botttom: pivotOffset = new Vector3(0.0f, coneRadius, 0.0f);
                    break;
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -coneRadius, 0.0f);
                    break;
            }

            for (int i = 0; i <= sidesSlice+1; i++)
            {
                theta += step;

//                if (i == sidesSlice)
//                {
//                    theta = pipeAngle;
//                }

                p = pNext;

                // compute point on torus
                pNext = new Vector3(torusRadius * Mathf.Cos(theta), 0.0f, torusRadius * Mathf.Sin(theta));

                if (i > 0)
                {
                    var T = pNext - p;
                    var N = pNext + p;

                    // find vectors B and N perpendicular to tangent point at torus circle point p
                    var B = Vector3.Cross(T, N);
                    N = Vector3.Cross(B, T);

                    N.Normalize();
                    B.Normalize();

                    var theta2 = 0.0f;
                    var step2 = 2.0f*Mathf.PI/coneSegments;

                    for (int j=0; j<=coneSegments; j++)
                    {
                        theta2 += step2;

                        var s = coneRadius*Mathf.Sin(theta2);
                        var t = coneRadius*Mathf.Cos(theta2);

                        // find point u on cone radius
                        var u = (N*s) + (B*t);

                        vertices[vertIndex + 0] = pNext + u + pivotOffset;
                        normals[vertIndex + 0] = u.normalized;
                        uvs[vertIndex + 0] = new Vector2(1.0f - (float)(i-1) / torusSegments, (float)j / coneSegments);

                        vertIndex += 1;

                        // generate caps
                        if (sidesSlice < torusSegments)
                        {
                            if (i == sidesSlice + 1)
                            {
                                vertices[numVertices] = pNext + pivotOffset;
                                uvs[numVertices] = new Vector2(0.5f, 0.5f);
                                normals[numVertices] = T.normalized;

                                vertices[vertCapIndex + 0] = pNext + u + pivotOffset;
                                normals[vertCapIndex + 0] = T.normalized;
                                uvs[vertCapIndex + 0] = new Vector2(0.5f, 0.5f) + new Vector2(u.x * 0.5f, u.y * 0.5f);

                                if (j < coneSegments)
                                {
                                    triangles[triCapIndex + 0] = vertCapIndex + 1;
                                    triangles[triCapIndex + 1] = vertCapIndex + 0;
                                    triangles[triCapIndex + 2] = numVertices;
                                    triCapIndex += 3;
                                }

                                vertCapIndex += 1;
                            }
                            else if (i == 1)
                            {
                                var c1 = pNext;

                                vertices[lastTri] = c1 + pivotOffset;
                                uvs[lastTri] = new Vector2(0.5f, 0.5f);
                                normals[lastTri] = -T.normalized;

                                vertices[vertCapIndex + 0] = c1 + u + pivotOffset;
                                normals[vertCapIndex + 0] = -T.normalized;
                                uvs[vertCapIndex + 0] = new Vector2(0.5f, 0.5f) + new Vector2(u.x * 0.5f, u.y * 0.5f);

                                if (j < coneSegments)
                                {
                                    triangles[triCapIndex + 0] = lastTri;
                                    triangles[triCapIndex + 1] = vertCapIndex + 0;
                                    triangles[triCapIndex + 2] = vertCapIndex + 1;
                                    triCapIndex += 3;
                                }

                                vertCapIndex += 1;
                            }
                        }
                    }

                    if (i <= sidesSlice)
                    {
                        var torSeg = (i - 1) * (coneSegments + 1);
                        var nextTorSeg = (i) * (coneSegments + 1);
                        var trivert = 0;

                        for (int j = 0; j < coneSegments; j++)
                        {
                            triangles[triIndex + 0] = nextTorSeg + 0 + trivert;
                            triangles[triIndex + 1] = torSeg + 1 + trivert;
                            triangles[triIndex + 2] = torSeg + 0 + trivert;

                            triangles[triIndex + 3] = nextTorSeg + 1 + trivert;
                            triangles[triIndex + 4] = torSeg + 1 + trivert;
                            triangles[triIndex + 5] = nextTorSeg + 0 + trivert;

                            triIndex += 6;
                            trivert += 1;
                        }
                    }
                }
            }

            // duplicate shared vertices for face vertices
            if (normalsType == NormalsType.Face)
            {
                MeshUtils.DuplicateSharedVertices(ref vertices, ref uvs, triangles, -1);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            if (normalsType == NormalsType.Vertex)
            {
                mesh.normals = normals;
            }
            else
            {
                mesh.RecalculateNormals();
            }

            mesh.uv = uvs;
            mesh.RecalculateBounds();
            MeshUtils.CalculateTangents(mesh);
            mesh.Optimize();

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
    }
}
