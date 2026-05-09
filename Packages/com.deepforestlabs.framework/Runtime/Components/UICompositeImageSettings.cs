#nullable enable
#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace DeepForestLabs.Components
{
    internal sealed class UICompositeImageSettings : ScriptableObject
    {
        public static readonly int[] AllowedRadii = new int[] { 1, 2, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 128, 192, 256, 384, 512, 768, 1024 };
        public const string kEditorSettingsPath = "Assets/Editor/ProceduralImage/EditorSettings.asset";
        private static UICompositeImageSettings? _instance = null;
        
        public static UICompositeImageSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<UICompositeImageSettings>(kEditorSettingsPath);
                }
                if (_instance == null)
                {
                    string? directory = Path.GetDirectoryName(kEditorSettingsPath);
                    if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                        AssetDatabase.Refresh();
                    }
                    _instance = CreateInstance<UICompositeImageSettings>();
                    AssetDatabase.CreateAsset(_instance, kEditorSettingsPath);
                }
                return _instance;
            }
        }

        [SerializeField] private DefaultAsset _exportDirectory = default!;
        [SerializeField] private DefaultAsset _searchForAssetsDirectory = default!;
        [SerializeField] private string _atlasName = "ui_alpha8";
        [SerializeField] private string _spriteNamePrefix = "ui";

        [SerializeField] [Range(256, 4096)] private int _maxTextureSize = 2048; // stepped by 2x
        [SerializeField] [HideInInspector] private int _maxAllowedRadius = 1024; // auto-calculated from maxTextureSize

        public DefaultAsset ExportDirectory => _exportDirectory;
        public DefaultAsset SearchForAssetsDirectory => _searchForAssetsDirectory;
        public string AtlasName => _atlasName;
        public string SpriteNamePrefix => _spriteNamePrefix;
        public int MaxTextureSize => _maxTextureSize;
        public int MaxAllowedRadius => _maxAllowedRadius;

        public void OnValidate()
        {
            _maxAllowedRadius = _maxTextureSize / 2;
        }
    }
}
#endif