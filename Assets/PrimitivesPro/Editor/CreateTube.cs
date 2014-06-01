// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Editor;
using PrimitivesPro.Primitives;
using UnityEditor;

[CustomEditor(typeof(PrimitivesPro.GameObjects.Tube))]
public class CreateTube : CreatePrimitive
{
    private bool useFlipNormals = false;

    [MenuItem(MenuDefinition.Tube)]
    static void Create()
    {
        var obj = PrimitivesPro.GameObjects.Tube.Create(0.5f, 1f, 2, 20, 1, 0.0f, false, NormalsType.Vertex, PivotPosition.Center);
        obj.SaveStateAll();

        Selection.activeGameObject = obj.gameObject;
    }

    public override void OnInspectorGUI()
    {
        var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.Tube>();

        if (target != obj)
        {
            return;
        }

        Utils.Toggle("Show scene handles", ref obj.showSceneHandles);
        bool colliderChange = Utils.MeshColliderSelection(obj);

        EditorGUILayout.Separator();

        useFlipNormals = obj.flipNormals;
        bool uiChange = false;

        uiChange |= Utils.SliderEdit("Inner radius", 0, 100, ref obj.radius0);
        uiChange |= Utils.SliderEdit("Outer radius", 0, 100, ref obj.radius1);
        uiChange |= Utils.SliderEdit("Height", 0, 100, ref obj.height);

        if (obj.radius0 > obj.radius1)
        {
            obj.radius0 = obj.radius1;
        }

        EditorGUILayout.Separator();

        uiChange |= Utils.SliderEdit("Sides", 3, 100, ref obj.sides);
        uiChange |= Utils.SliderEdit("Height segments", 1, 100, ref obj.heightSegments);
        uiChange |= Utils.SliderEdit("Slice", 0, 360, ref obj.slice);
        uiChange |= Utils.Toggle("Radial mapping", ref obj.radialMapping);

        EditorGUILayout.Separator();

        uiChange |= Utils.NormalsType(ref obj.normalsType);
        uiChange |= Utils.PivotPosition(ref obj.pivotPosition);

        uiChange |= Utils.Toggle("Flip normals", ref useFlipNormals);
        uiChange |= Utils.Toggle("Share material", ref obj.shareMaterial);
        uiChange |= Utils.Toggle("Fit collider", ref obj.fitColliderOnChange);

        Utils.StatWindow(Selection.activeGameObject);

        Utils.ShowButtons<PrimitivesPro.GameObjects.Tube>(this);

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
