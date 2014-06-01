// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using PrimitivesPro.Editor;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrimitivesPro.GameObjects.DefaultObject))]
public class CustomDefaultObject : Editor
{
    private bool useFlipNormals;

    public override void OnInspectorGUI()
    {
        var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.DefaultObject>();

        if (target != obj)
        {
            return;
        }

        useFlipNormals = obj.flipNormals;
        bool uiChange = false;

        uiChange |= Utils.Toggle("Flip normals", ref useFlipNormals);
        uiChange |= Utils.Toggle("Share material", ref obj.shareMaterial);
        uiChange |= Utils.Toggle("Fit collider", ref obj.fitColliderOnChange);

        Utils.StatWindow(Selection.activeGameObject);

        Utils.ShowButtons<PrimitivesPro.GameObjects.DefaultObject>(this);

        if (uiChange)
        {
            if (useFlipNormals)
            {
                obj.FlipNormals();
            }
        }
    }
}
