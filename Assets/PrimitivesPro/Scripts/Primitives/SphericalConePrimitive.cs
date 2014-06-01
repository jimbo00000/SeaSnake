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
    public class SphericalConePrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for Spherical cone
        /// 
        /// references: 
        /// http://mathworld.wolfram.com/SphericalCone.html
        /// http://en.wikipedia.org/wiki/Spherical_sector
        /// 
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="radius">radius of sphere</param>
        /// <param name="segments">number of segments</param>
        /// <param name="coneAngle">angle of conus in DEG, 360 ... complete sphere, 180 ... half-sphere</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float radius, int segments, float coneAngle, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            radius = Mathf.Clamp(radius, 0, 100);
            segments = Mathf.Clamp(segments, 4, 100);
            coneAngle = Mathf.Clamp(coneAngle, 0, 360);
            var hemisphere = 1.0f - (coneAngle/360.0f);

            mesh.Clear();

            int rings = segments - 1;
            int sectors = segments;

            var hemisphereCapY = -1 + hemisphere * 2;
            int hemisphereCapRing = rings;
//            var hemisphereYpos = -radius;

            float R = 1 / (float)(rings - 1);
            float S = 1 / (float)(sectors - 1);

            // calculate hemisphere parameters
//            var lastY = 0.0f;
            for (int r = 0; r < rings; r++)
            {
                var y = Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * r * R);

                if (y < hemisphereCapY)
                {
                    hemisphereCapRing = r;
//                    hemisphereYpos = lastY * radius;
                    break;
                }

//                lastY = y;
            }

            int verticesNum = 0;
            var trianglesNum = (hemisphereCapRing) * (sectors) * 6;
            var verticesHemisphereNum = segments + 1;
            var trianglesHemisphereNum = segments * 3;

            if (hemisphereCapRing == rings)
            {
                trianglesNum -= sectors * 3;
            }

            if (normalsType == NormalsType.Vertex)
            {
                verticesNum = (hemisphereCapRing + 1) * (sectors + 1);
            }
            else
            {
                verticesNum = trianglesNum;
            }

            if (hemisphereCapRing == rings)
            {
                verticesNum -= sectors + 1;
            }

            var pivotOffset = Vector3.zero;
            switch (pivotPosition)
            {
                case PivotPosition.Botttom: pivotOffset = new Vector3(0.0f, radius, 0.0f);
                    break;
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -radius, 0.0f);
                    break;
                case PivotPosition.Center: pivotOffset = Vector3.zero;
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
                var sinR = Mathf.Sin(Mathf.PI * r * R);

                for (int s = 0; s < sectors; s++)
                {
                    float x = Mathf.Sin(2 * Mathf.PI * s * S) * sinR;
                    float z = Mathf.Cos(2 * Mathf.PI * s * S) * sinR;

                    vertices[vertIndex + 0] = new Vector3(x, y, z) * radius + pivotOffset;
                    normals[vertIndex + 0] = new Vector3(x, y, z);
                    uvs[vertIndex + 0] = new Vector2(1.0f - (s * S), (r * R));

                    if (r < hemisphereCapRing - 1 && s < sectors - 1)
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

                var zeroIndex = 0;

                for (int i = 0; i < sectors; i++)
                {
                    var x = Mathf.Sin(2 * Mathf.PI * i * S) * sinR;
                    var z = Mathf.Cos(2 * Mathf.PI * i * S) * sinR;
                    var v = new Vector3(x, y, z);

                    vertices[vertIndex + 0] = v * radius + pivotOffset;
//                    normals[vertIndex + 0] = new Vector3(0.0f, 1.0f, 0.0f);

                    if (i > 0)
                    {
                        normals[vertIndex + 0] = -MeshUtils.ComputePolygonNormal(pivotOffset, vertices[vertIndex], vertices[vertIndex - 1]);

                        if (i == sectors - 1)
                        {
                            normals[zeroIndex] = MeshUtils.ComputePolygonNormal(pivotOffset, vertices[zeroIndex], vertices[zeroIndex+1]);
                        }
                    }
                    else
                    {
                        zeroIndex = vertIndex;
                    }

                    // make planar uv mapping for hemisphere plane
                    var uvV = new Vector2(v.x * 0.5f, v.z * .5f);
                    var uvCenter = new Vector2(0.5f, 0.5f);

                    uvs[vertIndex + 0] = uvCenter + uvV;

                    vertIndex += 1;
                }

                vertices[vertIndex + 0] = pivotOffset;
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
