// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using UnityEngine;

namespace PrimitivesPro.Utils
{
    public class Plane
    {
        /// <summary>
        /// tolerance distance epsylon for points on plane
        /// meaning points with distance less then epsylon from plane are "on the plane"
        /// </summary>
        private const float epsylon = 0.0001f;

        /// <summary>
        /// normal of the plane
        /// Points x on the plane satisfy Dot(n,x) = d
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// one of the creation point on plane (this is just for debugging)
        /// </summary>
        public Vector3 Pnt { get; private set; }

        /// <summary>
        /// distance of the plane
        /// d = dot(n,p) for a given point p on the plane
        /// </summary>
        public float Distance;

        /// <summary>
        /// 3 points constructor
        /// </summary>
	    public Plane(Vector3 a, Vector3 b, Vector3 c)
	    {
            Normal = (Vector3.Cross(b - a, c - a)).normalized;
            Distance = Vector3.Dot(Normal, a);

            Pnt = a;
	    }

        /// <summary>
        /// normal, point constructor
        /// </summary>
        public Plane(Vector3 normal, Vector3 p)
        {
            Normal = normal.normalized;
            Distance = Vector3.Dot(Normal, p);

            Pnt = p;
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        public Plane(Plane instance)
        {
            Normal = instance.Normal;
            Distance = instance.Distance;
            Pnt = instance.Pnt;
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
        public PointClass ClassifyPoint(Vector3 p)
        {
            var dot = Vector3.Dot(p, Normal) - Distance;
            return (dot < -epsylon) ? PointClass.Back : (dot > epsylon) ? PointClass.Front : PointClass.Coplanar;
        }

        /// <summary>
        /// test positive or negative side of the point n
        /// </summary>
        public bool GetSide(Vector3 n)
        {
            return Vector3.Dot(n, Normal) - Distance > epsylon;
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
        /// hack for collinear points
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool GetSideFix(ref Vector3 n)
        {
            var dot = Vector3.Dot(n, Normal) - Distance;

            var sign = 1.0f;
            var abs = dot;
            if (dot < 0)
            {
                sign = -1.0f;
                abs = -dot;
            }

            if (abs < epsylon + 0.001f)
            {
                n.x += Normal.x*0.001f*sign;
                n.y += Normal.y*0.001f*sign;
                n.z += Normal.z*0.001f*sign;

                n += Normal*0.001f*Mathf.Sign(dot);
            }

            return Vector3.Dot(n, Normal) - Distance > epsylon;
        }

        /// <summary>
        /// returns true if two points are on the same side of the plane
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool SameSide(Vector3 a, Vector3 b)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compute intersection between a segment line (a, b) and a plane (p)
        /// from Real-Time Collision Detection Book by Christer Ericson
        /// </summary>
        /// <param name="a">first point of a segment</param>
        /// <param name="b">second point of a segment</param>
        /// <param name="t">normalized distance of intersection point on vector (ab)</param>
        /// <param name="q">point in intersection</param>
        /// <returns>true if there is an intersection</returns>
        public bool IntersectSegment(Vector3 a, Vector3 b, out float t, out Vector3 q)
        {
            // Compute the t value for the directed line ab intersecting the plane
            var ab = b - a;

            if (Mathf.Abs(Vector3.Dot(Normal, ab)) < epsylon)
            {
                MeshUtils.Log("Plane intersection below epsylon");
            }

            t = (Distance - Vector3.Dot(Normal, a)) / Vector3.Dot(Normal, ab);

            // If t in [0..1] compute and return intersection point
            if (t >= 0.0f-epsylon && t <= 1.0f+epsylon)
            {
                q = a + t * ab;
                return true;
            }

            MeshUtils.Log("IntersectSegment failed: " + t);
            q = Vector3.zero;
            return false;
        }

        /// <summary>
        /// make inverse transformation of this plane to target space
        /// </summary>
        /// <param name="transform">target transformation space</param>
        public void InverseTransform(Transform transform)
        {
            // inverse transform normal
            var inverseNormal = transform.InverseTransformDirection(Normal);

            // inverse transform point
            var inversePoint = transform.InverseTransformPoint(Pnt);

            // update plane
            Normal = inverseNormal;
            Distance = Vector3.Dot(inverseNormal, inversePoint);
        }

        /// <summary>
        /// make inverse transformation of this plane to target space
        /// </summary>
        /// <param name="matrix">target transformation space matrix</param>
        public void InverseTransform(Matrix4x4 matrix)
        {
            var inv = matrix.inverse;

            // inverse transform normal
            var inverseNormal = inv.MultiplyVector(Normal);

            // inverse transform point
            var inversePoint = inv.MultiplyPoint(Pnt);

            // update plane
            Normal = inverseNormal;
            Distance = Vector3.Dot(inverseNormal, inversePoint);
        }

        public Matrix4x4 GetPlaneMatrix()
        {
            var m = new Matrix4x4();

            Quaternion rot = Quaternion.LookRotation(Normal);

            m.SetTRS(Pnt, rot, Vector3.one);

            return m;
        }
	}
}
