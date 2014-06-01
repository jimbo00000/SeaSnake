// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using UnityEngine;

namespace PrimitivesPro.GameObjects
{
    /// <summary>
    /// class for creating Circle primitive
    /// </summary>
    public class Circle : BaseObject
    {
        /// <summary>
        /// radius of the circle
        /// </summary>
        public float radius;

        /// <summary>
        /// number of spere segments
        /// </summary>
        public int segments;

        /// <summary>
        /// create Circle game object
        /// </summary>
        /// <param name="radius">radius of circle</param>
        /// <param name="segments">number of segments</param>
        /// <returns>Circle class with Circle game object</returns>
        public static Circle Create(float radius, int segments)
        {
            var sphereObject = new GameObject("CirclePro");

            sphereObject.AddComponent<MeshFilter>();
            var renderer = sphereObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var circle = sphereObject.AddComponent<Circle>();
            circle.GenerateGeometry(radius, segments);

            return circle;
        }

        /// <summary>
        /// create Circle game object
        /// </summary>
        /// <param name="radius">radius of circle</param>
        /// <param name="segments">number of segments</param>
        public void GenerateGeometry(float radius, int segments)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.EllipsePrimitive.GenerateGeometry(mesh, radius, radius, segments);

            this.radius = radius;
            this.segments = segments;
            this.flipNormals = false;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(radius, segments);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.EllipsePrimitive.GenerateGeometry(meshCollider, radius, radius, segments);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["radius"] = radius;
            dic["segments"] = segments;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            radius = (float)dic["radius"];
            segments = (int)dic["segments"];

            return dic;
        }

        /// <summary>
        /// helper to set width and length
        /// </summary>
        public override void SetWidth(float width0, float length0)
        {
            var r0 = width0 * 0.5f;
            var r1 = length0 * 0.5f;

            if (Mathf.Abs(r0 - radius) > Mathf.Abs(r1 - radius))
            {
                radius = r0;
            }
            else
            {
                radius = r1;
            }
        }
    }
}
