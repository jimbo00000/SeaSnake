// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Editor;
using PrimitivesPro.Primitives;
using UnityEditor;

[CustomEditor(typeof(PrimitivesPro.GameObjects.TorusKnot))]
public class CreateTorusKnot : CreatePrimitive
{
    private bool useFlipNormals = false;

    [MenuItem(MenuDefinition.TorusKnot)]
    static void Create()
    {
        var obj = PrimitivesPro.GameObjects.TorusKnot.Create(1, 0.5f, 80, 20, 2, 3, NormalsType.Vertex, PivotPosition.Center);
        obj.SaveStateAll();

        Selection.activeGameObject = obj.gameObject;
    }

    protected override bool ShowWidthHandles()
    {
        return false;
    }

    public override void OnInspectorGUI()
    {
        var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.TorusKnot>();

        if (target != obj)
        {
            return;
        }

        Utils.Toggle("Show scene handles", ref obj.showSceneHandles);
        bool colliderChange = Utils.MeshColliderSelection(obj);

        EditorGUILayout.Separator();

        useFlipNormals = obj.flipNormals;
        bool uiChange = false;

        uiChange |= Utils.SliderEdit("Torus radius", 0, 100, ref obj.radius0);
        uiChange |= Utils.SliderEdit("Cone radius", 0, 100, ref obj.radius1);

        EditorGUILayout.Separator();

        uiChange |= Utils.SliderEdit("P", 1, 20, ref obj.P);
        uiChange |= Utils.SliderEdit("Q", 1, 20, ref obj.Q);

        EditorGUILayout.Separator();

        uiChange |= Utils.SliderEdit("Torus segments", 3, 250, ref obj.torusSegments);
        uiChange |= Utils.SliderEdit("Cone segments", 3, 100, ref obj.coneSegments);

        EditorGUILayout.Separator();

        uiChange |= Utils.NormalsType(ref obj.normalsType);
        uiChange |= Utils.PivotPosition(ref obj.pivotPosition);

        uiChange |= Utils.Toggle("Flip normals", ref useFlipNormals);
        uiChange |= Utils.Toggle("Share material", ref obj.shareMaterial);
        uiChange |= Utils.Toggle("Fit collider", ref obj.fitColliderOnChange);

        Utils.StatWindow(Selection.activeGameObject);

        Utils.ShowButtons<PrimitivesPro.GameObjects.TorusKnot>(this);

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
