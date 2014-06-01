// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Diagnostics;
using UnityEngine;

namespace PrimitivesPro.Primitives
{
    /// <summary>
    /// class for creating Box primitive
    /// </summary>
    public class ArcPrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for box
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="width">width of cube</param>
        /// <param name="height1">height1 of cube</param>
        /// <param name="height2">height2 of cube</param>
        /// <param name="depth">depth of cube</param>
        /// <param name="arcSegments">depth of the </param>
        /// <param name="controlPoint">control point of arc curve</param>
        /// <param name="pivot">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float width, float height1, float height2, float depth, int arcSegments, Vector3 controlPoint, PivotPosition pivot)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            width = Mathf.Clamp(width, 0, 100);
            height1 = Mathf.Clamp(height1, 0, 100);
            height2 = Mathf.Clamp(height2, 0, 100);
            depth = Mathf.Clamp(depth, 0, 100);
            arcSegments = Mathf.Clamp(arcSegments, 1, 100);

            var height = Mathf.Max(height1, height2);

            mesh.Clear();

            int numTriangles = 36 + (arcSegments * 6) + (arcSegments * 3) * 2 + 6;
            int numVertices = 24 + (arcSegments * 6) * 2 + 2 + 2;

            var pivotOffset = Vector3.zero;
            switch (pivot)
            {
                case PivotPosition.Center: pivotOffset = new Vector3(0.0f, -height/2, 0.0f);
                    break;
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -height, 0.0f);
                    break;
            }

            if (numVertices > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[numVertices];
            var uvs = new Vector2[numVertices];
            var triangles = new int[numTriangles];

            var vertIdx = 0;
            var triIdx = 0;

            // make bottom points
            var p0 = vertices[0] = new Vector3(-width/2, 0.0f, -depth/2);
            var p1 = vertices[1] = new Vector3(width / 2, 0.0f, -depth / 2);
            var p2 = vertices[2] = new Vector3(width / 2, 0.0f, depth / 2);
            var p3 = vertices[3] = new Vector3(-width / 2, 0.0f, depth / 2);

            uvs[0] = new Vector2(0, 1);
            uvs[1] = new Vector2(1, 1);
            uvs[2] = new Vector2(1, 0);
            uvs[3] = new Vector2(0, 0);

            var p4 = p0;
            var p5 = p1;
            var p6 = p3;
            var p7 = p2;

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            vertIdx = 4;
            triIdx = 6;

            // make left side
            if (height1 > 0)
            {
                vertices[vertIdx + 0] = p0;
                vertices[vertIdx + 1] = p3;
                p4 = vertices[vertIdx + 2] = new Vector3(-width / 2, height1, -depth / 2);
                p6 = vertices[vertIdx + 3] = new Vector3(-width / 2, height1, depth / 2);

                uvs[vertIdx + 3] = new Vector2(0, height1/height);
                uvs[vertIdx + 2] = new Vector2(1, height1 / height);
                uvs[vertIdx + 1] = new Vector2(0, 0);
                uvs[vertIdx + 0] = new Vector2(1, 0);

                triangles[triIdx + 0] = vertIdx + 3;
                triangles[triIdx + 1] = vertIdx + 2;
                triangles[triIdx + 2] = vertIdx + 0;

                triangles[triIdx + 3] = vertIdx + 1;
                triangles[triIdx + 4] = vertIdx + 3;
                triangles[triIdx + 5] = vertIdx + 0;

                vertIdx += 4;
                triIdx += 6;
            }

            // make right side
            if (height2 > 0)
            {
                vertices[vertIdx + 0] = p1;
                vertices[vertIdx + 1] = p2;
                p5 = vertices[vertIdx + 2] = new Vector3(width / 2, height2, -depth / 2);
                p7 = vertices[vertIdx + 3] = new Vector3(width / 2, height2, depth / 2);

                uvs[vertIdx + 3] = new Vector2(1, height2 / height);
                uvs[vertIdx + 2] = new Vector2(0, height2 / height);
                uvs[vertIdx + 1] = new Vector2(1, 0);
                uvs[vertIdx + 0] = new Vector2(0, 0);

                triangles[triIdx + 0] = vertIdx + 2;
                triangles[triIdx + 1] = vertIdx + 1;
                triangles[triIdx + 2] = vertIdx + 0;

                triangles[triIdx + 3] = vertIdx + 1;
                triangles[triIdx + 4] = vertIdx + 2;
                triangles[triIdx + 5] = vertIdx + 3;

                vertIdx += 4;
                triIdx += 6;
            }

            vertices[vertIdx++] = p0;
            vertices[vertIdx++] = p1;
            vertices[vertIdx++] = p2;
            vertices[vertIdx++] = p3;

            var tri0 = vertIdx - 4;
            var tri1 = vertIdx - 3;
            var tri2 = vertIdx - 2;
            var tri3 = vertIdx - 1;

            uvs[tri0] = new Vector2(0.0f, 0.0f);
            uvs[tri1] = new Vector2(1.0f, 0.0f);
            uvs[tri2] = new Vector2(0.0f, 0.0f);
            uvs[tri3] = new Vector2(1.0f, 0.0f);

            var frontSideVertOffset = vertIdx + arcSegments * 2;
            var frontSideTriOffset = triIdx + arcSegments*6;

            var ctrl0 = 0;
            var ctrl1 = 3;
            var halfT = 0.5f;

            // make arc side
            for (int i = 0; i <= arcSegments; i++)
            {
                var t = (float) i/arcSegments;

                var v0 = MeshUtils.BezierQuadratic(p4, p5, new Vector3(controlPoint.x, controlPoint.y, -depth / 2), t);
                var v1 = MeshUtils.BezierQuadratic(p6, p7, new Vector3(controlPoint.x, controlPoint.y, depth / 2), t);

                vertices[vertIdx + 0] = v0;
                vertices[vertIdx + 1] = v1;

                uvs[vertIdx + 0] = new Vector2(t, 0.0f);
                uvs[vertIdx + 1] = new Vector2(t, 1.0f);

                if (i < arcSegments)
                {
                    // make top triangle quad
                    triangles[triIdx + 0] = vertIdx + 0;
                    triangles[triIdx + 1] = vertIdx + 1;
                    triangles[triIdx + 2] = vertIdx + 3;

                    triangles[triIdx + 3] = vertIdx + 3;
                    triangles[triIdx + 4] = vertIdx + 2;
                    triangles[triIdx + 5] = vertIdx + 0;

                    // make front side and back triangle
                    triangles[frontSideTriOffset + triIdx + 0] = frontSideVertOffset + vertIdx + 0;
                    triangles[frontSideTriOffset + triIdx + 1] = frontSideVertOffset + vertIdx + 2;

                    triangles[frontSideTriOffset + triIdx + 5] = frontSideVertOffset + vertIdx + 1;
                    triangles[frontSideTriOffset + triIdx + 4] = frontSideVertOffset + vertIdx + 3;

                    if (height1 > Mathf.Epsilon && height2 > Mathf.Epsilon)
                    {
                        if (t < 0.5f)
                        {
                            triangles[frontSideTriOffset + triIdx + 2] = tri0;
                            triangles[frontSideTriOffset + triIdx + 3] = tri3;
                        }
                        else
                        {
                            triangles[frontSideTriOffset + triIdx + 2] = tri1;
                            triangles[frontSideTriOffset + triIdx + 3] = tri2;
                        }
                    }
                    else
                    {
                        if (height1 > Mathf.Epsilon)
                        {
                            triangles[frontSideTriOffset + triIdx + 2] = tri0;
                            triangles[frontSideTriOffset + triIdx + 3] = tri3;
                        }
                        if (height2 > Mathf.Epsilon)
                        {
                            triangles[frontSideTriOffset + triIdx + 2] = tri1;
                            triangles[frontSideTriOffset + triIdx + 3] = tri2;
                        }
                    }

                    triIdx += 6;
                }

                // make front side
                vertices[frontSideVertOffset + vertIdx + 0] = v0;
                vertices[frontSideVertOffset + vertIdx + 1] = v1;

                uvs[frontSideVertOffset + vertIdx + 0] = new Vector2(t, v0.y/height);
                uvs[frontSideVertOffset + vertIdx + 1] = new Vector2(1-t, v0.y/height);

                if (t < 0.5f && t + (float)(i + 1)/(arcSegments) >= 0.5f)
                {
                    ctrl0 = vertIdx + 2;
                    ctrl1 = vertIdx + 3;
                    halfT = t;

                    if (arcSegments % 2 == 0)
                    {
                        halfT = 0.5f;
                    }
                }

                vertIdx += 2;
            }

            if (height1 > 0 && height2 > 0)
            {
                vertices[frontSideVertOffset + vertIdx + 0] = vertices[ctrl0];
                vertices[frontSideVertOffset + vertIdx + 1] = vertices[ctrl1];

                uvs[frontSideVertOffset + vertIdx + 0] = new Vector2(1.0f - halfT, vertices[ctrl0].y / height);
                uvs[frontSideVertOffset + vertIdx + 1] = new Vector2(halfT, vertices[ctrl1].y / height);

                // make closing triangle front
                triangles[frontSideTriOffset + triIdx + 2] = tri0;
                triangles[frontSideTriOffset + triIdx + 1] = tri1;
                triangles[frontSideTriOffset + triIdx + 0] = frontSideVertOffset + vertIdx + 0;

                // make closing triangle back
                triangles[frontSideTriOffset + triIdx + 3] = tri3;
                triangles[frontSideTriOffset + triIdx + 4] = tri2;
                triangles[frontSideTriOffset + triIdx + 5] = frontSideVertOffset + vertIdx + 1;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += pivotOffset;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            MeshUtils.CalculateTangents(mesh);
            mesh.Optimize();
            mesh.RecalculateBounds();

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
    }
}
