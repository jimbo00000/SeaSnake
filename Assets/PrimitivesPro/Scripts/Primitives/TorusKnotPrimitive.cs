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
    /// class for creating TorusKnot primitive
    /// </summary>
    public class TorusKnotPrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for TorusKnot
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="torusRadius">radius of torus</param>
        /// <param name="coneRadius">radius of cone</param>
        /// <param name="torusSegments">number of triangle of torus</param>
        /// <param name="coneSegments">number of triangle of torus cone</param>
        /// <param name="P">Knot parameter</param>
        /// <param name="Q">Knot parameter</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float torusRadius, float coneRadius, int torusSegments, int coneSegments, int P, int Q, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            torusRadius = Mathf.Clamp(torusRadius, 0, 100);
            coneRadius = Mathf.Clamp(coneRadius, 0, 100);
            torusSegments = Mathf.Clamp(torusSegments, 3, 250);
            coneSegments = Mathf.Clamp(coneSegments, 3, 100);
            P = Mathf.Clamp(P, 1, 20);
            Q = Mathf.Clamp(Q, 1, 20);

            mesh.Clear();

            int numTriangles = 2 * (coneSegments) * (torusSegments);
            int numVertices = 0;

            if (normalsType == NormalsType.Vertex)
            {
                numVertices = (torusSegments + 1)*(coneSegments + 1);
            }
            else
            {
                numVertices = numTriangles * 3;
            }

            if (numVertices > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[numVertices];
            var normals = new Vector3[numVertices];
            var uvs = new Vector2[numVertices];
            var triangles = new int[numTriangles * 3];

            var theta = 0.0f;
            float step = 2.0f*Mathf.PI/torusSegments;
            var p = new Vector3();
            var pNext = new Vector3();
            var vertIndex = 0;
            var triIndex = 0;

            var minY = float.MaxValue;
            var maxY = float.MinValue;

            for (int i = 0; i <= torusSegments+1; i++)
            {
                theta += step;

                var r = torusRadius*0.5f*(2.0f + Mathf.Sin(Q*theta));
                p = pNext;

                // compute point on torus
                pNext = new Vector3(r * Mathf.Cos(P * theta), r * Mathf.Cos(Q * theta), r * Mathf.Sin(P * theta));

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

                        vertices[vertIndex + 0] = pNext + u;
                        normals[vertIndex + 0] = u.normalized;
                        uvs[vertIndex + 0] = new Vector2((float)(i-1) / torusSegments, (float)j / coneSegments);

                        if (vertices[vertIndex].y < minY)
                        {
                            minY = vertices[vertIndex].y;
                        }
                        if (vertices[vertIndex].y > maxY)
                        {
                            maxY = vertices[vertIndex].y;
                        }

                        vertIndex += 1;
                    }

                    if (i <= torusSegments)
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

            // adjust pivot position
            if (pivotPosition != PivotPosition.Center)
            {
                var pivotOffset = pivotPosition == PivotPosition.Botttom ? -minY : -maxY;

                for (int i=0; i<vertices.Length; i++)
                {
                    vertices[i].y += pivotOffset;
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
