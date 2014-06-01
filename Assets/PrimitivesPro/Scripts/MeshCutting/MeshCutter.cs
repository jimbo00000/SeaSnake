// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

//#define PROFILING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Poly2Tri;
using PrimitivesPro.Primitives;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PrimitivesPro.MeshCutting
{
    public class MeshCutter
    {
        private List<int>[] triangles;
        private List<Vector3>[] vertices;
        private List<Vector3>[] normals;
        private List<Vector2>[] uvs;

        private List<int> cutTris;
        private int[] triCache;

        private Vector3[] centroid;
        private int[] triCounter;

        private Contour contour;
        private Dictionary<long, int>[] cutVertCache;
        private Dictionary<int, int>[] cornerVertCache;
        private int contourBufferSize;

        /// <summary>
        /// cut object by plane
        /// </summary>
        /// <param name="obj">game object</param>
        /// <param name="plane">cutting plane</param>
        /// <param name="triangulateHoles">triangulate holes</param>
        /// <param name="deleteOriginal">delete original object</param>
        /// <param name="cut0">first new game object after cut</param>
        /// <param name="cut1">second new game object after cut</param>
        /// <param name="intersectionData">contours data</param>
        /// <returns>cutting time</returns>
        public float Cut(GameObject obj, Utils.Plane plane, bool triangulateHoles, bool deleteOriginal, out GameObject cut0, out GameObject cut1, out ContourData intersectionData)
        {
            var meshFilter = obj.GetComponent<MeshFilter>();

            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                cut0 = null;
                cut1 = null;
                intersectionData = null;
                MeshUtils.Log("Cutting object has no mesh filter!");
                return 0.0f;
            }

            // get mesh of the primitive
            var meshToCut = obj.GetComponent<MeshFilter>().sharedMesh;
            Material material = null;

            var meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                material = meshRenderer.sharedMaterial;
            }

            Mesh mesh0 = null, mesh1 = null;

            cut0 = null;
            cut1 = null;
            intersectionData = null;

            bool useCentroid = obj.transform.localScale == Vector3.one;

            Vector3 centroid0 = Vector3.zero;
            Vector3 centroid1 = Vector3.zero;

            var ms = 0.0f;

            // create 2 new objects
            if (useCentroid)
            {
                ms = Cut(meshToCut, obj.transform, plane, triangulateHoles, out mesh0, out mesh1, out centroid0, out centroid1, out intersectionData);
            }
            else
            {
                ms = Cut(meshToCut, obj.transform, plane, triangulateHoles, out mesh0, out mesh1, out intersectionData);
            }

            if (mesh0 != null)
            {
                var obj0 = new GameObject(obj.name + "_cut0");
                var meshFilter0 = obj0.AddComponent<MeshFilter>();

                if (meshFilter0 != null)
                {
                    meshFilter0.sharedMesh = mesh0;
                }

                var renderer0 = obj0.AddComponent<MeshRenderer>();

                if (renderer0 != null && material != null)
                {
                    renderer0.sharedMaterials = new Material[2] { new Material(material), new Material(material) };
                }

                obj0.transform.position = obj.transform.position;
                obj0.transform.rotation = obj.transform.rotation;
                obj0.transform.localScale = obj.transform.localScale;

                if (useCentroid)
                {
                    obj0.transform.Translate(centroid0);
                }

                cut0 = obj0;
            }

            if (mesh1 != null)
            {
                var obj1 = new GameObject(obj.name + "_cut1");
                var meshFilter1 = obj1.AddComponent<MeshFilter>();

                if (meshFilter1 != null)
                {
                    meshFilter1.sharedMesh = mesh1;
                }

                var renderer1 = obj1.AddComponent<MeshRenderer>();

                if (renderer1 != null && material != null)
                {
                    renderer1.sharedMaterials = new Material[2] { new Material(material), new Material(material) };
                }

                obj1.transform.position = obj.transform.position;
                obj1.transform.rotation = obj.transform.rotation;
                obj1.transform.localScale = obj.transform.localScale;

                if (useCentroid)
                {
                    obj1.transform.Translate(centroid1);
                }

                cut1 = obj1;

                if (deleteOriginal)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(obj);
#else
                    Object.Destroy(obj);
#endif
                }
            }

            return ms;
        }

        /// <summary>
        /// cut object by plane
        /// </summary>
        /// <param name="obj">game object</param>
        /// <param name="plane">cutting plane</param>
        /// <returns>contour data</returns>
        public ContourData GetIntersectionData(GameObject obj, Utils.Plane plane)
        {
            var meshFilter = obj.GetComponent<MeshFilter>();

            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                MeshUtils.Log("Cutting object has no mesh filter!");
                return null;
            }

            // get mesh of the primitive
            var meshToCut = obj.GetComponent<MeshFilter>().sharedMesh;

            Mesh mesh0 = null, mesh1 = null;
            Vector3 c0, c1;
            ContourData conourList;
            Cut(meshToCut, obj.transform, plane, true, false, true, true, out mesh0, out mesh1, out c0, out c1, out conourList);

            return conourList;
        }

        /// <summary>
        /// cut mesh by plane
        /// </summary>
        /// <param name="mesh">mesh to cut</param>
        /// <param name="meshTransform">transformation of the mesh</param>
        /// <param name="plane">cutting plane</param>
        /// <param name="mesh0">first part of the new mesh</param>
        /// <param name="mesh1">second part of the new mesh</param>
        /// <param name="triangulateHoles">flag for triangulation of holes</param>
        /// <returns>processing time</returns>
        public float Cut(Mesh mesh, Transform meshTransform, Utils.Plane plane, bool triangulateHoles, out Mesh mesh0, out Mesh mesh1)
        {
            Vector3 c0, c1;
            ContourData conourList;
            return Cut(mesh, meshTransform, plane, triangulateHoles, false, true, false, out mesh0, out mesh1, out c0, out c1, out conourList);
        }

        /// <summary>
        /// cut mesh by plane
        /// </summary>
        /// <param name="mesh">mesh to cut</param>
        /// <param name="meshTransform">transformation of the mesh</param>
        /// <param name="plane">cutting plane</param>
        /// <param name="mesh0">first part of the new mesh</param>
        /// <param name="mesh1">second part of the new mesh</param>
        /// <param name="triangulateHoles">flag for triangulation of holes</param>
        /// <param name="centroid0">center position of the new mesh0 - apply to transform.position to stay on the same position</param>
        /// <param name="centroid1">center position of the new mesh1 - apply to transform.position to stay on the same position</param>
        /// <param name="intersectionData">contour data</param>
        /// <returns>processing time</returns>
        public float Cut(Mesh mesh, Transform meshTransform, Utils.Plane plane, bool triangulateHoles, out Mesh mesh0, out Mesh mesh1, out Vector3 centroid0, out Vector3 centroid1, out ContourData intersectionData)
        {
            return Cut(mesh, meshTransform, plane, triangulateHoles, true, true, false, out mesh0, out mesh1, out centroid0, out centroid1, out intersectionData);
        }

        /// <summary>
        /// cut mesh by plane and output list of contour vertices
        /// </summary>
        /// <param name="mesh">mesh to cut</param>
        /// <param name="meshTransform">transformation of the mesh</param>
        /// <param name="plane">cutting plane</param>
        /// <param name="mesh0">first part of the new mesh</param>
        /// <param name="mesh1">second part of the new mesh</param>
        /// <param name="triangulateHoles">flag for triangulation of holes</param>
        /// <param name="intersectionData">list of contours</param>
        /// <returns>processing time</returns>
        public float Cut(Mesh mesh, Transform meshTransform, Utils.Plane plane, bool triangulateHoles, out Mesh mesh0, out Mesh mesh1, out ContourData intersectionData)
        {
            Vector3 c0, c1;
            return Cut(mesh, meshTransform, plane, triangulateHoles, false, true, false, out mesh0, out mesh1, out c0, out c1, out intersectionData);
        }

        void AllocateBuffers(int trianglesNum, int verticesNum)
        {
            // pre-allocate mesh data for both sides
            if (triangles == null || triangles[0].Capacity < trianglesNum)
            {
//                MeshUtils.Log("Allocating triangles: " + trianglesNum);

                triangles = new[] { new List<int>(trianglesNum), new List<int>(trianglesNum) };
            }
            else
            {
                triangles[0].Clear();
                triangles[1].Clear();
            }

            if (vertices == null || vertices[0].Capacity < verticesNum)
            {
//                MeshUtils.Log("Allocating vertices: " + verticesNum);

                vertices = new[] { new List<Vector3>(verticesNum), new List<Vector3>(verticesNum) };
                normals = new[] { new List<Vector3>(verticesNum), new List<Vector3>(verticesNum) };
                uvs = new[] { new List<Vector2>(verticesNum), new List<Vector2>(verticesNum) };
                centroid = new Vector3[2];

                triCache = new int[verticesNum + 1];
                triCounter = new int[2] { 0, 0 };
                cutTris = new List<int>(verticesNum / 3);
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    vertices[i].Clear();
                    normals[i].Clear();
                    uvs[i].Clear();
                    centroid[i] = Vector3.zero;
                    triCounter[i] = 0;
                }
                cutTris.Clear();
                for (int i = 0; i < triCache.Length; i++)
                {
                    triCache[i] = 0;
                }
            }
        }

        void AllocateContours(int cutTrianglesNum)
        {
            // pre-allocate contour data
            if (contour == null)
            {
//                MeshUtils.Log("Allocating contours buffes: " + cutTrianglesNum);

                contour = new Contour(cutTrianglesNum);
                cutVertCache = new[] { new Dictionary<long, int>(cutTrianglesNum * 2), new Dictionary<long, int>(cutTrianglesNum * 2) };
                cornerVertCache = new[] { new Dictionary<int, int>(cutTrianglesNum), new Dictionary<int, int>(cutTrianglesNum) };
                contourBufferSize = cutTrianglesNum;
            }
            else
            {
                if (contourBufferSize < cutTrianglesNum)
                {
//                    MeshUtils.Log("Re-allocating contours buffes: " + cutTrianglesNum);

                    cutVertCache = new[] { new Dictionary<long, int>(cutTrianglesNum * 2), new Dictionary<long, int>(cutTrianglesNum * 2) };
                    cornerVertCache = new[] { new Dictionary<int, int>(cutTrianglesNum), new Dictionary<int, int>(cutTrianglesNum) };

                    contourBufferSize = cutTrianglesNum;
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        cutVertCache[i].Clear();
                        cornerVertCache[i].Clear();
                    }
                }

                contour.AllocateBuffers(cutTrianglesNum);
            }
        }

        float Cut(Mesh mesh, Transform meshTransform, Utils.Plane plane, bool triangulateHoles, bool fixPivot, bool getContourList, bool dontCut,
                         out Mesh mesh0, out Mesh mesh1, out Vector3 centroid0, out Vector3 centroid1, out ContourData intersectionData)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

