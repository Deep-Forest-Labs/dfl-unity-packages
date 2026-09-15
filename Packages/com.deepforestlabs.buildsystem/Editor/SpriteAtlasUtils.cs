#nullable enable
using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.U2D;
using ZLinq;
using DeepForestLabs.Logger;

namespace DeepForestLabs.BuildSystems
{
    public static class SpriteAtlasUtils
    {
        [MenuItem("Deep Forest Labs/Tools/SpriteAtlasV2/Set 'Include In Build' Flag")]
        public static void SetAllIncludeInBuildMenuItem() => SetAllIncludeInBuild(true);

        [MenuItem("Deep Forest Labs/Tools/SpriteAtlasV2/Clear 'Include In Build' Flag")]
        public static void ResetAllIncludeInBuildMenuItem() => SetAllIncludeInBuild(false);

        /// <summary>
        /// Addressable atlases must not also be embedded in the player (double memory).
        /// Non-addressable atlases stay in the player — otherwise a store build has
        /// sprite objects and no atlas texture, and every Image draws white.
        /// </summary>
        public static void SetAllIncludeInBuild(bool enable, bool omitResources = true)
        {
            SpriteAtlas[] spriteAtlases = LoadSpriteAtlases(omitResources);

            foreach (SpriteAtlas atlas in spriteAtlases)
            {
                if (!enable && !IsAddressable(atlas))
                {
                    BuildLog.Info(
                        "SpriteAtlas: leaving IncludeInBuild on '{0}' (not in an Addressable group)",
                        atlas.name);
                    continue;
                }

                SetIncludeInBuild(atlas, enable);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static bool IsAddressable(SpriteAtlas atlas)
        {
            AddressableAssetSettings? settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return false;
            string path = AssetDatabase.GetAssetPath(atlas);
            if (string.IsNullOrEmpty(path))
                return false;
            string guid = AssetDatabase.AssetPathToGUID(path);
            return settings.FindAssetEntry(guid) != null;
        }
 
        private static void SetIncludeInBuild(SpriteAtlas spriteAtlas, bool enable)
        {
            SerializedObject so = new SerializedObject(spriteAtlas);
            SerializedProperty atlasEditorData = so.FindProperty("m_EditorData");
            SerializedProperty includeInBuild = atlasEditorData.FindPropertyRelative("bindAsDefault");
            includeInBuild.boolValue = enable;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(spriteAtlas);
        }
 
        private static SpriteAtlas[] LoadSpriteAtlases(bool omitResources)
        {
            string[] findAssets = AssetDatabase.FindAssets($"t: {nameof(SpriteAtlas)}");
 
            if (findAssets.Length == 0)
            {
                return Array.Empty<SpriteAtlas>();
            }
 
            return findAssets
                .AsValueEnumerable()
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !omitResources || !path.Contains("/Resources/"))
                .Select(AssetDatabase.LoadAssetAtPath<SpriteAtlas>)
                .ToArray();
        }
    }
}
#nullable disable