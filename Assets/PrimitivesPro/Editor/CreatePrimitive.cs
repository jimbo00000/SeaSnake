// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Editor;
using PrimitivesPro.Primitives;
using UnityEditor;
using UnityEngine;

public class CreatePrimitive : Editor
{
    private Vector3 top;
    private Vector3 bottom;
    private Vector3 width0, width1;
    private Vector3 length0, length1;

    private const float handleSize = 0.06f;

    protected virtual bool ShowHeightHandles()
    {
        return true;
    }

    protected virtual bool ShowWidthHandles()
    {
        return true;
    }

    protected virtual bool ShowLengthHandles()
    {
        return true;
    }

    protected virtual void OnSceneGUI()
    {
        var obj = Selection.activeGameObject;

        if (obj)
        {
            var primitive = obj.GetComponent<PrimitivesPro.GameObjects.BaseObject>();

            if (primitive && !primitive.showSceneHandles)
            {
                return;
            }

            // get mesh
            var meshFilter = obj.GetComponent<MeshFilter>();

            if (meshFilter && meshFilter.sharedMesh)
            {
                var bound = meshFilter.sharedMesh.bounds;

                var showHeight = ShowHeightHandles();

                if (showHeight)
                {
                    top = bound.center + Vector3.up * bound.size.y / 2;
                    bottom = bound.center - Vector3.up * bound.size.y / 2;

                    top = obj.transform.TransformPoint(top);
                    bottom = obj.transform.TransformPoint(bottom);

                    Handles.color = Color.green;

                    var t0 = Handles.FreeMoveHandle(top,
                        Quaternion.identity,
                        HandleUtility.GetHandleSize(obj.transform.position) * handleSize,
                        Vector3.zero,
                        Handles.DotCap);

                    var b0 = Handles.FreeMoveHandle(bottom,
                                Quaternion.identity,
                                HandleUtility.GetHandleSize(obj.transform.position) * handleSize,
                                Vector3.zero,
                                Handles.DotCap);

                    showHeight = Mathf.Abs((t0 - top).sqrMagnitude) > Mathf.Epsilon ||
                                 Mathf.Abs((b0 - bottom).sqrMagnitude) > Mathf.Epsilon;

                    top = t0;
                    bottom = b0;
                }

                var showWidth = ShowWidthHandles();

                if (showWidth)
                {
                    width0 = bound.center + new Vector3(1, 0, 0) * bound.size.x / 2;
                    width1 = bound.center - new Vector3(1, 0, 0) * bound.size.x / 2;

                    width0 = obj.transform.TransformPoint(width0);
                    width1 = obj.transform.TransformPoint(width1);

                    Handles.color = Color.green;

                    Vector3 w0 = width0;
                    Vector3 w1 = width1;

                    if (ShowLengthHandles())
                    {
                        w0 = Handles.FreeMoveHandle(width0,
                            Quaternion.identity,
                            HandleUtility.GetHandleSize(obj.transform.position) * handleSize,
                            Vector3.zero,
                            Handles.DotCap);

                        w1 = Handles.FreeMoveHandle(width1,
                                    Quaternion.identity,
                                    HandleUtility.GetHandleSize(obj.transform.position) * handleSize,
                                    Vector3.zero,
                                    Handles.DotCap);
                    }

                    length0 = bound.center + new Vector3(0, 0, 1) * bound.size.z / 2;
                    length1 = bound.center - new Vector3(0, 0, 1) * bound.size.z / 2;

                    length0 = obj.transform.TransformPoint(length0);
                    length1 = obj.transform.TransformPoint(length1);

                    Handles.color = Color.green;

                    Vector3 l0 = length0;
                    Vector3 l1 = length1;

                    {
                        l0 = Handles.FreeMoveHandle(length0,
                            Quaternion.identity,
                            HandleUtility.GetHandleSize(obj.transform.position) * handleSize,
                            Vector3.zero,
                            Handles.DotCap);

                        l1 = Handles.FreeMoveHandle(length1,
                                    Quaternion.identity,
                                    HandleUtility.GetHandleSize(obj.transform.position) * handleSize,
                                    Vector3.zero,
                                    Handles.DotCap);
                    }

                    showWidth = Mathf.Abs((w0 - width0).sqrMagnitude) > Mathf.Epsilon ||
                                Mathf.Abs((w1 - width1).sqrMagnitude) > Mathf.Epsilon ||
                                Mathf.Abs((l0 - length0).sqrMagnitude) > Mathf.Epsilon ||
                                Mathf.Abs((l1 - length1).sqrMagnitude) > Mathf.Epsilon;

                    width0 = w0;
                    width1 = w1;
                    length0 = l0;
                    length1 = l1;
                }

                if (primitive)
                {
                    if (showHeight)
                    {
                        primitive.SetHeight((top - bottom).magnitude);
                    }

                    if (showWidth)
                    {
                        primitive.SetWidth((width0 - width1).magnitude, (length0 - length1).magnitude);
                    }

                    if (showHeight || showWidth)
                    {
                        primitive.GenerateGeometry();
                    }
                }
            }
        }
    }
}
