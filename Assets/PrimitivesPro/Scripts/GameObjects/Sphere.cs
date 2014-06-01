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
    /// class for creating Sphere primitive
    /// </summary>
    public class Sphere : BaseObject
    {
        /// <summary>
        /// radius of the sphere
        /// </summary>
        public float radius;

        /// <summary>
        /// number of spere segments
        /// </summary>
        public int segments;

        /// <summary>
        /// hemisphere parameter
        /// 0 ... whole sphere
        /// 0.5 ... half-sphere
        /// 1.0 ... no sphere
        /// </summary>
        public float hemisphere;

        /// <summary>
        /// radius of the inner sphere
        /// if greater then 0 and hemisphere is greater than 0 than inside of the sphere will be generated like a 'bowl'
        /// </summary>
        public float innerRadius;

        /// <summary>
        /// create Sphere game object
        /// </summary>
        /// <param name="radius">radius of sphere</param>
        /// <param name="segments">number of segments</param>
        /// <param name="hemisphere">hemisphere, 0 ... complete sphere, 0.5 ... half-sphere</param>
        /// <param name="normals">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        /// <param name="innerRadius">radius of the inner sphere</param>
        /// <returns>Sphere class with Sphere game object</returns>
        public static Sphere Create(float radius, int segments, float hemisphere, float innerRadius, NormalsType normals, PivotPosition pivotPosition)
        {
            var sphereObject = new GameObject("SpherePro");

            sphereObject.AddComponent<MeshFilter>();
            var renderer = sphereObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var sphere = sphereObject.AddComponent<Sphere>();
            sphere.GenerateGeometry(radius, segments, hemisphere, innerRadius, normals, pivotPosition);

            return sphere;
        }

        /// <summary>
        /// create Sphere game object
        /// </summary>
        /// <param name="radius">radius of sphere</param>
        /// <param name="segments">number of segments</param>
        /// <param name="hemisphere">hemisphere, 0 ... complete sphere, 0.5 ... half-sphere</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="innerRadius">radius of the inner sphere</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public void GenerateGeometry(float radius, int segments, float hemisphere, float innerRadius, NormalsType normalsType, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.SpherePrimitive.GenerateGeometry(mesh, radius, segments, hemisphere, innerRadius, normalsType, pivotPosition);

            this.radius = radius;
            this.segments = segments;
            this.hemisphere = hemisphere;
            this.normalsType = normalsType;
            this.flipNormals = false;
            this.innerRadius = innerRadius;
            this.pivotPosition = pivotPosition;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(radius, segments, hemisphere, innerRadius, normalsType, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.SpherePrimitive.GenerateGeometry(meshCollider, radius, segments, hemisphere, innerRadius, normalsType, pivotPosition);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["radius"] = radius;
            dic["segments"] = segments;
            dic["innerRadius"] = innerRadius;
            dic["hemisphere"] = hemisphere;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            radius = (float)dic["radius"];
            innerRadius = (float)dic["innerRadius"];
            segments = (int)dic["segments"];
            hemisphere = (float)dic["hemisphere"];

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
