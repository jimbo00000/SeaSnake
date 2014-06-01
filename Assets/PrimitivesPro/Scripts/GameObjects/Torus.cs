// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using PrimitivesPro.Primitives;
using UnityEngine;
using System.Collections;

namespace PrimitivesPro.GameObjects
{
    /// <summary>
    /// class for creating Torus primitive
    /// </summary>
    public class Torus : BaseObject
    {
        /// <summary>
        /// first radius of the tube
        /// </summary>
        public float radius0;

        /// <summary>
        /// second radius of the tube
        /// </summary>
        public float radius1;

        /// <summary>
        /// number of triangle segments of torus
        /// </summary>
        public int torusSegments;

        /// <summary>
        /// number of triangle segments of torus cone
        /// </summary>
        public int coneSegments;

        /// <summary>
        /// slice in range (0, 1)
        /// </summary>
        public float slice;

        /// <summary>
        /// create Torus game object
        /// </summary>
        /// <param name="radius0">first radius of tube</param>
        /// <param name="radius1">second radius of tube</param>
        /// <param name="torusSegments">number of triangle segments of torus</param>
        /// <param name="coneSegments">number of triangle segments or torus cone</param>
        /// <returns>Torus class with Torus game object</returns>
        /// <param name="slice">slice</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static Torus Create(float radius0, float radius1, int torusSegments, int coneSegments, float slice, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var cylinderObject = new GameObject("TorusPro");

            cylinderObject.AddComponent<MeshFilter>();
            var renderer = cylinderObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var torus = cylinderObject.AddComponent<Torus>();
            torus.GenerateGeometry(radius0, radius1, torusSegments, coneSegments, slice, normalsType, pivotPosition);

            return torus;
        }

        /// <summary>
        /// re/generate mesh geometry based on parameters
        /// </summary>
        /// <param name="radius0">fist radius of tube</param>
        /// <param name="radius1">second radius of tube</param>
        /// <param name="torusSegments">number of triangle of torus</param>
        /// <param name="coneSegments">number of triangle of torus cone</param>
        /// <param name="slice">slice</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public void GenerateGeometry(float radius0, float radius1, int torusSegments, int coneSegments, float slice, NormalsType normalsType, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.TorusPrimitive.GenerateGeometry(mesh, radius0, radius1, torusSegments, coneSegments, slice, normalsType, pivotPosition);

            this.radius0 = radius0;
            this.radius1 = radius1;
            this.torusSegments = torusSegments;
            this.coneSegments = coneSegments;
            this.normalsType = normalsType;
            this.slice = slice;
            this.flipNormals = false;
            this.pivotPosition = pivotPosition;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(radius0, radius1, torusSegments, coneSegments, slice, normalsType, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.TorusPrimitive.GenerateGeometry(meshCollider, radius0, radius1, torusSegments, coneSegments, slice, normalsType, pivotPosition);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["radius0"] = radius0;
            dic["radius1"] = radius1;
            dic["torusSegments"] = torusSegments;
            dic["coneSegments"] = coneSegments;
            dic["slice"] = slice;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            radius0 = (float)dic["radius0"];
            radius1 = (float)dic["radius1"];
            torusSegments = (int)dic["torusSegments"];
            coneSegments = (int)dic["coneSegments"];
            slice = (int) dic["slice"];

            return dic;
        }

        /// <summary>
        /// helper to set height
        /// </summary>
        public override void SetHeight(float height)
        {
            this.radius0 = height;
        }

        /// <summary>
        /// helper to set width and length
        /// </summary>
        public override void SetWidth(float width0, float length0)
        {
        }
    }
}
