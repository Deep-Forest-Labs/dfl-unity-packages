//#define ENABLE_ASSETDATABASE_REPAIRS
#if ENABLE_ASSETDATABASE_REPAIRS
#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DeepForestLabs.Logger;
using DeepForestLabs.Data;
using DeepForestLabs.MVC.Views;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Utils
{
    internal static partial class AssetDatabaseAuditUtil
    {
        private const string kOptionalTempSuffix = "OptionalTemp";

        private const BindingFlags kFieldInfoFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static void RepairSerializedReferences(Object instance)
        {
            RepairSerializedReferencesRecursive(instance);
        }
        
        private static void RepairSerializedReferencesRecursive(object instance)
        {
            Type t = instance.GetType();
            HashSet<FieldInfo> serializedFields = new();
            HashSet<FieldInfo> serializeReferences = new();
            foreach (FieldInfo fieldInfo in t.GetFields(kFieldInfoFlags))
            {
                Type ft = fieldInfo.FieldType;
                if (ft.IsArray)
                {
                    Array? array = fieldInfo.GetValue(instance) as Array;
                    if (array == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < array.Length; i++)
                    {
                        object? value = array.GetValue(i);
                        if (value == null)
                        {
                            continue;
                        }

                        RepairSerializedReferencesRecursive(value);
                    }
                    continue;
                }

                if (ft.IsGenericType && typeof(IList).IsAssignableFrom(ft.GetGenericTypeDefinition()))
                {
                    IList? list = fieldInfo.GetValue(instance) as IList;
                    if (list == null)
                    {
                        continue;
                    }
                    
                    foreach (var value in list)
                    {
                        if (value == null)
                        {
                            continue;
                        }
                        RepairSerializedReferencesRecursive(value);
                    }
                    continue;
                }
                
                
                if (!typeof(AssetRef).IsAssignableFrom(ft))
                {
                    continue;
                }

                if (fieldInfo.GetCustomAttribute<SerializeReference>() != null)
                {
                    Log.Assert(fieldInfo.GetCustomAttribute<OptionalAttribute>() != null, "fieldInfo.GetCustomAttribute<OptionalAttribute>() != null");
                    serializeReferences.Add(fieldInfo);
                    continue;
                }

                if (fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null)
                {
                    serializedFields.Add(fieldInfo);
                }
            }

            foreach (FieldInfo fieldInfo in serializeReferences)
            {
                if (!fieldInfo.Name.EndsWith(kOptionalTempSuffix))
                {
                    continue;
                }

                string serializedFieldName = fieldInfo.Name.Replace(kOptionalTempSuffix, "");
                FieldInfo? serializedFieldInfo =
                    serializedFields.FirstOrDefault(fi => fi.Name.Equals(serializedFieldName));
                if (serializedFieldInfo == null)
                {
                    continue;
                }
                
                AssetRef? assetRef = serializedFieldInfo.GetValue(instance) as AssetRef;
                if (assetRef == null || !assetRef.IsValid())
                {
                    fieldInfo.SetValue(instance, null);
                }
                else
                {
                    fieldInfo.SetValue(instance, assetRef);
                }
            }
        }
        
        private static void RepairSerializedReference(string[] paths)
        {
            try
            {
                int total = paths.Length;
                for (int i = 0; i < total; i++)
                {
                    string path = paths[i];

                    if (path.StartsWith("Packages"))
                    {
                        continue;
                    }

                    EditorUtility.DisplayProgressBar(
                        "Repairing SerializedReferences in ",
                        path,
                        total == 0 ? 1f : (float)i / total);

                    Scene? scene = null;
                    GameObject? prefab = null;
                    try
                    {
                        scene = null;
                        prefab = null;
                        bool modified = false;
                        Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                        if (type == typeof(SceneAsset))
                        {
                            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                            foreach (GameObject? go in scene.Value.GetRootGameObjects())
                            {
                                foreach (ValidatedBehaviour c in go.GetComponentsInChildren<ValidatedBehaviour>(true))
                                {
                                    if (c == null)
                                    {
                                        continue;
                                    }

                                    RepairSerializedReferences(c);

                                    EditorUtility.SetDirty(c);
                                    modified = true;
                                }
                            }

                            if (modified)
                            {
                                EditorSceneManager.SaveScene(scene.Value);
                            }
                        }
                        else if (type == typeof(GameObject))
                        {
                            prefab = PrefabUtility.LoadPrefabContents(path);
                            if (prefab == null)
                            {
                                continue;
                            }

                            // Find all ValidatedViews components (including inactive & nested)
                            ValidatedBehaviour[]? comps = prefab.GetComponentsInChildren<ValidatedBehaviour>(true);
                            if (comps == null || comps.Length == 0)
                            {
                                continue;
                            }

                            foreach (ValidatedBehaviour? c in comps)
                            {
                                if (c == null)
                                {
                                    continue; // missing script guard
                                }

                                RepairSerializedReferences(c);

                                EditorUtility.SetDirty(c);
                                modified = true;
                            }

                            if (modified)
                            {
                                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                            }
                        }
                        else if (typeof(ScriptableObject).IsAssignableFrom(type))
                        {
                            ValidatedData? validatedData = AssetDatabase.LoadAssetAtPath<ValidatedData>(path);
                            if (validatedData != null)
                            {
                                RepairSerializedReferences(validatedData);

                                EditorUtility.SetDirty(validatedData);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    finally
                    {
                        if (scene != null)
                        {
                            EditorSceneManager.CloseScene(scene.Value, true);
                        }

                        if (prefab != null)
                        {
                            PrefabUtility.UnloadPrefabContents(prefab);
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("Tools/AssetDatabase/Repair/All", false, 1)]
        private static void RepairSerializedReferences()
        {
            List<string> guids = AssetDatabase.FindAssets("t:Prefab t:ScriptableObject")
                .ToList();
            guids.AddRange(AssetDatabase.FindAssets("t:Scene"));
                    
            RepairSerializedReference(guids.Select(AssetDatabase.GUIDToAssetPath).ToArray());
        }

        [MenuItem("Tools/AssetDatabase/Repair/Selected", false, 2)]
        private static void RepairDooberRewardInSelectedPrefabs()
        {
            RepairSerializedReference(GetSelectedPaths());
        }
    }
}
#endif