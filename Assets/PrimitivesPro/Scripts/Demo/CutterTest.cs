using System;
using System.Collections.Generic;
using PrimitivesPro.MeshCutting;
using PrimitivesPro.Primitives;
using UnityEngine;
using Plane = PrimitivesPro.Utils.Plane;
using Random = UnityEngine.Random;

internal class CutterTest : MonoBehaviour
{
    public GameObject OriginalObject = null;

    private GameObject cut0, cut1;
    private Plane plane;
    private float GenerationTimeMS;

    private int success = 0;
    private int failed = 0;
    private float minTime = float.MaxValue;
    private float maxTime = float.MinValue;
    private float sumTime = 0.0f;
    private int sumSteps;

    private bool test = false;
    private bool triangulate = true;

    private MeshCutter cutter;

    private void Start()
    {
        plane = new Plane(new Vector3(0, 1, 0), Vector3.zero);

        PrimitivesPro.MeshUtils.SetGameObjectActive(OriginalObject.gameObject, false);

        UnityEngine.Random.seed = System.DateTime.Now.Millisecond;

        cutter = new MeshCutter();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width-200, 0, 200, 30), "Test"))
        {
            test = true;
        }

        GUI.Label(new Rect(Screen.width - 200, 60, 200, 30), "Tris: " + OriginalObject.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3);
        GUI.Label(new Rect(Screen.width - 200, 80, 200, 30), "Verts: " + OriginalObject.GetComponent<MeshFilter>().sharedMesh.vertexCount);
        GUI.Label(new Rect(Screen.width - 200, 100, 200, 30), "Time: " + GenerationTimeMS);
        GUI.Label(new Rect(Screen.width - 200, 120, 200, 30), "MinTime: " + minTime);
        GUI.Label(new Rect(Screen.width - 200, 140, 200, 30), "MaxTime: " + maxTime);
        GUI.Label(new Rect(Screen.width - 200, 160, 200, 30), "AvgTime: " + (sumTime / sumSteps));
        GUI.Label(new Rect(Screen.width - 200, 180, 200, 30), "Success: " + success);
        GUI.Label(new Rect(Screen.width - 200, 200, 200, 30), "Failed: " + failed);
        GUI.Label(new Rect(Screen.width - 200, 220, 200, 30), "Failure ratio: " + ((float)failed / success) * 100.0f);

        triangulate = GUI.Toggle(new Rect(10, 10, 100, 30), triangulate, "Triangulate");
    }

    void RandomizePlane()
    {
        plane = new Plane(Random.onUnitSphere, Random.insideUnitSphere);
    }

    private void Update()
    {
        if (test)
        {
            RandomizePlane();
            Cut();
//            test = false;
        }
    }

    private void Cut()
    {
        if (cut0)
        {
            Destroy(cut0);
        }
        if (cut1)
        {
            Destroy(cut1);
        }

        PrimitivesPro.MeshUtils.SetGameObjectActive(OriginalObject.gameObject, false);

        // get mesh of the primitive
        var meshToCut = OriginalObject.GetComponent<MeshFilter>().sharedMesh;

        Mesh mesh0 = null, mesh1 = null;
        ContourData contours;

        try
        {
            // create 2 new objects
            GenerationTimeMS = cutter.Cut(meshToCut, OriginalObject.transform, plane, triangulate, out mesh0, out mesh1, out contours);

            if (true)
            {
                success++;

                if (GenerationTimeMS > maxTime)
                {
                    maxTime = GenerationTimeMS;
                }

                if (GenerationTimeMS < minTime)
                {
                    minTime = GenerationTimeMS;
                }

//                foreach (var vector3se in contours)
//                {
//                    foreach (var vector3 in vector3se)
//                    {
//                        var pri = PrimitivesPro.GameObjects.Sphere.Create(0.1f, 10, 0, 0, NormalsType.Vertex, PivotPosition.Center);
//                        pri.transform.position = OriginalObject.transform.position;
//                        pri.transform.rotation = OriginalObject.transform.rotation;
//                        pri.transform.localScale = OriginalObject.transform.localScale;
//
//                        pri.transform.Translate(vector3);
//                    }
//                }

                sumTime += GenerationTimeMS;
                sumSteps++;
            }
        }
        catch (Exception ex)
        {
            failed++;

            UnityEngine.Debug.Log("Exception!!! " + ex);
            UnityEngine.Debug.Break();

            return;
        }

        if (mesh0 != null)
        {
            cut0 = new GameObject(OriginalObject.name + "_cut0");
            var meshFilter0 = cut0.AddComponent<MeshFilter>();
            meshFilter0.sharedMesh = mesh0;

            var renderer0 = cut0.AddComponent<MeshRenderer>();
            renderer0.sharedMaterial = new Material(Shader.Find("Diffuse"));

            cut0.transform.position = OriginalObject.transform.position;
            cut0.transform.rotation = OriginalObject.transform.rotation;
            cut0.transform.localScale = OriginalObject.transform.localScale;

            cut0.transform.position += plane.Normal*1f;
        }

        if (mesh1 != null)
        {
            cut1 = new GameObject(OriginalObject.name + "_cut1");
            var meshFilter1 = cut1.AddComponent<MeshFilter>();
            meshFilter1.sharedMesh = mesh1;

            var renderer1 = cut1.AddComponent<MeshRenderer>();
            renderer1.sharedMaterial = new Material(Shader.Find("Diffuse"));

            cut1.transform.position = OriginalObject.transform.position;
            cut1.transform.rotation = OriginalObject.transform.rotation;
            cut1.transform.localScale = OriginalObject.transform.localScale;

            cut1.transform.position -= plane.Normal*1f;
        }
    }
}