#if PROFILING
            MeasureIt.Begin("CutAllocations");
#endif

            // cache mesh data
            var trianglesNum = mesh.triangles.Length;
            var verticesNum = mesh.vertices.Length;
            var meshTriangles = mesh.triangles;
            var meshVertices = mesh.vertices;
            var meshNormals = mesh.normals;
            var meshUV = mesh.uv;

            // preallocate buffers
            AllocateBuffers(trianglesNum, verticesNum);

#if PROFILING
            MeasureIt.End("CutAllocations");
            MeasureIt.Begin("CutCycleFirstPass");
#endif

            // inverse transform cutting plane
            plane.InverseTransform(meshTransform);

            // first pass - find complete triangles on both sides of the plane
            for (int i = 0; i < trianglesNum; i += 3)
            {
                // get triangle points
                var v0 = meshVertices[meshTriangles[i]];
                var v1 = meshVertices[meshTriangles[i + 1]];
                var v2 = meshVertices[meshTriangles[i + 2]];

                var side0 = plane.GetSideFix(ref v0);
                var side1 = plane.GetSideFix(ref v1);
                var side2 = plane.GetSideFix(ref v2);

                meshVertices[meshTriangles[i]] = v0;
                meshVertices[meshTriangles[i + 1]] = v1;
                meshVertices[meshTriangles[i + 2]] = v2;

                // all points on one side
                if (side0 == side1 && side1 == side2)
                {
                    var idx = side0 ? 0 : 1;

                    if (triCache[meshTriangles[i]] == 0)
                    {
                        triangles[idx].Add(triCounter[idx]);
                        vertices[idx].Add(meshVertices[meshTriangles[i]]);
                        normals[idx].Add(meshNormals[meshTriangles[i]]);
                        uvs[idx].Add(meshUV[meshTriangles[i]]);

                        centroid[idx] += meshVertices[meshTriangles[i]];

                        triCache[meshTriangles[i]] = triCounter[idx] + 1;
                        triCounter[idx]++;
                    }
                    else
                    {
                        triangles[idx].Add(triCache[meshTriangles[i]] - 1);
                    }

                    if (triCache[meshTriangles[i + 1]] == 0)
                    {
                        triangles[idx].Add(triCounter[idx]);
                        vertices[idx].Add(meshVertices[meshTriangles[i + 1]]);
                        normals[idx].Add(meshNormals[meshTriangles[i + 1]]);
                        uvs[idx].Add(meshUV[meshTriangles[i + 1]]);

                        centroid[idx] += meshVertices[meshTriangles[i + 1]];

                        triCache[meshTriangles[i + 1]] = triCounter[idx] + 1;
                        triCounter[idx]++;
                    }
                    else
                    {
                        triangles[idx].Add(triCache[meshTriangles[i + 1]] - 1);
                    }

                    if (triCache[meshTriangles[i + 2]] == 0)
                    {
                        triangles[idx].Add(triCounter[idx]);
                        vertices[idx].Add(meshVertices[meshTriangles[i + 2]]);
                        normals[idx].Add(meshNormals[meshTriangles[i + 2]]);
                        uvs[idx].Add(meshUV[meshTriangles[i + 2]]);

                        centroid[idx] += meshVertices[meshTriangles[i + 2]];

                        triCache[meshTriangles[i + 2]] = triCounter[idx] + 1;
                        triCounter[idx]++;
                    }
                    else
                    {
                        triangles[idx].Add(triCache[meshTriangles[i + 2]] - 1);
                    }
                }
                else
                {
                    // intersection triangles add to list and process it in second pass
                    cutTris.Add(i);
                }
            }

            if (vertices[0].Count == 0)
            {
                centroid[0] = meshVertices[0];
            }
            else
            {
                centroid[0] /= vertices[0].Count;
            }

            if (vertices[1].Count == 0)
            {
                centroid[1] = meshVertices[1];
            }
            else
            {
                centroid[1] /= vertices[1].Count;
            }

