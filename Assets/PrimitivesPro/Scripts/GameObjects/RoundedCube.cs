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
    /// class for creating RoundedCube primitive
    /// </summary>
    public class RoundedCube : BaseObject
    {
        /// <summary>
        /// width of the superellipsoid
        /// </summary>
        public float width;

        /// <summary>
        /// height of the superellipsoid
        /// </summary>
        public float height;

        /// <summary>
        /// length of the superellipsoid
        /// </summary>
        public float length;

        /// <summary>
        /// number of spere segments
        /// </summary>
        public int segments;

        /// <summary>
        /// roundness coefficient
        /// </summary>
        public float roundness;

        /// <summary>
        /// create RoundedCube game object
        /// </summary>
        /// <param name="width">width of the cube</param>
        /// <param name="height">height of the cube</param>
        /// <param name="length">length of the cube</param>
        /// <param name="segments">number of segments</param>
        /// <param name="roundness">roudness coefficient</param>
        /// <param name="normals">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        /// <returns>RoundedCube class with RoundedCube game object</returns>
        public static RoundedCube Create(float width, float height, float length, int segments, float roundness, NormalsType normals, PivotPosition pivotPosition)
        {
            var sphereObject = new GameObject("RoundedCubePro");

            sphereObject.AddComponent<MeshFilter>();
            var renderer = sphereObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var superellipsoid = sphereObject.AddComponent<RoundedCube>();
            superellipsoid.GenerateGeometry(width, height, length, segments, roundness, normals, pivotPosition);

            return superellipsoid;
        }

        /// <summary>
        /// create RoundedCube game object
        /// </summary>
        /// <param name="width">width of the cube</param>
        /// <param name="height">height of the cube</param>
        /// <param name="length">length of the cube</param>
        /// <param name="segments">number of segments</param>
        /// <param name="roundness">roudness coefficient</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public void GenerateGeometry(float width, float height, float length, int segments, float roundness, NormalsType normalsType, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.SuperEllipsoidPrimitive.GenerateGeometry(mesh, width, height, length, segments, roundness, roundness, normalsType, pivotPosition);

            this.width = width;
            this.height = height;
            this.length = length;
            this.segments = segments;
            this.roundness = roundness;
            this.normalsType = normalsType;
            this.flipNormals = false;
            this.pivotPosition = pivotPosition;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(width, height, length, segments, roundness, normalsType, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.SuperEllipsoidPrimitive.GenerateGeometry(meshCollider, width, height, length, segments, roundness, roundness, normalsType, pivotPosition);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["width"] = width;
            dic["height"] = height;
            dic["length"] = length;
            dic["segments"] = segments;
            dic["roundness"] = roundness;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            width = (float)dic["width"];
            height = (float)dic["height"];
            length = (float)dic["length"];
            segments = (int)dic["segments"];
            roundness = (float)dic["roundness"];

            return dic;
        }

        /// <summary>
        /// helper to set height
        /// </summary>
        public override void SetHeight(float height)
        {
            this.height = height * 0.5f;
        }

        /// <summary>
        /// helper to set width and length
        /// </summary>
        public override void SetWidth(float width0, float length0)
        {
            this.width = width0 * 0.5f;
            this.length = length0 * 0.5f;
        }
    }
}
