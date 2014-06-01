// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PrimitivesPro
{
    public static class MeshUtils
	{
        /// <summary>
        /// Calculate Tangents of the mesh
        /// Derived from
        /// Lengyel, Eric. Computing Tangent Space Basis Vectors for an Arbitrary Mesh. Terathon Software 3D Graphics Library, 2001.
        /// http://www.terathon.com/code/tangent.html
        /// </summary>
        /// <param name="mesh">mesh to process</param>
        public static void CalculateTangents(Mesh mesh)
        {
            var vertexCount = mesh.vertexCount;
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var texcoords = mesh.uv;
            var triangles = mesh.triangles;
            var triangleCount = triangles.Length / 3;
            var tangents = new Vector4[vertexCount];
            var tan1 = new Vector3[vertexCount];
            var tan2 = new Vector3[vertexCount];
            var tri = 0;

            for (var i = 0; i < triangleCount; i++)
            {
                var i1 = triangles[tri];
                var i2 = triangles[tri + 1];
                var i3 = triangles[tri + 2];

                var v1 = vertices[i1];
                var v2 = vertices[i2];
                var v3 = vertices[i3];

                var w1 = texcoords[i1];
                var w2 = texcoords[i2];
                var w3 = texcoords[i3];

                var x1 = v2.x - v1.x;
                var x2 = v3.x - v1.x;
                var y1 = v2.y - v1.y;
                var y2 = v3.y - v1.y;
                var z1 = v2.z - v1.z;
                var z2 = v3.z - v1.z;

                var s1 = w2.x - w1.x;
                var s2 = w3.x - w1.x;
                var t1 = w2.y - w1.y;
                var t2 = w3.y - w1.y;

                float div = s1 * t2 - s2 * t1;
                float r = Math.Abs(div - 0.0f) < 0.0001f ? 0.0f : 1.0f / div;
                var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;

                tri += 3;
            }

            for (var i = 0; i < (vertexCount); i++)
            {
                var n = normals[i];
                var t = tan1[i];

                // Gram-Schmidt orthogonalize
                Vector3.OrthoNormalize(ref n, ref t);

                tangents[i].x = t.x;
                tangents[i].y = t.y;
                tangents[i].z = t.z;

                // Calculate handedness
                tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0) ? -1.0f : 1.0f;
            }

            mesh.tangents = tangents;
        }

        /// <summary>
        /// Reverse normals of the mesh
        /// http://wiki.unity3d.com/index.php?title=ReverseNormals
        /// </summary>
        /// <param name="mesh">mesh to process</param>
        public static void ReverseNormals(Mesh mesh)
        {
            var normals = mesh.normals;

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i];
            }

            mesh.normals = normals;

            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                var triangles = mesh.GetTriangles(m);

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }

                mesh.SetTriangles(triangles, m);
            }
        }

        /// <summary>
        /// compute vertex normals for smooth shading
        /// this is equivalent to Unity's mesh.RecalculateNormals
        /// </summary>
        /// <param name="vertices">vertices array</param>
        /// <param name="triangles">triangles array</param>
        /// <param name="normals">normals to return</param>
        public static void ComputeVertexNormals(Vector3[] vertices, int[] triangles, out Vector3[] normals)
        {
            normals = new Vector3[vertices.Length];

            for (int i = 0; i < triangles.Length; i += 3)
            {
                var a = vertices[triangles[i + 0]];
                var b = vertices[triangles[i + 1]];
                var c = vertices[triangles[i + 2]];

                // calculate triangle normal
                var normal = Vector3.Cross(b - a, c - a);

                normals[triangles[i + 0]] += normal;
                normals[triangles[i + 1]] += normal;
                normals[triangles[i + 2]] += normal;
            }

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i].Normalize();
            }
        }

        /// <summary>
        /// compute normal vector of polygon (a, b, c)
        /// </summary>
        /// <param name="a">point of a polygon</param>
        /// <param name="b">point of a polygon</param>
        /// <param name="c">point of a polygon</param>
        /// <returns>polygon normal</returns>
        public static Vector3 ComputePolygonNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(b - a, c - a).normalized;
        }

	    /// <summary>
	    /// duplicate all shared vertices so each triangle has unique vertices
	    /// this method overwrite vertices and uvs array!
	    /// </summary>
	    /// <param name="vertices">vertices array</param>
	    /// <param name="uvs">texture coordinate array</param>
	    /// <param name="triangles">triangle indices array</param>
	    /// <param name="trianglesMax">specify maximum number of triangles to duplicate
	    /// by default all triangles will be duplicated</param>
	    public static void DuplicateSharedVertices(ref Vector3[] vertices, ref Vector2[] uvs, int[] triangles, int trianglesMax)
	    {
	        var verticesDup = new Vector3[triangles.Length];
	        var uvsDup = new Vector2[triangles.Length];

            if (trianglesMax == -1)
            {
                trianglesMax = triangles.Length;
            }

	        var vertIdx = 0;

	        for (int i = 0; i < trianglesMax; i += 3)
	        {
	            verticesDup[vertIdx + 0] = vertices[triangles[i + 0]];
	            verticesDup[vertIdx + 1] = vertices[triangles[i + 1]];
	            verticesDup[vertIdx + 2] = vertices[triangles[i + 2]];

	            uvsDup[vertIdx + 0] = uvs[triangles[i + 0]];
	            uvsDup[vertIdx + 1] = uvs[triangles[i + 1]];
	            uvsDup[vertIdx + 2] = uvs[triangles[i + 2]];

	            triangles[i + 0] = vertIdx + 0;
	            triangles[i + 1] = vertIdx + 1;
	            triangles[i + 2] = vertIdx + 2;

	            vertIdx += 3;
	        }

            for (int i = trianglesMax; i < triangles.Length; i++)
            {
                verticesDup[i] = vertices[triangles[i]];
                uvsDup[i] = uvs[triangles[i]];
            }

            vertices = verticesDup;
	        uvs = uvsDup;
	    }

        /// <summary>
        /// Compute barycentric coordinates (u, v, w) for point p with respect to triangle (a, b, c)
        /// from Real-Time Collision Detection Book by Christer Ericson
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Vector3 ComputeBarycentricCoordinates(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;
            
            var v = (d11 * d20 - d01 * d21) / denom;
            var w = (d00 * d21 - d01 * d20) / denom;
            var u = 1.0f - v - w;

            return new Vector3(u, v, w);
        }

        /// <summary>
        /// adjust vertices around the centroid
        /// </summary>
        /// <param name="vertices">list of vertices</param>
        /// <param name="centroid">centroid position</param>
        public static void CenterPivot(Vector3[] vertices, Vector3 centroid)
        {
            int count = vertices.Length;
            for (int i=0; i<count; i++)
            {
                var val = vertices[i];

                val.x -= centroid.x;
                val.y -= centroid.y;
                val.z -= centroid.z;

                vertices[i] = val;
            }
        }

        /// <summary>
        /// swap
        /// </summary>
        public static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

   		/// <summary>
        /// duplicate the mesh
        /// </summary>
        /// <param name="orginalMesh">mesh to diplicate</param>
        /// <returns>duplicated mesh</returns>
        public static Mesh CopyMesh(Mesh orginalMesh)
        {
            var newmesh = new Mesh
            {
                vertices = orginalMesh.vertices,
                triangles = orginalMesh.triangles,
                uv = orginalMesh.uv,
                normals = orginalMesh.normals,
                colors = orginalMesh.colors,
                tangents = orginalMesh.tangents
            };

            return newmesh;
        }

        /// <summary>
        /// duplicate all materials from the object
        /// </summary>
        /// <param name="materials">materials to duplicate</param>
        /// <returns>duplicated materials</returns>
        public static Material[] CopyMaterials(Material[] materials)
        {
            var newMat = new Material[materials.Length];

            for (int i=0; i<materials.Length; i++)
            {
                newMat[i] = new Material(materials[i]);
            }

            return newMat;
        }

        /// <summary>
        /// compute point on quadratic bezier curve with respect to start point p0, end point p1, control point cp and time t
        /// </summary>
        public static Vector3 BezierQuadratic(Vector3 p0, Vector3 p1, Vector3 cp, float t)
        {
            return (1.0f - t) * (1.0f - t) * p0 + 2.0f * (1.0f - t) * t * cp + t * t * p1;
        }

        /// <summary>
        /// just assert ...
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                UnityEngine.Debug.LogError("Assert! " + message);
                UnityEngine.Debug.Break();
            }
        }

        /// <summary>
        /// unity log
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        /// <summary>
        /// unity version specific isActive (to suppress errors)
        /// </summary>
        public static bool IsGameObjectActive(GameObject obj)
        {
#if !(UNITY_2_6	|| UNITY_2_6_1 || UNITY_3_0	|| UNITY_3_0_0 || UNITY_3_1	|| UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
            return obj.activeSelf;
#else
            return obj.active;
#endif
        }

        /// <summary>
        /// unity version specific SetActive (to suppress errors)
        /// </summary>
        public static void SetGameObjectActive(GameObject obj, bool status)
        {
#if !(UNITY_2_6	|| UNITY_2_6_1 || UNITY_3_0	|| UNITY_3_0_0 || UNITY_3_1	|| UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
            obj.SetActive(status);
#else
            obj.active = status;
#endif
        }
	}
}
