// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEngine;

namespace PrimitivesPro.GameObjects
{
    /// <summary>
    /// class for creating Triangle primitive
    /// </summary>
    public class Triangle : BaseObject
    {
        /// <summary>
        /// side length of the triangle
        /// </summary>
        public float side;

        /// <summary>
        /// subdivision of the triangle
        /// </summary>
        public int subdivision;

        /// <summary>
        /// create Triangle game object
        /// </summary>
        /// <param name="side">length of side</param>
        /// <param name="subdivision">subdivision of the triangle</param>
        /// <returns>Triangle class with Triangle game object</returns>
        public static Triangle Create(float side, int subdivision)
        {
            var sphereObject = new GameObject("TrianglePro");

            sphereObject.AddComponent<MeshFilter>();
            var renderer = sphereObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var triangle = sphereObject.AddComponent<Triangle>();
            triangle.GenerateGeometry(side, subdivision);

            return triangle;
        }

        /// <summary>
        /// create Triangle game object
        /// </summary>
        /// <param name="side">length of side</param>
        /// <param name="subdivision">subdivison of the triangle</param>
        public void GenerateGeometry(float side, int subdivision)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.TrianglePrimitive.GenerateGeometry(mesh, side, subdivision);

            this.subdivision = subdivision;
            this.side = side;
            this.flipNormals = false;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(side, subdivision);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.TrianglePrimitive.GenerateGeometry(meshCollider, side, subdivision);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["side"] = side;
            dic["subdivision"] = subdivision;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            side = (float)dic["side"];
            subdivision = (int)dic["subdivision"];

            return dic;
        }

        /// <summary>
        /// helper to set width and length
        /// </summary>
        public override void SetWidth(float width0, float length0)
        {
            side = length0;
        }
    }
}
