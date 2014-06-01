// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Editor;
using PrimitivesPro.Primitives;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrimitivesPro.GameObjects.Arc))]
public class CreateArc : CreatePrimitive
{
    private bool useCube;
    private bool useRatio;
    private bool useFlipNormals;
    private bool useFlipUVMapping;
    private bool useSameHeight;
    private object selection;
    private Vector3 lastControlPoint;

    [MenuItem(MenuDefinition.Arc)]
    static void Create()
    {
        var obj = PrimitivesPro.GameObjects.Arc.Create(1, 1, 1, 1, 10, PivotPosition.Botttom);
        obj.SaveStateAll();

        Selection.activeGameObject = obj.gameObject;
    }

    public override void OnInspectorGUI()
    {
        var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.Arc>();

        if (target != obj)
        {
            return;
        }

        Utils.Toggle("Show scene handles", ref obj.showSceneHandles);
        bool colliderChange = Utils.MeshColliderSelection(obj);

        EditorGUILayout.Separator();

        useFlipNormals = obj.flipNormals;
        useFlipUVMapping = obj.flipUVMapping;
        bool uiChange = false;
        var oldwidth = obj.width;
        var oldLength1 = obj.height1;
        var oldLength2 = obj.height2;

        uiChange |= Utils.SliderEdit("Width", 0, 100, ref obj.width);
        uiChange |= Utils.SliderEdit("Height1", 0, 100, ref obj.height1);
        uiChange |= Utils.SliderEdit("Height2", 0, 100, ref obj.height2);
        uiChange |= Utils.SliderEdit("Depth", 0, 100, ref obj.depth);
        uiChange |= Utils.Toggle("Cube", ref useCube);
        uiChange |= Utils.Toggle("Same height", ref useSameHeight);
        EditorGUILayout.Separator();

        if (useCube)
        {
            if (oldwidth != obj.width)
            {
                obj.depth = obj.height1 = obj.height2 = obj.width;
            }
            else if (oldLength1 != obj.height1)
            {
                obj.width = obj.depth = obj.height2 = obj.height1;
            }
            else if (oldLength2 != obj.height2)
            {
                obj.width = obj.depth = obj.height1 = obj.height2;
            }
            else
            {
                obj.width = obj.height1 = obj.height2 = obj.depth;
            }
        }
        else if (useSameHeight)
        {
            if (oldLength1 != obj.height1)
            {
                obj.height2 = obj.height1;
            }
            else if (oldLength2 != obj.height2)
            {
                obj.height1 = obj.height2;
            }
        }

        uiChange |= Utils.SliderEdit("Arc segments", 1, 100, ref obj.arcSegments);
        EditorGUILayout.Separator();
        uiChange |= Utils.PivotPosition(ref obj.pivotPosition);

        uiChange |= Utils.Toggle("Flip normals", ref useFlipNormals);
        uiChange |= Utils.Toggle("Share material", ref obj.shareMaterial);
        uiChange |= Utils.Toggle("Fit collider", ref obj.fitColliderOnChange);

        Utils.StatWindow(Selection.activeGameObject);

        Utils.ShowButtons<PrimitivesPro.GameObjects.Arc>(this);

        if (uiChange || colliderChange)
        {
            if (obj.generationMode == 0 && !colliderChange)
            {
                obj.flipUVMapping = useFlipUVMapping;
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
