#nullable enable
using System.Collections.Generic;
using UnityEditor;
using ZLinq;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Utils
{
    internal static class ForceReserializeAllAssets
    {
        [MenuItem("Tools/AssetDatabase/Reserialize/All Assets")]
        private static void MenuItemAllAsset() => ExecuteAllAssets();
        
        [MenuItem("Tools/AssetDatabase/Reserialize/Scriptable Objects")]
        private static void MenuItemScriptableObjects() => ExecuteScriptableObject();
        
        [MenuItem("Tools/AssetDatabase/Reserialize/Prefabs")]
        private static void MenuItemPrefabs() => ExecutePrefabs();
        
        [MenuItem("Tools/AssetDatabase/Reserialize/Selected")]
        private static void MenuItemSelected() => ExecuteSelected();

        private static void ExecuteAllAssets()
        {
            AssetDatabase.ForceReserializeAssets(AssetDatabase.GetAllAssetPaths());
        }
        
        private static void ExecuteScriptableObject()
        {
            AssetDatabase.ForceReserializeAssets(
                AssetDatabase.FindAssets("t:ScriptableObject")
                    .AsValueEnumerable()
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToArray());
        }
        
        private static void ExecutePrefabs()
        {
            AssetDatabase.ForceReserializeAssets(
                AssetDatabase.FindAssets("t:Prefab")
                    .AsValueEnumerable()
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToArray());
        }
        
        private static void ExecuteSelected()
        {
            HashSet<string> paths = new HashSet<string>();
            foreach (Object? obj in Selection.objects)
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

                // If a folder is selected, include contained
                if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (string? guid in AssetDatabase.FindAssets(string.Empty, new[] { path }))
                    {
                        paths.Add(AssetDatabase.GUIDToAssetPath(guid));
                    }
                }
                else
                {
                    paths.Add(path);
                }
            }

            AssetDatabase.ForceReserializeAssets(paths);
        }
    }
}
