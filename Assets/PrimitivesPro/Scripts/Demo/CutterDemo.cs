using System;
using System.Collections;
using PrimitivesPro.MeshCutting;
using UnityEngine;
using Plane = PrimitivesPro.Utils.Plane;
using Random = UnityEngine.Random;

internal class CutterDemo : MonoBehaviour
{
    public GameObject[] OriginalObjects = null;

    private GameObject OriginalObject;
    private int objectIdx;
    private GameObject cut0, cut1;
    private Plane plane;
    private float GenerationTimeMS;

    private int success = 0;
    private float minTime = float.MaxValue;
    private float maxTime = float.MinValue;
    private float sumTime = 0.0f;
    private int sumSteps;
    private float cutTimeout;
    private bool triangulate = true;
    private Vector3[] tweenAmount = new Vector3[2];
    private float targetTweenTimeout;

    private MeshCutter cutter;

    private void Start()
    {
        foreach (var obj in OriginalObjects)
        {
            PrimitivesPro.MeshUtils.SetGameObjectActive(obj.gameObject, false);
        }

        OriginalObject = OriginalObjects[objectIdx];

        plane = new Plane(new Vector3(0, 1, 0), Vector3.zero);

        UnityEngine.Random.seed = System.DateTime.Now.Millisecond;

        cutter = new MeshCutter();
    }

    private void OnGUI()
    {
        GUI.Window(0, new Rect(10, /*300*/10, 300, 150), wnd, "Statistics");

        if (GUI.Button(new Rect(Screen.width - 130, 20, 60, 30), "Next"))
        {
            NextObject();
        }

        if (GUI.Button(new Rect(Screen.width - 200, 20, 60, 30), "Prev"))
        {
            PrevObject();
        }
    }

    void wnd(int id)
    {
        GUI.Label(new Rect(10, 20, 200, 30), "Triangles: " + OriginalObject.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3);
        GUI.Label(new Rect(10, 40, 200, 30), "Vertices: " + OriginalObject.GetComponent<MeshFilter>().sharedMesh.vertexCount);
        GUI.Label(new Rect(10, 60, 300, 30), "Cutting time: " + GenerationTimeMS + " [ms]");
        GUI.Label(new Rect(10, 80, 300, 30), "Avg cutting time: " + (sumTime / sumSteps) + " [ms]");
        triangulate = GUI.Toggle(new Rect(10, 110, 300, 30), triangulate, "Triangulate holes");
    }

    void Reset()
    {
        Destroy(cut0);
        Destroy(cut1);

        cutTimeout = 0.0f;

        sumTime = 0.0f;
        sumSteps = 0;
    }

    void NextObject()
    {
        objectIdx++;

        if (objectIdx >= OriginalObjects.Length)
        {
            objectIdx = 0;
        }

        PrimitivesPro.MeshUtils.SetGameObjectActive(OriginalObject.gameObject, false);
        OriginalObject = OriginalObjects[objectIdx];
        PrimitivesPro.MeshUtils.SetGameObjectActive(OriginalObject.gameObject, true);

        Reset();
    }

    void PrevObject()
    {
        objectIdx--;

        if (objectIdx < 0)
        {
            objectIdx = OriginalObjects.Length - 1;
        }

        PrimitivesPro.MeshUtils.SetGameObjectActive(OriginalObject.gameObject, false);
        OriginalObject = OriginalObjects[objectIdx];
        PrimitivesPro.MeshUtils.SetGameObjectActive(OriginalObject.gameObject, true);

        Reset();
    }

    void RandomizePlane()
    {
        plane = new Plane(Random.insideUnitCircle, Vector3.zero);
    }

    private void Update()
    {
        cutTimeout -= Time.deltaTime;

        if (cutTimeout < 0.0f)
        {
            RandomizePlane();
            Cut();
        }
        else if (targetTweenTimeout >= 0)
        {
            if (cut0 != null)
            {
                cut0.gameObject.transform.Translate(tweenAmount[0]);
                cut1.gameObject.transform.Translate(tweenAmount[1]);
            }

            targetTweenTimeout -= Time.deltaTime;
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

        try
        {
            ContourData contour;

            // create 2 new objects
            GenerationTimeMS = cutter.Cut(OriginalObject.gameObject, plane, triangulate, false, out cut0, out cut1, out contour);

            success++;

            if (GenerationTimeMS > maxTime)
            {
                maxTime = GenerationTimeMS;
            }

            if (GenerationTimeMS < minTime)
            {
                minTime = GenerationTimeMS;
            }

            sumTime += GenerationTimeMS;
            sumSteps++;
        }
        catch (Exception)
        {
            Debug.Log("Cutter exception!");
            return;
        }

        if (cut0 != null)
        {
            tweenAmount[0] = plane.Normal*0.02f;
            tweenAmount[1] = plane.Normal*-0.02f;
            targetTweenTimeout = 0.5f;
            cutTimeout = 0.5f;
        }
    }
}
