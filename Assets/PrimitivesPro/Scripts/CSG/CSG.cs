// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

//
// This CSG library is based on javascript library made by Evan Wallace
// http://evanw.github.io/csg.js/ distributed under MIT license.
// Copyright (c) 2011 Evan Wallace (http://madebyevan.com/), under the MIT license.
//
// For more information about algorithm, see http://www.mcs.csueastbay.edu/~tebo/papers/siggraph90.pdf.
//

using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrimitivesPro.CSG
{
	class CSG
	{
        /// <summary>
        /// root of bsp tree
        /// </summary>
	    private CSGNode root;

        /// <summary>
        /// list of polygons
        /// </summary>
	    private List<CSGPolygon> Polygons;

        /// <summary>
        /// empty constructor
        /// </summary>
        public CSG()
        {
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="instance">copy instance</param>
        public CSG(CSG instance)
        {
            Polygons = new List<CSGPolygon>(instance.Polygons.Count);
            foreach (var polygon in instance.Polygons)
            {
                Polygons.Add(new CSGPolygon(polygon));
            }
            root = new CSGNode();
            root.Build(Polygons);
        }

	    /// <summary>
	    /// construct a new bsp tree from unity mesh
	    /// </summary>
	    /// <param name="mesh">unity mesh</param>
	    /// <param name="meshTransform">transformation</param>
	    /// <param name="id">id of the mesh</param>
	    public void Construct(Mesh mesh, Transform meshTransform, int id)
        {
            // cache mesh data
            var trianglesNum = mesh.triangles.Length;
            var meshTriangles = mesh.triangles;
            var meshVertices = mesh.vertices;
            var meshNormals = mesh.normals;
            var meshUV = mesh.uv;

            Polygons = new List<CSGPolygon>(trianglesNum/3);

            for (int i = 0; i < trianglesNum; i+=3)
            {
                var id0 = meshTriangles[i];
                var id1 = meshTriangles[i + 1];
                var id2 = meshTriangles[i + 2];

                Polygons.Add(new CSGPolygon(id, new CSGVertex(meshTransform.TransformPoint(meshVertices[id0]), meshTransform.TransformDirection(meshNormals[id0]), meshUV[id0]),
                                             new CSGVertex(meshTransform.TransformPoint(meshVertices[id1]), meshTransform.TransformDirection(meshNormals[id1]), meshUV[id1]),
                                             new CSGVertex(meshTransform.TransformPoint(meshVertices[id2]), meshTransform.TransformDirection(meshNormals[id2]), meshUV[id2])));
            }

            root = new CSGNode();
            root.Build(Polygons);
        }

        public void TestDepth()
        {
            root.TestDepth();
        }

        /// <summary>
        /// return unity mesh from this tree
        /// </summary>
        /// <returns>unity mesh</returns>
        public Mesh ToMesh()
        {
            var mesh = new Mesh();

            var polygons = root.AllPolygons();

            var triCount = polygons.Count*3*2;

            var triangles0 = new List<int>(triCount);
            var triangles1 = new List<int>(triCount);
            var vertices = new List<Vector3>(triCount);
            var normals = new List<Vector3>(triCount);
            var uvs = new List<Vector2>(triCount);

            var vertCache = new Dictionary<int, int>(triCount);
            var hash = new VertexHash(0.001f, triCount*3*2);

            var vertIdx = 0;

            foreach (var polygon in polygons)
            {
                var polyPoints = polygon.Vertices.Count;

                for (int i = 2; i < polyPoints; i++)
                {
                    var v0 = AddVertex(polygon.Vertices[0], vertices, normals, uvs, vertCache, hash);
                    var v1 = AddVertex(polygon.Vertices[i-1], vertices, normals, uvs, vertCache, hash);
                    var v2 = AddVertex(polygon.Vertices[i], vertices, normals, uvs, vertCache, hash);

                    if (polygon.Id == 0)
                    {
                        triangles0.Add(v0);
                        triangles0.Add(v1);
                        triangles0.Add(v2);
                    }
                    else
                    {
                        triangles1.Add(v0);
                        triangles1.Add(v1);
                        triangles1.Add(v2);
                    }

                    vertIdx += 3;
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.subMeshCount = 2;
            mesh.SetTriangles(triangles0.ToArray(), 0);
            mesh.SetTriangles(triangles1.ToArray(), 1);

            mesh.RecalculateNormals();

            mesh.Optimize();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// helper for caching duplicated vertices
        /// </summary>
        int AddVertex(CSGVertex v0, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, Dictionary<int, int> cache, VertexHash hash)
        {
            var h = hash.Hash(v0);

            int value;
            if (cache.TryGetValue(h, out value))
            {
                return value;
            }

            vertices.Add(v0.P);
            normals.Add(v0.N);
            uvs.Add(v0.UV);

            var id = vertices.Count - 1;

            cache.Add(h, id);

            return id;
        }

        /// <summary>
        /// make a csg union operation between this and parameter
        /// </summary>
        /// <param name="csg">csg tree</param>
        /// <returns>new csg tree</returns>
        public CSG Union(CSG csg)
        {
            var a = new CSG(this);
            var b = new CSG(csg);

            a.root.ClipTo(b.root);
            b.root.ClipTo(a.root);
            b.root.Invert();
            b.root.ClipTo(a.root);
            b.root.Invert();
            a.root.Build(b.root.AllPolygons());
            return a;
        }

        /// <summary>
        /// make a csg substract operation between this and parameter
        /// </summary>
        /// <param name="csg">csg tree</param>
        /// <returns>new csg tree</returns>
        public CSG Substract(CSG csg)
        {
            var a = new CSG(this);
            var b = new CSG(csg);

//            a.root.Invert();
//            a.root.ClipTo(b.root);
//            a.root.Build(b.root.AllPolygons());
//            a.root.Invert();

            a.root.Invert();
            a.root.ClipTo(b.root);
            b.root.ClipTo(a.root);
            b.root.Invert();
            b.root.ClipTo(a.root);
            b.root.Invert();
            a.root.Build(b.root.AllPolygons());
            a.root.Invert();

            return a;
        }

        /// <summary>
        /// make a csg intersect operation between this and parameter
        /// </summary>
        /// <param name="csg">csg tree</param>
        /// <returns>new csg tree</returns>
        public CSG Intersect(CSG csg)
        {
            var a = new CSG(this);
            var b = new CSG(csg);

            a.root.Invert();
            b.root.ClipTo(a.root);
            b.root.Invert();
            a.root.ClipTo(b.root);
            b.root.ClipTo(a.root);
            a.root.Build(b.root.AllPolygons());
            a.root.Invert();
            return a;
        }

        public CSG Test()
        {
            return this;
        }

        /// <summary>
        /// make a csg inverse operation of this tree
        /// </summary>
        /// <returns>new csg tree</returns>
        public CSG Inverse()
        {
            var a = new CSG(this);

            foreach (var polygon in Polygons)
            {
                polygon.Flip();
            }

            return a;
        }
	}
}
