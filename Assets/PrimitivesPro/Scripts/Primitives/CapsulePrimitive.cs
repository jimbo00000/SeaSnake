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
    /// class for creating Capsule primitive
    /// </summary>
    public class CapsulePrimitive : Primitive
    {
        /// <summary>
        /// generate geometry for capsule
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="radius">radius of capsule</param>
        /// <param name="sides">number of segments</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="heightSegments">number of segments of central cylinder</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float radius, float height, int sides, int heightSegments, bool preserveHeight, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            radius = Mathf.Clamp(radius, 0, 100);
            height = Mathf.Clamp(height, 0, 100);
            heightSegments = Mathf.Clamp(heightSegments, 1, 250);
            sides = Mathf.Clamp(sides, 4, 250);

            mesh.Clear();

            if (preserveHeight)
            {
                height = height - radius * 2;
                if (height < 0)
                {
                    height = 0;
                }
            }

            int rings = sides;
            int sectors = sides+1;

            if ((rings&1) == 0)
            {
                rings += 1;
                sectors = sides+1;
            }

            float R = 1 / (float)(rings - 1);
            float S = 1 / (float)(sectors - 1);
            var midRing = rings/2 + 1;

            int verticesNum = 0;
            var trianglesNum = (rings - 1) * (sectors - 1) * 6;
            var verticesCylinder = (sides + 1) * (heightSegments + 1);
            var trianglesCylinder = sides * 6 * heightSegments;

            if (normalsType == NormalsType.Vertex)
            {
                verticesNum = rings*sectors +sectors;
            }
            else
            {
                verticesNum = (midRing - 1)*(sectors - 1)*4 + ((rings - 1) - (midRing - 1))*(sectors - 1)*4;
                verticesCylinder = sides * (4 + (heightSegments - 1) * 2);
            }

            if (verticesNum + verticesCylinder > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[verticesNum + verticesCylinder];
            var normals = new Vector3[verticesNum + verticesCylinder];
            var uvs = new Vector2[verticesNum + verticesCylinder];
            var triangles = new int[trianglesNum + trianglesCylinder];

            var capsuleRadius = radius + height/2;

            var pivotOffset = Vector3.zero;
            switch (pivotPosition)
            {
                case PivotPosition.Botttom: pivotOffset = new Vector3(0.0f, capsuleRadius, 0.0f);
                    break;
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -capsuleRadius, 0.0f);
                    break;
            }

            var vertIndex = 0;
            var triIndex = 0;

            var vertIndexCyl = 0;
            var triIndexCyl = 0;

            // calculate capsule with vertex normals
            if (normalsType == NormalsType.Vertex)
            {
                // generate upper hemisphere
                for (int r = 0; r < midRing; r++)
                {
                    var y = Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * r * R);
                    var sinR = Mathf.Sin(Mathf.PI * r * R);

                    for (int s = 0; s < sectors; s++)
                    {
                        float x = Mathf.Sin(2*Mathf.PI*s*S)*sinR;
                        float z = Mathf.Cos(2 * Mathf.PI * s * S) * sinR;

                        vertices[vertIndex + 0] = new Vector3(x, y, z) * radius + pivotOffset;
                        normals[vertIndex + 0] = new Vector3(x, y, z);

                        vertices[vertIndex + 0].y += height / 2;

                        var uv = GetSphericalUV(vertices[vertIndex + 0] - pivotOffset);
                        //uvs[vertIndex + 0] = new Vector2(s * S, 1.0f - (r * R));
                        uvs[vertIndex + 0] = new Vector2(1.0f - s * S, uv.y);

                        if (r < midRing-1 && s < sectors - 1)
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

                // generate central cylinder
                if (height > 0)
                {
                    vertIndexCyl = verticesNum;
                    triIndexCyl = trianglesNum;
                    var triVertCyl = verticesNum;
                    var heightRatio = height/heightSegments;
                    var bottomCenter = new Vector3(0.0f, -height/2, 0.0f);

                    var sinR = Mathf.Sin(Mathf.PI * (midRing-1) * R);

                    for (int s = 0; s <= sides; s++)
                    {
                        float x = Mathf.Sin(2 * Mathf.PI * s * S) * sinR;
                        float z = Mathf.Cos(2 * Mathf.PI * s * S) * sinR;

                        var v0 = new Vector3(x, 0.0f, z);
//                        var texV = (midRing - 1) * R;

                        var currHeight = 0.0f;

                        for (int j = 0; j <= heightSegments; j++)
                        {
                            // generate vertices
                            vertices[vertIndexCyl + 0] = bottomCenter + v0 * radius + new Vector3(0.0f, currHeight, 0.0f) + pivotOffset;
                            normals[vertIndexCyl + 0] = v0;
//                            uvs[vertIndexCyl + 0] = new Vector2(s * S, texV/2);
//
//                            texV += 1.0f/heightSegments;

                            var uv = GetSphericalUV(vertices[vertIndexCyl + 0] - pivotOffset);
//                        uvs[vertIndex + 0] = new Vector2(s * S, 1.0f - (r * R));
                            uvs[vertIndexCyl + 0] = new Vector2(1.0f - s * S, uv.y);

                            vertIndexCyl += 1;
                            currHeight += heightRatio;
                        }
                    }

                    for (int i = 0; i < sides; i++)
                    {
                        var triVertNext = verticesNum + (i + 1)*(heightSegments + 1);

                        for (int j = 0; j < heightSegments; j++)
                        {
                            triangles[triIndexCyl + 0] = triVertNext + 0;
                            triangles[triIndexCyl + 1] = triVertNext + 1;
                            triangles[triIndexCyl + 2] = triVertCyl + 0;

                            triangles[triIndexCyl + 3] = triVertNext + 1;
                            triangles[triIndexCyl + 4] = triVertCyl + 1;
                            triangles[triIndexCyl + 5] = triVertCyl + 0;

                            triIndexCyl += 6;
                            triVertCyl += 1;
                            triVertNext += 1;
                        }

                        triVertCyl += 1;
                    }
                }

                // generate bottom hemisphere
                for (int r = midRing-1; r < rings; r++)
                {
                    var y = Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * r * R);
                    var sinR = Mathf.Sin(Mathf.PI * r * R);

                    for (int s = 0; s < sectors; s++)
                    {
                        float x = Mathf.Sin(2 * Mathf.PI * s * S) * sinR;
                        float z = Mathf.Cos(2 * Mathf.PI * s * S) * sinR;

                        vertices[vertIndex + 0] = new Vector3(x, y, z) * radius;
                        normals[vertIndex + 0] = new Vector3(x, y, z);

                        vertices[vertIndex] += pivotOffset;

                        vertices[vertIndex + 0].y -= height/2;

                        var uv = GetSphericalUV(vertices[vertIndex + 0] - pivotOffset);
//                        uvs[vertIndex + 0] = new Vector2(s * S, 1.0f - (r * R));
                        uvs[vertIndex + 0] = new Vector2(1.0f - s * S, uv.y);

                        if (r < rings-1 && s < sectors - 1)
                        {
                            triangles[triIndex + 0] = ((r+1) + 1) * sectors + (s);
                            triangles[triIndex + 1] = (r+1) * sectors + (s + 1);
                            triangles[triIndex + 2] = (r+1) * sectors + s;

                            triangles[triIndex + 3] = ((r+1) + 1) * sectors + (s + 1);
                            triangles[triIndex + 4] = (r+1) * sectors + (s + 1);
                            triangles[triIndex + 5] = ((r+1) + 1) * sectors + (s);

                            triIndex += 6;
                        }

                        vertIndex += 1;
                    }
                }
            }
            else
            {
                // generate upper hemisphere
                for (int r = 0; r < midRing-1; r++)
                {
                    var y = Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * r * R);
                    var yNext = Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * (r+1) * R);

                    var sinR = Mathf.Sin(Mathf.PI * r * R);
                    var sinR1 = Mathf.Sin(Mathf.PI * (r + 1) * R);

                    for (int s = 0; s < sectors-1; s++)
                    {
                        var sinS = Mathf.Sin(2*Mathf.PI*s*S);
                        var sinS1 = Mathf.Sin(2*Mathf.PI*(s + 1)*S);
                        var cosS = Mathf.Cos(2*Mathf.PI*(s)*S);
                        var cosS1 = Mathf.Cos(2*Mathf.PI*(s + 1)*S);

                        var x = sinS*sinR;
                        var xNext = sinS1*sinR;
                        var xNextR = sinS*sinR1;
                        var xNextRS = sinS1*sinR1;
                        var z = Mathf.Cos(2*Mathf.PI*s*S)*sinR;
                        var zNext = cosS1*sinR;
                        var zNextR = cosS*sinR1;
                        var zNextRS = cosS1*sinR1;

                        // r, s
                        vertices[vertIndex + 0] = new Vector3(x, y, z) * radius + pivotOffset;
//                        uvs[vertIndex + 0] = new Vector2(s * S, 1.0f - r * R);
                        vertices[vertIndex + 0].y += height/2;

                        var uv = GetSphericalUV(vertices[vertIndex + 0] - pivotOffset);
                        uvs[vertIndex + 0] = new Vector2(1.0f - s * S, uv.y);

                        // r+1, s
                        vertices[vertIndex + 1] = new Vector3(xNextR, yNext, zNextR) * radius + pivotOffset;
//                        uvs[vertIndex + 1] = new Vector2(s * S, 1.0f - (r+1) * R);
                        vertices[vertIndex + 1].y += height / 2;
                        uv = GetSphericalUV(vertices[vertIndex + 1] - pivotOffset);
                        uvs[vertIndex + 1] = new Vector2(1.0f - s * S, uv.y);

                        // r, s+1
                        vertices[vertIndex + 2] = new Vector3(xNext, y, zNext) * radius + pivotOffset;
//                        uvs[vertIndex + 2] = new Vector2((s+1) * S, 1.0f - (r) * R);
                        vertices[vertIndex + 2].y += height / 2;
                        uv = GetSphericalUV(vertices[vertIndex + 2] - pivotOffset);
                        uvs[vertIndex + 2] = new Vector2(1.0f - (s+1) * S, uv.y);

                        // r+1, s+1
                        vertices[vertIndex + 3] = new Vector3(xNextRS, yNext, zNextRS) * radius + pivotOffset;
//                        uvs[vertIndex + 3] = new Vector2((s+1) * S, 1.0f - (r+1) * R);
                        vertices[vertIndex + 3].y += height / 2;
                        uv = GetSphericalUV(vertices[vertIndex + 3] - pivotOffset);
                        uvs[vertIndex + 3] = new Vector2(1.0f - (s+1) * S, uv.y);

                        triangles[triIndex + 0] = vertIndex + 1;
                        triangles[triIndex + 1] = vertIndex + 2;
                        triangles[triIndex + 2] = vertIndex + 0;

                        triangles[triIndex + 3] = vertIndex + 3;
                        triangles[triIndex + 4] = vertIndex + 2;
                        triangles[triIndex + 5] = vertIndex + 1;

                        triIndex += 6;

                        vertIndex += 4;
                    }
                }

                // generate central cylinder
                if (height > 0)
                {
                    vertIndexCyl = verticesNum;
                    triIndexCyl = trianglesNum;
                    var heightRatio = height / heightSegments;
                    var bottomCenter = new Vector3(0.0f, -height / 2, 0.0f);

                    var sinR = Mathf.Sin(Mathf.PI * (midRing - 1) * R);

                    for (int s = 0; s < sides; s++)
                    {
                        var v0 = new Vector3(Mathf.Sin(2 * Mathf.PI * s * S) * sinR, 0.0f, Mathf.Cos(2 * Mathf.PI * s * S) * sinR);
                        var v1 = new Vector3(Mathf.Sin(2 * Mathf.PI * (s+1) * S) * sinR, 0.0f, Mathf.Cos(2 * Mathf.PI * (s+1) * S) * sinR);

//                        var texV = (midRing - 1) * R;
                        var currHeight = 0.0f;

                        var triVertCyl = vertIndexCyl;

                        var normal = (v0 + v1).normalized;

                        for (int j = 0; j <= heightSegments; j++)
                        {
                            // generate vertices
                            vertices[vertIndexCyl + 0] = bottomCenter + v0 * radius + new Vector3(0.0f, currHeight, 0.0f) + pivotOffset;
                            vertices[vertIndexCyl + 1] = bottomCenter + v1 * radius + new Vector3(0.0f, currHeight, 0.0f) + pivotOffset;

                            normals[vertIndexCyl + 0] = normal;
                            normals[vertIndexCyl + 1] = normal;

//                            uvs[vertIndexCyl + 0] = new Vector2(s * S, texV/2);
//                            uvs[vertIndexCyl + 1] = new Vector2((s + 1) * S, texV/2);
//                            texV += 1.0f/heightSegments;

                            var uv = GetSphericalUV(vertices[vertIndexCyl + 0] - pivotOffset);
                            uvs[vertIndexCyl + 0] = new Vector2(1.0f - s * S, uv.y);
                            uvs[vertIndexCyl + 1] = new Vector2(1.0f - (s+1) * S, uv.y);

                            vertIndexCyl += 2;

                            currHeight += heightRatio;
                        }

                        for (int j = 0; j < heightSegments; j++)
                        {
                            triangles[triIndexCyl + 0] = triVertCyl + 0;
                            triangles[triIndexCyl + 1] = triVertCyl + 1;
                            triangles[triIndexCyl + 2] = triVertCyl + 3;

                            triangles[triIndexCyl + 3] = triVertCyl + 3;
                            triangles[triIndexCyl + 4] = triVertCyl + 2;
                            triangles[triIndexCyl + 5] = triVertCyl + 0;

                            triIndexCyl += 6;
                            triVertCyl += 2;
                        }
                    }
                }

                // generate bottom hemisphere
                for (int r = midRing-1; r < rings - 1; r++)
                {
                    var y = Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * r * R);
                    var yNext = Mathf.Cos(-Mathf.PI * 2.0f + Mathf.PI * (r + 1) * R);

                    var sinR = Mathf.Sin(Mathf.PI * r * R);
                    var sinR1 = Mathf.Sin(Mathf.PI * (r + 1) * R);

                    for (int s = 0; s < sectors - 1; s++)
                    {
                        var sinS = Mathf.Sin(2 * Mathf.PI * s * S);
                        var sinS1 = Mathf.Sin(2 * Mathf.PI * (s + 1) * S);
                        var cosS = Mathf.Cos(2 * Mathf.PI * (s) * S);
                        var cosS1 = Mathf.Cos(2 * Mathf.PI * (s + 1) * S);

                        var x = sinS * sinR;
                        var xNext = sinS1 * sinR;
                        var xNextR = sinS * sinR1;
                        var xNextRS = sinS1 * sinR1;
                        var z = Mathf.Cos(2 * Mathf.PI * s * S) * sinR;
                        var zNext = cosS1 * sinR;
                        var zNextR = cosS * sinR1;
                        var zNextRS = cosS1 * sinR1;

                        // r, s
                        vertices[vertIndex + 0] = new Vector3(x, y, z) * radius + pivotOffset;
//                        uvs[vertIndex + 0] = new Vector2(s * S, 1.0f - r * R);
                        vertices[vertIndex + 0].y -= height / 2;
                        var uv = GetSphericalUV(vertices[vertIndex + 0] - pivotOffset);
                        uvs[vertIndex + 0] = new Vector2(1.0f - s * S, uv.y);

                        // r+1, s
                        vertices[vertIndex + 1] = new Vector3(xNextR, yNext, zNextR) * radius + pivotOffset;
//                        uvs[vertIndex + 1] = new Vector2(s * S, 1.0f - (r + 1) * R);
                        vertices[vertIndex + 1].y -= height / 2;
                        uv = GetSphericalUV(vertices[vertIndex + 1] - pivotOffset);
                        uvs[vertIndex + 1] = new Vector2(1.0f - s * S, uv.y);

                        // r, s+1
                        vertices[vertIndex + 2] = new Vector3(xNext, y, zNext) * radius + pivotOffset;
//                        uvs[vertIndex + 2] = new Vector2((s + 1) * S, 1.0f - (r) * R);
                        vertices[vertIndex + 2].y -= height / 2;
                        uv = GetSphericalUV(vertices[vertIndex + 2] - pivotOffset);
                        uvs[vertIndex + 2] = new Vector2(1.0f - (s+1) * S, uv.y);

                        // r+1, s+1
                        vertices[vertIndex + 3] = new Vector3(xNextRS, yNext, zNextRS) * radius + pivotOffset;
//                        uvs[vertIndex + 3] = new Vector2((s + 1) * S, 1.0f - (r + 1) * R);
                        vertices[vertIndex + 3].y -= height / 2;
                        uv = GetSphericalUV(vertices[vertIndex + 3] - pivotOffset);
                        uvs[vertIndex + 3] = new Vector2(1.0f - (s + 1) * S, uv.y);

                        triangles[triIndex + 0] = vertIndex + 1;
                        triangles[triIndex + 1] = vertIndex + 2;
                        triangles[triIndex + 2] = vertIndex + 0;

                        triangles[triIndex + 3] = vertIndex + 3;
                        triangles[triIndex + 4] = vertIndex + 2;
                        triangles[triIndex + 5] = vertIndex + 1;

                        triIndex += 6;

                        vertIndex += 4;
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            // generate normals by unity in face case
            if (normalsType == NormalsType.Face)
            {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
            MeshUtils.CalculateTangents(mesh);
            mesh.Optimize();

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// return uv spherical mapping for point on sphere
        /// </summary>
        /// <param name="pnt"></param>
        /// <returns></returns>
        static Vector2 GetSphericalUV(Vector3 pnt)
        {
            var v0 = pnt.normalized;

            return new Vector2((0.5f + Mathf.Atan2(v0.z, v0.x) / (Mathf.PI * 2.0f)),
                                1.0f - (0.5f - Mathf.Asin(v0.y) / Mathf.PI));
        }
    }
}
