// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Editor;
using PrimitivesPro.Primitives;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrimitivesPro.GameObjects.ArcGizmo))]
public class ArcGizmoEditor : Editor
{
    private Vector3 lastPos;

    void OnSceneGUI()
    {
        if (Selection.activeGameObject)
        {
            var obj = Selection.activeGameObject.GetComponent<PrimitivesPro.GameObjects.ArcGizmo>();

            if (obj)
            {
                if ((lastPos - obj.transform.position).magnitude > Mathf.Epsilon)
                {
                    var parent = obj.transform.parent;

                    if (parent)
                    {
                        var arc = parent.gameObject.GetComponent<PrimitivesPro.GameObjects.Arc>();

                        if (arc)
                        {
                            arc.GenerateGeometry();
                        }
                    }
                }

                lastPos = obj.transform.position;
            }
        }
    }
}
