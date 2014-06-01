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
    /// class for creating Ellipse primitive
    /// </summary>
    public class Ellipse : BaseObject
    {
        /// <summary>
        /// radius0 of the ellipse
        /// </summary>
        public float radius0;

        /// <summary>
        /// radius1 of the ellipse
        /// </summary>
        public float radius1;

        /// <summary>
        /// number of spere segments
        /// </summary>
        public int segments;

        /// <summary>
        /// create Ellipse game object
        /// </summary>
        /// <param name="radius0">radius0 of ellipse</param>
        /// <param name="radius1">radius1 of ellipse</param>
        /// <param name="segments">number of segments</param>
        /// <returns>Ellipse game object</returns>
        public static Ellipse Create(float radius0, float radius1, int segments)
        {
            var sphereObject = new GameObject("EllipsePro");

            sphereObject.AddComponent<MeshFilter>();
            var renderer = sphereObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var ellipse = sphereObject.AddComponent<Ellipse>();
            ellipse.GenerateGeometry(radius0, radius1, segments);

            return ellipse;
        }

        /// <summary>
        /// create Ellipse game object
        /// </summary>
        /// <param name="radius0">radius0 of ellipse</param>
        /// <param name="radius1">radiu1 of ellipse</param>
        /// <param name="segments">number of segments</param>
        public void GenerateGeometry(float radius0, float radius1, int segments)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.EllipsePrimitive.GenerateGeometry(mesh, radius0, radius1, segments);

            this.radius0 = radius0;
            this.radius1 = radius1;
            this.segments = segments;
            this.flipNormals = false;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(radius0, radius1, segments);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.EllipsePrimitive.GenerateGeometry(meshCollider, radius0, radius1, segments);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["radius0"] = radius0;
            dic["radius1"] = radius1;
            dic["segments"] = segments;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            radius0 = (float)dic["radius0"];
            radius1 = (float)dic["radius1"];
            segments = (int)dic["depth"];

            return dic;
        }

        /// <summary>
        /// helper to set width and length
        /// </summary>
        public override void SetWidth(float width0, float length0)
        {
            this.radius0 = width0*0.5f;
            this.radius1 = length0*0.5f;
        }
    }
}
