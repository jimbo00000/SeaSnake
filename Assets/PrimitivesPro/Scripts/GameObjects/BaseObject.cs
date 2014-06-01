// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Collections.Generic;
using PrimitivesPro.Primitives;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PrimitivesPro.GameObjects
{
    public abstract class BaseObject : MonoBehaviour
    {
        /// <summary>
        /// flag for generating geometry every frame
        /// </summary>
        public bool generateGemoetryEveryFrame = false;

        /// <summary>
        /// flag to flip normals
        /// </summary>
        public bool flipNormals;

        /// <summary>
        /// flag to flip uv mapping
        /// </summary>
        public bool flipUVMapping;

        /// <summary>
        /// whether or not duplicate materials
        /// </summary>
        public bool shareMaterial;

        /// <summary>
        /// mesh collider business
        /// </summary>
        public bool generateMeshCollider;

        /// <summary>
        /// always recalculate collider when the mesh is changed
        /// </summary>
        public bool fitColliderOnChange;

        /// <summary>
        /// flag for showing scene handles for easier primitive manipulation
        /// </summary>
        public bool showSceneHandles = true;

        /// <summary>
        /// mode for mesh or collider generation
        /// </summary>
        public int generationMode;

        /// <summary>
        /// normals type to be generated
        /// </summary>
        public NormalsType normalsType;

        /// <summary>
        /// position of the model pivot
        /// </summary>
        public PivotPosition pivotPosition;

        /// <summary>
        /// current state of primitives variables
        /// </summary>
        public Dictionary<string, object> state = new Dictionary<string, object>();

        /// <summary>
        /// current state of primitives collision variables
        /// </summary>
        public Dictionary<string, object> stateCollision = new Dictionary<string, object>();

        /// <summary>
        /// save state of this class
        /// </summary>
        public virtual Dictionary<string, object> SaveState(bool collision)
        {
            return collision ? stateCollision : state;
        }

        /// <summary>
        /// load state of this class
        /// </summary>
        /// <param name="collision"></param>
        public virtual Dictionary<string, object> LoadState(bool collision)
        {
            return collision ? stateCollision : state;
        }

        public void SaveStateAll()
        {
            SaveState(true);
            SaveState(false);
        }

        public virtual void GenerateColliderGeometry()
        {
            SaveState(true);
        }

        public virtual void GenerateGeometry()
        {
            if (fitColliderOnChange)
            {
                FitCollider();
            }
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(this))
            {
                return;
            }
#endif

            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter && meshFilter.sharedMesh == null)
            {
                GenerateGeometry();
            }

            var meshCollider = GetComponent<MeshCollider>();

            if (meshCollider && meshCollider.sharedMesh == null)
            {
                GenerateColliderGeometry();
            }

            var meshRenderer = GetComponent<MeshRenderer>();

            if (meshRenderer && meshRenderer.sharedMaterial == null)
            {
                meshRenderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
            }
        }

        protected Mesh GetColliderMesh()
        {
            var meshCollider = GetComponent<MeshCollider>();
            var meshFilter = GetComponent<MeshFilter>();

            if (meshCollider && meshFilter)
            {
                if (meshCollider.sharedMesh == meshFilter.sharedMesh || meshCollider.sharedMesh == null)
                {
                    meshCollider.sharedMesh = new Mesh();
                }

                return meshCollider.sharedMesh;
            }

            return null;
        }

        /// <summary>
        /// this is necessary for updating mesh visualization in the editor
        /// </summary>
        protected void RefreshMeshCollider()
        {
            var meshCollider = GetComponent<MeshCollider>();

            if (meshCollider)
            {
                meshCollider.enabled = false;
                meshCollider.enabled = true;
            }
        }

        void Update()
        {
            if (generateGemoetryEveryFrame)
            {
                GenerateGeometry();

                if (generateMeshCollider)
                {
                    AddMeshCollider(true);
                    GenerateColliderGeometry();
                }
            }
        }

        /// <summary>
        /// Flip normals to opposite direction
        /// </summary>
        public void FlipNormals()
        {
            MeshUtils.Log("FlipNormals: " + generationMode);

            if (generationMode == 1)
            {
                return;
            }

            flipNormals = !flipNormals;

            var mesh = GetComponent<MeshFilter>().sharedMesh;

            // reverse normals
            MeshUtils.ReverseNormals(mesh);

            // recalculate tangents
            MeshUtils.CalculateTangents(mesh);
        }

        public void FlipUVMapping()
        {
            flipUVMapping = !flipUVMapping;
        }

        /// <summary>
        /// add collider to the object
        /// </summary>
        public void AddCollider()
        {
            var mesh = GetComponent<MeshFilter>().sharedMesh;

            if (mesh)
            {
                if (GetType() == typeof(Sphere) || GetType() == typeof(Ellipsoid) || GetType() == typeof(GeoSphere))
                {
                    gameObject.AddComponent<SphereCollider>();
                }
                else if (GetType() == typeof(Capsule) || GetType() == typeof(Cone) || GetType() == typeof(Cylinder) || GetType() == typeof(Tube))
                {
                    gameObject.AddComponent<CapsuleCollider>();
                }
                else
                {
                    gameObject.AddComponent<BoxCollider>();
                }
            }
        }

        /// <summary>
        /// option for offseting fitting points
        /// </summary>
        public enum FitOffsetOption
        {
            Bottom,
            Top,
            Both,
            None,
        }

        /// <summary>
        /// fit primitive between one point in the bottom and one point in the top
        /// </summary>
        /// <param name="bottom">bottom point</param>
        /// <param name="top">top point</param>
        /// <param name="offset">distance offset from point</param>
        /// <param name="option">offset option</param>
        public void FitBetweenPoints(Vector3 bottom, Vector3 top, float offset, FitOffsetOption option)
        {
            var dir = (top - bottom).normalized;

            switch (option)
            {
                case FitOffsetOption.Top:
                    top -= dir*offset;
                    break;
                case FitOffsetOption.Bottom:
                    bottom += dir*offset;
                    break;
                case FitOffsetOption.Both:
                    top -= dir*offset;
                    bottom += dir*offset;
                    break;
            }

            var length = (top - bottom).magnitude;
            SetHeight(length);

            switch (pivotPosition)
            {
                case PivotPosition.Botttom:
                    transform.position = bottom;
                    break;
                case PivotPosition.Center:
                    transform.position = bottom + (top - bottom)*0.5f;
                    break;
                case PivotPosition.Top:
                    transform.position = top;
                    break;
            }

            GenerateGeometry();

            transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        }

        /// <summary>
        /// helper to set height
        /// </summary>
        public virtual void SetHeight(float height)
        {
        }

        /// <summary>
        /// helper to set width and length
        /// </summary>
        public virtual void SetWidth(float width0, float length0)
        {
        }

        /// <summary>
        /// update collider to fit the geometry
        /// </summary>
        public void FitCollider()
        {
            if (collider is MeshCollider)
            {
                GenerateColliderGeometry();
            }
            else
            {
                // get bounding box of the object from render
                if (renderer)
                {
                    var meshFilter = GetComponent<MeshFilter>();

                    var size = meshFilter.sharedMesh.bounds.size;
                    var localCenter = meshFilter.sharedMesh.bounds.center;

                    if (collider is CapsuleCollider)
                    {
                        ((CapsuleCollider) collider).radius = size.z/2;
                        ((CapsuleCollider) collider).height = size.y;
                        ((CapsuleCollider) collider).center = localCenter;
                    }
                    else if (collider is BoxCollider)
                    {
                        ((BoxCollider) collider).center = localCenter;
                        ((BoxCollider) collider).size = size;
                    }
                    else if (collider is SphereCollider)
                    {
                        ((SphereCollider) collider).center = localCenter;
                        ((SphereCollider)collider).radius = Mathf.Max(size.x, size.y, size.z) / 2;
                    }
                }
            }
        }

        /// <summary>
        /// duplicate this object
        /// </summary>
        public GameObject Duplicate(bool duplicateMaterials)
        {
            var duplicate = Object.Instantiate(gameObject) as GameObject;
            var originalMesh = GetComponent<MeshFilter>().sharedMesh;
            var dupMesh = MeshUtils.CopyMesh(originalMesh);
            duplicate.GetComponent<MeshFilter>().sharedMesh = dupMesh;

            if (duplicateMaterials)
            {
                var meshRenderer = duplicate.GetComponent<MeshRenderer>();

                if (meshRenderer && meshRenderer.sharedMaterials.Length > 0)
                {
                    meshRenderer.sharedMaterials = MeshUtils.CopyMaterials(meshRenderer.sharedMaterials);
                }
            }

            return duplicate;
        }

        /// <summary>
        /// add mesh collider
        /// </summary>
        public void AddMeshCollider(bool add)
        {
            if (add)
            {
                var meshCollider = gameObject.AddComponent<MeshCollider>();

                if (meshCollider)
                {
                    meshCollider.enabled = false;
                    meshCollider.enabled = true;
                }
            }
            else
            {
                var meshCollider = gameObject.GetComponent<MeshCollider>();

                if (meshCollider)
                {
                    Object.DestroyImmediate(meshCollider);
                }
            }
        }

        /// <summary>
        /// measured time [ms] of last mesh generation
        /// </summary>
        public float GenerationTimeMS { get; set; }
    }
}
