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
    /// class for creating SuperEllipsoid primitive
    /// 
    /// for possible combinations of parameters n1 and n2 with picture reference see:
    /// http://en.wikipedia.org/wiki/Superellipsoid
    /// </summary>
    public class SuperEllipsoid : BaseObject
    {
        /// <summary>
        /// width of the roundedCube
        /// </summary>
        public float width;

        /// <summary>
        /// height of the roundedCube
        /// </summary>
        public float height;

        /// <summary>
        /// length of the roundedCube
        /// </summary>
        public float length;

        /// <summary>
        /// number of spere segments
        /// </summary>
        public int segments;

        /// <summary>
        /// first parameter
        /// </summary>
        public float n1;

        /// <summary>
        /// second parameter
        /// </summary>
        public float n2;

        /// <summary>
        /// create SuperEllipsoid game object
        /// </summary>
        /// <param name="width">width of the cube</param>
        /// <param name="height">height of the cube</param>
        /// <param name="length">length of the cube</param>
        /// <param name="segments">number of segments</param>
        /// <param name="normals">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        /// <param name="n1">first parameter</param>
        /// <param name="n2">second parameter</param>
        /// <returns>SuperEllipsoid class with SuperEllipsoid game object</returns>
        public static SuperEllipsoid Create(float width, float height, float length, int segments, float n1, float n2, NormalsType normals, PivotPosition pivotPosition)
        {
            var sphereObject = new GameObject("SuperEllipsoidPro");

            sphereObject.AddComponent<MeshFilter>();
            var renderer = sphereObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var roundedCube = sphereObject.AddComponent<SuperEllipsoid>();
            roundedCube.GenerateGeometry(width, height, length, segments, n1, n2, normals, pivotPosition);

            return roundedCube;
        }

        /// <summary>
        /// create SuperEllipsoid game object
        /// </summary>
        /// <param name="width">width of the cube</param>
        /// <param name="height">height of the cube</param>
        /// <param name="length">length of the cube</param>
        /// <param name="segments">number of segments</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public void GenerateGeometry(float width, float height, float length, int segments, float n1, float n2, NormalsType normalsType, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.SuperEllipsoidPrimitive.GenerateGeometry(mesh, width, height, length, segments, n1, n2, normalsType, pivotPosition);

            this.width = width;
            this.height = height;
            this.length = length;
            this.segments = segments;
            this.n1 = n1;
            this.n2 = n2;
            this.normalsType = normalsType;
            this.flipNormals = false;
            this.pivotPosition = pivotPosition;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(width, height, length, segments, n1, n2, normalsType, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.SuperEllipsoidPrimitive.GenerateGeometry(meshCollider, width, height, length, segments, n1, n2, normalsType, pivotPosition);

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
            dic["n1"] = n1;
            dic["n2"] = n2;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            width = (float)dic["width"];
            height = (float)dic["height"];
            length = (float)dic["length"];
            segments = (int)dic["segments"];
            n1 = (float)dic["n1"];
            n2 = (float)dic["n2"];

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
