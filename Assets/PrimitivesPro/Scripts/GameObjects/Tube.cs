// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Primitives;
using UnityEngine;

namespace PrimitivesPro.GameObjects
{
    /// <summary>
    /// class for creating Tube primitive
    /// </summary>
    public class Tube : BaseObject
    {
        /// <summary>
        /// first radius of the tube
        /// </summary>
        public float radius0;

        /// <summary>
        /// second radius of the tube
        /// </summary>
        public float radius1;

        /// <summary>
        /// height of the tube
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
        /// slice parameter
        /// 0 ... whole tube
        /// 180 ... half-pipe (tube cut in half)
        /// 360 ... no tube
        /// </summary>
        public float slice;

        /// <summary>
        /// using radial uv mapping on the top/bottom of the tube
        /// </summary>
        public bool radialMapping;

        /// <summary>
        /// create Tube game object
        /// </summary>
        /// <param name="radius0">first radius of tube</param>
        /// <param name="radius1">second radius of tube</param>
        /// <param name="height">height of tube</param>
        /// <param name="sides">number of triangle segments in radius direction</param>
        /// <param name="heightSegments">number of triangle segments in height direction</param>
        /// <returns>Tube class with Tube game object</returns>
        /// <param name="slice">slicing parameter</param>
        /// <param name="radialMapping">using radial uv mapping on the top/bottom of the tube</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public static Tube Create(float radius0, float radius1, float height, int sides, int heightSegments, float slice, bool radialMapping, NormalsType normalsType, PivotPosition pivotPosition)
        {
            var cylinderObject = new GameObject("TubePro");

            cylinderObject.AddComponent<MeshFilter>();
            var renderer = cylinderObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var tube = cylinderObject.AddComponent<Tube>();
            tube.GenerateGeometry(radius0, radius1, height, sides, heightSegments, slice, radialMapping, normalsType, pivotPosition);

            return tube;
        }

        /// <summary>
        /// re/generate mesh geometry based on parameters
        /// </summary>
        /// <param name="radius0">fist radius of tube</param>
        /// <param name="radius1">second radius of tube</param>
        /// <param name="height">height of tube</param>
        /// <param name="sides">number of triangle segments in radius</param>
        /// <param name="heightSegments">number of triangle segments in height</param>
        /// <param name="slice">slicing parameter</param>
        /// <param name="radialMapping">using radial uv mapping on the top/bottom of the tube</param>
        /// <param name="normalsType">type of normals to be generated</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public void GenerateGeometry(float radius0, float radius1, float height, int sides, int heightSegments, float slice, bool radialMapping, NormalsType normalsType, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.TubePrimitive.GenerateGeometry(mesh, radius0, radius1, height, sides, heightSegments, slice, radialMapping, normalsType, pivotPosition);

            this.radius0 = radius0;
            this.radius1 = radius1;
            this.height = height;
            this.sides = sides;
            this.heightSegments = heightSegments;
            this.slice = slice;
            this.radialMapping = radialMapping;
            this.normalsType = normalsType;
            this.flipNormals = false;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(radius0, radius1, height, sides, heightSegments, slice, radialMapping, normalsType, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.TubePrimitive.GenerateGeometry(meshCollider, radius0, radius1, height, sides, heightSegments, slice, radialMapping, normalsType, pivotPosition);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["radius0"] = radius0;
            dic["radius1"] = radius1;
            dic["height"] = height;
            dic["sides"] = sides;
            dic["heightSegments"] = heightSegments;
            dic["slice"] = slice;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            radius0 = (float)dic["radius0"];
            radius1 = (float)dic["radius1"];
            height = (float)dic["height"];
            sides = (int)dic["sides"];
            heightSegments = (int)dic["heightSegments"];
            slice = (float)dic["slice"];

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

            if (Mathf.Abs(r0 - radius1) > Mathf.Abs(r1 - radius1))
            {
                radius1 = r0;
            }
            else
            {
                radius1 = r1;
            }
        }
    }
}
