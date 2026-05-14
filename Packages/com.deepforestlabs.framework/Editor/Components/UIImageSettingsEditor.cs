#nullable enable
using UnityEditor;

namespace DeepForestLabs.Components
{
    [CustomEditor(typeof(UICompositeImageSettings))]
    public sealed class UIImageSettingsEditor : Editor
    {
        private const string kSelectEditorSettings = "Deep Forest Labs/Tools/Composite Image/Settings";
        
        [MenuItem(kSelectEditorSettings)]
        public static void SelectEditorSettings()
        {
            Selection.activeObject = UICompositeImageSettings.Instance;
        }
        
        private UICompositeImageSettings _target = default!;
        private SerializedProperty _exportDirectory = default!;
        private SerializedProperty _searchForAssetsDirectory = default!;
        private SerializedProperty _atlasName = default!;
        private SerializedProperty _spriteNamePrefix = default!;
        private SerializedProperty _maxTextureSize = default!;
        private SerializedProperty _maxAllowedRadius = default!;
        
        private void OnEnable()
        {
            _target = (target as UICompositeImageSettings)!;
            _exportDirectory = serializedObject.FindProperty("_exportDirectory");
            _searchForAssetsDirectory = serializedObject.FindProperty("_searchForAssetsDirectory");
            _atlasName = serializedObject.FindProperty("_atlasName");
            _spriteNamePrefix = serializedObject.FindProperty("_spriteNamePrefix");
            _maxTextureSize = serializedObject.FindProperty("_maxTextureSize");
            _maxAllowedRadius = serializedObject.FindProperty("_maxAllowedRadius");
        }

        private void OnDisable()
        {
            _target = null!;
            _exportDirectory = null!;
            _atlasName = null!;
            _spriteNamePrefix = null!;

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            if (_target == null)
            {
                return;
            }

            serializedObject.Update();

            EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_searchForAssetsDirectory);
            EditorGUILayout.PropertyField(_exportDirectory);
            EditorGUILayout.PropertyField(_atlasName);
            EditorGUILayout.PropertyField(_spriteNamePrefix);

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Baking Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_maxTextureSize);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_maxAllowedRadius);
            EditorGUI.EndDisabledGroup();
            
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                _target.OnValidate();
                EditorUtility.SetDirty(_target);
            }
        }
    }
}