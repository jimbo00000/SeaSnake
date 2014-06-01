// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Editor;
using PrimitivesPro.Primitives;
using UnityEditor;

[CustomEditor(typeof(PrimitivesPro.GameObjects.Cone))]
public class CreateCone : CreatePrimitive
{
    private bool useFlipNormals = false;
    private bool hollow = false;

    [MenuItem(MenuDefinition.Cone)]
    static void Create()
    {
        var obj = PrimitivesPro.GameObjects.Cone.Create(2, 0, -1, 2, 20, 1, NormalsType.Vertex, PivotPosition.Center);
        obj.SaveStateAll();

        Selection.activeGameObject = obj.gameObject;
    }

    public override void  OnInspectorGUI()
    {
        var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.Cone>();

        if (target != obj)
        {
            return;
        }

        Utils.Toggle("Show scene handles", ref obj.showSceneHandles);
        bool colliderChange = Utils.MeshColliderSelection(obj);

        EditorGUILayout.Separator();

        useFlipNormals = obj.flipNormals;
        bool uiChange = false;

        hollow = obj.thickness >= 0;

        uiChange |= Utils.Toggle("Hollow cone", ref hollow);
        uiChange |= Utils.SliderEdit("Radius0", 0, 100, ref obj.radius0);
        uiChange |= Utils.SliderEdit("Radius1", 0, 100, ref obj.radius1);
        uiChange |= Utils.SliderEdit("Height", 0, 100, ref obj.height);

        if (hollow)
        {
            uiChange |= Utils.SliderEdit("Thickness", 0, 100, ref obj.thickness);
        }
        else
        {
            obj.thickness = -1;
        }

        EditorGUILayout.Separator();

        uiChange |= Utils.SliderEdit("Sides", 3, 100, ref obj.sides);
        uiChange |= Utils.SliderEdit("Height segments", 1, 100, ref obj.heightSegments);

        EditorGUILayout.Separator();

        uiChange |= Utils.NormalsType(ref obj.normalsType);
        uiChange |= Utils.PivotPosition(ref obj.pivotPosition);

        uiChange |= Utils.Toggle("Flip normals", ref useFlipNormals);
        uiChange |= Utils.Toggle("Share material", ref obj.shareMaterial);
        uiChange |= Utils.Toggle("Fit collider", ref obj.fitColliderOnChange);

        Utils.StatWindow(Selection.activeGameObject);

        Utils.ShowButtons<PrimitivesPro.GameObjects.Cone>(this);

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
