// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

// by uncommenting this line you will be able to use boolean operation feature in this package
// Note that this algorithm is experimental and not very stable, I don't recommend to use it
// for production
//#define BOOLEAN_EXPERIMENTAL
#if BOOLEAN_EXPERIMENTAL

using PrimitivesPro.Editor;
using PrimitivesPro.Primitives;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrimitivesPro.GameObjects.BooleanOp))]
public class CreateBooleanOp : Editor
{
    private object selection;

    [MenuItem(MenuDefinition.Boolean)]
    static void Create()
    {
        Selection.activeGameObject = PrimitivesPro.GameObjects.BooleanOp.Create().gameObject;
    }

    public override void OnInspectorGUI()
    {
        var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.BooleanOp>();

        if (target != obj)
        {
            return;
        }

        obj.A = EditorGUILayout.ObjectField("operand A", obj.A, typeof(GameObject), true) as GameObject;
        obj.B = EditorGUILayout.ObjectField("operand B", obj.B, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Separator();

        Utils.Toggle("Delete original objects", ref obj.DeleteOriginal);

        EditorGUILayout.Separator();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Union"))
        {
            obj.Union();
        }

        if (GUILayout.Button("Substract"))
        {
            obj.Substract();
        }

        if (GUILayout.Button("Intersect"))
        {
            obj.Intersect();
        }

        if (GUILayout.Button("Inverse"))
        {
            obj.Inverse();
        }

//        if (GUILayout.Button("TEST"))
//        {
//            obj.Test();
//        }

        GUILayout.EndHorizontal();

        Utils.StatWindow(obj.Result);
    }
}

#endif
