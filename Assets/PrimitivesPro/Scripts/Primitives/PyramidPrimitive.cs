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
    /// class for creating Pyramid primitive
    /// </summary>
    public class PyramidPrimitive : Primitive
    {
        /// <summary>
        /// generate mesh geometry for Pyramid
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="width">width of pyramid</param>
        /// <param name="height">height of pyramid</param>
        /// <param name="depth">depth of pyramid</param>
        /// <param name="widthSegments">number of triangle segments in width direction</param>
        /// <param name="heightSegments">number of triangle segments in height direction</param>
        /// <param name="depthSegments">number of triangle segments in depth direction</param>
        /// <param name="pyramidMap">enable pyramid map uv mapping</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float width, float height, float depth, int widthSegments, int heightSegments, int depthSegments, bool pyramidMap, PivotPosition pivotPosition)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            width = Mathf.Clamp(width, 0, 100);
            height = Mathf.Clamp(height, 0, 100);
            width = Mathf.Clamp(width, 0, 100);
            widthSegments = Mathf.Clamp(widthSegments, 1, 100);
            heightSegments = Mathf.Clamp(heightSegments, 1, 100);
            depthSegments = Mathf.Clamp(depthSegments, 1, 100);

            mesh.Clear();

            int numTriangles = widthSegments*heightSegments*6*2 +
                               depthSegments*heightSegments*6*2 +
                               widthSegments*depthSegments*6;

            int numVertices = (widthSegments + 1)*(heightSegments + 1)*2 +
                              (depthSegments + 1)*(heightSegments + 1)*2 +
                              (widthSegments + 1)*(depthSegments + 1);

            if (numVertices > 60000)
            {
                UnityEngine.Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            var vertices = new Vector3[numVertices];
            var uvs = new Vector2[numVertices];
            var triangles = new int[numTriangles];

            var apex = new Vector3(0, height, 0);

            var pivotOffset = Vector3.zero;
            switch (pivotPosition)
            {
                case PivotPosition.Center: pivotOffset = new Vector3(0.0f, -height/2, 0.0f);
                    break;
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -height, 0.0f);
                    break;
            }

            int vertIndex = 0;

            float widthRatio = width/widthSegments;

            var offsetTris = widthSegments*heightSegments*6;
            var offsetVert = (widthSegments + 1)*(heightSegments + 1);

            var a0 = new Vector3(depth / 2, 0.0f, 0 * widthRatio - width / 2) + pivotOffset;
            var b0 = new Vector3(depth / 2, 0.0f, widthSegments * widthRatio - width / 2) + pivotOffset;

            var a1 = new Vector3(-depth / 2, 0.0f, 0 * widthRatio - width / 2) + pivotOffset;
            var b1 = new Vector3(-depth / 2, 0.0f, widthSegments * widthRatio - width / 2) + pivotOffset;

            for (int i = 0; i < widthSegments+1; i++)
            {
                var bottom0 = new Vector3(depth / 2, 0.0f, i * widthRatio - width / 2);
                var v0 = apex - bottom0;

                var bottom1 = new Vector3(-depth / 2, 0.0f, i * widthRatio - width / 2);
                var v1 = apex - bottom1;

                for (int j=0; j < heightSegments+1; j++)
                {
                    vertices[vertIndex + 0] = bottom0 + (float)j / heightSegments * v0 + pivotOffset;
                    vertices[offsetVert + vertIndex + 0] = bottom1 + (float)j / heightSegments * v1 + pivotOffset;

                    if (pyramidMap)
                    {
                        var baryCoordinates = MeshUtils.ComputeBarycentricCoordinates(apex + pivotOffset, a0, b0, vertices[vertIndex]);
                        uvs[vertIndex + 0] = GetPyramidUVMap(PyramidSide.side0, baryCoordinates);

                        baryCoordinates = MeshUtils.ComputeBarycentricCoordinates(apex + pivotOffset, b1, a1, vertices[offsetVert + vertIndex + 0]);
                        uvs[offsetVert + vertIndex + 0] = GetPyramidUVMap(PyramidSide.side2, baryCoordinates);
                    }
                    else
                    {
                        uvs[vertIndex + 0] = new Vector2(vertices[vertIndex + 0].z/width, (float)j/heightSegments);
                        uvs[offsetVert + vertIndex + 0] = new Vector2(vertices[offsetVert + vertIndex + 0].z / width, (float)j / heightSegments);
                    }

                    vertIndex ++;
                }
            }

            int triIndex = 0;
            int triVert = 0;

            for (int i = 0; i < widthSegments; i++)
            {
                var nextSegment = (heightSegments + 1);

                for (int j = 0; j < heightSegments; j++)
                {
                    triangles[triIndex + 0] = triVert + 0;
                    triangles[triIndex + 1] = triVert + 1;
                    triangles[triIndex + 2] = triVert + nextSegment + 0;

                    triangles[triIndex + 3] = triVert + nextSegment + 0;
                    triangles[triIndex + 4] = triVert + 1;
                    triangles[triIndex + 5] = triVert + nextSegment + 1;

                    triangles[offsetTris + triIndex + 0] = offsetVert + triVert + nextSegment + 0;
                    triangles[offsetTris + triIndex + 1] = offsetVert + triVert + 1;
                    triangles[offsetTris + triIndex + 2] = offsetVert + triVert + 0;

                    triangles[offsetTris + triIndex + 3] = offsetVert + triVert + nextSegment + 1;
                    triangles[offsetTris + triIndex + 4] = offsetVert + triVert + 1;
                    triangles[offsetTris + triIndex + 5] = offsetVert + triVert + nextSegment + 0;

                    triIndex += 6;
                    triVert += 1;
                }

                triVert += 1;
            }

            widthRatio = depth / depthSegments;

            vertIndex = (widthSegments + 1) * (heightSegments + 1) * 2;
            triIndex = widthSegments*heightSegments*6*2;
            triVert = vertIndex;

            offsetTris = depthSegments * heightSegments * 6;
            offsetVert = (depthSegments + 1) * (heightSegments + 1);

            a0 = new Vector3(0*widthRatio - depth/2, 0.0f, width/2) + pivotOffset;
            b0 = new Vector3(depthSegments * widthRatio - depth / 2, 0.0f, width / 2) + pivotOffset;
            a1 = new Vector3(0 * widthRatio - depth / 2, 0.0f, -width / 2) + pivotOffset;
            b1 = new Vector3(depthSegments * widthRatio - depth / 2, 0.0f, -width / 2) + pivotOffset;

            for (int i = 0; i < depthSegments + 1; i++)
            {
                var bottom0 = new Vector3(i * widthRatio - depth / 2, 0.0f, width/2);
                var v0 = apex - bottom0;

                var bottom1 = new Vector3(i * widthRatio - depth / 2, 0.0f, -width/2);
                var v1 = apex - bottom1;

                for (int j = 0; j < heightSegments + 1; j++)
                {
                    vertices[vertIndex + 0] = bottom0 + (float)j / heightSegments * v0 + pivotOffset;
                    vertices[offsetVert + vertIndex + 0] = bottom1 + (float)j / heightSegments * v1 + pivotOffset;

                    if (pyramidMap)
                    {
                        var baryCoordinates = MeshUtils.ComputeBarycentricCoordinates(apex + pivotOffset, b0, a0, vertices[vertIndex]);
                        uvs[vertIndex + 0] = GetPyramidUVMap(PyramidSide.side1, baryCoordinates);

                        baryCoordinates = MeshUtils.ComputeBarycentricCoordinates(apex + pivotOffset, a1, b1, vertices[offsetVert + vertIndex + 0]);
                        uvs[offsetVert + vertIndex + 0] = GetPyramidUVMap(PyramidSide.side3, baryCoordinates);
                    }
                    else
                    {
                        uvs[vertIndex + 0] = new Vector2(vertices[vertIndex + 0].x / depth, (float)j / heightSegments);
                        uvs[offsetVert + vertIndex + 0] = new Vector2(vertices[offsetVert + vertIndex + 0].x / depth, (float)j / heightSegments);
                    }

                    vertIndex++;
                }
            }

            for (int i = 0; i < depthSegments; i++)
            {
                var nextSegment = (heightSegments + 1);

                for (int j = 0; j < heightSegments; j++)
                {
                    triangles[triIndex + 0] = triVert + nextSegment + 0;
                    triangles[triIndex + 1] = triVert + 1;
                    triangles[triIndex + 2] = triVert + 0;

                    triangles[triIndex + 3] = triVert + nextSegment + 1;
                    triangles[triIndex + 4] = triVert + 1;
                    triangles[triIndex + 5] = triVert + nextSegment + 0;

                    triangles[offsetTris + triIndex + 0] = offsetVert + triVert + 0;
                    triangles[offsetTris + triIndex + 1] = offsetVert + triVert + 1;
                    triangles[offsetTris + triIndex + 2] = offsetVert + triVert + nextSegment + 0;

                    triangles[offsetTris + triIndex + 3] = offsetVert + triVert + nextSegment + 0;
                    triangles[offsetTris + triIndex + 4] = offsetVert + triVert + 1;
                    triangles[offsetTris + triIndex + 5] = offsetVert + triVert + nextSegment + 1;

                    triIndex += 6;
                    triVert += 1;
                }

                triVert += 1;
            }

            widthRatio = width / widthSegments;
            var depthRatio = depth/depthSegments;

            vertIndex = (widthSegments + 1)*(heightSegments + 1)*2 + (depthSegments + 1)*(heightSegments + 1)*2;
            triIndex = widthSegments*heightSegments*6*2 + depthSegments*heightSegments*6*2;
            triVert = vertIndex;

            for (int i = 0; i < depthSegments + 1; i++)
            {
                for (int j = 0; j < widthSegments + 1; j++)
                {
                    vertices[vertIndex + 0] = new Vector3(depth / 2 - depthRatio * i, 0.0f, width / 2 - j * widthRatio) + pivotOffset;

                    if (pyramidMap)
                    {
                        uvs[vertIndex + 0] = GetPyramidUVMap(PyramidSide.bottom, new Vector2((float)j / widthSegments, (float)i / depthSegments));
                    }
                    else
                    {
                        uvs[vertIndex + 0] = new Vector2((float) j/widthSegments, (float) i/depthSegments);
                    }

                    vertIndex++;
                }
            }

            for (int i = 0; i < depthSegments; i++)
            {
                var nextSegment = (widthSegments + 1);

                for (int j = 0; j < widthSegments; j++)
                {
                    triangles[triIndex + 0] = triVert + nextSegment + 0;
                    triangles[triIndex + 1] = triVert + 1;
                    triangles[triIndex + 2] = triVert + 0;

                    triangles[triIndex + 3] = triVert + nextSegment + 1;
                    triangles[triIndex + 4] = triVert + 1;
                    triangles[triIndex + 5] = triVert + nextSegment + 0;

                    triIndex += 6;
                    triVert += 1;
                }

                triVert += 1;
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

        enum PyramidSide
        {
            side0,
            side1,
            side2,
            side3,
            bottom,
        }

        /// <summary>
        /// get uv from pyramid texture map
        /// </summary>
        /// <param name="side">side of the pyramid</param>
        /// <param name="baryCoords">point in barycentric coordinates in respect to side of the pyramid</param>
        /// <returns>uv coordinate of a point</returns>
        static Vector2 GetPyramidUVMap(PyramidSide side, Vector3 baryCoords)
        {
            var a = new Vector2(2.0f/(2 + Mathf.Sqrt(3)), 0.5f);

            switch (side)
            {
                case PyramidSide.side0:
                    return new Vector2(baryCoords.x*a.x + baryCoords.y*a.x + baryCoords.z*1,
                                       baryCoords.x*a.y + baryCoords.y*0.0f + baryCoords.z*(a.y/2));
                case PyramidSide.side1:
                    return new Vector2(baryCoords.x * a.x + baryCoords.y * 1 + baryCoords.z * 1,
                                       baryCoords.x * a.y + baryCoords.y * a.y / 2.0f + baryCoords.z * (a.y * 3.0f / 2.0f));
                case PyramidSide.side2:
                    return new Vector2(baryCoords.x * a.x + baryCoords.y * 1 + baryCoords.z * a.x,
                                       baryCoords.x * a.y + baryCoords.y * (a.y * 3.0f/2.0f) + baryCoords.z * 1);
                case PyramidSide.side3:
                    return new Vector2(baryCoords.x*a.x + baryCoords.y*a.x + baryCoords.z*(a.x- a.y * Mathf.Sqrt(3.0f)/2.0f),
                                       baryCoords.x*a.y + baryCoords.y*1 + baryCoords.z*(a.y*3.0f/2.0f));
                case PyramidSide.bottom:
                    return new Vector2(baryCoords.x*a.x, baryCoords.y*a.y);
            }

            return Vector2.zero;
        }
    }
}
