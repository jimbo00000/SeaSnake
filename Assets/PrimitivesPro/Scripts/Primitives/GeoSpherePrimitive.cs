// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PrimitivesPro.Primitives
{
    /// <summary>
    /// class for creating GeoSphere primitive
    /// </summary>
    public class GeoSpherePrimitive : Primitive
    {
        /// <summary>
        /// type of generation primitive
        /// </summary>
        public enum BaseType
        {
            Tetrahedron,        // 4 faces
            Octahedron,         // 8 faces
            Icosahedron,        // 20 faces
            Icositetrahedron,   // 24 faces
        };

        /// <summary>
        /// generate geometry for GeoSphere
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="radius">radius of sphere</param>
        /// <param name="subdivision">number of subdivision</param>
        /// <param name="baseType">type of generation primitive</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static float GenerateGeometry(Mesh mesh, float radius, int subdivision, BaseType baseType, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            radius = Mathf.Clamp(radius, 0, 100);
            subdivision = Mathf.Clamp(subdivision, 0, 6);

            mesh.Clear();

            var sharedVertices = normalsType == NormalsType.Vertex;

            var verticesNum = GetVertCount(baseType, subdivision, sharedVertices);
            var trianglesNum = GetTriCount(baseType, subdivision);

            // fix for too much vertices
            while (verticesNum > 60000)
            {
                subdivision -= 1;

                verticesNum = GetVertCount(baseType, subdivision, sharedVertices);
                trianglesNum = GetTriCount(baseType, subdivision);
            }

            var pivotOffset = Vector3.zero;
            switch (pivotPosition)
            {
                case PivotPosition.Botttom: pivotOffset = new Vector3(0.0f, radius, 0.0f);
                    break;
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -radius, 0.0f);
                    break;
            }

            var triangles = new int[trianglesNum * 3];
            var trianglesTmp = new int[trianglesNum*3];
            var vertices = new Vector3[verticesNum];
            var uvs = new Vector2[verticesNum];
            Vector3[] normals = null;

            // initialize basic primitive
            InitBasePrimitive(radius, baseType, vertices, uvs, triangles);

            var indexLookup = new Dictionary<int, int>();

            var vertIndex = GetVertCount(baseType, 0, sharedVertices);

            for (int i = 0; i < subdivision; i++)
            {
                var newTriIdx = 0;
                var triCount = GetTriCount(baseType, i)*3;

                for (int triIdx = 0; triIdx < triCount; triIdx += 3)
                {
                    // get triangle
                    var v1 = triangles[triIdx + 0];
                    var v2 = triangles[triIdx + 1];
                    var v3 = triangles[triIdx + 2];

                    // split each edge in half
                    var va = AddMidPoint(vertices, radius, vertIndex++, v1, v2, indexLookup);
                    var vb = AddMidPoint(vertices, radius, vertIndex++, v2, v3, indexLookup);
                    var vc = AddMidPoint(vertices, radius, vertIndex++, v3, v1, indexLookup);

                    // create 4 new triangles
                    trianglesTmp[newTriIdx + 0] = v1;
                    trianglesTmp[newTriIdx + 1] = va;
                    trianglesTmp[newTriIdx + 2] = vc;

                    trianglesTmp[newTriIdx + 3] = v2;
                    trianglesTmp[newTriIdx + 4] = vb;
                    trianglesTmp[newTriIdx + 5] = va;

                    trianglesTmp[newTriIdx + 6] = v3;
                    trianglesTmp[newTriIdx + 7] = vc;
                    trianglesTmp[newTriIdx + 8] = vb;

                    trianglesTmp[newTriIdx + 9] = va;
                    trianglesTmp[newTriIdx + 10] = vb;
                    trianglesTmp[newTriIdx + 11] = vc;

                    newTriIdx += 12;
                }

                var swapTmp = trianglesTmp;
                trianglesTmp = triangles;
                triangles = swapTmp;
            }

            // duplicate shared vertices if we are dealing with faces normals
            if (normalsType == NormalsType.Face)
            {
                MeshUtils.DuplicateSharedVertices(ref vertices, ref uvs, triangles, -1);
            }

            // generate spherical uv mapping
            for (int i = 0; i < verticesNum; i++)
            {
                uvs[i] = GetSphericalUV(ref vertices[i]);
            }

            var verticesList = new List<Vector3>(vertices);
            var uvList = new List<Vector2>(uvs);
            var triangleList = new List<int>(triangles);

            CorrectSeam(verticesList, uvList, triangleList);
            CorrectPoles(verticesList, uvList, ref triangleList, radius);

            vertices = verticesList.ToArray();
            triangles = triangleList.ToArray();

            if (normalsType == NormalsType.Vertex)
            {
                CalculateNormals(vertices, out normals);
            }
            else
            {
                MeshUtils.ComputeVertexNormals(vertices, triangles, out normals);
            }

            CorrectPivot(verticesList, pivotPosition, ref pivotOffset);

            trianglesTmp = null;

            if (verticesList.Count > 60000)
            {
                Debug.LogError("Too much vertices!");
                return 0.0f;
            }

            mesh.vertices = verticesList.ToArray();
            mesh.uv = uvList.ToArray();
            mesh.triangles = triangleList.ToArray();
            mesh.normals = normals;

            mesh.RecalculateBounds();
            MeshUtils.CalculateTangents(mesh);
            mesh.Optimize();

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// create new vertex by spliting triangle edge
        /// </summary>
        /// <param name="vertices">array of vertices</param>
        /// <param name="radius">radius of geoSphere</param>
        /// <param name="vertIndex">current vertex index</param>
        /// <param name="id0">index of first edge point</param>
        /// <param name="id1">index of second edge point</param>
        /// <param name="indexLookup">lookup cache for already computed vertices</param>
        /// <returns>returns index of new point</returns>
        static int AddMidPoint(Vector3[] vertices, float radius, int vertIndex, int id0, int id1, Dictionary<int, int> indexLookup)
        {
            int key = 0;

            // for shared vertices make a lookup for used index
            key = id0 < id1 ? (id0 << 16) + id1 : (id1 << 16) + id0;

            int result;
            if (indexLookup.TryGetValue(key, out result))
            {
                return result;
            }

            // make unit vector in direction to split edge
            var v0 = ((vertices[id0] + vertices[id1]) * 0.5f).normalized;

            // make it length to sphere radius
            var midPoint = v0*radius;

            vertices[vertIndex] = midPoint;

            // add index to lookup
            indexLookup.Add(key, vertIndex);

            return vertIndex;
        }

        /// <summary>
        /// set vertices and triangles for base geosphere primitive
        /// </summary>
        /// <param name="radius">radius of geosphere</param>
        /// <param name="baseType">base type of geosphere</param>
        /// <param name="vertices">vertices array</param>
        /// <param name="uvs">uv array</param>
        /// <param name="triangles">triangle indices array</param>
        static void InitBasePrimitive(float radius, BaseType baseType, Vector3[] vertices, Vector2[] uvs, int[] triangles)
        {
            switch (baseType)
            {
                case BaseType.Tetrahedron:
                {
                    var b = 1.0f / Mathf.Sqrt(2);
                    var c = 1.0f / Mathf.Sqrt(1.0f + 0.5f);
                    var a = 0.67f * radius / c;
                    b = 0.67f * radius * b / c;

                    // 4 vertices
                    SetVertices(vertices, new Vector3(a, 0, -b), new Vector3(-a, 0, -b),
                                          new Vector3(0, a, b), new Vector3(0, -a, b));

                    // 4 triangles
                    SetTriangles(triangles, 0, 1, 2, 1, 3, 2, 0, 2, 3, 0, 3, 1);
                }
                break;

                case BaseType.Octahedron:
                {
                    // 6 vertices
                    SetVertices(vertices, new Vector3(0, -radius, 0),  //0
                                          new Vector3(-radius, 0, 0),  //1
                                          new Vector3(0, 0, -radius),  //2
                                          new Vector3(radius, 0, 0),   //3
                                          new Vector3(0, 0, radius),   //4
                                          new Vector3(0, radius, 0));  //5

                    // 8 triangles
                    SetTriangles(triangles, 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 1,
                                            5, 2, 1, 5, 3, 2, 5, 4, 3, 5, 1, 4);
                }
                break;

                case BaseType.Icosahedron:
                {
                    var a = 1.0f;
                    var b = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;
                    var scale = radius / Mathf.Sqrt(a * a + b * b);
                    a = a * scale;
                    b = b * scale;

                    // 12 vertices
                    SetVertices(vertices, new Vector3(-a, b, 0), new Vector3(a, b, 0),
                                          new Vector3(-a, -b, 0), new Vector3(a, -b, 0),
                                          new Vector3(0, -a, b), new Vector3(0, a, b),
                                          new Vector3(0, -a, -b), new Vector3(0, a, -b),
                                          new Vector3(b, 0, -a), new Vector3(b, 0, a),
                                          new Vector3(-b, 0, -a), new Vector3(-b, 0, a));

                    // 20 triangles
                    SetTriangles(triangles, 0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10,
                                            0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2,
                                            10, 7, 6, 7, 1, 8, 3, 9, 4, 3, 4, 2,
                                            3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5,
                                            2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1);
                }
                break;

                case BaseType.Icositetrahedron:
                {
                    float c = radius;
                    float r = radius / Mathf.Sqrt(3);

                    // 14 vertices
                    SetVertices(vertices, new Vector3(0, c, 0), new Vector3(0, -c, 0),
                                          new Vector3(c, 0, 0), new Vector3(-c, 0, 0),
                                          new Vector3(0, 0, c), new Vector3(0, 0, -c),
                                          new Vector3(-r, r, r), new Vector3(-r, r, -r),
                                          new Vector3(r, r, -r), new Vector3(r, r, r),
                                          new Vector3(-r, -r, r), new Vector3(-r, -r, -r),
                                          new Vector3(r, -r, -r), new Vector3(r, -r, r));

                    // 24 triangles
                    SetTriangles(triangles, 0, 7, 6, 0, 8, 7, 0, 9, 8, 0, 6, 9,
                                            1, 10, 11, 1, 11, 12, 1, 12, 13, 1, 13, 10,
                                            2, 8, 9, 2, 9, 13, 2, 13, 12, 2, 12, 8,
                                            3, 6, 7, 3, 7, 11, 3, 11, 10, 3, 10, 6,
                                            4, 9, 6, 4, 13, 9, 4, 10, 13, 4, 6, 10,
                                            5, 7, 8, 5, 8, 12, 5, 12, 11, 5, 11, 7);
                }
                break;
            }
        }

        /// <summary>
        /// return number of triangles for geosphere
        /// </summary>
        /// <param name="type">base type of geosphere</param>
        /// <param name="subdivision">level of subdivision</param>
        /// <returns>number of triangles</returns>
        static int GetTriCount(BaseType type, int subdivision)
        {
            var baseTriCount = 0;
            switch (type)
            {
                case BaseType.Tetrahedron:
                    baseTriCount = 4;
                    break;
                case BaseType.Octahedron:
                    baseTriCount = 8;
                    break;
                case BaseType.Icosahedron:
                    baseTriCount = 20;
                    break;
                case BaseType.Icositetrahedron:
                    baseTriCount = 24;
                    break;
            }

            return baseTriCount*(int) Mathf.Pow(4, subdivision);
        }

        /// <summary>
        /// return number of vertices for geosphere
        /// </summary>
        /// <param name="type">base type of geosphere</param>
        /// <param name="subdivision">level of subdivision</param>
        /// <param name="sharedVertices">flag if we are using shared vertices</param>
        /// <returns>number of vertices</returns>
        static int GetVertCount(BaseType type, int subdivision, bool sharedVertices)
        {
            var tris = GetTriCount(type, subdivision);

            if (sharedVertices)
            {
                return tris;
            }

            return tris * 3;
        }

        /// <summary>
        /// copy array of vertices into vertices array
        /// </summary>
        /// <param name="vertices">destination array of vertices</param>
        /// <param name="verts">source array of vertices</param>
        static void SetVertices(Vector3[] vertices, params Vector3[] verts)
        {
            for (int i = 0; i < verts.Length; i++)
            {
                vertices[i] = verts[i];
            }
        }

        /// <summary>
        /// copy array of triangle indices into triangle array
        /// </summary>
        /// <param name="triangles">destination triangle array</param>
        /// <param name="tris">source array of indices</param>
        static void SetTriangles(int[] triangles, params int[] tris)
        {
            for (int i = 0; i < tris.Length; i++)
            {
                triangles[i] = tris[i];
            }
        }

        /// <summary>
        /// calculate normals on sphere
        /// </summary>
        /// <param name="vertices">array of vertices</param>
        /// <param name="normals">normals to calculate</param>
        static void CalculateNormals(Vector3[] vertices, out Vector3[] normals)
        {
            normals = new Vector3[vertices.Length];

            for (int i=0; i<vertices.Length; i++)
            {
                normals[i] = vertices[i].normalized;
            }
        }

        /// <summary>
        /// return uv spherical mapping for point on sphere
        /// </summary>
        /// <param name="pnt"></param>
        /// <returns></returns>
        static Vector2 GetSphericalUV(ref Vector3 pnt)
        {
            var v0 = pnt.normalized;

            return new Vector2((0.5f + Mathf.Atan2(v0.z, v0.x) / (Mathf.PI * 2.0f)),
                                1.0f - (0.5f - Mathf.Asin(v0.y) / Mathf.PI));
        }

        /// <summary>
        /// correct pivot center
        /// </summary>
        /// <param name="vertices">vertices array</param>
        /// <param name="pivotPosition">type of pivot position</param>
        /// <param name="pivotOffset">pivot offset from center</param>
        static void CorrectPivot(List<Vector3> vertices, PivotPosition pivotPosition, ref Vector3 pivotOffset)
        {
            if (pivotPosition != PivotPosition.Center)
            {
                for (int i=0; i<vertices.Count; i++)
                {
                    vertices[i] += pivotOffset;
                }
            }
        }

        /// <summary>
        /// correct uv mapping on seam
        /// </summary>
        /// <param name="vertices">vertices array</param>
        /// <param name="uvs">uv array</param>
        /// <param name="triangles">triangles array</param>
        static void CorrectSeam(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
        {
            var newTriangles = new List<int>();

            var indexCache = new Dictionary<int, int>();

            for (int i = triangles.Count - 3; i >= 0; i -= 3)
            {
                // check texture coordinates if they are in counter-clock-wise order
                var v0 = uvs[triangles[i + 0]];
                var v1 = uvs[triangles[i + 1]];
                var v2 = uvs[triangles[i + 2]];

                var cross = Vector3.Cross(v0 - v1, v2 - v1);

                if (cross.z <= 0)
                {
                    // this should only happen if the face crosses a texture boundary
                    for (var j = i; j < i + 3; j++)
                    {
                        int index = triangles[j];
                        var vertex = vertices[index];
                        var uv = uvs[index];

                        if (uv.x >= 0.8f)
                        {
                            // need to correct this vertex
                            if (indexCache.ContainsKey(index))
                            {
                                newTriangles.Add(indexCache[index]);
                            }
                            else
                            {
                                // fix uv
                                var texCoord = uv;
                                texCoord.x -= 1;

                                // add new vertex and uv
                                vertices.Add(vertex);
                                uvs.Add(texCoord);

                                int correctedVertexIndex = vertices.Count - 1;

                                indexCache.Add(index, correctedVertexIndex);

                                newTriangles.Add(correctedVertexIndex);
                            }
                        }
                        else
                        {
                            newTriangles.Add(index);
                        }
                    }
                }
                else
                {
                    newTriangles.AddRange(triangles.GetRange(i, 3));
                }
            }

            triangles.Clear();
            triangles.AddRange(newTriangles);
        }

        /// <summary>
        /// correct uv mapping on poles
        /// </summary>
        /// <param name="vertices">vertices array</param>
        /// <param name="uvs">uv array</param>
        /// <param name="triangles">triangles array</param>
        /// <param name="radius">radius of the sphere</param>
        static void CorrectPoles(List<Vector3> vertices, List<Vector2> uvs, ref List<int> triangles, float radius)
        {
            var newTriangles = new List<int>(triangles);

            // loop all vertices and check for pole position
            for (int i = 0; i < triangles.Count; i+=3)
            {
                var v1 = triangles[i];
                var v2 = triangles[i + 1];
                var v3 = triangles[i + 2];

                if (Math.Abs(vertices[v1].y) == radius)
                {
                    var newVertex = vertices[v1];
                    var newUv = uvs[v1];

                    float offset = 0.5f;

                    if (vertices[v1].y == radius)
                    {
                        if (uvs[v2].x > uvs[v3].x)
                        {
                            offset = 0.0f;
                        }
                    }
                    else
                    {
                        if (uvs[v2].x < uvs[v3].x)
                        {
                            offset = 0;
                        }
                    }

                    newUv.x = (uvs[v2].x + uvs[v3].x)*0.5f + offset;
                    newUv.y = 1.0f - (0.5f - Mathf.Asin(Mathf.Sign(newVertex.y)) / Mathf.PI);

                    vertices.Add(newVertex);
                    uvs.Add(newUv);
                    newTriangles.Add(vertices.Count-1);
                    newTriangles.Add(v2);
                    newTriangles.Add(v3);
                }
                else if (Math.Abs(vertices[v2].y) == radius)
                {
                    var newVertex = vertices[v2];
                    var newUv = uvs[v2];

//                    float offset = 0.5f;
//
//                    if (Math.Abs(vertices[v2].y - 1) < epsylon)
//                    {
//                        if (uvs[v1].x > uvs[v3].x)
//                        {
//                            offset = 0.0f;
//                        }
//                    }
//                    else
//                    {
//                        if (uvs[v1].x < uvs[v3].x)
//                        {
//                            offset = 0;
//                        }
//                    }

                    newUv.x = (uvs[v1].x + uvs[v3].x)*0.5f;// +offset;
                    newUv.y = 1.0f - (0.5f - Mathf.Asin(Mathf.Sign(newVertex.y)) / Mathf.PI);

                    vertices.Add(newVertex);
                    uvs.Add(newUv);
                    newTriangles.Add(v1);
                    newTriangles.Add(vertices.Count - 1);
                    newTriangles.Add(v3);
                }
                else if (Math.Abs(vertices[v3].y) == radius)
                {
                    var newVertex = vertices[v3];
                    var newUv = uvs[v3];

                    float offset = 0.5f;

                    if (vertices[v3].y == radius)
                    {
                        if (uvs[v1].x > uvs[v2].x)
                        {
                            offset = 0.0f;
                        }
                    }
                    else
                    {
                        if (uvs[v1].x < uvs[v2].x)
                        {
                            offset = 0;
                        }
                    }

                    newUv.x = (uvs[v1].x + uvs[v2].x) * 0.5f + offset;
                    newUv.y = 1.0f - (0.5f - Mathf.Asin(Mathf.Sign(newVertex.y)) / Mathf.PI);

                    vertices.Add(newVertex);
                    uvs.Add(newUv);
                    newTriangles.Add(v1);
                    newTriangles.Add(v2);
                    newTriangles.Add(vertices.Count - 1);
                }
                else
                {
                    newTriangles.Add(v1);
                    newTriangles.Add(v2);
                    newTriangles.Add(v3);
                }
            }

            triangles = newTriangles;
        }
    }
}
