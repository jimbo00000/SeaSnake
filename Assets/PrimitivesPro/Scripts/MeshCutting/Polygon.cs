using System.Collections.Generic;
using UnityEngine;

namespace PrimitivesPro.MeshCutting
{
    /// <summary>
    /// represents a polygon
    /// </summary>
    public class Polygon
    {
        public Vector2[] Points;
        public readonly float Area;
        public Vector2 Min, Max;

        private readonly List<Polygon> holes; 

        /// <summary>
        /// c-tor
        /// </summary>
        /// <param name="pnts">points of the polygon</param>
        public Polygon(Vector2[] pnts)
        {
            MeshUtils.Assert(pnts.Length >= 3, "Invalid polygon!");

            Points = pnts;
            Area = GetArea();

            holes = new List<Polygon>();
        }

        /// <summary>
        /// compute area of the polygon
        /// </summary>
        /// <returns>area of the polygon</returns>
        public float GetArea()
        {
            Min.x = float.MaxValue;
            Min.y = float.MaxValue;
            Max.x = float.MinValue;
            Max.y = float.MinValue;

            int n = Points.Length;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                var pval = Points[p];
                var qval = Points[q];
                A += pval.x * qval.y - qval.x * pval.y;

                if (pval.x < Min.x)
                {
                    Min.x = pval.x;
                }
                if (pval.y < Min.y)
                {
                    Min.y = pval.y;
                }
                if (pval.x > Max.x)
                {
                    Max.x = pval.x;
                }
                if (pval.y > Max.y)
                {
                    Max.y = pval.y;
                }
            }

            return (A * 0.5f);
        }

        /// <summary>
        /// return true if a point is inside the polygon
        /// </summary>
        /// <param name="p">tested point</param>
        /// <returns>true if point is inside</returns>
        public bool IsPointInside(Vector3 p)
        {
            int numVerts = Points.Length;

            var p0 = Points[numVerts - 1];

            bool bYFlag0 = (p0.y >= p.y);
            var p1 = Vector2.zero;

            bool bInside = false;
            for (int j = 0; j < numVerts; ++j)
            {
                p1 = Points[j];
                bool bYFlag1 = (p1.y >= p.y);
                if (bYFlag0 != bYFlag1)
                {
                    if (((p1.y - p.y) * (p0.x - p1.x) >= (p1.x - p.x) * (p0.y - p1.y)) == bYFlag1)
                    {
                        bInside = !bInside;
                    }
                }

                // Move to the next pair of vertices, retaining info as possible.
                bYFlag0 = bYFlag1;
                p0 = p1;
            }

            return bInside;
        }

        /// <summary>
        /// quick test if another polygon is inside this polygon
        /// </summary>
        /// <param name="polygon">testing polygon</param>
        /// <returns></returns>
        public bool IsPolygonInside(Polygon polygon)
        {
            if (Area > polygon.Area)
            {
                return IsPointInside(polygon.Points[0]);
            }

            return false;
        }

        /// <summary>
        /// add hole (polygon inside this polygon)
        /// </summary>
        /// <param name="polygon">polygon representing the hole</param>
        public void AddHole(Polygon polygon)
        {
            holes.Add(polygon);
        }

        /// <summary>
        /// triangulate polygon using Poly2Tri library
        /// http://code.google.com/p/poly2tri/
        /// </summary>
        /// <returns>index list of triangle points</returns>
        public List<int> Triangulate()
        {
            var p2tPoints = new List<Poly2Tri.PolygonPoint>(Points.Length);

            foreach (var point in Points)
            {
                p2tPoints.Add(new Poly2Tri.PolygonPoint(point.x, point.y));
            }

            // create p2t polygon
            var p2tPolygon = new Poly2Tri.Polygon(p2tPoints);

            // add holes
            foreach (var polygonHole in holes)
            {
                var p2tHolePoints = new List<Poly2Tri.PolygonPoint>(polygonHole.Points.Length);

                foreach (var polygonPoint in polygonHole.Points)
                {
                    p2tHolePoints.Add(new Poly2Tri.PolygonPoint(polygonPoint.x, polygonPoint.y));
                }

                p2tPolygon.AddHole(new Poly2Tri.Polygon(p2tHolePoints));
            }

            Poly2Tri.P2T.Triangulate(p2tPolygon);

            var triangles = p2tPolygon.Triangles.Count;

            var indices = new List<int>(triangles*3);
            Points = new Vector2[triangles*3];
            var j = 0;

            for (int i = 0; i < triangles; i++)
            {
                indices.Add((j + 0));
                indices.Add((j + 1));
                indices.Add((j + 2));

                Points[j + 2].x = (float)p2tPolygon.Triangles[i].Points._0.X;
                Points[j + 2].y = (float)p2tPolygon.Triangles[i].Points._0.Y;

                Points[j + 1].x = (float)p2tPolygon.Triangles[i].Points._1.X;
                Points[j + 1].y = (float)p2tPolygon.Triangles[i].Points._1.Y;

                Points[j + 0].x = (float)p2tPolygon.Triangles[i].Points._2.X;
                Points[j + 0].y = (float)p2tPolygon.Triangles[i].Points._2.Y;

                j += 3;
            }

            return indices;
        }
    }
}
