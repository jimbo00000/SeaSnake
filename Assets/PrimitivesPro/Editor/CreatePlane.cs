// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Editor;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrimitivesPro.GameObjects.PlaneObject))]
public class CreatePlane : CreatePrimitive
{
    private bool useRatio = false;
    private bool useSquarePlane = false;
    private bool useFlipNormals = false;

    [MenuItem(MenuDefinition.Plane)]
    static void Create()
    {
        var obj = PrimitivesPro.GameObjects.PlaneObject.Create(1, 1, 1, 1);
        obj.SaveStateAll();

        Selection.activeGameObject = obj.gameObject;
    }

    protected override bool ShowHeightHandles()
    {
        return false;
    }

    public override void OnInspectorGUI()
    {
        var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.PlaneObject>();

        if (target != obj)
        {
            return;
        }

        Utils.Toggle("Show scene handles", ref obj.showSceneHandles);
        bool colliderChange = Utils.MeshColliderSelection(obj);

        EditorGUILayout.Separator();

        useFlipNormals = obj.flipNormals;
        bool uiChange = false;
        var oldwidth = obj.width;

        uiChange |= Utils.SliderEdit("Width", 0, 100, ref obj.width);
        uiChange |= Utils.SliderEdit("Length", 0, 100, ref obj.length);

        uiChange |= Utils.Toggle("Square", ref useSquarePlane);
        EditorGUILayout.Separator();

        if (useSquarePlane)
        {
            if (oldwidth != obj.width)
            {
                obj.length = obj.width;
            }
            else
            {
                obj.width = obj.length;
            }
        }

        var oldWs = obj.widthSegments;

        uiChange |= Utils.SliderEdit("Width segments", 1, 100, ref obj.widthSegments);
        uiChange |= Utils.SliderEdit("Length segments", 1, 100, ref obj.lengthSegments);
        uiChange |= Utils.Toggle("Uniform segments", ref useRatio);

        EditorGUILayout.Separator();

        if (useRatio)
        {
            if (oldWs != obj.widthSegments)
            {
                var ratio = obj.length / obj.width;
                obj.lengthSegments = (int)(obj.widthSegments * ratio);
                obj.lengthSegments = Mathf.Clamp(obj.lengthSegments, 1, 250);
            }
            else
            {
                var ratio = obj.width / obj.length;
                obj.widthSegments = (int)(obj.lengthSegments * ratio);
                obj.widthSegments = Mathf.Clamp(obj.widthSegments, 1, 250);
            }
        }

        uiChange |= Utils.Toggle("Flip normals", ref useFlipNormals);
        uiChange |= Utils.Toggle("Share material", ref obj.shareMaterial);
        uiChange |= Utils.Toggle("Fit collider", ref obj.fitColliderOnChange);

        Utils.StatWindow(Selection.activeGameObject);

        Utils.ShowButtons<PrimitivesPro.GameObjects.PlaneObject>(this);

        if (uiChange || colliderChange)
        {
            if (obj.generationMode == 0 && !colliderChange)
            {
                obj.GenerateGeometry();

                if (useFlipNormals)
                {
                    obj.FlipNormals();
                }
            }
            else
            {
                obj.GenerateColliderGeometry();
            }
        }
    }
}
