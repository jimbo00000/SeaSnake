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
    /// class for creating Ring primitive
    /// </summary>
    public class Ring : BaseObject
    {
        /// <summary>
        /// radius0 of the ring
        /// </summary>
        public float radius0;

        /// <summary>
        /// radius1 of the ring
        /// </summary>
        public float radius1;

        /// <summary>
        /// number of spere segments
        /// </summary>
        public int segments;

        /// <summary>
        /// create Ring game object
        /// </summary>
        /// <param name="radius0">radius0 of ring</param>
        /// <param name="radius1">radius1 of ring</param>
        /// <param name="segments">number of segments</param>
        /// <returns>Ring game object</returns>
        public static Ring Create(float radius0, float radius1, int segments)
        {
            var sphereObject = new GameObject("RingPro");

            sphereObject.AddComponent<MeshFilter>();
            var renderer = sphereObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var ring = sphereObject.AddComponent<Ring>();
            ring.GenerateGeometry(radius0, radius1, segments);

            return ring;
        }

        /// <summary>
        /// create Ring game object
        /// </summary>
        /// <param name="radius0">radius0 of ring</param>
        /// <param name="radius1">radiu1 of ring</param>
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
            GenerationTimeMS = Primitives.RingPrimitive.GenerateGeometry(mesh, radius0, radius1, segments);

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

                Primitives.RingPrimitive.GenerateGeometry(meshCollider, radius0, radius1, segments);

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

            var radius = Mathf.Max(radius0, radius1);

            if (Mathf.Abs(r0 - radius) > Mathf.Abs(r1 - radius))
            {
                radius = r0;
            }
            else
            {
                radius = r1;
            }

            var diff = radius0 - radius1;

            if (radius0 > radius1)
            {
                radius0 = radius;
                radius1 = radius0 - diff;
            }
            else
            {
                radius1 = radius;
                radius0 = radius1 + diff;
            }
        }
    }
}
