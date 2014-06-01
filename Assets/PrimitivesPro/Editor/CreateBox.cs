// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Editor;
using PrimitivesPro.Primitives;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrimitivesPro.GameObjects.Box))]
public class CreateBox : CreatePrimitive
{
    private bool useCube;
    private bool useRatio;
    private bool useFlipNormals;
    private bool useFlipUVMapping;
    private object selection;

    [MenuItem(MenuDefinition.Box)]
    static void Create()
    {
        var obj = PrimitivesPro.GameObjects.Box.Create(1, 1, 1, 1, 1, 1, false, null, PivotPosition.Center);
        obj.SaveStateAll();

        Selection.activeGameObject = obj.gameObject;
    }

    public override void OnInspectorGUI()
    {
        var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.Box>();

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
        var oldLength = obj.height;

        uiChange |= Utils.SliderEdit("Width", 0, 100, ref obj.width);
        uiChange |= Utils.SliderEdit("Height", 0, 100, ref obj.height);
        uiChange |= Utils.SliderEdit("Depth", 0, 100, ref obj.depth);

        for (var i=0; i<obj.edgeOffsets.Length; i++)
        {
            uiChange |= Utils.SliderEdit("EdgeOffset" + i, -100, 100, ref obj.edgeOffsets[i]);
        }

        uiChange |= Utils.Toggle("Cube", ref useCube);
        EditorGUILayout.Separator();

        if (useCube)
        {
            if (oldwidth != obj.width)
            {
                obj.depth = obj.height = obj.width;
            }
            else if (oldLength != obj.height)
            {
                obj.width = obj.depth = obj.height;
            }
            else
            {
                obj.width = obj.height = obj.depth;
            }

            for (var i = 0; i < obj.edgeOffsets.Length; i++)
            {
                obj.edgeOffsets[i] = 0.0f;
            }
        }

        var oldWs = obj.widthSegments;
        var oldLs = obj.heightSegments;

        uiChange |= Utils.SliderEdit("Width segments", 1, 100, ref obj.widthSegments);
        uiChange |= Utils.SliderEdit("Height segments", 1, 100, ref obj.heightSegments);
        uiChange |= Utils.SliderEdit("Length segments", 1, 100, ref obj.depthSegments);
        uiChange |= Utils.Toggle("Uniform segments", ref useRatio);
        EditorGUILayout.Separator();
        uiChange |= Utils.PivotPosition(ref obj.pivotPosition);

        if (useRatio)
        {
            if (oldWs != obj.widthSegments)
            {
                obj.heightSegments = (int)(obj.widthSegments * (obj.height / obj.width));
                obj.depthSegments = (int)(obj.widthSegments * (obj.depth / obj.width));
            }
            else if (oldLs != obj.heightSegments)
            {
                obj.widthSegments = (int)(obj.heightSegments * (obj.width / obj.height));
                obj.depthSegments = (int)(obj.heightSegments * (obj.depth / obj.height));
            }
            else
            {
                obj.widthSegments = (int)(obj.depthSegments * (obj.width / obj.depth));
                obj.heightSegments = (int)(obj.depthSegments * (obj.height / obj.depth));
            }
        }

        obj.heightSegments = Mathf.Clamp(obj.heightSegments, 1, 100);
        obj.widthSegments = Mathf.Clamp(obj.widthSegments, 1, 100);
        obj.depthSegments = Mathf.Clamp(obj.depthSegments, 1, 100);

        uiChange |= Utils.Toggle("Flip normals", ref useFlipNormals);
        uiChange |= Utils.Toggle("Cube map", ref obj.cubeMap);
        uiChange |= Utils.Toggle("Flip UV mapping", ref useFlipUVMapping);
        uiChange |= Utils.Toggle("Share material", ref obj.shareMaterial);
        uiChange |= Utils.Toggle("Fit collider", ref obj.fitColliderOnChange);

        Utils.StatWindow(Selection.activeGameObject);

        Utils.ShowButtons<PrimitivesPro.GameObjects.Box>(this);

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
