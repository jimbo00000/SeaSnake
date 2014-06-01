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
    class CSGPlane
    {
        /// <summary>
        /// tolerance distance epsylon for points on plane
        /// meaning points with distance less then epsylon from plane are "on the plane"
        /// </summary>
        private const float epsylon = 0.000001f;

        /// <summary>
        /// normal of the plane
        /// Points x on the plane satisfy Dot(n,x) = d
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// distance of the plane
        /// d = dot(n,p) for a given point p on the plane
        /// </summary>
        public float Distance;

        /// <summary>
        /// 3 points constructor
        /// </summary>
	    public CSGPlane(Vector3 a, Vector3 b, Vector3 c)
	    {
            Normal = (Vector3.Cross(b - a, c - a)).normalized;
            Distance = Vector3.Dot(Normal, a);
	    }

        /// <summary>
        /// normal, point constructor
        /// </summary>
        public CSGPlane(Vector3 normal, Vector3 p)
        {
            Normal = normal.normalized;
            Distance = Vector3.Dot(Normal, p);
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        public CSGPlane(CSGPlane instance)
        {
            Normal = instance.Normal;
            Distance = instance.Distance;
        }

        /// <summary>
        /// classification of the point with this plane
        /// </summary>
        [Flags]
        public enum PointClass
        {
            Coplanar = 0,
            Front = 1,
            Back = 2,
            Intersection = 3,
        }

        /// <summary>
        /// classify point
        /// </summary>
        public Plane.PointClass ClassifyPoint(Vector3 p)
        {
            var dot = Vector3.Dot(p, Normal) - Distance;
            return (dot < -epsylon) ? Plane.PointClass.Back : (dot > epsylon) ? Plane.PointClass.Front : Plane.PointClass.Coplanar;
        }

        /// <summary>
        /// flip normal
        /// </summary>
        public void Flip()
        {
            Normal = -Normal;
            Distance = -Distance;
        }

        /// <summary>
        /// split polygon by plane and push the results to polygon lists
        /// </summary>
        /// <param name="polygon">polygon to split</param>
        /// <param name="coplanarFront">list of coplanar polygons in front plane</param>
        /// <param name="coplanarBack">list of coplanar polygons in back plane</param>
        /// <param name="front">list of polygons in front plane</param>
        /// <param name="back">list of polygons in back plane</param>
        public void Split(CSGPolygon polygon, List<CSGPolygon> coplanarFront, List<CSGPolygon> coplanarBack, List<CSGPolygon> front, List<CSGPolygon> back)
        {
            var verticesCount = polygon.Vertices.Count;

            var classes = new Plane.PointClass[verticesCount];
            var polygonClass = Plane.PointClass.Coplanar;

            for (int i = 0; i < verticesCount; i++)
            {
                var t = Vector3.Dot(Normal, polygon.Vertices[i].P) - Distance;
                var type = (t < -epsylon
                                ? Plane.PointClass.Back
                                : (t > epsylon ? Plane.PointClass.Front : Plane.PointClass.Coplanar));
                //classes[i] = ClassifyPoint(polygon.Vertices[i].P);

                polygonClass |= type; //classes[i];
                classes[i] = type;
            }

            switch (polygonClass)
            {
                case Plane.PointClass.Coplanar:
                    {
                        if (Vector3.Dot(Normal, polygon.Plane.Normal) > 0)
                        {
                            coplanarFront.Add(polygon);
                        }
                        else
                        {
                            coplanarBack.Add(polygon);
                        }
                    }
                    break;

                case Plane.PointClass.Front:
                    {
                        front.Add(polygon);
                    }
                    break;

                case Plane.PointClass.Back:
                    {
                        back.Add(polygon);
                    }
                    break;

                case Plane.PointClass.Intersection:
                    {
                        var frontList = new List<CSGVertex>(4);
                        var backList = new List<CSGVertex>(4);

                        for (int i = 0; i < verticesCount; i++)
                        {
                            var j = (i + 1) % verticesCount;

                            var class0 = classes[i];
                            var class1 = classes[j];

                            var v0 = polygon.Vertices[i];
                            var v1 = polygon.Vertices[j];

                            if (class0 != Plane.PointClass.Back)
                            {
                                frontList.Add(v0);
                            }

                            if (class0 != Plane.PointClass.Front)
                            {
                                backList.Add(class0 != Plane.PointClass.Back ? new CSGVertex(v0) : v0);
                            }

                            if ((class0 | class1) == Plane.PointClass.Intersection)
                            {
                                var q = new CSGVertex(polygon.Vertices[0]);

                                // find intersection point
                                //IntersectSegment(v0.P, v1.P, out t, out q.P);
                                var t = (this.Distance - Vector3.Dot(this.Normal, v0.P)) / Vector3.Dot(this.Normal, v1.P - v0.P);
                                
                                // interpolate
                                q.P = Vector3.Lerp(v0.P, v1.P, t);
                                q.N = Vector3.Lerp(v0.N, v1.N, t);
                                q.UV = Vector2.Lerp(v0.UV, v1.UV, t);

                                frontList.Add(q);
                                backList.Add(new CSGVertex(q));
                            }
                        }

                        if (frontList.Count >= 3)
                        {
                            front.Add(new CSGPolygon(polygon.Id, frontList));
                        }

                        if (backList.Count >= 3)
                        {
                            back.Add(new CSGPolygon(polygon.Id, backList));
                        }
                    }
                    break;
            }
        }
    }
}
