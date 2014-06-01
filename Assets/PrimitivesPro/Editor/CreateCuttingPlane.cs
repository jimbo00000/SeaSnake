// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using PrimitivesPro.Editor;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrimitivesPro.GameObjects.CuttingPlane))]
public class CreateCuttingPlane : Editor
{
    private bool useFlipNormals;

    [MenuItem(MenuDefinition.CuttingPlane)]
    static void Create()
    {
        Selection.activeGameObject = PrimitivesPro.GameObjects.CuttingPlane.Create(1).gameObject;
    }

    public override void OnInspectorGUI()
    {
        var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.CuttingPlane>();

        if (target != obj)
        {
            return;
        }

        useFlipNormals = obj.flipNormals;
        bool uiChange = false;

        uiChange |= Utils.SliderEdit("Size", 0, 100, ref obj.size);

        EditorGUILayout.Separator();

        uiChange |= Utils.Toggle("Flip normals", ref useFlipNormals);

        uiChange |= Utils.Toggle("Triangulate holes", ref obj.triangulateHoles);

        uiChange |= Utils.Toggle("Delete original object", ref obj.deleteOriginal);

        obj.cuttingObject = EditorGUILayout.ObjectField("Object to cut (optional)", obj.cuttingObject, typeof(GameObject), true) as GameObject;

        Utils.StatWindow(Selection.activeGameObject);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Cut"))
        {
            if (obj.cuttingObject != null && obj.cuttingObject.GetComponent<MeshFilter>())
            {
                obj.Cut(obj.cuttingObject, obj.triangulateHoles, obj.deleteOriginal);
            }
            else
            {
                var objects = FindObjectsOfType(typeof(GameObject));

                foreach (GameObject o in objects)
                {
                    if (o != obj.gameObject && PrimitivesPro.MeshUtils.IsGameObjectActive(o) && o.GetComponent<MeshFilter>())
                    {
                        obj.Cut(o, obj.triangulateHoles, obj.deleteOriginal);
                    }
                }
            }
        }

        GUILayout.EndHorizontal();

        if (uiChange)
        {
            obj.GenerateGeometry();

            if (useFlipNormals)
            {
                obj.FlipNormals();
            }
        }
    }
}
