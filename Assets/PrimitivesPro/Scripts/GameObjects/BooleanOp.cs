// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Diagnostics;
using PrimitivesPro.Primitives;
using UnityEngine;

namespace PrimitivesPro.GameObjects
{
    /// <summary>
    /// class for boolean operations
    /// </summary>
    public class BooleanOp : BaseObject
    {
        public GameObject A;
        public GameObject B;

        public GameObject Result;

        public bool DeleteOriginal;

        /// <summary>
        /// create game object for boolean operations
        /// </summary>
        /// <returns>boolean operations object</returns>
        public static BooleanOp Create()
        {
            var obj = new GameObject("BooleanOperations");
            return obj.AddComponent<BooleanOp>();
        }

        public void Substract()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var csgA = new CSG.CSG();
            csgA.Construct(A.GetComponent<MeshFilter>().sharedMesh, A.transform, 0);

            var csgB = new CSG.CSG();
            csgB.Construct(B.GetComponent<MeshFilter>().sharedMesh, B.transform, 1);

            var substract = csgA.Substract(csgB);
            var newMesh = substract.ToMesh();

            Result = new GameObject("Substract");
            var defObj = Result.AddComponent<DefaultObject>();
            var meshFilter = Result.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = newMesh;
            var renderer = Result.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { A.GetComponent<MeshRenderer>().sharedMaterial, B.GetComponent<MeshRenderer>().sharedMaterial };

            if (DeleteOriginal)
            {
                Object.DestroyImmediate(A);
                Object.DestroyImmediate(B);
            }

            stopWatch.Stop();
            defObj.GenerationTimeMS = stopWatch.ElapsedMilliseconds;
        }

        public void Test()
        {
            var csg = new CSG.CSG();
            csg.Construct(A.GetComponent<MeshFilter>().sharedMesh, A.transform, 0);

            var newMesh = csg.Test().ToMesh();

            Result = new GameObject("Test");
            Result.AddComponent<DefaultObject>();
            var meshFilter = Result.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = newMesh;
            var renderer = Result.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { A.GetComponent<MeshRenderer>().sharedMaterial, B.GetComponent<MeshRenderer>().sharedMaterial };
        }

        public void Inverse()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var csgA = new CSG.CSG();
            csgA.Construct(A.GetComponent<MeshFilter>().sharedMesh, A.transform, 0);

            var substract = csgA.Inverse();
            var newMesh = substract.ToMesh();

            Result = new GameObject("Inverse");
            var defObj = Result.AddComponent<DefaultObject>();
            var meshFilter = Result.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = newMesh;
            var renderer = Result.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(A.GetComponent<MeshRenderer>().sharedMaterial);

            if (DeleteOriginal)
            {
                Object.DestroyImmediate(A);
            }

            stopWatch.Stop();
            defObj.GenerationTimeMS = stopWatch.ElapsedMilliseconds;
        }

        public void Union()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var csgA = new CSG.CSG();
            csgA.Construct(A.GetComponent<MeshFilter>().sharedMesh, A.transform, 0);

            var csgB = new CSG.CSG();
            csgB.Construct(B.GetComponent<MeshFilter>().sharedMesh, B.transform, 1);

            var substract = csgA.Union(csgB);
            var newMesh = substract.ToMesh();

            Result = new GameObject("Union");
            var defObj = Result.AddComponent<DefaultObject>();
            var meshFilter = Result.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = newMesh;
            var renderer = Result.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { A.GetComponent<MeshRenderer>().sharedMaterial, B.GetComponent<MeshRenderer>().sharedMaterial };

            if (DeleteOriginal)
            {
                Object.DestroyImmediate(A);
                Object.DestroyImmediate(B);
            }

            stopWatch.Stop();
            defObj.GenerationTimeMS = stopWatch.ElapsedMilliseconds;
        }

        public void Intersect()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var csgA = new CSG.CSG();
            csgA.Construct(A.GetComponent<MeshFilter>().sharedMesh, A.transform, 0);

            var csgB = new CSG.CSG();
            csgB.Construct(B.GetComponent<MeshFilter>().sharedMesh, B.transform, 1);

            var substract = csgA.Intersect(csgB);
            var newMesh = substract.ToMesh();

            Result = new GameObject("Intersect");
            var defObj = Result.AddComponent<DefaultObject>();
            var meshFilter = Result.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = newMesh;
            var renderer = Result.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { A.GetComponent<MeshRenderer>().sharedMaterial, B.GetComponent<MeshRenderer>().sharedMaterial };

            if (DeleteOriginal)
            {
                Object.DestroyImmediate(A);
                Object.DestroyImmediate(B);
            }

            stopWatch.Stop();
            defObj.GenerationTimeMS = stopWatch.ElapsedMilliseconds;
        }
    }
}
