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
using PrimitivesPro.Utils;

namespace PrimitivesPro.CSG
{
    /// <summary>
    /// this class represents a single node of a bsp tree
    /// </summary>
	class CSGNode
	{
        /// <summary>
        /// list of polygons in this node
        /// </summary>
	    private List<CSGPolygon> Polygons;

        /// <summary>
        /// splitting plane
        /// </summary>
	    private CSGPlane Plane;

        /// <summary>
        /// front child
        /// </summary>
	    private CSGNode FrontChild;

        /// <summary>
        /// back child
        /// </summary>
	    private CSGNode BackChild;

        /// <summary>
        /// c-tor
        /// </summary>
        public CSGNode()
        {
            Polygons = new List<CSGPolygon>();
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        public CSGNode(CSGNode instance)
        {
            if (Plane != null)
            {
                Plane = new CSGPlane(instance.Plane);
            }

            if (instance.FrontChild != null)
            {
                FrontChild = new CSGNode(instance.FrontChild);
            }

            if (instance.BackChild != null)
            {
                BackChild = new CSGNode(instance.BackChild);
            }

            Polygons = new List<CSGPolygon>(instance.Polygons.Count);

            foreach (var polygon in instance.Polygons)
            {
                Polygons.Add(new CSGPolygon(polygon));
            }
        }

        /// <summary>
        /// inverse polygons (flip normals, reverse point order) in this node and all its children
        /// </summary>
        public void Invert()
        {
            foreach (var polygon in Polygons)
            {
                polygon.Flip();
            }

            Plane.Flip();

            if (FrontChild != null)
            {
                FrontChild.Invert();
            }

            if (BackChild != null)
            {
                BackChild.Invert();
            }

            MeshUtils.Swap(ref FrontChild, ref BackChild);
        }

        /// <summary>
        /// remove all polygons that are inside this subtree
        /// </summary>
        /// <param name="polygons">polygons to remove</param>
        private List<CSGPolygon> ClipPolygons(List<CSGPolygon> polygons)
        {
            if (Plane == null)
            {
                return new List<CSGPolygon>(polygons);
            }

            var frontList = new List<CSGPolygon>();
            var backList = new List<CSGPolygon>();

            foreach (var polygon in polygons)
            {
                Plane.Split(polygon, frontList, backList, frontList, backList);
            }

            if (FrontChild != null)
            {
                frontList = FrontChild.ClipPolygons(frontList);
            }
            if (BackChild != null)
            {
                backList = BackChild.ClipPolygons(backList);
            }
            else
            {
                backList.Clear();
            }

            var concat = new List<CSGPolygon>(frontList);
            concat.AddRange(backList);

            return concat;
        }

        /// <summary>
        /// remove all polygons from this tree that are inside the other tree
        /// </summary>
        public void ClipTo(CSGNode node)
        {
            Polygons = node.ClipPolygons(Polygons);

            if (FrontChild != null)
            {
                FrontChild.ClipTo(node);
            }

            if (BackChild != null)
            {
                BackChild.ClipTo(node);
            }
        }

        /// <summary>
        /// returns list of all polygons in this bsp tree
        /// </summary>
        /// <returns>list of polygons</returns>
        public List<CSGPolygon> AllPolygons()
        {
            var list = new List<CSGPolygon>();

            GetPolygons(list);

            return list;
        }

        /// <summary>
        /// recursive tree traversal helper for getting all polygons
        /// </summary>
        /// <param name="polygons">polygons list</param>
        private void GetPolygons(List<CSGPolygon> polygons)
        {
            polygons.AddRange(Polygons);

            if (BackChild != null)
            {
                BackChild.GetPolygons(polygons);
            }
            if (FrontChild != null)
            {
                FrontChild.GetPolygons(polygons);
            }
        }

        public void TestDepth()
        {
            MeshUtils.Log("All: " + Polygons.Count);

            if (BackChild != null)
            {
                BackChild.TestDepth();
            }

            if (FrontChild != null)
            {
                FrontChild.TestDepth();
            }
        }
 
        /// <summary>
        /// build new bsp tree from polygons
        /// </summary>
        /// <param name="polygons">input polygons</param>
        public void Build(List<CSGPolygon> polygons)
        {
            if (polygons.Count == 0)
            {
                return;
            }

            if (Plane == null)
            {
                Plane = new CSGPlane(polygons[0].Plane);
            }

            var frontList = new List<CSGPolygon>();
            var backList = new List<CSGPolygon>();

            foreach (var polygon in polygons)
            {
                Plane.Split(polygon, Polygons, Polygons, frontList, backList);
            }

            if (frontList.Count > 0)
            {
                if (FrontChild == null)
                {
                    FrontChild = new CSGNode();
                }
                FrontChild.Build(frontList);
            }

            if (backList.Count > 0)
            {
                if (BackChild == null)
                {
                    BackChild = new CSGNode();
                }
                BackChild.Build(backList);
            }
        }
	}
}
