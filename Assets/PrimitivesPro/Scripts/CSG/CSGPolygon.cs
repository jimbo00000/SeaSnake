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
using Plane = PrimitivesPro.Utils.Plane;

namespace PrimitivesPro.CSG
{
    /// <summary>
    /// represents a single polygon
    /// </summary>
	class CSGPolygon
    {
        /// <summary>
        /// vertices
        /// </summary>
        public List<CSGVertex> Vertices;

        /// <summary>
        /// plane made from this polygon
        /// </summary>
        public CSGPlane Plane;

        /// <summary>
        /// id of the polygon (useful for marking which objects this polygon belongs to)
        /// </summary>
        public int Id;

        /// <summary>
        /// constructor with points, normals and uvs for 3 vertices
        /// </summary>
        public CSGPolygon(int id, List<CSGVertex> vertices)
        {
            Vertices = vertices;
            Plane = new CSGPlane(vertices[0].P, vertices[1].P, vertices[2].P);
            Id = id;
        }

        /// <summary>
        /// constructor with parameters
        /// </summary>
        /// <param name="id">id of the polygon</param>
        /// <param name="vertices"></param>
        public CSGPolygon(int id, params CSGVertex[] vertices)
        {
            Vertices = new List<CSGVertex>(vertices);
            Plane = new CSGPlane(vertices[0].P, vertices[1].P, vertices[2].P);
            Id = id;
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="instance">copy instance</param>
        public CSGPolygon(CSGPolygon instance)
        {
            Vertices = new List<CSGVertex>(instance.Vertices.Count);

            foreach (var vertex in instance.Vertices)
            {
                Vertices.Add(new CSGVertex(vertex));
            }

            Plane = new CSGPlane(instance.Plane);
            Id = instance.Id;
        }

        /// <summary>
        /// reverse vertices and flip normals
        /// </summary>
        public void Flip()
        {
            Vertices.Reverse();

            foreach (var vertex in Vertices)
            {
                vertex.Flip();
            }
        }
	}
}
