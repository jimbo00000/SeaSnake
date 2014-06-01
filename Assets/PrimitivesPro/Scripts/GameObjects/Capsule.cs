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
    /// class for creating Capsule primitive
    /// </summary>
    public class Capsule : BaseObject
    {
        /// <summary>
        /// radius of the capsule
        /// </summary>
        public float radius;

        /// <summary>
        /// height of the capsule (height of the central cylinder)
        /// </summary>
        public float height;

        /// <summary>
        /// number of capsule sides
        /// </summary>
        public int sides;

        /// <summary>
        /// number of segments of central cylinder
        /// </summary>
        public int heightSegments;

        /// <summary>
        /// flag for keeping height when changing radius
        /// </summary>
        public bool preserveHeight;

        /// <summary>
        /// create Capsule game object
        /// </summary>
        /// <param name="radius">radius of capsule</param>
        /// <param name="sides">number of segments</param>
        /// <param name="heightSegments">number of segments of central cylinder</param>
        /// <param name="normals">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        /// <returns>Capsule class with Capsule game object</returns>
        public static Capsule Create(float radius, float height, int sides, int heightSegments, bool preserveHeight, NormalsType normals, PivotPosition pivotPosition)
        {
            var capsuleObject = new GameObject("CapsulePro");

            capsuleObject.AddComponent<MeshFilter>();
            var renderer = capsuleObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var capsule = capsuleObject.AddComponent<Capsule>();
            capsule.GenerateGeometry(radius, height, sides, heightSegments, preserveHeight, normals, pivotPosition);

            return capsule;
        }

        /// <summary>
        /// create Capsule game object
        /// </summary>
        /// <param name="radius">radius of capsule</param>
        /// <param name="sides">number of segments</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="heightSegments">number of segments of central cylinder</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public void GenerateGeometry(float radius, float height, int sides, int heightSegments, bool preserverHeight, NormalsType normalsType, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.CapsulePrimitive.GenerateGeometry(mesh, radius, height, sides, heightSegments, preserverHeight, normalsType, pivotPosition);

            this.radius = radius;
            this.height = height;
            this.heightSegments = heightSegments;
            this.sides = sides;
            this.preserveHeight = preserverHeight;
            this.normalsType = normalsType;
            this.flipNormals = false;
            this.pivotPosition = pivotPosition;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(radius, height, sides, heightSegments, preserveHeight, normalsType, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.CapsulePrimitive.GenerateGeometry(meshCollider, radius, height, sides, heightSegments, preserveHeight, normalsType, pivotPosition);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["radius"] = radius;
            dic["height"] = height;
            dic["sides"] = sides;
            dic["heightSegments"] = heightSegments;
            dic["preserveHeight"] = preserveHeight;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            radius = (float)dic["radius"];
            height = (float)dic["height"];
            sides = (int)dic["sides"];
            heightSegments = (int)dic["heightSegments"];
            preserveHeight = (bool) dic["preserveHeight"];

            return dic;
        }

        /// <summary>
        /// helper to set height
        /// </summary>
        public override void SetHeight(float height)
        {
            if (preserveHeight)
            {
                this.height = height;
            }
            else
            {
                this.height = height - radius*2;
            }
        }

        /// <summary>
        /// helper to set width and length
        /// </summary>
        public override void SetWidth(float width0, float length0)
        {
            var r0 = width0*0.5f;
            var r1 = length0*0.5f;

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
