// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.IO;
using UnityEditor;
using UnityEngine;
using PrimitivesPro.Primitives;

namespace PrimitivesPro.Editor
{
    static class Utils
    {
        /// <summary>
        /// display statistics window with information about mesh
        /// </summary>
        public static void StatWindow(GameObject primitive)
        {
            var trianglesCount = 0;
            var verticesCount = 0;
            var generationTime = 0.0f;

            if (primitive)
            {
                var mesh = primitive.GetComponent<MeshFilter>();

                if (mesh && mesh.sharedMesh)
                {
                    trianglesCount = mesh.sharedMesh.triangles.Length/3;
                    verticesCount = mesh.sharedMesh.vertexCount;

                    var primitivePro = primitive.GetComponent<GameObjects.BaseObject>();

                    generationTime = primitivePro.GenerationTimeMS;
                }
            }

            // make bacground texture
            var result = new Texture2D(1, 1);
            result.SetPixels(new []{new Color(0.5f, 0.5f, 0.5f)});
            result.Apply();

            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            var style = new GUIStyle();
            style.alignment = TextAnchor.MiddleLeft;
            style.border = new RectOffset(5, 5, 5, 5);
            style.margin = new RectOffset(6, 0, 0, 0);
            style.fixedWidth = 150;
            style.normal.background = result;

            GUILayout.Box("Triangle count: " + trianglesCount.ToString() + "\n" +
                            "Vertices count: " + verticesCount.ToString() + "\n" +
                            "Generation time: " + generationTime.ToString() + " [ms]", style);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// create slider with edit box for float values
        /// </summary>
        /// <param name="label">name of the slider</param>
        /// <param name="min">minimum value</param>
        /// <param name="max">maximum value</param>
        /// <param name="value">current value</param>
        /// <returns>true if value has been changed</returns>
        public static bool SliderEdit(string label, float min, float max, ref float value)
        {
            EditorGUILayout.BeginHorizontal();
            var oldValue = value;
            value = EditorGUILayout.FloatField(label, value);
            value = GUILayout.HorizontalSlider(value, min, max);
            EditorGUILayout.EndHorizontal();

            value = Mathf.Clamp(value, min, max);

            return oldValue != value;
        }

        /// <summary>
        /// create slider with edit box for float values
        /// </summary>
        /// <param name="label">name of the slider</param>
        /// <param name="value">current value</param>
        /// <returns>true if value has been changed</returns>
        public static bool Vector2Edit(string label, ref Vector2 value)
        {
            EditorGUILayout.BeginHorizontal();
            var oldValue = value;
            value = EditorGUILayout.Vector2Field(label, value);
            EditorGUILayout.EndHorizontal();
            return oldValue != value;
        }

        /// <summary>
        /// display options for mesh collider generation
        /// </summary>
        /// <param name="obj"></param>
        public static bool MeshColliderSelection(PrimitivesPro.GameObjects.BaseObject obj)
        {
            var change = false;

            if (Utils.Toggle("Generate mesh collider", ref obj.generateMeshCollider))
            {
                obj.AddMeshCollider(obj.generateMeshCollider);
                obj.SaveState(true);
                change = true;
            }

            var lastMode = obj.generationMode;

            if (change && !obj.generateMeshCollider)
            {
                if (obj.generationMode == 1)
                {
                    obj.LoadState(false);
                    obj.SaveState(true);
                }
            }

            if (obj.generateMeshCollider)
            {
                obj.generationMode = EditorGUILayout.Popup("Edit mode", obj.generationMode, new string[] { "Mesh", "Collider" });
            }
            else
            {
                obj.generationMode = 0;
            }

            if (change)
            {
                obj.SaveState(false);
            }

            if (!change && lastMode != obj.generationMode)
            {
                if (obj.generationMode == 0)
                {
                    obj.SaveState(true);
                    obj.LoadState(false);
                }
                else
                {
                    obj.SaveState(false);
                    obj.LoadState(true);
                }
            }

            return change;
        }

        /// <summary>
        /// create slider with edit box for int values
        /// </summary>
        /// <param name="label">name of the slider</param>
        /// <param name="min">minimum value</param>
        /// <param name="max">maximum value</param>
        /// <param name="value">current value</param>
        /// <returns>true if value has been changed</returns>
        public static bool SliderEdit(string label, int min, int max, ref int value)
        {
            EditorGUILayout.BeginHorizontal();
            var oldValue = value;
            value = EditorGUILayout.IntField(label, value);
            value = (int)GUILayout.HorizontalSlider(value, min, max);
            EditorGUILayout.EndHorizontal();

            value = Mathf.Clamp(value, min, max);

            return oldValue != value;
        }

        /// <summary>
        /// create toggle control
        /// </summary>
        /// <param name="label">name of the toggle</param>
        /// <param name="value">bool value of toggle</param>
        /// <returns>true if value has been changed</returns>
        public static bool Toggle(string label, ref bool value)
        {
            var oldValue = value;

            EditorGUILayout.BeginHorizontal();
            value = EditorGUILayout.Toggle(label, value);
            EditorGUILayout.EndHorizontal();

            return oldValue != value;
        }

        /// <summary>
        /// create combo box with pivot position values
        /// </summary>
        /// <param name="value">value of the pivot position</param>
        /// <returns>true if value has been changed</returns>
        public static bool PivotPosition(ref PivotPosition value)
        {
            var oldPivot = value;
            EditorGUILayout.BeginHorizontal();
            value = (PivotPosition)EditorGUILayout.EnumPopup("Pivot position", value);
            EditorGUILayout.EndHorizontal();
            return oldPivot != value;
        }

        /// <summary>
        /// craete combo box with normals type values
        /// </summary>
        /// <param name="value">value of normals type</param>
        /// <returns>true if value has been changed</returns>
        public static bool NormalsType(ref NormalsType value)
        {
            var oldNormalsType = value;
            EditorGUILayout.BeginHorizontal();
            value = (NormalsType)EditorGUILayout.EnumPopup("Normals type", value);
            EditorGUILayout.EndHorizontal();
            return oldNormalsType != value;
        }

        /// <summary>
        /// returns current editor selection (PrimitivePro only)
        /// </summary>
        public static T GetSelection<T>(UnityEditor.Editor editorWindow) where T : GameObjects.BaseObject
        {
            var obj = Selection.activeGameObject.GetComponent<T>();

            if (editorWindow.target != obj)
            {
                return null;
            }

            return obj;
        }

        public static void SaveMesh<T>(UnityEditor.Editor editor) where T : GameObjects.BaseObject
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save mesh"))
            {
                var selection = GetSelection<T>(editor);

                if (selection != null)
                {
                    var orginalMesh = selection.GetComponent<MeshFilter>().sharedMesh;
                    var newMesh = PrimitivesPro.MeshUtils.CopyMesh(orginalMesh);

                    var path = EditorUtility.SaveFilePanelInProject("Save mesh", "myMesh.asset", "asset", "Enter File Name");

                    if (path.Length != 0)
                    {
                        AssetDatabase.CreateAsset(newMesh, path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        public static void SavePrefab<T>(UnityEditor.Editor editor) where T : GameObjects.BaseObject
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Create prefab"))
            {
                var selection = GetSelection<T>(editor);

                if (selection != null)
                {
                    var orginalMesh = selection.GetComponent<MeshFilter>().sharedMesh;
                    var newMesh = PrimitivesPro.MeshUtils.CopyMesh(orginalMesh);

                    var path = EditorUtility.SaveFilePanelInProject("Create prefab", "myPrimitive.prefab", "prefab", "Enter File Name");

                    if (path.Length != 0)
                    {
                        var prefabName = Path.GetFileNameWithoutExtension(path);
                        var meshPath = Path.GetDirectoryName(path) + "/" + prefabName + ".asset";
                        var materialPath = Path.GetDirectoryName(path) + "/" + prefabName + ".mat";
                        var colliderMeshPath = Path.GetDirectoryName(path) + "/" + prefabName + "meshCollider.asset";
                        var usingMeshCollider = false;

                        // first save a mesh
                        AssetDatabase.CreateAsset(newMesh, meshPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        // save mesh collider mesh
                        if (selection.generateMeshCollider)
                        {
                            var collider = selection.GetComponent<MeshCollider>();

                            if (collider)
                            {
                                usingMeshCollider = true;
                                AssetDatabase.CreateAsset(collider.sharedMesh, colliderMeshPath);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }
                        }

                        // create a prefab
                        var obj = new GameObject("Primitive");
                        obj.AddComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh)) as Mesh;

                        // add mesh collider
                        if (usingMeshCollider)
                        {
                            obj.AddComponent<MeshCollider>().sharedMesh = AssetDatabase.LoadAssetAtPath(colliderMeshPath, typeof(Mesh)) as Mesh;
                        }

                        // save a material
                        if (selection.GetComponent<MeshRenderer>())
                        {
                            var material = selection.GetComponent<MeshRenderer>().sharedMaterial;

                            if (!AssetDatabase.IsMainAsset(material))
                            {
                                if (material)
                                {
                                    AssetDatabase.CreateAsset(material, materialPath);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();

                                    obj.AddComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
                                }
                            }
                            else
                            {
                                obj.AddComponent<MeshRenderer>().sharedMaterial = material;
                            }
                        }

                        obj.transform.localScale = selection.transform.localScale;

                        PrefabUtility.CreatePrefab(path, obj);
                        Object.DestroyImmediate(obj);
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        public static void DuplicateObject<T>(UnityEditor.Editor editor) where T : GameObjects.BaseObject
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Duplicate"))
            {
                var selection = GetSelection<T>(editor);

                if (selection != null)
                {
                    var primitive = selection.GetComponent<T>();
                    primitive.Duplicate(!primitive.shareMaterial);
                }
            }

            GUILayout.EndHorizontal();
        }

        public static void ExportToOBJ<T>(UnityEditor.Editor editor) where T : GameObjects.BaseObject
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Export to OBJ"))
            {
                var selection = GetSelection<T>(editor);

                if (selection != null)
                {
                    var primitive = selection.GetComponent<T>();

                    var meshFilter = primitive.GetComponent<MeshFilter>();

                    var path = EditorUtility.SaveFilePanel("Export to OBJ", "", primitive.name + ".obj", "obj");

                    if (path.Length != 0 && meshFilter)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(path);
                        var rootPath = Path.GetDirectoryName(path);

                        ObjExporter.MeshToFile(meshFilter, rootPath, fileName, true);
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        public static void FitCollider<T>(UnityEditor.Editor editor) where T : GameObjects.BaseObject
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Fit collider"))
            {
                var selection = GetSelection<T>(editor);

                if (selection != null)
                {
                    var primitive = selection.GetComponent<T>();
                    primitive.FitCollider();
                }
            }

            GUILayout.EndHorizontal();
        }

        public static void ShowButtons<T>(UnityEditor.Editor editor) where T : GameObjects.BaseObject
        {
            SaveMesh<T>(editor);
            SavePrefab<T>(editor);
            DuplicateObject<T>(editor);
            ExportToOBJ<T>(editor);
            FitCollider<T>(editor);
        }
    }
}