#if PROFILING
            MeasureIt.End("CutCycleFirstPass");
            MeasureIt.Begin("CutCycleSecondPass");
#endif
            mesh0 = null;
            mesh1 = null;
            centroid0 = centroid[0];
            centroid1 = centroid[1];
            intersectionData = null;

            if (cutTris.Count < 1)
            {
                stopWatch.Stop();
                return stopWatch.ElapsedMilliseconds;
            }

            AllocateContours(cutTris.Count);

            // second pass - cut intersecting triangles in half
            foreach (var cutTri in cutTris)
            {
                var triangle = new Triangle
                {
                    ids = new[] { meshTriangles[cutTri + 0], meshTriangles[cutTri + 1], meshTriangles[cutTri + 2] },
                    pos = new[] { meshVertices[meshTriangles[cutTri + 0]], meshVertices[meshTriangles[cutTri + 1]], meshVertices[meshTriangles[cutTri + 2]] },
                    normal = new[] { meshNormals[meshTriangles[cutTri + 0]], meshNormals[meshTriangles[cutTri + 1]], meshNormals[meshTriangles[cutTri + 2]] },
                    uvs = new[] { meshUV[meshTriangles[cutTri + 0]], meshUV[meshTriangles[cutTri + 1]], meshUV[meshTriangles[cutTri + 2]] }
                };

                // check points with a plane
                var side0 = plane.GetSide(triangle.pos[0]);
                var side1 = plane.GetSide(triangle.pos[1]);
                var side2 = plane.GetSide(triangle.pos[2]);

                float t0, t1;
                Vector3 s0, s1;

                var idxLeft = side0 ? 0 : 1;
                var idxRight = 1 - idxLeft;

                if (side0 == side1)
                {
                    var a = plane.IntersectSegment(triangle.pos[2], triangle.pos[0], out t0, out s0);
                    var b = plane.IntersectSegment(triangle.pos[2], triangle.pos[1], out t1, out s1);

                    MeshUtils.Assert(a && b, "!!!!!!!!!!!!!!!");

                    // left side ... 2 triangles
                    var s0Left = AddIntersectionPoint(s0, triangle, triangle.ids[2], triangle.ids[0], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);
                    var s1Left = AddIntersectionPoint(s1, triangle, triangle.ids[2], triangle.ids[1], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);
                    var v0Left = AddTrianglePoint(triangle.pos[0], triangle.normal[0], triangle.uvs[0], triangle.ids[0], triCache, cornerVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);
                    var v1Left = AddTrianglePoint(triangle.pos[1], triangle.normal[1], triangle.uvs[1], triangle.ids[1], triCache, cornerVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);

                    // Triangle (s0, v0, s1)
                    triangles[idxLeft].Add(s0Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(s1Left);

                    // Triangle (s1, v0, v1)
                    triangles[idxLeft].Add(s1Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(v1Left);

                    // right side ... 1 triangle
                    var s0Right = AddIntersectionPoint(s0, triangle, triangle.ids[2], triangle.ids[0], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight]);
                    var s1Right = AddIntersectionPoint(s1, triangle, triangle.ids[2], triangle.ids[1], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight]);
                    var v2Right = AddTrianglePoint(triangle.pos[2], triangle.normal[2], triangle.uvs[2], triangle.ids[2], triCache, cornerVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight]);

                    // Triangle (v2, s0, s1)
                    triangles[idxRight].Add(v2Right);
                    triangles[idxRight].Add(s0Right);
                    triangles[idxRight].Add(s1Right);

                    // buffer intersection vertices for triangulation
                    if (triangulateHoles)
                    {
                        if (idxLeft == 0)
                        {
                            contour.AddTriangle(cutTri, s0Left, s1Left, s0, s1);
                        }
                        else
                        {
                            contour.AddTriangle(cutTri, s0Right, s1Right, s0, s1);
                        }
                    }
                }
                else if (side0 == side2)
                {
                    var a = plane.IntersectSegment(triangle.pos[1], triangle.pos[0], out t0, out s1);
                    var b = plane.IntersectSegment(triangle.pos[1], triangle.pos[2], out t1, out s0);

                    MeshUtils.Assert(a && b, "!!!!!!!!!!!!!");

                    // left side ... 2 triangles
                    var s0Left = AddIntersectionPoint(s0, triangle, triangle.ids[1], triangle.ids[2], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);
                    var s1Left = AddIntersectionPoint(s1, triangle, triangle.ids[1], triangle.ids[0], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);
                    var v0Left = AddTrianglePoint(triangle.pos[0], triangle.normal[0], triangle.uvs[0], triangle.ids[0], triCache, cornerVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);
                    var v2Left = AddTrianglePoint(triangle.pos[2], triangle.normal[2], triangle.uvs[2], triangle.ids[2], triCache, cornerVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);

                    // Triangle (v2, s1, s0)
                    triangles[idxLeft].Add(v2Left);
                    triangles[idxLeft].Add(s1Left);
                    triangles[idxLeft].Add(s0Left);

                    // Triangle (v2, v0, s1)
                    triangles[idxLeft].Add(v2Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(s1Left);

                    // right side ... 1 triangle
                    var s0Right = AddIntersectionPoint(s0, triangle, triangle.ids[1], triangle.ids[2], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight]);
                    var s1Right = AddIntersectionPoint(s1, triangle, triangle.ids[1], triangle.ids[0], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight]);
                    var v1Right = AddTrianglePoint(triangle.pos[1], triangle.normal[1], triangle.uvs[1], triangle.ids[1], triCache, cornerVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight]);

                    // Triangle (s0, s1, v1)
                    triangles[idxRight].Add(s0Right);
                    triangles[idxRight].Add(s1Right);
                    triangles[idxRight].Add(v1Right);

                    // buffer intersection vertices for triangulation
                    if (triangulateHoles)
                    {
                        if (idxLeft == 0)
                        {
                            contour.AddTriangle(cutTri, s0Left, s1Left, s0, s1);
                        }
                        else
                        {
                            contour.AddTriangle(cutTri, s0Right, s1Right, s0, s1);
                        }
                    }
                }
                else
                {
                    var a = plane.IntersectSegment(triangle.pos[0], triangle.pos[1], out t0, out s0);
                    var b = plane.IntersectSegment(triangle.pos[0], triangle.pos[2], out t1, out s1);

                    MeshUtils.Assert(a && b, "!!!!!!!!!!!!!");

                    // right side ... 2 triangles
                    var s0Right = AddIntersectionPoint(s0, triangle, triangle.ids[0], triangle.ids[1], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight]);
                    var s1Right = AddIntersectionPoint(s1, triangle, triangle.ids[0], triangle.ids[2], cutVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight]);
                    var v1Right = AddTrianglePoint(triangle.pos[1], triangle.normal[1], triangle.uvs[1], triangle.ids[1], triCache, cornerVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight]);
                    var v2Right = AddTrianglePoint(triangle.pos[2], triangle.normal[2], triangle.uvs[2], triangle.ids[2], triCache, cornerVertCache[idxRight], vertices[idxRight], normals[idxRight], uvs[idxRight]);

                    // Triangle (v2, s1, v1)
                    triangles[idxRight].Add(v2Right);
                    triangles[idxRight].Add(s1Right);
                    triangles[idxRight].Add(v1Right);

                    // Triangle (s1, s0, v1)
                    triangles[idxRight].Add(s1Right);
                    triangles[idxRight].Add(s0Right);
                    triangles[idxRight].Add(v1Right);

                    // left side ... 1 triangle
                    var s0Left = AddIntersectionPoint(s0, triangle, triangle.ids[0], triangle.ids[1], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);
                    var s1Left = AddIntersectionPoint(s1, triangle, triangle.ids[0], triangle.ids[2], cutVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);
                    var v0Left = AddTrianglePoint(triangle.pos[0], triangle.normal[0], triangle.uvs[0], triangle.ids[0], triCache, cornerVertCache[idxLeft], vertices[idxLeft], normals[idxLeft], uvs[idxLeft]);

                    // Triangle (s1, v0, s0)
                    triangles[idxLeft].Add(s1Left);
                    triangles[idxLeft].Add(v0Left);
                    triangles[idxLeft].Add(s0Left);

                    // buffer intersection vertices for triangulation
                    if (triangulateHoles)
                    {
                        if (idxLeft == 0)
                        {
                            contour.AddTriangle(cutTri, s0Left, s1Left, s0, s1);
                        }
                        else
                        {
                            contour.AddTriangle(cutTri, s0Right, s1Right, s0, s1);
                        }
                    }
                }
            }

