// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;
using PrimitivesPro.Primitives;
using UnityEngine;
using System.Collections;

namespace PrimitivesPro.GameObjects
{
    /// <summary>
    /// class for creating GeoSphere primitive
    /// </summary>
    public class GeoSphere : BaseObject
    {
        /// <summary>
        /// radius of the sphere
        /// </summary>
        public float radius;

        /// <summary>
        /// number of spere subdivision
        /// </summary>
        public int subdivision;

        /// <summary>
        /// type of generation primitive
        /// </summary>
        public Primitives.GeoSpherePrimitive.BaseType baseType;

        /// <summary>
        /// create GeoSphere game object
        /// </summary>
        /// <param name="radius">radius of sphere</param>
        /// <param name="subdivision">number of subdivision</param>
        /// <param name="baseType">type of generation primitive</param>
        /// <param name="normals">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        /// <returns>GeoSphere class with GeoSphere game object</returns>
        public static GeoSphere Create(float radius, int subdivision, Primitives.GeoSpherePrimitive.BaseType baseType, NormalsType normals, PivotPosition pivotPosition)
        {
            var sphereObject = new GameObject("GeoSpherePro");

            sphereObject.AddComponent<MeshFilter>();
            var renderer = sphereObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var sphere = sphereObject.AddComponent<GeoSphere>();
            sphere.GenerateGeometry(radius, subdivision, baseType, normals, pivotPosition);

            return sphere;
        }

        /// <summary>
        /// create GeoSphere game object
        /// </summary>
        /// <param name="radius">radius of sphere</param>
        /// <param name="subdivision">number of subdivision</param>
        /// <param name="baseType">type of generation primitive</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public void GenerateGeometry(float radius, int subdivision, Primitives.GeoSpherePrimitive.BaseType baseType, NormalsType normalsType, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // geneate geometry
            GenerationTimeMS = Primitives.GeoSpherePrimitive.GenerateGeometry(mesh, radius, subdivision, baseType, normalsType, pivotPosition);

            this.radius = radius;
            this.subdivision = subdivision;
            this.baseType = baseType;
            this.normalsType = normalsType;
            this.flipNormals = false;
            this.pivotPosition = pivotPosition;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(radius, subdivision, baseType, normalsType, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.GeoSpherePrimitive.GenerateGeometry(meshCollider, radius, subdivision, baseType, normalsType, pivotPosition);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["radius"] = radius;
            dic["subdivision"] = subdivision;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            radius = (float)dic["radius"];
            subdivision = (int)dic["subdivision"];

            return dic;
        }

        /// <summary>
        /// helper to set height
        /// </summary>
        public override void SetHeight(float height)
        {
            this.radius = height/2;
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
