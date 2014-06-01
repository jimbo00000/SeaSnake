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
    /// class for creating TorusKnot primitive
    /// </summary>
    public class TorusKnot : BaseObject
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
        /// number of triangle segments of torusKnot
        /// </summary>
        public int torusSegments;

        /// <summary>
        /// number of triangle segments of torusKnot cone
        /// </summary>
        public int coneSegments;

        /// <summary>
        /// knot parameter
        /// </summary>
        public int P;

        /// <summary>
        /// knot parameter
        /// </summary>
        public int Q;

        /// <summary>
        /// create TorusKnot game object
        /// </summary>
        /// <param name="radius0">first radius of tube</param>
        /// <param name="radius1">second radius of tube</param>
        /// <param name="torusSegments">number of triangle segments of torusKnot</param>
        /// <param name="coneSegments">number of triangle segments or torusKnot cone</param>
        /// <returns>TorusKnot class with TorusKnot game object</returns>
        /// <param name="P">knot parameter</param>
        /// <param name="Q">knot parameter</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static TorusKnot Create(float radius0, float radius1, int torusSegments, int coneSegments, int P, int Q, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var cylinderObject = new GameObject("TorusKnotPro");

            cylinderObject.AddComponent<MeshFilter>();
            var renderer = cylinderObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var torusKnot = cylinderObject.AddComponent<TorusKnot>();
            torusKnot.GenerateGeometry(radius0, radius1, torusSegments, coneSegments, P, Q, normalsType, pivotPosition);

            return torusKnot;
        }

        /// <summary>
        /// re/generate mesh geometry based on parameters
        /// </summary>
        /// <param name="radius0">fist radius of tube</param>
        /// <param name="radius1">second radius of tube</param>
        /// <param name="torusSegments">number of triangle of torusKnot</param>
        /// <param name="coneSegments">number of triangle of torusKnot cone</param>
        /// <param name="P">knot parameter</param>
        /// <param name="Q">knot parameter</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public void GenerateGeometry(float radius0, float radius1, int torusSegments, int coneSegments, int P, int Q, NormalsType normalsType, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.TorusKnotPrimitive.GenerateGeometry(mesh, radius0, radius1, torusSegments, coneSegments, P, Q, normalsType, pivotPosition);

            this.radius0 = radius0;
            this.radius1 = radius1;
            this.torusSegments = torusSegments;
            this.coneSegments = coneSegments;
            this.P = P;
            this.Q = Q;
            this.normalsType = normalsType;
            this.flipNormals = false;
            this.pivotPosition = pivotPosition;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(radius0, radius1, torusSegments, coneSegments, P, Q, normalsType, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.TorusKnotPrimitive.GenerateGeometry(meshCollider, radius0, radius1, torusSegments, coneSegments, P, Q, normalsType, pivotPosition);

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
            dic["P"] = P;
            dic["Q"] = Q;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            radius0 = (float)dic["radius0"];
            radius1 = (float)dic["radius1"];
            torusSegments = (int)dic["torusSegments"];
            coneSegments = (int)dic["coneSegments"];
            P = (int)dic["P"];
            Q = (int)dic["Q"];

            return dic;
        }

        /// <summary>
        /// helper to set height
        /// </summary>
        public override void SetHeight(float height)
        {
            this.radius0 = height*0.3f;
        }
    }
}
