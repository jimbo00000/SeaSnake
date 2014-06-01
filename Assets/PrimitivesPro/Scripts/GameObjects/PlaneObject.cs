// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEngine;

namespace PrimitivesPro.GameObjects
{
    /// <summary>
    /// class for creating Plane primitive
    /// </summary>
    public class PlaneObject : BaseObject
    {
        /// <summary>
        /// width of the plane
        /// </summary>
        public float width;

        /// <summary>
        /// length of the plane
        /// </summary>
        public float length;

        /// <summary>
        /// number of triangle segments in width direction
        /// </summary>
        public int widthSegments;

        /// <summary>
        /// number of triangle segments in length direction
        /// </summary>
        public int lengthSegments;

        /// <summary>
        /// create Plane game object
        /// </summary>
        /// <param name="width">width of plane</param>
        /// <param name="length">length of plane</param>
        /// <param name="widthSegments">number of triangle segments in width direction</param>
        /// <param name="lengthSegments">number of triangle segments in length direction</param>
        /// <returns>Plane class with Plane game object</returns>
        public static PlaneObject Create(float width, float length, int widthSegments, int lengthSegments)
        {
            var planeObject = new GameObject("PlanePro");

            planeObject.AddComponent<MeshFilter>();
            var renderer = planeObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var plane = planeObject.AddComponent<PlaneObject>();
            plane.GenerateGeometry(width, length, widthSegments, lengthSegments);

            return plane;
        }

        /// <summary>
        /// re/generate mesh geometry based on parameters
        /// </summary>
        /// <param name="width">width of plane</param>
        /// <param name="length">length of plane</param>
        /// <param name="widthSegments">number of triangle segments in width</param>
        /// <param name="lengthSegments">number of triangle segments in length</param>
        public void GenerateGeometry(float width, float length, int widthSegments, int lengthSegments)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.PlanePrimitive.GenerateGeometry(mesh, width, length, widthSegments, lengthSegments);

            this.width = width;
            this.length = length;
            this.widthSegments = widthSegments;
            this.lengthSegments = lengthSegments;
            this.flipNormals = false;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(width, length, widthSegments, lengthSegments);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.PlanePrimitive.GenerateGeometry(meshCollider, width, length, widthSegments, lengthSegments);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["width"] = width;
            dic["length"] = length;
            dic["widthSegments"] = widthSegments;
            dic["lengthSegments"] = lengthSegments;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            width = (float)dic["width"];
            length = (float)dic["length"];
            widthSegments = (int)dic["widthSegments"];
            lengthSegments = (int)dic["lengthSegments"];

            return dic;
        }

        /// <summary>
        /// helper to set width and length
        /// </summary>
        public override void SetWidth(float width0, float length0)
        {
            this.width = width0;
            this.length = length0;
        }
    }
}
