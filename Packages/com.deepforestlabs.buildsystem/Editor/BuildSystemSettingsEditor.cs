#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Text;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.BuildSystems
{
    [CustomEditor(typeof(BuildSystemSettings))]
    public class BuildSystemSettingsEditor : Editor
    {
	    [MenuItem("Edit/Build/Build System Settings %#e")]
	    public static void Edit() => BuildSystemEntryPoint.EditBuildSystemSettings();

	    private IReadOnlyList<EnvironmentBuildSettings> _environments = Array.Empty<EnvironmentBuildSettings>();
	    private string[] _environmentLabels = Array.Empty<string>();
	    
	    private SerializedProperty _appNameProp = null!;
	    private SerializedProperty _targetFpsProp = null!;
	    private SerializedProperty _vscyncCountProp = null!;
	    private SerializedProperty _analyticsAppidProp = null!;
	    private SerializedProperty _analyticsSubversionProp = null!;
	    private SerializedProperty _debugBuildOptionsProp = null!;
	    private SerializedProperty _releaseBuildOptionsProp = null!;
	    private SerializedProperty _environmentsUrlProp = null!;
		private SerializedProperty _serverCommsKeyProp = null!;
		private SerializedProperty _userServiceKeyProp = null!;
		private BuildSettingsEditor _buildSettingsEditor = null!;
	    
	    public BuildSystemSettings Target => target as BuildSystemSettings ?? throw new InvalidCastException();
	    

	    public override void OnInspectorGUI()
	    {
		    Cache();
			
			// Addressables;
			_buildSettingsEditor.CacheSerializedProperties();

			OnAddressablesGUI();
			OnEnvironmentGUI();
			OnBuildGUI();

			if (serializedObject.hasModifiedProperties)
			{
				serializedObject.ApplyModifiedProperties();
				_buildSettingsEditor.CopyBuildSystemSettings((target as BuildSystemSettings)!);
			}

			if (_buildSettingsEditor.serializedObject.hasModifiedProperties)
			{
				_buildSettingsEditor.serializedObject.ApplyModifiedProperties();
			}
	    }

	    private void OnAddressablesGUI()
	    {
		    EditorGUILayout.PropertyField(_buildSettingsEditor.ContainerLogFlagProp );
		    EditorGUILayout.PropertyField(_buildSettingsEditor.UniqueIdProp);
		    EditorGUILayout.PropertyField(_buildSettingsEditor.AssetIdProp);
	    }
	    private void OnEnvironmentGUI()
	    {
		    EnvironmentBuildSettings? selected = _environments
			    .FirstOrDefault(e => e.Name.Equals(_buildSettingsEditor.Target.Environment.Name));
		    int selectedIndex = Mathf.Max(0, Array.IndexOf(_environments.ToArray(), selected));

		    EditorGUILayout.BeginHorizontal();
		    int editedIndex = EditorGUILayout.Popup("Environment", selectedIndex, _environmentLabels);
			bool refreshClicked = GUILayout.Button("Refresh", GUILayout.MaxWidth(60));
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.PropertyField(_serverCommsKeyProp);
			EditorGUILayout.PropertyField(_userServiceKeyProp);
			EditorGUILayout.PropertyField(_environmentsUrlProp);

			if (refreshClicked)
			{
				_environments = EnvironmentsDownloader.Refresh(Target.EnvironmentsUrl);
				_environmentLabels = _environments.Select(e => ZString.Format("  {0}", e.Name)).ToArray();
			}
		    if (editedIndex != selectedIndex)
		    {
			    editedIndex = Mathf.Clamp(editedIndex, 0, _environments.Count);
			    _buildSettingsEditor.SetEnvironment(Target, _environments[editedIndex]);
		    }
	    }
	    
	    private void OnBuildGUI()
	    {
		    EditorGUILayout.PropertyField(_appNameProp);
		    EditorGUILayout.PropertyField(_targetFpsProp);
		    EditorGUILayout.PropertyField(_vscyncCountProp);
		    EditorGUILayout.PropertyField(_analyticsAppidProp);
		    EditorGUILayout.PropertyField(_analyticsSubversionProp);
		    EditorGUILayout.PropertyField(_debugBuildOptionsProp);
		    EditorGUILayout.PropertyField(_releaseBuildOptionsProp);
	    }

	    private void Cache()
	    {
		    _appNameProp = serializedObject.FindProperty(nameof(BuildSystemSettings._appName));
		    _targetFpsProp = serializedObject.FindProperty(nameof(BuildSystemSettings._targetFps));
		    _vscyncCountProp = serializedObject.FindProperty(nameof(BuildSystemSettings._vscyncCount));
		    _analyticsAppidProp = serializedObject.FindProperty(nameof(BuildSystemSettings._analyticsAppid));
		    _analyticsSubversionProp = serializedObject.FindProperty(nameof(BuildSystemSettings._analyticsSubversion));
		    _debugBuildOptionsProp = serializedObject.FindProperty(nameof(BuildSystemSettings._debugBuildOptions));
		    _releaseBuildOptionsProp = serializedObject.FindProperty(nameof(BuildSystemSettings._releaseBuildOptions));
		    _environmentsUrlProp = serializedObject.FindProperty(nameof(BuildSystemSettings._environmentsUrl));
		    _serverCommsKeyProp = serializedObject.FindProperty(nameof(BuildSystemSettings._serverCommsKey));
			_userServiceKeyProp = serializedObject.FindProperty(nameof(BuildSystemSettings._userServiceKey));

			Editor editor = _buildSettingsEditor;
			CreateCachedEditor(BuildSettings.Instance, typeof(BuildSettingsEditor), ref editor);
			_buildSettingsEditor = editor as BuildSettingsEditor ?? throw new InvalidCastException();
			
			_environments = EnvironmentsDownloader.GetEnvironments(Target.EnvironmentsUrl);
			_environmentLabels = _environments.Select(e => ZString.Format("  {0}", e.Name)).ToArray();
	    }
    }
}
#nullable disable
