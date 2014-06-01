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
    /// class for creating SphericalCone primitive
    /// 
    /// references: 
    /// http://mathworld.wolfram.com/SphericalCone.html
    /// http://en.wikipedia.org/wiki/Spherical_sector
    /// </summary>
    public class SphericalCone : BaseObject
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
        /// angle of the conus in DEG
        /// 360 ... whole sphere
        /// 180 ... half-sphere
        /// 0.0 ... no sphere
        /// </summary>
        public float coneAngle;

        /// <summary>
        /// create SphericalCone game object
        /// </summary>
        /// <param name="radius">radius of sphere</param>
        /// <param name="segments">number of segments</param>
        /// <param name="coneAngle">angle of conus in DEG, 360 ... complete sphere, 180 ... half-sphere</param>
        /// <param name="normals">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        /// <returns>SphericalCone class with SphericalCone game object</returns>
        public static SphericalCone Create(float radius, int segments, float coneAngle, NormalsType normals, PivotPosition pivotPosition)
        {
            var sphereObject = new GameObject("SphericalConePro");

            sphereObject.AddComponent<MeshFilter>();
            var renderer = sphereObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var sphere = sphereObject.AddComponent<SphericalCone>();
            sphere.GenerateGeometry(radius, segments, coneAngle, normals, pivotPosition);

            return sphere;
        }

        /// <summary>
        /// create SphericalCone game object
        /// </summary>
        /// <param name="radius">radius of sphere</param>
        /// <param name="segments">number of segments</param>
        /// <param name="coneAngle">angle of conus in DEG, 360 ... complete sphere, 180 ... half-sphere</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public void GenerateGeometry(float radius, int segments, float coneAngle, NormalsType normalsType, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.SphericalConePrimitive.GenerateGeometry(mesh, radius, segments, coneAngle, normalsType, pivotPosition);

            this.radius = radius;
            this.segments = segments;
            this.coneAngle = coneAngle;
            this.normalsType = normalsType;
            this.flipNormals = false;
            this.pivotPosition = pivotPosition;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(radius, segments, coneAngle, normalsType, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.SphericalConePrimitive.GenerateGeometry(meshCollider, radius, segments, coneAngle, normalsType, pivotPosition);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["radius"] = radius;
            dic["segments"] = segments;
            dic["coneAngle"] = coneAngle;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            radius = (float)dic["radius"];
            segments = (int)dic["segments"];
            coneAngle = (float)dic["coneAngle"];

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
