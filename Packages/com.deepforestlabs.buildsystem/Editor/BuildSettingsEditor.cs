#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace DeepForestLabs.BuildSystems
{
    [CustomEditor(typeof(BuildSettings))]
    public class BuildSettingsEditor : Editor
    {
        public static readonly string ASSET_DATABASE_PATH = ZString.Concat("Assets/Resources/", BuildSettings.RESOURCES_PATH, ".asset");
	    
        [MenuItem("Edit/Build/Build Settings")]
        private static void EditMenuItem()
        {
            Selection.activeObject = BuildSettings.Instance;
        }
        

        // Build Settings
        private SerializedProperty _appNameProp = null!;
        private SerializedProperty _buildTarget = null!;
        private SerializedProperty _isCommandLineBuildProp = null!;
        private SerializedProperty _buildNumberProp = null!;
        private SerializedProperty _versionNumberProp = null!;
        private SerializedProperty _shortVersionNumberProp = null!;
        private SerializedProperty _isDebugBuildProp = null!;
        private SerializedProperty _isReleaseBuildProp = null!;
        private SerializedProperty _testFlightBuildProp = null!;
        private SerializedProperty _scriptingDefinesProp = null!;
        private SerializedProperty _targetFpsProp = null!;
        private SerializedProperty _vscyncCountProp = null!;
        private SerializedProperty _analyticsAppidProp = null!;
        private SerializedProperty _analyticsSubversionProp = null!;
        private SerializedProperty _containerLogFlagProp = null!;
	    
        // Addressable Build Settings
        private SerializedProperty _uniqueIdProp = null!;
        private SerializedProperty _assetIdProp = null!;
        private SerializedProperty _enableJsonCatalogProp = null!;
        private SerializedProperty _activePlayModeIndex = null!;

        // Environment Build Settings
        private SerializedProperty _environmentNameProp = null!;
        private SerializedProperty _serverUrlProp = null!;
        private SerializedProperty _analyticsUrlProp = null!;
        private SerializedProperty _apiUrlProp = null!;
        private SerializedProperty _serverCommsKeyProp = null!;
        private SerializedProperty _userServiceKeyProp = null!;

        public BuildSettings Target => target as BuildSettings ?? throw new InvalidCastException();

        public SerializedProperty UniqueIdProp => _uniqueIdProp;
        public SerializedProperty AssetIdProp => _assetIdProp;
        public SerializedProperty ContainerLogFlagProp => _containerLogFlagProp;
        
        public static BuildSettings Create()
        {
            BuildSettings buildSettings = CreateInstance<BuildSettings>();
            AssetDatabase.CreateAsset(buildSettings, ASSET_DATABASE_PATH);
            
            BuildSettingsEditor editor = CreateEditor(buildSettings, typeof(BuildSettingsEditor)) as BuildSettingsEditor
                ?? throw new InvalidCastException();

            editor.CacheSerializedProperties();
            editor._uniqueIdProp.stringValue = AddressablesBuildSettings.DEFAULT_UNIQUE_VALUE;
            editor._assetIdProp.stringValue = AddressablesBuildSettings.RELEASE_ASSET_ID;
            editor._enableJsonCatalogProp.boolValue = true;

            if (BuildSystemSettings.SettingsExist)
            {
                BuildSystemSettings bss = BuildSystemSettings.GetSettings();
                editor.CopyBuildSystemSettings(bss);
                editor.SetEnvironment(bss, EnvironmentsDownloader.GetEnvironments().FirstOrDefault(e => e.Name == "dev"));
            }
            
            editor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return buildSettings;
        }
        
        public static void Write(BuilderIndex builderIndex)
        {
            BuildSettings settings;
            if (!File.Exists(ASSET_DATABASE_PATH))
            {
                settings = CreateInstance<BuildSettings>();
                AssetDatabase.CreateAsset(settings, ASSET_DATABASE_PATH);
            }
            else
            {
                settings = AssetDatabase.LoadAssetAtPath<BuildSettings>(ASSET_DATABASE_PATH);
            }
			
            BuildSettingsEditor editor =  CreateEditor(settings, typeof(BuildSettingsEditor))
                as BuildSettingsEditor ?? throw new InvalidCastException();
            editor.WriteInternal(builderIndex);
        }

        public static void Write(CommandLineArgs args)
        {
            BuildSettings settings;
            if (!File.Exists(ASSET_DATABASE_PATH))
            {
                settings = CreateInstance<BuildSettings>();
                AssetDatabase.CreateAsset(settings, ASSET_DATABASE_PATH);
            }
            else
            {
                settings = AssetDatabase.LoadAssetAtPath<BuildSettings>(ASSET_DATABASE_PATH);
            }
			
            BuildSettingsEditor editor =  CreateEditor(settings, typeof(BuildSettingsEditor))
                as BuildSettingsEditor ?? throw new InvalidCastException();
            editor.WriteInternal(args);
        }

        private void Awake()
        {
            CacheSerializedProperties();
        }

        public override void OnInspectorGUI()
        {
            CacheSerializedProperties();
            OnBuildGUI();
            OnEnvironmentGUI();
            OnAddressablesGUI();

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        
        private void WriteInternal(BuilderIndex builderIndex)
        {
            if (!BuildSystemSettings.SettingsExist)
            {
                string error = "Missing BuildSystemSettings.";
                if (BuildPipeline.isBuildingPlayer)
                {
                    throw new BuildFailedException(error);
                }

                throw new BuildException(error);
            }
            
            _activePlayModeIndex.enumValueIndex = (int)builderIndex;

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
            }
        }

        public void CopyBuildSystemSettings(BuildSystemSettings buildSystemSettings)
        {
            _appNameProp.stringValue = buildSystemSettings.AppName;
            _targetFpsProp.intValue = buildSystemSettings.TargetFps;
            _vscyncCountProp.intValue = buildSystemSettings.VscyncCount;
            _analyticsAppidProp.stringValue = buildSystemSettings.AnalyticsAppid;

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();    
            }
        }
        
        private void WriteInternal(CommandLineArgs args)
        {
            if (!BuildSystemSettings.SettingsExist)
            {
                throw new BuildFailedException("Missing BuildSystemSettings.");
            }
            
            BuildSystemSettings buildSystemSettings = BuildSystemSettings.GetSettings();
            string url = string.IsNullOrWhiteSpace(args.OverrideEnvironmentUri)
                ? buildSystemSettings.EnvironmentsUrl
                : args.OverrideEnvironmentUri;
            IReadOnlyList<EnvironmentBuildSettings> environments = EnvironmentsDownloader.Refresh(url);
            EnvironmentBuildSettings? environment = environments.FirstOrDefault(e => e.Name.Equals(args.Environment));
            if (environment == null)
            {
                throw new BuildFailedException(ZString.Format("Failed to locate environment named '{0}'.",
                    args.Environment));
            }

            _appNameProp.stringValue = buildSystemSettings.AppName;
            _targetFpsProp.intValue = buildSystemSettings.TargetFps;
            _vscyncCountProp.intValue = buildSystemSettings.VscyncCount;
            _analyticsAppidProp.stringValue = buildSystemSettings.AnalyticsAppid;

            _buildTarget.stringValue = args.BuildTarget;
            _isCommandLineBuildProp.boolValue = args.IsCommandLineBuild;
            _buildNumberProp.intValue = args.BuildNumber;
            _versionNumberProp.stringValue = args.Version;
            _shortVersionNumberProp.stringValue = args.ShortVersion;
            _isDebugBuildProp.boolValue = args.IsDebugBuild;
            _isReleaseBuildProp.boolValue = args.IsReleaseBuild;
            _testFlightBuildProp.boolValue = args.IsTestFlightBuild;
            _scriptingDefinesProp.stringValue = args.ScriptingDefines;
            _uniqueIdProp.stringValue = args.UniqueId;
            _assetIdProp.stringValue = args.AssetId;
            _enableJsonCatalogProp.boolValue = args.EnableJsonCatalog;

            SetEnvironment(buildSystemSettings, environment.Value);

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
            }
        }
        
        public void SetEnvironment(BuildSystemSettings bss, EnvironmentBuildSettings selected)
        {
            _environmentNameProp.stringValue = selected.Name;
            _serverUrlProp.stringValue = selected.ServerUrl;
            _analyticsUrlProp.stringValue = selected.AnalyticsUrl;
            _apiUrlProp.stringValue = selected.ApiUrl;
            _serverCommsKeyProp.stringValue = !string.IsNullOrWhiteSpace(selected.ServerCommsKey) ? selected.ServerCommsKey : bss.ServerCommsKey;
            _userServiceKeyProp.stringValue = !string.IsNullOrWhiteSpace(selected.UserServiceKey) ? selected.UserServiceKey : bss.UserServiceKey;

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
            }
        }

        private void OnBuildGUI()
        {
            _appNameProp.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_appNameProp.isExpanded, "Client Build Settings");
            if (_appNameProp.isExpanded)
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_appNameProp);
                EditorGUILayout.PropertyField(_targetFpsProp);
                EditorGUILayout.PropertyField(_vscyncCountProp);
                EditorGUILayout.PropertyField(_analyticsAppidProp);
                EditorGUILayout.PropertyField(_analyticsSubversionProp);
                EditorGUILayout.PropertyField(_containerLogFlagProp);
                EditorGUILayout.PropertyField(_buildTarget);
                EditorGUILayout.PropertyField(_isCommandLineBuildProp);
                EditorGUILayout.PropertyField(_buildNumberProp);
                EditorGUILayout.PropertyField(_versionNumberProp);
                EditorGUILayout.PropertyField(_shortVersionNumberProp);
                EditorGUILayout.PropertyField(_isDebugBuildProp);
                EditorGUILayout.PropertyField(_isReleaseBuildProp);
                EditorGUILayout.PropertyField(_scriptingDefinesProp);

                GUI.enabled = true;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void OnEnvironmentGUI()
        {
            _environmentNameProp.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_environmentNameProp.isExpanded, "Environment Build Settings");
            if (_environmentNameProp.isExpanded)
            {
                GUI.enabled = false;
                    
                EditorGUILayout.PropertyField(_environmentNameProp);
                EditorGUILayout.PropertyField(_serverUrlProp);
                EditorGUILayout.PropertyField(_analyticsUrlProp);
                EditorGUILayout.PropertyField(_apiUrlProp);
                EditorGUILayout.PropertyField(_serverCommsKeyProp);
                EditorGUILayout.PropertyField(_userServiceKeyProp);
                
                GUI.enabled = true;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void OnAddressablesGUI()
        {
            _uniqueIdProp.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_uniqueIdProp.isExpanded, "Addressables Build Settings");
            if (_uniqueIdProp.isExpanded)
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_uniqueIdProp);
                if (string.IsNullOrWhiteSpace(_uniqueIdProp.stringValue))
                {
                    _uniqueIdProp.stringValue = AddressablesBuildSettings.DEFAULT_UNIQUE_VALUE;
                }
                EditorGUILayout.PropertyField(_assetIdProp);
                if (string.IsNullOrWhiteSpace(_assetIdProp.stringValue))
                {
                    _assetIdProp.stringValue = AddressablesBuildSettings.RELEASE_ASSET_ID;
                }
                EditorGUILayout.PropertyField(_enableJsonCatalogProp);
                GUI.enabled = true;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        public void CacheSerializedProperties()
        {
            // Build
            _appNameProp = serializedObject.FindProperty(nameof(BuildSettings._appName))!;
            _buildTarget = serializedObject.FindProperty(nameof(BuildSettings._buildTarget))!;
            _isCommandLineBuildProp = serializedObject.FindProperty(nameof(BuildSettings._isCommandLineBuild))!;
            _buildNumberProp = serializedObject.FindProperty(nameof(BuildSettings._buildNumber))!;
            _versionNumberProp = serializedObject.FindProperty(nameof(BuildSettings._versionNumber))!;
            _shortVersionNumberProp = serializedObject.FindProperty(nameof(BuildSettings._shortVersion))!;
            _isDebugBuildProp = serializedObject.FindProperty(nameof(BuildSettings._isDebugBuild))!;
            _isReleaseBuildProp = serializedObject.FindProperty(nameof(BuildSettings._isReleaseBuild))!;
            _testFlightBuildProp = serializedObject.FindProperty(nameof(BuildSettings._isTestFlightBuild))!;
            _scriptingDefinesProp = serializedObject.FindProperty(nameof(BuildSettings._scriptingDefines))!;
            _targetFpsProp = serializedObject.FindProperty(nameof(BuildSettings._targetFps))!;
            _vscyncCountProp = serializedObject.FindProperty(nameof(BuildSettings._vscyncCount))!;
            _analyticsAppidProp = serializedObject.FindProperty(nameof(BuildSettings._analyticsAppid))!;
            _analyticsSubversionProp = serializedObject.FindProperty(nameof(BuildSettings._analyticsSubversion))!;
            _containerLogFlagProp = serializedObject.FindProperty(nameof(BuildSettings._containerLogFlag))!;

            // Addressables
            SerializedProperty addressables = serializedObject.FindProperty(nameof(BuildSettings._addressables))!;
            _uniqueIdProp = addressables.FindPropertyRelative(ZString.Format("{0}", nameof(AddressablesBuildSettings._uniqueId)));
            _assetIdProp = addressables.FindPropertyRelative(ZString.Format("{0}", nameof(AddressablesBuildSettings._assetId)));
            _enableJsonCatalogProp = addressables.FindPropertyRelative(ZString.Format("{0}", nameof(AddressablesBuildSettings._enableJsonCatalog)));
            _activePlayModeIndex = addressables.FindPropertyRelative(ZString.Format("{0}", nameof(AddressablesBuildSettings._activePlayModeIndex)));

            // Environment
            SerializedProperty environment = serializedObject.FindProperty(nameof(BuildSettings._environment))!;
            _environmentNameProp = environment.FindPropertyRelative(ZString.Format("{0}", nameof(EnvironmentBuildSettings.name)));
            _serverUrlProp = environment.FindPropertyRelative(ZString.Format("{0}", nameof(EnvironmentBuildSettings.serverUrl)));
            _analyticsUrlProp = environment.FindPropertyRelative(ZString.Format("{0}", nameof(EnvironmentBuildSettings.analyticsUrl)));
            _apiUrlProp = environment.FindPropertyRelative(ZString.Format("{0}", nameof(EnvironmentBuildSettings.apiUrl))); 
            _serverCommsKeyProp = environment.FindPropertyRelative(ZString.Format("{0}", nameof(EnvironmentBuildSettings.serverCommsKey)));
            _userServiceKeyProp = environment.FindPropertyRelative(ZString.Format("{0}", nameof(EnvironmentBuildSettings.userServiceKey)));
        }
    }
}
#nullable disable
