#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZLinq;
using System.Runtime.CompilerServices;
using DeepForestLabs.Logger;
using Cysharp.Text;
using DeepForestLabs.Data;
using DeepForestLabs.MVC.Views;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Utils
{
    internal static partial class AssetDatabaseAuditUtil
    {
        private const string RunAllMenuPath = "Tools/AssetDatabase/Audit/Serialized Fields/All";
        private const string RunSelectedMenuPath = "Tools/AssetDatabase/Audit/Serialized Fields/Selected";

        [MenuItem(RunAllMenuPath, false, 1)]
        private static void AuditAllMenuItem()
        {
            string[] paths = AssetDatabase.FindAssets("t:Prefab t:ScriptableObject t:Scene", new[] {"Assets"})
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
            
            if (!Audit(paths))
            {
                EditorUtility.DisplayDialog("Audit Failed", "Asset audit failed.  See logs for details", "ok");
            }
            else
            {
                EditorUtility.DisplayDialog("Audit Passed", "Asset audit passed.", "ok");
            }
        }
        
        [MenuItem(RunSelectedMenuPath, false, 2)]
        private static void AuditSelectedMenuItem()
        {
            string[] paths = GetSelectedPaths();
            if (!Audit(paths))
            {
                EditorUtility.DisplayDialog("Audit Failed", "Asset audit failed.  See logs for details", "ok");
            }
            else
            {
                EditorUtility.DisplayDialog("Audit Passed", "Asset audit passed.", "ok");
            }
        }

        private static bool Audit(string[] paths)
        {
            bool isValid = true;
            int total = paths.Length;
            for (int i = 0; i < total; i++)
            {
                string path = paths[i];

                EditorUtility.DisplayProgressBar("Auditing ", path, (float)i / total);

                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset == null)
                {
                    continue;
                }

                if (IsInDevelopment(asset))
                {
                    continue;
                }

                if (asset is ValidatedData validatedData)
                {
                    isValid &= AuditRecursive(validatedData, validatedData, string.Empty);
                }
                else if (asset is GameObject gameObject)
                {
                    foreach (ValidatedBehaviour validatedBehaviour in gameObject.GetComponentsInChildren<ValidatedBehaviour>(true))
                    {
                        if (validatedBehaviour == null)
                        {
                            continue;
                        }
                        
                        isValid &= AuditRecursive(gameObject, validatedBehaviour,  GetHierarchyPath(gameObject, validatedBehaviour));
                    }
                }
                else if (asset is SceneAsset sceneAsset)
                {
                    Scene? scene = null;
                    try
                    {
                        scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                        foreach (GameObject? go in scene.Value.GetRootGameObjects())
                        {
                            foreach (ValidatedBehaviour validatedBehaviour in go.GetComponentsInChildren<ValidatedBehaviour>(true))
                            {
                                if (validatedBehaviour == null)
                                {
                                    continue;
                                }
                                
                                isValid &= AuditRecursive(sceneAsset, validatedBehaviour, GetHierarchyPath(go, validatedBehaviour));
                            }
                        }
                    }
                    finally
                    {
                        if (scene != null)
                        {
                            EditorSceneManager.CloseScene(scene.Value, true);
                        }
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            
            return isValid;
        }

        private static bool AuditRecursive(Object context, object instance, string path)
        {
            bool isValid = true;
            Type t = instance.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (FieldInfo f in t.GetFields(flags))
            {
                // only public or [SerializeField]
                if (!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null &&
                    f.GetCustomAttribute<SerializeReference>() == null)
                {
                    continue;
                }

                Type ft = f.FieldType;
                bool isOptional = f.GetCustomAttributes().Any(a => a is OptionalAttribute);
                object? value = GetFieldValueSafe(instance, f);
                value = NormalizeUnityNull(value);

                // handle arrays
                if (ft.IsArray && value is Array arr)
                {
                    Type elementType = ft.GetElementType()!;
                    if (elementType.IsPrimitive)
                    {
                        continue;
                    }
                    
                    bool isNullableElements = f.IsArrayOfNullableReferenceType();
                    if (isNullableElements && !isOptional)
                    {
                        Log.Validate(context, false, "Field `{0}` is nullable '?' but missing [Optional] attribute.", path);
                        isValid = false;
                        continue;
                    }
                    if (!isNullableElements && isOptional)
                    {
                        Log.Validate(context, false, "Field `{0}` has [Optional] attribute, but is missing nullable `?` flag.", path);
                        isValid = false;
                        continue;
                    }
                    
                    for (int i = 0; i < arr.Length; i++)
                    {
                        object? element = NormalizeUnityNull(arr.GetValue(i));
                        if (element == null)
                        {
                            continue;
                        }

                        string elementPath = string.IsNullOrEmpty(path) 
                            ? ZString.Format("{0}[{1}]", f.Name, i) 
                            : ZString.Format("{0}.{1}[{2}]", path, f.Name, i);
                        if (typeof(AssetRef).IsAssignableFrom(elementType))
                        {
                            isValid &= Audit(context, element as AssetRef, elementType, elementPath);
                        }
                        else if (typeof(Object).IsAssignableFrom(elementType))
                        {
                            isValid &= Audit(context, element as Object, elementType, elementPath);
                        }
                        else if (element is not Object && element != null && !ft.IsPrimitive)
                        {
                            isValid &= AuditRecursive(context, element, elementPath);    
                        }
                    }

                    continue;
                }

                // handle List<T>
                if (ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(List<>) && value is IList list)
                {
                    Type elementType = ft.GetGenericArguments()[0];
                    if (elementType.IsPrimitive)
                    {
                        continue;
                    }
                    
                    bool isNullableElements = f.IsListOfNullableReferenceType();
                    if (isNullableElements && !isOptional)
                    {
                        Log.Validate(context, false, "Field `{0}` is nullable '?' but missing [Optional] attribute.", path);
                        isValid = false;
                        continue;
                    }
                    if (!isNullableElements && isOptional)
                    {
                        Log.Validate(context, false, "Field `{0}` has [Optional] attribute, but is missing nullable `?` flag.", path);
                        isValid = false;
                        continue;
                    }
                    
                    for (int i = 0; i < list.Count; i++)
                    {
                        object? element = NormalizeUnityNull(list[i]);
                        if (element == null)
                        {
                            continue;
                        }
                        
                        if (isOptional && element == null)
                        {
                            continue;
                        }

                        string elementPath = string.IsNullOrEmpty(path) 
                            ? ZString.Format("{0}[{1}]", f.Name, i) 
                            : ZString.Format("{0}.{1}[{2}]", path, f.Name, i);
                        if (typeof(AssetRef).IsAssignableFrom(elementType))
                        {
                            isValid &= Audit(context, element as AssetRef, elementType, elementPath);
                        }
                        else if (typeof(Object).IsAssignableFrom(elementType))
                        {
                            isValid &= Audit(context, element as Object, elementType, elementPath);
                        }
                        else if (element is not Object && element != null && !ft.IsPrimitive)
                        {
                            isValid &= AuditRecursive(context, element, elementPath);    
                        }
                    }

                    continue;
                }
                
                // handle AssetRef
                if (typeof(AssetRef).IsAssignableFrom(ft))
                {
                    string fieldPath = string.IsNullOrEmpty(path) 
                        ? f.Name 
                        : ZString.Format("{0}.{1}", path, f.Name);
                    
                    bool isNullable = f.IsNullableReference();
                    if (isNullable && !isOptional)
                    {
                        Log.Validate(context, false, "Field `{0}` is nullable '?' but missing [Optional] attribute.", fieldPath);
                        isValid = false;
                        continue;
                    }
                    if (!isNullable && isOptional)
                    {
                        Log.Validate(context, false, "Field `{0}` has [Optional] attribute, but is missing nullable `?` flag.", fieldPath);
                        isValid = false;
                        continue;
                    }
                    
                    if (isOptional && value == null)
                    {
                        continue;
                    }

                    isValid &= Audit(context, value as AssetRef, ft, fieldPath);
                    continue;
                }

                // handle UnityEngine.Objects
                if (typeof(Object).IsAssignableFrom(ft))
                {
                    string fieldPath = string.IsNullOrEmpty(path) 
                        ? f.Name 
                        : ZString.Format("{0}.{1}", path, f.Name);
                    
                    bool isNullable = f.IsNullableReference();
                    if (isNullable && !isOptional)
                    {
                        Log.Validate(context, false, "Field `{0}` is nullable '?' but missing [Optional] attribute.", fieldPath);
                        isValid = false;
                        continue;
                    }
                    if (!isNullable && isOptional)
                    {
                        Log.Validate(context, false, "Field `{0}` has [Optional] attribute, but is missing nullable `?` flag.", fieldPath);
                        isValid = false;
                        continue;
                    }
                    
                    if (isOptional && value == null)
                    {
                        continue;
                    }
                    
                    isValid &= Audit(context, value as Object, ft, fieldPath);
                    
                    continue;
                }

                if (value is not Object && value != null && !ft.IsPrimitive)
                {
                    isValid &= AuditRecursive(context, value, ZString.Format("{0}.{1}", path, f.Name));
                }
            }

            return isValid;
        }
        
        private static bool Audit(Object context, AssetRef? assetRef, Type ft, string? path)
        {
            Type? assetType = null;
            if (ft == typeof(SceneAssetRef))
            {
                assetType = typeof(SceneAsset);
            }
            else
            {
                Type? assetRefType = ft;
                while (assetRefType != null && assetRefType != typeof(object))
                {
                    if (assetRefType.IsGenericType &&
                        assetRefType.GetGenericTypeDefinition() == typeof(AssetRefT<>))
                    {
                        assetType = assetRefType.GetGenericArguments()[0];
                        break;
                    }

                    assetRefType = assetRefType.BaseType;
                }
            }
            Log.Assert(assetType != null, "assetType != null");

            bool isValid = assetRef != null;
            Log.Validate(context, assetRef != null, "{0} field `{1}` is null.", assetType.Name, path);
            if (assetRef != null)
            {
                isValid &= assetRef.IsEditorValid();
                Log.Validate(context, assetRef.IsEditorValid(), "{0} field `{1}` is not a valid.", assetType.Name, path);
            }

            return isValid;
        }
        
        private static bool Audit(Object context, Object? obj, Type type, string? path)
        {
            bool isValid = obj != null;
            Log.Validate(context, obj != null, "{0} field `{1}` is null.", type, path);
            return isValid;
        }
        
        
        internal static bool IsInDevelopment(Object asset)
        {
            return GetLabels(asset)?.Contains("IsInDevelopment") ?? false;
        }

        private static string[]? GetLabels(Object asset)
        {
            if (asset is Component component)
            {
                asset = component.gameObject;
            }
            
            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                if (asset is GameObject gameObject)
                {
                    // If this object isn’t part of any prefab instance, bail.
                    GameObject? instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
                    if (instanceRoot != null)
                    {
                        // Get the corresponding asset-side root for this instance root.
                        GameObject? assetRoot = PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot);
                        if (assetRoot != null)
                        {
                            asset = assetRoot;
                        }
                    }
                }

                // Get path to the .asset file
                path = AssetDatabase.GetAssetPath(asset);
            }

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return AssetDatabase.GetLabels(asset);
        }
        
        private static string[] GetSelectedPaths()
        {
            Object[]? selected = Selection.objects;
            HashSet<string> paths = new HashSet<string>();
            HashSet<string> scenePaths = new HashSet<string>();
            foreach (Object? obj in selected)
            {
                if (obj == null)
                {
                    continue;
                }

                string? path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                // If a folder is selected, include contained prefabs
                if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (string? guid in AssetDatabase.FindAssets("t:Prefab t:Asset", new[] { path }))
                    {
                        paths.Add(AssetDatabase.GUIDToAssetPath(guid));
                    }

                    foreach (string? guid in AssetDatabase.FindAssets("t:Scene", new[] { path }))
                    {
                        scenePaths.Add(AssetDatabase.GUIDToAssetPath(guid));
                    }
                }
                else if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ||
                         path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                {
                    paths.Add(path);
                }
                else if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                {
                    scenePaths.Add(path);
                }
            }

            List<string> allPath = new List<string>(paths);
            allPath.AddRange(scenePaths);
            return allPath.ToArray();
        }
        
        private static object? GetFieldValueSafe(object instance, FieldInfo f)
        {
            try
            {
                return f.GetValue(instance);
            }
            catch (UnassignedReferenceException)
            {
                // Serialized UnityEngine.Object not assigned in the Inspector
                return null;
            }
            catch (TargetInvocationException tie) when (tie.InnerException is UnassignedReferenceException)
            {
                // Defensive: some runtimes wrap it
                return null;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object? NormalizeUnityNull(object? obj)
        {
            return obj is Object uo && !uo ? null : obj;
        }

        private static string GetHierarchyPath(GameObject root, Component target)
        {
            if (root == null || target == null)
                return string.Empty;

            Transform current = target.transform;
            var path = target.GetType().Name; // append component type at the end

            while (current != null)
            {
                path = current.name + "/" + path;

                if (current.gameObject == root)
                    break;

                current = current.parent;
            }

            return path;
        }
    }
}