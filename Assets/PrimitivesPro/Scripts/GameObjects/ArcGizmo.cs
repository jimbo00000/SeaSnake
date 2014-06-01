// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using PrimitivesPro.Primitives;
using UnityEngine;

namespace PrimitivesPro.GameObjects
{
    /// <summary>
    /// class for creating Arc gizmo helper
    /// </summary>
    public class ArcGizmo : MonoBehaviour
    {
        public static ArcGizmo Create()
        {
            var gizmoObject = new GameObject("ArcGizmoPro");
            var gizmo = gizmoObject.AddComponent<ArcGizmo>();

            return gizmo;
        }
    }
}
