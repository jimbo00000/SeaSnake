// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Primitives;
using UnityEngine;

namespace PrimitivesPro.GameObjects
{
    /// <summary>
    /// class for creating Arc primitive
    /// </summary>
    public class Arc : BaseObject
    {
        /// <summary>
        /// width of the cube
        /// </summary>
        public float width;

        /// <summary>
        /// height1 of the cube
        /// </summary>
        public float height1;

        /// <summary>
        /// height2 of the cube
        /// </summary>
        public float height2;

        /// <summary>
        /// depth of the cube
        /// </summary>
        public float depth;

        /// <summary>
        /// level of arc subdivision
        /// </summary>
        public int arcSegments;

        /// <summary>
        /// control point of the arc cuvre
        /// </summary>
        public ArcGizmo gizmo;

        /// <summary>
        /// create Arc game object
        /// </summary>
        /// <param name="width">width of cube</param>
        /// <param name="height1">height1 of cube</param>
        /// <param name="height2">height2 of cube</param>
        /// <param name="depth">depth of cube</param>
        /// <param name="arcSegments">depth of the </param>
        /// <param name="pivot">position of the model pivot</param>
        /// <returns>Arc class with Arc game object</returns>
        public static Arc Create(float width, float height1, float height2, float depth, int arcSegments, PivotPosition pivot)
        {
            var planeObject = new GameObject("ArcPro");

            planeObject.AddComponent<MeshFilter>();
            var renderer = planeObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var cube = planeObject.AddComponent<Arc>();

            cube.gizmo = ArcGizmo.Create();
            cube.gizmo.transform.parent = planeObject.transform;
            cube.GenerateGeometry(width, height1, height2, depth, arcSegments, pivot);

            return cube;
        }

        /// <summary>
        /// re/generate mesh geometry based on parameters
        /// </summary>
        /// <param name="width">width of cube</param>
        /// <param name="height1">height1 of cube</param>
        /// <param name="height2">height2 of cube</param>
        /// <param name="depth">depth of cube</param>
        /// <param name="arcSegments">depth of the </param>
        /// <param name="pivot">position of the model pivot</param>
        public void GenerateGeometry(float width, float height1, float height2, float depth, int arcSegments, PivotPosition pivot)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;
            mesh.Clear();

            // generate geometry
            GenerationTimeMS = Primitives.ArcPrimitive.GenerateGeometry(mesh, width, height1, height2, depth, arcSegments, gizmo.transform.position, pivot);

            this.width = width;
            this.height1 = height1;
            this.height2 = height2;
            this.depth = depth;
            this.arcSegments = arcSegments;
            this.flipNormals = false;
            this.pivotPosition = pivot;
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(width, height1, height2, depth, arcSegments, pivotPosition);
            base.GenerateGeometry();
        }

        public override void GenerateColliderGeometry()
        {
            var meshCollider = GetColliderMesh();

            if (meshCollider)
            {
                meshCollider.Clear();

                Primitives.ArcPrimitive.GenerateGeometry(meshCollider, width, height1, height2, depth, arcSegments, gizmo.transform.position, pivotPosition);

                RefreshMeshCollider();
            }

            base.GenerateColliderGeometry();
        }

        public override System.Collections.Generic.Dictionary<string, object> SaveState(bool collision)
        {
            var dic = base.SaveState(collision);

            dic["width"] = width;
            dic["height1"] = height1;
            dic["height2"] = height2;
            dic["depth"] = depth;
            dic["arcSegments"] = arcSegments;
            dic["controlPoint"] = gizmo.transform.position;

            return dic;
        }

        public override System.Collections.Generic.Dictionary<string, object> LoadState(bool collision)
        {
            var dic = base.LoadState(collision);

            width = (float)dic["width"];
            height1 = (float)dic["height1"];
            height2 = (float)dic["height2"];
            depth = (float)dic["depth"];
            arcSegments = (int)dic["arcSegments"];
            gizmo.transform.position = (Vector3)dic["controlPoint"];

            return dic;
        }

        /// <summary>
        /// helper to set height
        /// </summary>
        public override void SetHeight(float height)
        {
            var diff = height1 - height2;
            if (diff > 0)
            {
                height1 = height;
                height2 = height1 - diff;
            }
            else
            {
                height2 = height;
                height1 = height2 + diff;
            }
        }

        /// <summary>
        /// helper to set width and length
        /// </summary>
        public override void SetWidth(float width0, float length0)
        {
            this.width = width0;
            this.depth = length0;
        }
    }
}
