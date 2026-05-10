#nullable enable
using System;
using System.Collections.Generic;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DeepForestLabs.Components
{
    public static class PrefabOverrideStripper
    {
        [MenuItem("Tools/Prefabs/Strip All Prefab Overrides")]
        private static void RevertOverrides()
        {
            AssetDatabase.StartAssetEditing();
            string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
            int total = prefabGUIDs.Length;

            for (int i = 0; i < total; i++)
            {
                string prefabGuid = prefabGUIDs[i];
                string path = "";
                GameObject? prefab = null;
                bool modified = false;
                try
                {
                    modified = false;
                    path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                    prefab = PrefabUtility.LoadPrefabContents(path);

                    List<GameObject> subPrefabs = new List<GameObject>();
                    RecurseSubPrefab(prefab, subPrefabs);

                    foreach (GameObject subPrefab in subPrefabs)
                    {
                        modified |= RevertOverrides(subPrefab);
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to revert prefab {path} overrides: {e.Message}");
                }
                finally
                {
                    if (prefab != null)
                    {
                        if (modified)
                        {
                            PrefabUtility.SaveAsPrefabAsset(prefab, path);
                        }
                        PrefabUtility.UnloadPrefabContents(prefab);
                    }
                }
                EditorUtility.DisplayProgressBar("Stripping Overrides", path, (float)i / total);
            }
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        private static void RecurseSubPrefab(GameObject parent, List<GameObject> subPrefabs)
        {
            //if (PrefabUtility.IsAddedGameObjectOverride(parent) || PrefabUtility.IsOutermostPrefabInstanceRoot(parent))
            if (PrefabUtility.IsOutermostPrefabInstanceRoot(parent))
            {
                subPrefabs.Add(parent);
            }

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                RecurseSubPrefab(parent.transform.GetChild(i).gameObject, subPrefabs);
            }
        }
        
        private static bool RevertOverrides(GameObject gameObject)
        {
            bool modified = false;
            List<ObjectOverride> objectOverrides = PrefabUtility
                .GetObjectOverrides(gameObject);

            foreach (ObjectOverride objectOverride in objectOverrides)
            {
                if (objectOverride == null)
                {
                    Debug.Log("No matching overrides to revert.");
                    continue;
                }

                SerializedObject targetSO = new SerializedObject(objectOverride.instanceObject);
                SerializedObject rootSO = new SerializedObject(objectOverride.GetAssetObject());
                SerializedProperty propTarget = targetSO.GetIterator();
                SerializedProperty propRoot = rootSO.GetIterator();

                if (!propTarget.NextVisible(true) || !propRoot.NextVisible(true))
                {
                    return false;
                }

                modified |= RecurseSync(propTarget, propRoot);
            }

            return modified;
        }
        
        private static bool RecurseSync(SerializedProperty propA, SerializedProperty propB)
        {
            bool modified = false;
            SerializedProperty? iteratorA = propA.Copy();
            SerializedProperty? iteratorB = propB.Copy();

            while (iteratorA.NextVisible(true) && iteratorB.NextVisible(true))
            {
                // Skip script reference
                if (iteratorA.name == "m_Script")
                {
                    continue;
                }

                // If the paths don't match, stop — this only works if tree structure is identical
                if (iteratorA.propertyPath != iteratorB.propertyPath)
                {
                    Debug.LogWarning($"Mismatched path: {iteratorA.propertyPath} vs {iteratorB.propertyPath}");
                    continue;
                }
                
                if (!iteratorA.editable || iteratorA.propertyPath.Contains("Module"))
                {
                    continue;
                }
                
                if (iteratorA.hasChildren && !iteratorA.isArray && iteratorA.propertyType == SerializedPropertyType.Generic)
                {
                    continue; // likely a struct like TrailModule
                }

                if (AreValuesEqual(iteratorA, iteratorB))
                {
                    PrefabUtility.RevertPropertyOverride(iteratorA, InteractionMode.UserAction);
                    Debug.Log($"Reverted: {iteratorA.propertyPath}");
                    modified = true;
                }
            }

            return modified;
        }
        
        private static bool AreValuesEqual(SerializedProperty a, SerializedProperty b)
        {
            if (a.propertyType != b.propertyType) return false;

            switch (a.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return a.intValue == b.intValue;
                case SerializedPropertyType.Boolean:
                    return a.boolValue == b.boolValue;
                case SerializedPropertyType.Float:
                    return Mathf.Approximately(a.floatValue, b.floatValue);
                case SerializedPropertyType.String:
                    return a.stringValue == b.stringValue;
                case SerializedPropertyType.Color:
                    return a.colorValue == b.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return a.objectReferenceEntityIdValue == b.objectReferenceEntityIdValue;
                case SerializedPropertyType.LayerMask:
                    return a.intValue == b.intValue;
                case SerializedPropertyType.Enum:
                    return a.enumValueIndex == b.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return a.vector2Value == b.vector2Value;
                case SerializedPropertyType.Vector3:
                    return a.vector3Value == b.vector3Value;
                case SerializedPropertyType.Vector4:
                    return a.vector4Value == b.vector4Value;
                case SerializedPropertyType.Rect:
                    return a.rectValue == b.rectValue;
                case SerializedPropertyType.Bounds:
                    return a.boundsValue == b.boundsValue;
                case SerializedPropertyType.Quaternion:
                    return a.quaternionValue == b.quaternionValue;
                case SerializedPropertyType.AnimationCurve:
                    return a.animationCurveValue.Equals(b.animationCurveValue);
                case SerializedPropertyType.ExposedReference:
                    return a.exposedReferenceValue == b.exposedReferenceValue;
                case SerializedPropertyType.Vector2Int:
                    return a.vector2IntValue == b.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return a.vector3IntValue == b.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return a.rectIntValue.Equals(b.rectIntValue);
                case SerializedPropertyType.BoundsInt:
                    return a.boundsIntValue == b.boundsIntValue;
                case SerializedPropertyType.Hash128:
                    return a.hash128Value == b.hash128Value;
                default:
                    return false;
            }
        }
    }
}