#if PROFILING
            MeasureIt.End("CutCycleSecondPass");
#endif


            if (triangulateHoles || getContourList)
            {
#if PROFILING
                MeasureIt.Begin("FindContours");
#endif

                contour.FindContours();

#if PROFILING
                MeasureIt.End("FindContours");
#endif
            }

            List<int>[] trianglesCut = null;

            if (triangulateHoles)
            {
#if PROFILING
                MeasureIt.Begin("Triangulate");
#endif

                trianglesCut = new List<int>[2] { new List<int>(contour.MidPointsCount), new List<int>(contour.MidPointsCount) };
                Triangulate(contour.contour, plane, vertices, normals, uvs, trianglesCut, true);

#if PROFILING
                MeasureIt.End("Triangulate");
#endif
            }

            intersectionData = null;

            if (getContourList)
            {

#if PROFILING
                MeasureIt.Begin("GetContourList");
#endif
                List<Vector3[]> contoursList = null;

                GetContourList(contour.contour, vertices[0], out contoursList);

                intersectionData = new ContourData(contoursList, meshTransform);

#if PROFILING
                MeasureIt.End("GetContourList");
#endif
            }

            centroid0 = centroid[0];
            centroid1 = centroid[1];

            if (dontCut)
            {
                MeshUtils.Assert(intersectionData != null, "Fuck");
                MeshUtils.Assert(getContourList != false, "Fuck2");
                mesh0 = null;
                mesh1 = null;
                return stopWatch.ElapsedMilliseconds;
            }

            if (vertices[0].Count > 0 && vertices[1].Count > 0)
            {
#if PROFILING
                MeasureIt.Begin("CutEndCopyBack");
#endif

                mesh0 = new Mesh();
                mesh1 = new Mesh();

                var verticesArray0 = vertices[0].ToArray();
                var verticesArray1 = vertices[1].ToArray();

#if PROFILING
                MeasureIt.Begin("FixPivot");
#endif

                if (fixPivot)
                {
                    MeshUtils.CenterPivot(verticesArray0, centroid[0]);
                    MeshUtils.CenterPivot(verticesArray1, centroid[1]);
                }

#if PROFILING
                MeasureIt.End("FixPivot");
#endif

                mesh0.vertices = verticesArray0;
                mesh0.normals = normals[0].ToArray();
                mesh0.uv = uvs[0].ToArray();

                mesh1.vertices = verticesArray1;
                mesh1.normals = normals[1].ToArray();
                mesh1.uv = uvs[1].ToArray();

                if (triangulateHoles && trianglesCut[0].Count > 0)
                {
                    mesh0.subMeshCount = 2;
                    mesh0.SetTriangles(triangles[0].ToArray(), 0);
                    mesh0.SetTriangles(trianglesCut[0].ToArray(), 1);

                    mesh1.subMeshCount = 2;
                    mesh1.SetTriangles(triangles[1].ToArray(), 0);
                    mesh1.SetTriangles(trianglesCut[1].ToArray(), 1);
                }
                else
                {
                    mesh0.triangles = triangles[0].ToArray();
                    mesh1.triangles = triangles[1].ToArray();  
                }

#if PROFILING
                MeasureIt.End("CutEndCopyBack");
#endif

                stopWatch.Stop();
                return stopWatch.ElapsedMilliseconds;
            }

            mesh0 = null;
            mesh1 = null;
            stopWatch.Stop();

