// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Diagnostics;
using PrimitivesPro.Utils;
using UnityEngine;

namespace PrimitivesPro.Primitives
{
    /// <summary>
    /// class for creating Box primitive
    /// </summary>
    public class BoxPrimitive : Primitive
    {
        private static bool dbg;

        /// <summary>
        /// generate mesh geometry for box
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="width">width of cube</param>
        /// <param name="height">height of cube</param>
        /// <param name="depth">depth of cube</param>
        /// <param name="widthSegments">number of triangle segments in width direction</param>
        /// <param name="heightSegments">number of triangle segments in height direction</param>
        /// <param name="depthSegments">number of triangle segments in depth direction</param>
        /// <param name="cubeMap">enable 6-sides cube map uv mapping</param>
        /// <param name="edgeOffsets">offsets on edges for creating a ramp</param>
        /// <param name="flipUV">flag to flip uv mapping</param>
        /// <param name="pivot">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float width, float height, float depth, int widthSegments, int heightSegments, int depthSegments, bool cubeMap, float[] edgeOffsets, bool flipUV, PivotPosition pivot)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            width = Mathf.Clamp(width, 0, 100);
            height = Mathf.Clamp(height, 0, 100);
            depth = Mathf.Clamp(depth, 0, 100);
            heightSegments = Mathf.Clamp(heightSegments, 1, 100);
            widthSegments = Mathf.Clamp(widthSegments, 1, 100);
            depthSegments = Mathf.Clamp(depthSegments, 1, 100);

            mesh.Clear();

            int numTriangles = widthSegments*depthSegments*6 +
                               widthSegments*heightSegments*6 +
                               depthSegments*heightSegments*6;

            int numVertices = (widthSegments + 1)*(depthSegments + 1) +
                              (widthSegments + 1)*(heightSegments + 1) +
                              (depthSegments + 1)*(heightSegments + 1);

            numTriangles *= 2;
            numVertices *= 2;

            var pivotOffset = Vector3.zero;
            switch (pivot)
            {
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -height/2, 0.0f);
                    break;
                case PivotPosition.Botttom: pivotOffset = new Vector3(0.0f, height/2, 0.0f);
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

            int vertIndex = 0;
            int triIndex = 0;

            var a0 = new Vector3(-width / 2, pivotOffset.y - height / 2, -depth / 2);
            var b0 = new Vector3(-width / 2, pivotOffset.y - height / 2, depth / 2);
            var c0 = new Vector3(width / 2, pivotOffset.y - height / 2, depth / 2);
            var d0 = new Vector3(width / 2, pivotOffset.y - height / 2, -depth / 2);

            var a1 = new Vector3(-width / 2, height / 2 + pivotOffset.y, -depth / 2);
            var b1 = new Vector3(-width / 2, height / 2 + pivotOffset.y, depth / 2);
            var c1 = new Vector3(width / 2, height / 2 + pivotOffset.y, depth / 2);
            var d1 = new Vector3(width / 2, height / 2 + pivotOffset.y, -depth / 2);

            b1.x += edgeOffsets[0];
            a1.x += edgeOffsets[0];
            b0.x += edgeOffsets[1];
            a0.x += edgeOffsets[1];

            c0.x += edgeOffsets[3];
            c1.x += edgeOffsets[2];
            d0.x += edgeOffsets[3];
            d1.x += edgeOffsets[2];

            CreatePlane(a0, b0, c0, d0, widthSegments, depthSegments, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex);
            CreatePlane(b1, a1, d1, c1, widthSegments, depthSegments, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex);

            CreatePlane(b0, b1, c1, c0, widthSegments, heightSegments, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex);
            CreatePlane(d0, d1, a1, a0, widthSegments, heightSegments, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex);

            CreatePlane(a0, a1, b1, b0, depthSegments, heightSegments, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex);
            CreatePlane(c0, c1, d1, d0, depthSegments, heightSegments, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex);

//                            if (cubeMap)
//                            {
//                                uvs[index] = GetCube6UV(i, s, uvs[index]);
//                            }

            if (flipUV)
            {
                for (var i = 0; i < uvs.Length; i++)
                {
                    uvs[i].x = 1.0f - uvs[i].x;
                }
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

        static void CreatePlane(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int segX, int segY, ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles, ref int vertIndex, ref int triIndex)
        {
            var uvFactorX = 1.0f / segX;
            var uvFactorY = 1.0f / segY;

            var vDown = d - a;
            var vUp = c - b;

            var vertOffset = vertIndex;

//            UnityEngine.Debug.DrawRay(a, (vDown)*100, Color.red);
//            UnityEngine.Debug.DrawRay(b, (vUp)*100, Color.yellow);

            for (var y = 0.0f; y < segY+1; y++)
            {
                for (var x = 0.0f; x < segX+1; x++)
                {
                    var pDown = a + vDown*y*uvFactorY;
                    var pUp = b + vUp*y*uvFactorY;

                    var v = pDown + (pUp - pDown)*x*uvFactorX;

                    vertices[vertIndex] = v;
                    uvs[vertIndex] = new Vector2(x * uvFactorX, y * uvFactorY);

                    vertIndex++;
                }
            }

            var hCount2 = segX + 1;

            for (int y = 0; y < segY; y++)
            {
                for (int x = 0; x < segX; x++)
                {
                    triangles[triIndex + 0] = vertOffset + (y * hCount2) + x;
                    triangles[triIndex + 1] = vertOffset + ((y + 1) * hCount2) + x;
                    triangles[triIndex + 2] = vertOffset + (y * hCount2) + x + 1;

                    triangles[triIndex + 3] = vertOffset + ((y + 1) * hCount2) + x;
                    triangles[triIndex + 4] = vertOffset + ((y + 1) * hCount2) + x + 1;
                    triangles[triIndex + 5] = vertOffset + (y * hCount2) + x + 1;
                    triIndex += 6;
                }
            }
        }

        /// <summary>
        /// generate uv coordinates for a texture with 6 sides of the box
        /// </summary>
        static Vector2 GetCube6UV(int sideID, int paralel, Vector2 factor)
        {
            factor.x = factor.x*0.3f;
            factor.y = factor.y*0.5f;

            switch (sideID)
            {
                case 0:
                    if (paralel == 0)
                    {
                        factor.y += 0.5f;
                        return factor;
                    }
                    else
                    {
                        factor.y += 0.5f;
                        factor.x += 2.0f / 3;
                        return factor;
                    }
                case 1:
                    if (paralel == 0)
                    {
                        factor.x += 1.0f / 3;
                        return factor;
                    }
                    else
                    {
                        factor.x += 2.0f / 3;
                        return factor;
                    }
                case 2:
                    if (paralel == 0)
                    {
                        factor.y += 0.5f;
                        factor.x += 1.0f / 3;
                        return factor;
                    }
                    else
                    {
                        return factor;
                    }
            }

            return Vector2.zero;
        }
    }
}
