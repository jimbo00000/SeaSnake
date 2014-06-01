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
    /// class for creating Pyramid primitive
    /// </summary>
    public class Pyramid : BaseObject
    {
        /// <summary>
        /// width of the pyramid
        /// </summary>
        public float width;

        /// <summary>
        /// height of the pyramid
        /// </summary>
        public float height;

        /// <summary>
        /// depth of the pyramid
        /// </summary>
        public float depth;

        /// <summary>
        /// number of triangle segments in width direction
        /// </summary>
        public int widthSegments;

        /// <summary>
        /// number of triangle segments in height direction
        /// </summary>
        public int heightSegments;

        /// <summary>
        /// number of triangle segments in depth direction
        /// </summary>
        public int depthSegments;

        /// <summary>
        /// flag for using pyramid uv mapping
        /// </summary>
        public bool pyramidMap;

        /// <summary>
        /// create Pyramid game object
        /// </summary>
        /// <param name="width">width of pyramid</param>
        /// <param name="height">height of pyramid</param>
        /// <param name="depth">depth of pyramid</param>
        /// <param name="widthSegments">number of triangle segments in width direction</param>
        /// <param name="heightSegments">number of triangle segments in height direction</param>
        /// <param name="depthSegments">number of triangle segments in depth direction</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        /// <param name="pyramidMap">enable pyramid map uv mapping</param>
        /// <returns>Pyramid class with Pyramid game object</returns>
        public static Pyramid Create(float width, float height, float depth, int widthSegments, int heightSegments, int depthSegments, bool pyramidMap, PivotPosition pivotPosition)
        {
            var planeObject = new GameObject("PyramidPro");

            planeObject.AddComponent<MeshFilter>();
            var renderer = planeObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var pyramid = planeObject.AddComponent<Pyramid>();
            pyramid.GenerateGeometry(width, height, depth, widthSegments, heightSegments, depthSegments, pyramidMap, pivotPosition);

            return pyramid;
        }

        /// <summary>
        /// re/generate mesh geometry based on parameters
        /// </summary>
        /// <param name="width">width of pyramid</param>
        /// <param name="height">height of pyramid</param>
        /// <param name="depth">depth of pyramid</param>
        /// <param name="widthSegments">number of triangle segments in width direction</param>
        /// <param name="heightSegments">number of triangle segments in height direction</param>
        /// <param name="depthSegments">number of triangle segments in depth direction</param>
        /// <param name="pyramidMap">enable pyramid map uv mapping</param>
        /// <param name="pivotPosition">position of the model pivot</param>
        public void GenerateGeometry(float width, float height, float depth, int widthSegments, int heightSegments, int depthSegments,  bool pyramidMap, PivotPosition pivotPosition)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.PyramidPrimitive.GenerateGeometry(mesh, width, height, depth, widthSegments, heightSegments, depthSegments, pyramidMap, pivotPosition);

            this.width = width;
            this.height = height;
            this.depth = depth;
            this.widthSegments = widthSegments;
            this.heightSegments = heightSegments;
            this.depthSegments = depthSegments;
            this.flipNormals = false;
            this.pyramidMap = pyramidMap;
            this.pivotPosition = pivotPosition;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(width, height, depth, widthSegments, heightSegments, depthSegments, pyramidMap, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.PyramidPrimitive.GenerateGeometry(meshCollider, width, height, depth, widthSegments, heightSegments, depthSegments, pyramidMap, pivotPosition);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["width"] = width;
            dic["height"] = height;
            dic["depth"] = depth;
            dic["widthSegments"] = widthSegments;
            dic["heightSegments"] = heightSegments;
            dic["depthSegments"] = depthSegments;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            width = (float)dic["width"];
            height = (float)dic["height"];
            depth = (float)dic["depth"];
            widthSegments = (int)dic["widthSegments"];
            heightSegments = (int)dic["heightSegments"];
            depthSegments = (int)dic["depthSegments"];

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
            this.width = length0;
            this.depth = width0;
        }
    }
}