//            UnityEngine.Debug.Log("Empty cut! " + vertices[0].Count + " " + vertices[1].Count);

            return stopWatch.ElapsedMilliseconds;
        }

        struct Triangle
        {
            public int[] ids;
            public Vector3[] pos;
            public Vector3[] normal;
            public Vector2[] uvs;
        }

        int AddIntersectionPoint(Vector3 pos, Triangle tri, int edge0, int edge1, Dictionary<long, int> cache, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs)
        {
            //! TODO: figure out position hash for shared vertices
//            var key = pos.GetHashCode();
            var key = edge0 < edge1 ? (edge0 << 16) + edge1 : (edge1 << 16) + edge0;

            int result;
            if (cache.TryGetValue(key, out result))
            {
                // cache hit!
                return result;
            }

            // compute barycentric coordinates for a new point to interpolate normal and uv
            var baryCoord = MeshUtils.ComputeBarycentricCoordinates(tri.pos[0], tri.pos[1], tri.pos[2], pos);

            vertices.Add(pos);

            normals.Add(new Vector3(baryCoord.x * tri.normal[0].x + baryCoord.y * tri.normal[1].x + baryCoord.z * tri.normal[2].x,
                                    baryCoord.x * tri.normal[0].y + baryCoord.y * tri.normal[1].y + baryCoord.z * tri.normal[2].y,
                                    baryCoord.x * tri.normal[0].z + baryCoord.y * tri.normal[1].z + baryCoord.z * tri.normal[2].z));

            uvs.Add(new Vector2(baryCoord.x * tri.uvs[0].x + baryCoord.y * tri.uvs[1].x + baryCoord.z * tri.uvs[2].x,
                                baryCoord.x * tri.uvs[0].y + baryCoord.y * tri.uvs[1].y + baryCoord.z * tri.uvs[2].y));

            var vertIndex = vertices.Count - 1;

            cache.Add(key, vertIndex);

            return vertIndex;
        }

        int AddTrianglePoint(Vector3 pos, Vector3 normal, Vector2 uv, int idx, int[] triCache, Dictionary<int, int> cache,  List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs)
        {
            // tricache
            if (triCache[idx] != 0)
            {
                // cache hit!
                return triCache[idx] - 1;
            }

            // second cache
            int result;
            if (cache.TryGetValue(idx, out result))
            {
                // cache hit!
                return result;
            }

            vertices.Add(pos);
            normals.Add(normal);
            uvs.Add(uv);

            var vertIndex = vertices.Count - 1;

            cache.Add(idx, vertIndex);

            return vertIndex;
        }

        void GetContourList(List<Dictionary<int, int>> contours, List<Vector3> vertices, out List<Vector3[]> contoursList)
        {
            contoursList = null;

            if (contours.Count == 0 || contours[0].Count < 3)
            {
                return;
            }

            contoursList = new List<Vector3[]>(contours.Count);

            var count = contours.Count;
            for (int i=0; i<count; i++)
            {
                contoursList.Add(new Vector3[contours[i].Count]);

                var idx = 0;

                foreach (var vertexID in contours[i])
                {
                    contoursList[i][idx++] = vertices[vertexID.Value];
                }
            }
        }

        void Triangulate(List<Dictionary<int, int>> contours, Utils.Plane plane, List<Vector3>[] vertices, List<Vector3>[] normals, List<Vector2>[] uvs, List<int>[] triangles, bool uvCutMesh)
        {
            if (contours.Count == 0 || contours[0].Count < 3)
            {
                return;
            }

            // prepare plane matrix
            var m = plane.GetPlaneMatrix();
            var mInv = m.inverse;

            var zShit = 0.0f;

            var polygons = new List<Polygon>(contours.Count);

            // construct polygons from contours
            Polygon highAreaPoly = null;
            foreach (var ctr in contours)
            {
                var polygonPoints = new Vector2[ctr.Count];
                var j = 0;

                foreach (var i in ctr.Values)
                {
                    var p = mInv*vertices[0][i];
                    polygonPoints[j++] = p;

                    // save z-coordinate
                    zShit = p.z;
                }

                var polygon = new Polygon(polygonPoints);
                polygons.Add(polygon);

                if (highAreaPoly == null || highAreaPoly.Area < polygon.Area)
                {
                    highAreaPoly = polygon;
                }
            }

            MeshUtils.Assert(polygons.Count > 0, "Zero polygons!");

            // test for holes
            if (polygons.Count > 0)
            {
                var polyToRemove = new List<Polygon>();

                foreach (var polygon in polygons)
                {
                    if (polygon != highAreaPoly)
                    {
                        if (highAreaPoly.IsPointInside(polygon.Points[0]))
                        {
                            highAreaPoly.AddHole(polygon);
                            polyToRemove.Add(polygon);
                        }
                    }
                }

                foreach (var polygon in polyToRemove)
                {
                    polygons.Remove(polygon);
                }
            }

            var vertCounter0 = vertices[0].Count;
            var vertCounter1 = vertices[1].Count;

            foreach (var polygon in polygons)
            {
                var indices = polygon.Triangulate();

                // get polygon bounding square size
                var min = Mathf.Min(polygon.Min.x, polygon.Min.y);
                var max = Mathf.Max(polygon.Max.x, polygon.Max.y);
                var polygonSize = min - max;

//                MeshUtils.Log("PolygonSize: " + polygonSize + " " + polygon.Min + " " + polygon.Max);

                foreach (var polyPoint in polygon.Points)
                {
                    var p = m * new Vector3(polyPoint.x, polyPoint.y, zShit);

                    vertices[0].Add(p);
                    vertices[1].Add(p);
                    normals[0].Add(-plane.Normal);
                    normals[1].Add(plane.Normal);

                    if (uvCutMesh)
                    {
                        uvs[0].Add(new Vector2((polyPoint.x - min) / polygonSize,
                                               (polyPoint.y - min) / polygonSize));
                        uvs[1].Add(new Vector2((polyPoint.x - min) / polygonSize,
                                               (polyPoint.y - min) / polygonSize));
                    }
                    else
                    {
                        uvs[0].Add(Vector2.zero);
                        uvs[1].Add(Vector2.zero);
                    }
                }

                var indicesCount = indices.Count;
                var j = indicesCount - 1;
                for (int i = 0; i < indicesCount; i++)
                {
                    triangles[0].Add(vertCounter0 + indices[i]);
                    triangles[1].Add(vertCounter1 + indices[j]);
                    j--;
                }

                vertCounter0 += polygon.Points.Length;
                vertCounter1 += polygon.Points.Length;
            }
        }
    }
}
