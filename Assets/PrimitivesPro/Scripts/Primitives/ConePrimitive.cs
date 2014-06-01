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
    /// class for creating Cone primitive
    /// </summary>
    public class ConePrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for cone
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="radius0">fist radius of cone</param>
        /// <param name="radius1">second radius of cone</param>
        /// <param name="height">height of cone</param>
        /// <param name="sides">number of triangle segments in radius</param>
        /// <param name="heightSegments">number of triangle segments in height</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float radius0, float radius1, float height, int sides, int heightSegments, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            mesh.Clear();

            radius0 = Mathf.Clamp(radius0, 0, 100);
            radius1 = Mathf.Clamp(radius1, 0, 100);
            height = Mathf.Clamp(height, 0, 100);
            sides = Mathf.Clamp(sides, 3, 100);
            heightSegments = Mathf.Clamp(heightSegments, 1, 100);

            int numVertices = 0;
            int numTriangles = sides * 6 * heightSegments;
            int numVerticesCaps = 2*(sides + 1);
            int numTrianglesCaps = 2*(3*sides);

            if (normalsType == NormalsType.Face)
            {
                numVertices = sides*(4 + (heightSegments-1)*2);
            }
            else
            {
                numVertices = (sides+1)*(heightSegments + 1);
            }

            if (numVertices + numVerticesCaps > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[numVertices + numVerticesCaps];
            var normals = new Vector3[numVertices + numVerticesCaps];
            var uvs = new Vector2[numVertices + numVerticesCaps];
            var triangles = new int[numTriangles + numTrianglesCaps];

            var bottomCenter = Vector3.zero;
            var coneHeight = (new Vector3(bottomCenter.x + radius0, bottomCenter.y, bottomCenter.z) -
                             new Vector3(bottomCenter.x + radius1, bottomCenter.y + height, bottomCenter.z)).magnitude;

            var pivotOffset = Vector3.zero;
            switch (pivotPosition)
            {
                case PivotPosition.Center: pivotOffset = new Vector3(0.0f, -height / 2, 0.0f);
                    break;
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -height, 0.0f);
                    break;
            }

            if (normalsType == NormalsType.Face)
            {
                var vertIndex = 0;
                var triIndex = 0;
                var heightRatio = coneHeight/heightSegments;

                for (int i = 0; i < sides; i++)
                {
                    float angle0 = ((float)i / sides) * Mathf.PI * 2.0f;
                    var v0 = new Vector3(Mathf.Cos(angle0), 0.0f, Mathf.Sin(angle0)).normalized;

                    float angle1 = ((float)(i+1) / sides) * Mathf.PI * 2.0f;
                    var v1 = new Vector3(Mathf.Cos(angle1), 0.0f, Mathf.Sin(angle1)).normalized;

                    var currHeight = 0.0f;
                    var triVert = vertIndex;

                    var upVector0 = ((bottomCenter + new Vector3(0, height, 0)) + v0*radius1) - (bottomCenter + v0*radius0);
                    var upVector1 = ((bottomCenter + new Vector3(0, height, 0)) + v1 * radius1) - (bottomCenter + v1*radius0);

                    upVector0.Normalize();
                    upVector1.Normalize();

                    var normal = (v0 + v1).normalized;

                    for (int j = 0; j <= heightSegments; j++)
                    {
                        // generate vertices
                        vertices[vertIndex + 0] = bottomCenter + v0*radius0 + upVector0*currHeight + pivotOffset;
                        vertices[vertIndex + 1] = bottomCenter + v1 * radius0 + upVector1 * currHeight + pivotOffset;

                        normals[vertIndex + 0] = normal;
                        normals[vertIndex + 1] = normal;

                        uvs[vertIndex + 0] = new Vector2((float)i / sides, (float)j/heightSegments);
                        uvs[vertIndex + 1] = new Vector2((float)(i + 1) / sides, (float)j/heightSegments);

                        vertIndex += 2;

                        currHeight += heightRatio;
                    }

                    for (int j = 0; j < heightSegments; j++)
                    {
                        triangles[triIndex + 0] = triVert + 0;
                        triangles[triIndex + 1] = triVert + 2;
                        triangles[triIndex + 2] = triVert + 1;

                        triangles[triIndex + 3] = triVert + 2;
                        triangles[triIndex + 4] = triVert + 3;
                        triangles[triIndex + 5] = triVert + 1;

                        triIndex += 6;
                        triVert += 2;
                    }
                }
            }
            else
            {
                var vertIndex = 0;
                var triIndex = 0;
                var triVert = 0;
                var heightRatio = coneHeight / heightSegments;

                for (int i = 0; i <= sides; i++)
                {
                    float angle0 = ((float)i / sides) * Mathf.PI * 2.0f;
                    var v0 = new Vector3(Mathf.Cos(angle0), 0.0f, Mathf.Sin(angle0)).normalized;

                    var upVector = ((bottomCenter + new Vector3(0, height, 0)) + v0 * radius1) - (bottomCenter + v0 * radius0);
                    upVector.Normalize();

                    var currHeight = 0.0f;

                    for (int j = 0; j <= heightSegments; j++)
                    {
                        // generate vertices
                        vertices[vertIndex + 0] = bottomCenter + v0 * radius0 + upVector * currHeight + pivotOffset;
                        normals[vertIndex + 0] = v0;
                        uvs[vertIndex + 0] = new Vector2((float)i / sides, (float)j / heightSegments);

                        vertIndex += 1;
                        currHeight += heightRatio;
                    }
                }

                for (int i=0; i<sides; i++)
                {
                    var triVertNext = (i+1)*(heightSegments+1);

                    for (int j=0; j<heightSegments; j++)
                    {
                        triangles[triIndex + 0] = triVert + 0;
                        triangles[triIndex + 1] = triVert + 1;
                        triangles[triIndex + 2] = triVertNext + 0;

                        triangles[triIndex + 3] = triVertNext + 0;
                        triangles[triIndex + 4] = triVert + 1;
                        triangles[triIndex + 5] = triVertNext + 1;

                        triIndex += 6;
                        triVert += 1;
                        triVertNext += 1;
                    }

                    triVert += 1;
                }
            }

            // generate caps
            {
                var vertIndex = numVertices;
                var triIndex = numTriangles;
                var triVert = vertIndex;

                for (int i = 0; i <sides; i++)
                {
                    float angle0 = ((float)i / sides) * Mathf.PI * 2.0f;
                    var v0 = new Vector3(Mathf.Cos(angle0), 0.0f, Mathf.Sin(angle0)).normalized;

                    vertices[vertIndex + 0] = bottomCenter + v0 * radius0 + pivotOffset;
                    normals[vertIndex + 0] = new Vector3(0, -1, 0);
                    vertices[vertIndex + 1] = bottomCenter + v0 * radius1 + new Vector3(0, height, 0) + pivotOffset;
                    normals[vertIndex + 1] = new Vector3(0, 1, 0);

                    var uvV = new Vector2(v0.x*0.5f, v0.z*.5f);
                    var uvCenter = new Vector2(0.5f, 0.5f);
                    uvs[vertIndex + 0] = uvCenter + uvV;
                    uvs[vertIndex + 1] = uvCenter + uvV;

                    vertIndex += 2;
                }

                vertices[vertIndex + 0] = new Vector3(0, 0, 0) + pivotOffset;
                vertices[vertIndex + 1] = new Vector3(0, height, 0) + pivotOffset;
                normals[vertIndex + 0] = new Vector3(0, -1, 0);
                normals[vertIndex + 1] = new Vector3(0, 1, 0);
                uvs[vertIndex + 0] = new Vector2(0.5f, 0.5f);
                uvs[vertIndex + 1] = new Vector2(0.5f, 0.5f);

                vertIndex += 2;

                for (int i=0; i<sides; i++)
                {
                    triangles[triIndex + 0] = triVert + 0;
                    triangles[triIndex + 2] = vertIndex - 2;
                    triangles[triIndex + 3] = triVert + 1;
                    triangles[triIndex + 4] = vertIndex - 1;

                    if (i == sides - 1)
                    {
                        triangles[triIndex + 1] = numVertices;
                        triangles[triIndex + 5] = numVertices + 1;
                    }
                    else
                    {
                        triangles[triIndex + 1] = triVert + 2;
                        triangles[triIndex + 5] = triVert + 3;
                    }

                    triIndex += 6;
                    triVert += 2;
                }
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
