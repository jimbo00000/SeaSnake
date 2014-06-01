// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.MeshCutting;
using PrimitivesPro.Primitives;
using UnityEngine;

namespace PrimitivesPro.GameObjects
{
    /// <summary>
    /// class for creating CuttingPlane primitive
    /// </summary>
    public class CuttingPlane : BaseObject
    {
        /// <summary>
        /// size of the plane
        /// </summary>
        public float size;

        /// <summary>
        /// triangulate holes after cut
        /// </summary>
        public bool triangulateHoles;

        /// <summary>
        /// delete original object after cut
        /// </summary>
        public bool deleteOriginal;

        /// <summary>
        /// game object to be cut
        /// </summary>
        public GameObject cuttingObject;

        /// <summary>
        /// create CuttingPlane game object
        /// </summary>
        /// <param name="size">size of plane</param>
        /// <returns>CuttingPlane class with CuttingPlane game object</returns>
        public static CuttingPlane Create(float size)
        {
            var planeObject = new GameObject("CuttingPlanePro");

            planeObject.AddComponent<MeshFilter>();
            var renderer = planeObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

            var plane = planeObject.AddComponent<CuttingPlane>();
            plane.GenerateGeometry(size);

            plane.triangulateHoles = true;
            plane.deleteOriginal = true;

            return plane;
        }

        void Awake()
        {
        }

        /// <summary>
        /// re/generate mesh geometry based on parameters
        /// </summary>
        /// <param name="size">size of plane</param>
        public void GenerateGeometry(float size)
        {
            // generate new mesh and clear old one
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            var mesh = meshFilter.sharedMesh;

            // generate geometry
            GenerationTimeMS = Primitives.PlanePrimitive.GenerateGeometry(mesh, size, size, 1, 1);

            this.size = size;
            this.flipNormals = false;
        }

        public Utils.Plane ComputePlane()
        {
            return new Utils.Plane(transform.position, transform.position + transform.right, transform.position + transform.forward);
        }

        /// <summary>
        /// run cutting algorithm
        /// </summary>
        public void Cut(GameObject primitive, bool fillHoles, bool deleteOriginal)
        {
            MeshUtils.Log("Cutting: " + primitive.name);

            var cutter = new MeshCutter();

            // create cutting plane
            var plane = ComputePlane();

            ContourData contour;

            GameObject cut0, cut1;
            GenerationTimeMS = cutter.Cut(primitive, plane, fillHoles, deleteOriginal, out cut0, out cut1, out contour);

//            contour.ShowContourDBG(float.MaxValue);
//            contour.CreateGameObject(true);

            if (cut0 != null)
            {
                cut0.AddComponent<DefaultObject>();
                cut1.AddComponent<DefaultObject>();
            }
        }

        public void Cut()
        {
            MeshUtils.Log("Cutting: " + cuttingObject.name);

            var cutter = new MeshCutter();

            // create cutting plane
            var plane = ComputePlane();

            ContourData contour;

            GameObject cut0, cut1;
            GenerationTimeMS = cutter.Cut(cuttingObject, plane, triangulateHoles, deleteOriginal, out cut0, out cut1, out contour);

            if (cut0 != null)
            {
                cut0.AddComponent<DefaultObject>();
                cut1.AddComponent<DefaultObject>();
            }
        }

        /// <summary>
        /// regenerate mesh geometry with class variables
        /// </summary>
        public override void GenerateGeometry()
        {
            GenerateGeometry(size);
        }
    }
}
