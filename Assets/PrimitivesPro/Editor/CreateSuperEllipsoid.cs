// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Editor;
using PrimitivesPro.Primitives;
using UnityEditor;

[CustomEditor(typeof(PrimitivesPro.GameObjects.SuperEllipsoid))]
public class CreateSuperEllipsoid : CreatePrimitive
{
    private bool useCube = false;
    private bool useFlipNormals = false;

    [MenuItem(MenuDefinition.SuperEllipsoid)]
    static void Create()
    {
        var obj = PrimitivesPro.GameObjects.SuperEllipsoid.Create(1, 1, 1, 20, 0.5f, 1.0f, NormalsType.Vertex, PivotPosition.Center);
        obj.SaveStateAll();

        Selection.activeGameObject = obj.gameObject;
    }

    public override void OnInspectorGUI()
    {
        var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.SuperEllipsoid>();

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
        var oldLength = obj.height;

        uiChange |= Utils.SliderEdit("Width", 0, 100, ref obj.width);
        uiChange |= Utils.SliderEdit("Height", 0, 100, ref obj.height);
        uiChange |= Utils.SliderEdit("Depth", 0, 100, ref obj.length);
        uiChange |= Utils.Toggle("Cube", ref useCube);
        EditorGUILayout.Separator();

        if (useCube)
        {
            if (oldwidth != obj.width)
            {
                obj.length = obj.height = obj.width;
            }
            else if (oldLength != obj.height)
            {
                obj.width = obj.length = obj.height;
            }
            else
            {
                obj.width = obj.height = obj.length;
            }
        }

        uiChange |= Utils.SliderEdit("Segments", 0, 100, ref obj.segments);
        uiChange |= Utils.SliderEdit("N1", 0, 5, ref obj.n1);
        uiChange |= Utils.SliderEdit("N2", 0, 5, ref obj.n2);

        EditorGUILayout.Separator();

        uiChange |= Utils.NormalsType(ref obj.normalsType);
        uiChange |= Utils.PivotPosition(ref obj.pivotPosition);

        uiChange |= Utils.Toggle("Flip normals", ref useFlipNormals);
        uiChange |= Utils.Toggle("Share material", ref obj.shareMaterial);
        uiChange |= Utils.Toggle("Fit collider", ref obj.fitColliderOnChange);

        Utils.StatWindow(Selection.activeGameObject);

        Utils.ShowButtons<PrimitivesPro.GameObjects.SuperEllipsoid>(this);

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
