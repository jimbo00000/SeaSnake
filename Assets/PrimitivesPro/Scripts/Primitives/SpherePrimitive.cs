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
    /// class for creating Sphere primitive
    /// </summary>
    public class SpherePrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for Sphere
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="radius">radius of sphere</param>
        /// <param name="segments">number of segments</param>
        /// <param name="hemisphere">hemisphere, 0 ... complete sphere, 0.5 ... half-sphere</param>
        /// <param name="innerRadius">radius of the inner sphere</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float radius, int segments, float hemisphere, float innerRadius, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            radius = Mathf.Clamp(radius, 0, 100);
            segments = Mathf.Clamp(segments, 4, 100);
            hemisphere = Mathf.Clamp(hemisphere, 0.0f, 1.0f);

            mesh.Clear();

            int rings = segments-1;
            int sectors = segments;

            var hemisphereCapY = -1 + hemisphere * 2;
            int hemisphereCapRing = rings;
            var hemisphereYpos = -radius;

            float R = 1 / (float)(rings - 1);
            float S = 1 / (float)(sectors - 1);

            // calculate hemisphere parameters
            var lastY = 0.0f;
            for (int r = 0; r < rings; r++)
            {
                var y = Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * r * R);

                if (y < hemisphereCapY)
                {
                    hemisphereCapRing = r;
                    hemisphereYpos = lastY * radius;
                    break;
                }

                lastY = y;
            }

            int verticesNum = 0;
            var trianglesNum = (hemisphereCapRing/* - 1*/)*(sectors/* - 1*/)*6;
            var verticesHemisphereNum = segments + 1;
            var trianglesHemisphereNum = segments * 3;

            if (hemisphereCapRing == rings)
            {
                trianglesNum -= sectors * 3;
            }

            if (normalsType == NormalsType.Vertex)
            {
                verticesNum = (hemisphereCapRing+1) * (sectors+1);
            }
            else
            {
                verticesNum = trianglesNum;
            }

            if (hemisphereCapRing == rings)
            {
                verticesNum -= sectors + 1;
            }

            if (innerRadius > 0 && hemisphereCapRing < rings)
            {
                verticesNum *= 2;
                trianglesNum *= 2;

                verticesNum += sectors*2;
                trianglesNum += sectors*3;
            }

            var pivotOffset = Vector3.zero;
            var height = radius-hemisphereYpos;
            switch (pivotPosition)
            {
                case PivotPosition.Botttom: pivotOffset = new Vector3(0.0f, radius, 0.0f);
                    break;
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, hemisphereYpos, 0.0f);
                    break;
                case PivotPosition.Center: pivotOffset = new Vector3(0.0f, (hemisphereYpos + height/2), 0.0f);
                    break;
            }

            if (verticesNum + verticesHemisphereNum > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[verticesNum + verticesHemisphereNum];
            var normals = new Vector3[verticesNum + verticesHemisphereNum];
            var uvs = new Vector2[verticesNum + verticesHemisphereNum];
            var triangles = new int[trianglesNum + trianglesHemisphereNum];

            var vertIndex = 0;
            var triIndex = 0;

            for (int r = 0; r < hemisphereCapRing; r++)
            {
                var y = -Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * r * R);
                var sinR = Mathf.Sin(Mathf.PI*r*R);

                for (int s = 0; s < sectors; s++)
                {
                    float x = Mathf.Sin(2 * Mathf.PI * s * S) * sinR;
                    float z = Mathf.Cos(2 * Mathf.PI * s * S) * sinR;

                    vertices[vertIndex + 0] = new Vector3(x, y, z) * radius + pivotOffset;
                    normals[vertIndex + 0] = new Vector3(x, y, z);
                    uvs[vertIndex + 0] = new Vector2(1.0f - (s * S), (r * R));

                    if (r < hemisphereCapRing-1 && s < sectors-1)
                    {
                        //543
                        triangles[triIndex + 5] = (r + 1) * sectors + (s);
                        triangles[triIndex + 4] = r * sectors + (s + 1);
                        triangles[triIndex + 3] = r * sectors + s;
                        //210
                        triangles[triIndex + 2] = (r + 1) * sectors + (s + 1);
                        triangles[triIndex + 1] = r * sectors + (s + 1);
                        triangles[triIndex + 0] = (r + 1) * sectors + (s);

                        triIndex += 6;
                    }

                    vertIndex += 1;
                }
            }

            var hemisphereVertOffset = vertIndex;

            // calculate hemisphere plane
            if (hemisphereCapRing < rings)
            {
                // generate inner sphere
                if (innerRadius > 0)
                {
                    var vertOffset = hemisphereVertOffset;
                    var outerY = -Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * (hemisphereCapRing - 1) * R) * radius;
                    var innertY = -Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * (hemisphereCapRing-1) * R) * innerRadius;
                    var diff = new Vector3(0, outerY - innertY, 0);

                    for (int r = 0; r < hemisphereCapRing; r++)
                    {
                        var y = -Mathf.Cos(-Mathf.PI*2.0f + Mathf.PI*r*R);
                        var sinR = Mathf.Sin(Mathf.PI*r*R);

                        for (int s = 0; s < sectors; s++)
                        {
                            float x = Mathf.Sin(2*Mathf.PI*s*S)*sinR;
                            float z = Mathf.Cos(2*Mathf.PI*s*S)*sinR;

                            vertices[vertIndex + 0] = new Vector3(x, y, z)*innerRadius + pivotOffset + diff;
                            normals[vertIndex + 0] = -new Vector3(x, y, z);
                            uvs[vertIndex + 0] = new Vector2(1.0f - (s*S), (r*R));

                            if (r < hemisphereCapRing - 1 && s < sectors - 1)
                            {
                                triangles[triIndex + 0] = vertOffset + (r + 1)*sectors + (s);
                                triangles[triIndex + 1] = vertOffset + r*sectors + (s + 1);
                                triangles[triIndex + 2] = vertOffset + r*sectors + s;

                                triangles[triIndex + 3] = vertOffset + (r + 1)*sectors + (s + 1);
                                triangles[triIndex + 4] = vertOffset + r*sectors + (s + 1);
                                triangles[triIndex + 5] = vertOffset + (r + 1)*sectors + (s);

                                triIndex += 6;
                            }

                            vertIndex += 1;
                        }
                    }

                    hemisphereVertOffset = vertIndex;

                    // duplicate triangles in face case
                    if (normalsType == NormalsType.Face)
                    {
                        MeshUtils.DuplicateSharedVertices(ref vertices, ref uvs, triangles, triIndex);
                        hemisphereVertOffset = triIndex;
                    }

                    // connect two hemi-spheres
                    {
                        var y = -Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * (hemisphereCapRing - 1) * R);
                        var sinR = Mathf.Sin(Mathf.PI * (hemisphereCapRing - 1) * R);

                        var triVert = hemisphereVertOffset;
                        vertIndex = triVert;

                        for (int i = 0; i < sectors; i++)
                        {
                            var x = Mathf.Sin(2 * Mathf.PI * i * S) * sinR;
                            var z = Mathf.Cos(2 * Mathf.PI * i * S) * sinR;
                            var v = new Vector3(x, y, z);

                            v.Normalize();

                            vertices[vertIndex + 0] = v * radius + pivotOffset;
                            vertices[vertIndex + 1] = v*innerRadius + pivotOffset + diff;
                            normals[vertIndex + 0] = new Vector3(0.0f, 1.0f, 0.0f);
                            normals[vertIndex + 1] = new Vector3(0.0f, 1.0f, 0.0f);

                            // make planar uv mapping for hemisphere plane
                            var uvV = new Vector2(v.x * 0.5f, v.z * .5f);
                            var uvCenter = new Vector2(0.5f, 0.5f);
                            var uvV2 = new Vector2(v.x * innerRadius / radius * 0.5f, v.z * innerRadius /radius * 0.5f);

                            uvs[vertIndex + 0] = uvCenter + uvV;
                            uvs[vertIndex + 1] = uvCenter + uvV2;

                            vertIndex += 2;
                        }

                        for (int i = 0; i < rings; i++)
                        {
                            triangles[triIndex + 2] = triVert + 1;
                            triangles[triIndex + 1] = triVert + 2;
                            triangles[triIndex + 0] = triVert + 0;

                            triangles[triIndex + 4] = triVert + 3;
                            triangles[triIndex + 5] = triVert + 1;
                            triangles[triIndex + 3] = triVert + 2;

                            triIndex += 6;
                            triVert += 2;
                        }
                    }
                }
                else
                {
                    // duplicate triangles in face case
                    if (normalsType == NormalsType.Face)
                    {
                        MeshUtils.DuplicateSharedVertices(ref vertices, ref uvs, triangles, triIndex);
                        hemisphereVertOffset = triIndex;
                    }

                    var triVert = hemisphereVertOffset;
                    vertIndex = hemisphereVertOffset;

                    var y = -Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * (hemisphereCapRing - 1) * R);
                    var sinR = Mathf.Sin(Mathf.PI * (hemisphereCapRing - 1) * R);

                    for (int i = 0; i < sectors; i++)
                    {
                        var x = Mathf.Sin(2 * Mathf.PI * i * S) * sinR;
                        var z = Mathf.Cos(2 * Mathf.PI * i * S) * sinR;
                        var v = new Vector3(x, y, z);

                        vertices[vertIndex + 0] = v * radius + pivotOffset;
                        normals[vertIndex + 0] = new Vector3(0.0f, 1.0f, 0.0f);

                        // make planar uv mapping for hemisphere plane
                        var uvV = new Vector2(v.x * 0.5f, v.z * .5f);
                        var uvCenter = new Vector2(0.5f, 0.5f);

                        uvs[vertIndex + 0] = uvCenter + uvV;

                        vertIndex += 1;
                    }

                    vertices[vertIndex + 0] = new Vector3(0, -hemisphereYpos, 0) + pivotOffset;
                    normals[vertIndex + 0] = new Vector3(0, 1, 0);
                    uvs[vertIndex + 0] = new Vector2(0.5f, 0.5f);

                    vertIndex += 1;

                    for (int i = 0; i < rings; i++)
                    {
                        triangles[triIndex + 2] = triVert + 0;
                        triangles[triIndex + 1] = vertIndex - 1;

                        if (i == rings - 1)
                        {
                            triangles[triIndex + 0] = hemisphereVertOffset;
                        }
                        else
                        {
                            triangles[triIndex + 0] = triVert + 1;
                        }

                        triIndex += 3;
                        triVert += 1;
                    }
                }
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
