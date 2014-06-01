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
    /// class for creating Cone primitive
    /// </summary>
    public class Cone : BaseObject
    {
        /// <summary>
        /// first radius of the cone
        /// </summary>
        public float radius0;

        /// <summary>
        /// second radius of the cone
        /// </summary>
        public float radius1;

        /// <summary>
        /// thickness of the cone (0, 1)
        /// </summary>
        public float thickness;

        /// <summary>
        /// height of the cone
        /// </summary>
        public float height;

        /// <summary>
        /// number of triangle segments in radius direction
        /// </summary>
        public int sides;

        /// <summary>
        /// number of triangle segments in height direction
        /// </summary>
        public int heightSegments;

        /// <summary>
        /// create Cone game object
        /// </summary>
        /// <param name="radius0">first radius of cone</param>
        /// <param name="radius1">second radius of cone</param>
        /// <param name="thickness">thickness</param>
        /// <param name="height">height of cone</param>
        /// <param name="sides">number of triangle segments in radius direction</param>
        /// <param name="heightSegments">number of triangle segments in height direction</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        /// <returns>Cone class with Cone game object</returns>
        public static Cone Create(float radius0, float radius1, float thickness, float height, int sides, int heightSegments, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var cylinderObject = new GameObject("ConePro");

            cylinderObject.AddComponent<MeshFilter>();
            var renderer = cylinderObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var cone = cylinderObject.AddComponent<Cone>();
            cone.GenerateGeometry(radius0, radius1, thickness, height, sides, heightSegments, normalsType, pivotPosition);

            return cone;
        }

        /// <summary>
        /// re/generate mesh geometry based on parameters
        /// </summary>
        /// <param name="radius0">fist radius of cone</param>
        /// <param name="radius1">second radius of cone</param>
        /// <param name="height">height of cone</param>
        /// <param name="sides">number of triangle segments in radius</param>
        /// <param name="heightSegments">number of triangle segments in height</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        /// <returns>Cone class with Cone game object</returns>
        public void GenerateGeometry(float radius0, float radius1, float thickness, float height, int sides, int heightSegments, NormalsType normalsType, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            if (thickness >= 0)
            {
                GenerationTimeMS = Primitives.HollowConePrimitive.GenerateGeometry(mesh, radius0, radius1, thickness, height, sides, heightSegments, normalsType, pivotPosition);
            }
            else
            {
                GenerationTimeMS = Primitives.ConePrimitive.GenerateGeometry(mesh, radius0, radius1, height, sides, heightSegments, normalsType, pivotPosition);
            }

            this.radius0 = radius0;
            this.radius1 = radius1;
            this.height = height;
            this.thickness = thickness;
            this.sides = sides;
            this.heightSegments = heightSegments;
            this.normalsType = normalsType;
            this.flipNormals = false;
            this.pivotPosition = pivotPosition;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(radius0, radius1, thickness, height, sides, heightSegments, normalsType, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                if (thickness >= 0)
                {
                    Primitives.HollowConePrimitive.GenerateGeometry(meshCollider, radius0, radius1, thickness, height, sides, heightSegments, normalsType, pivotPosition);
                }
                else
                {
                    Primitives.ConePrimitive.GenerateGeometry(meshCollider, radius0, radius1, height, sides, heightSegments, normalsType, pivotPosition);
                }

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["radius0"] = radius0;
            dic["radius1"] = radius1;
            dic["thickness"] = thickness;
            dic["height"] = height;
            dic["sides"] = sides;
            dic["heightSegments"] = heightSegments;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            radius0 = (float)dic["radius0"];
            height = (float)dic["height"];
            thickness = (float) dic["thickness"];
            radius1 = (float)dic["radius1"];
            sides = (int)dic["sides"];
            heightSegments = (int)dic["heightSegments"];

            return dic;
        }

        /// <summary>
        /// helper to set height
        /// </summary>
        public override void SetHeight(float height)
        {
            this.height = height;
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
