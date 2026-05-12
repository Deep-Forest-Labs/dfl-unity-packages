#nullable enable
using System;
using System.Linq;
using UnityEditor;
using ZLinq;
using UnityEngine.U2D;

namespace DeepForestLabs.BuildSystems
{
    public static class SpriteAtlasUtils
    {
        [MenuItem("Tools/SpriteAtlasV2/Set 'Include In Build' Flag")]
        public static void SetAllIncludeInBuildMenuItem() => SetAllIncludeInBuild(true);

        [MenuItem("Tools/SpriteAtlasV2/Clear 'Include In Build' Flag")]
        public static void ResetAllIncludeInBuildMenuItem() => SetAllIncludeInBuild(false);
        
        public static void SetAllIncludeInBuild(bool enable, bool omitResources = true)
        {
            SpriteAtlas[] spriteAtlases = LoadSpriteAtlases(omitResources);
 
            foreach (SpriteAtlas atlas in spriteAtlases)
            {
                SetIncludeInBuild(atlas, enable);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !omitResources || !path.Contains("/Resources/"))
                .Select(AssetDatabase.LoadAssetAtPath<SpriteAtlas>)
                .ToArray();
        }
    }
}
#nullable disable