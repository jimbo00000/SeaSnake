// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Collections.Generic;
using UnityEngine;

namespace PrimitivesPro.MeshCutting
{
    public class ContourData
    {
        /// <summary>
        /// list of contours
        /// </summary>
        private readonly List<Vector3[]> contours;

        private Matrix4x4 transform;

        public ContourData(List<Vector3[]> contourList, Transform transformData)
        {
            contours = contourList;
            transform = new Matrix4x4();
            transform.SetTRS(transformData.position, transformData.rotation, transformData.localScale);
        }

        /// <summary>
        /// get list of contours in local position with respect to original cutting object
        /// </summary>
        public List<Vector3[]> GetLocalContours()
        {
            return contours;
        }

        /// <summary>
        /// get list of contours in world coordinates
        /// </summary>
        public List<Vector3[]> GetWorldContours()
        {
            var list = new List<Vector3[]>(contours.Count);

            for (int i = 0; i < contours.Count; i++)
            {
                var array = new Vector3[contours[i].Length];

                for (int j = 0; j < array.Length; j++)
                {
                    array[j] = TransformPoint(contours[i][j]);
                }

                list.Add(array);
            }

            return list;
        }

        /// <summary>
        /// create game object from contour data
        /// </summary>
        /// <param name="doubleSide">flag whether to fill polygons on both side of the plane</param>
        public GameObject CreateGameObject(bool doubleSide)
        {
            if (contours.Count == 0 || contours[0].Length < 3)
            {
                return null;
            }

            var wContour = GetWorldContours();

            var polygons = new List<Polygon>(wContour.Count);
            var mesh = new Mesh();
            var triangles = new List<int>();
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triCounter = 0;

            var plane = new Utils.Plane(GetNormal(wContour[0]), wContour[0][0]);
            var m = plane.GetPlaneMatrix();
            var mInv = m.inverse;
            var zWorld = (mInv * wContour[0][0]).z;

            foreach (var contour in wContour)
            {
                var contour2d = new Vector2[contour.Length];

                for (int i=0; i<contour.Length; i++)
                {
                    contour2d[i] = mInv * contour[i];
                }

                polygons.Add(new Polygon(contour2d));
            }

            CollapsePolygons(polygons);

            foreach (var polygon in polygons)
            {
                var triList = polygon.Triangulate();
                var min = Mathf.Min(polygon.Min.x, polygon.Min.y);
                var max = Mathf.Max(polygon.Max.x, polygon.Max.y);
                var polygonSize = min - max;

                foreach (var p in polygon.Points)
                {
                    Vector3 pWorld = m * new Vector3(p.x, p.y, zWorld);
                    vertices.Add(pWorld);
                    normals.Add(plane.Normal);
                    uvs.Add(new Vector2((p.x - min) / polygonSize, (p.y - min) / polygonSize));
                }

                foreach (var i in triList)
                {
                    triangles.Add(i + triCounter);
                }

                triCounter += triList.Count;
            }

            if (doubleSide)
            {
                var count = vertices.Count;

                for (int i = 0; i < count; i++)
                {
                    vertices.Add(vertices[i]);
                    normals.Add(-normals[0]);
                    uvs.Add(uvs[i]);
                }

                count = triangles.Count;

                for (int i = 0; i < count; i++)
                {
                    triangles.Add(triangles[count-i-1]);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.normals = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = triangles.ToArray();

            var co = GameObject.Find("ContourObject");
            if (co)
            {
                Object.Destroy(co);
            }

            var obj = new GameObject("ContourObject");
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));

            return obj;
        }

        private Vector3 TransformPoint(Vector3 localPos)
        {
            return transform.MultiplyPoint3x4(localPos);
        }

        private static void CollapsePolygons(List<Polygon> polygon)
        {
            if (polygon.Count > 0)
            {
                bool collapseFound = true;

                while (collapseFound)
                {
                    collapseFound = false;

                    for (int i = 0; i < polygon.Count; i++)
                    {
                        if (collapseFound)
                            break;

                        for (int j = 0; j < polygon.Count; j++)
                        {
                            if (collapseFound)
                                break;

                            if (i != j)
                            {
                                var p0 = polygon[i];
                                var p1 = polygon[j];

                                if (p0.IsPolygonInside(p1))
                                {
                                    p0.AddHole(p1);
                                    polygon.Remove(p1);
                                    collapseFound = true;
                                }

                                if (p1.IsPolygonInside(p0))
                                {
                                    p1.AddHole(p0);
                                    polygon.Remove(p0);
                                    collapseFound = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// display contour data using Unity debug line
        /// </summary>
        public void ShowContourDBG(float duration)
        {
            var list = GetWorldContours();

            foreach (var contour in list)
            {
                var prevPoint = contour[0];

                for (int i = 1; i < contour.Length; i++)
                {
                    UnityEngine.Debug.DrawLine(prevPoint, contour[i], Color.red, duration);
                    prevPoint = contour[i];
                }

                UnityEngine.Debug.DrawLine(prevPoint, contour[0], Color.red, duration);

                GetNormal(list[0]);
            }
        }

        static Vector3 GetNormal(Vector3[] points)
        {
            var p0 = points[0];
            var p1 = points[1];

            const float epsylon = 0.01f;

            int i = 1;
            while ((p0 - p1).sqrMagnitude < epsylon)
            {
                p1 = points[i++];

                if (i == points.Length)
                {
                    MeshUtils.Assert(false, "All points are collinear!");
                    return Vector3.zero;
                }
            }

            var p2 = points[i];
            while ((p0 - p2).sqrMagnitude < epsylon || (p1 - p2).sqrMagnitude < epsylon)
            {
                p2 = points[i++];

                if (i == points.Length)
                {
                    MeshUtils.Assert(false, "All points are collinear!");
                    return Vector3.zero;
                }
            }

            var normal = MeshUtils.ComputePolygonNormal(p0, p1, p2);

            if (normal.sqrMagnitude < epsylon)
            {
                MeshUtils.Assert(false, "All points are collinear!");
                return Vector3.zero;
            }

            return normal;
        }
    }
}
