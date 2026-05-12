#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Text;
using ZLinq;
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

	    private bool _identityFoldout = true;
	    private bool _environmentFoldout = true;
	    private bool _buildOptionsFoldout = true;
	    private bool _addressablesFoldout = true;

	    public BuildSystemSettings Target => target as BuildSystemSettings ?? throw new InvalidCastException();

	    public override void OnInspectorGUI()
	    {
		    Cache();

		    _buildSettingsEditor.CacheSerializedProperties();

		    DrawValidation();
		    EditorGUILayout.Space();

		    DrawIdentitySection();
		    EditorGUILayout.Space();

		    DrawEnvironmentSection();
		    EditorGUILayout.Space();

		    DrawBuildOptionsSection();
		    EditorGUILayout.Space();

		    DrawAddressablesSection();

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

	    private void DrawValidation()
	    {
		    if (string.IsNullOrWhiteSpace(_appNameProp.stringValue))
		    {
			    EditorGUILayout.HelpBox("App Name is empty. Set it before building.", MessageType.Warning);
		    }

		    string envUrl = _environmentsUrlProp.stringValue ?? string.Empty;
		    if (string.IsNullOrEmpty(envUrl) || envUrl.Contains("example.com"))
		    {
			    EditorGUILayout.HelpBox(
				    "Environments URL is the placeholder default. Update it to your project's environment list.",
				    MessageType.Warning);
		    }
	    }

	    private void DrawIdentitySection()
	    {
		    _identityFoldout = EditorGUILayout.Foldout(_identityFoldout, "Identity", true, EditorStyles.foldoutHeader);
		    if (!_identityFoldout) return;

		    EditorGUI.indentLevel++;
		    EditorGUILayout.PropertyField(_appNameProp);
		    EditorGUILayout.PropertyField(_targetFpsProp);
		    EditorGUILayout.PropertyField(_vscyncCountProp);
		    EditorGUILayout.PropertyField(_analyticsAppidProp);
		    EditorGUILayout.PropertyField(_analyticsSubversionProp);
		    EditorGUI.indentLevel--;
	    }

	    private void DrawEnvironmentSection()
	    {
		    _environmentFoldout = EditorGUILayout.Foldout(_environmentFoldout, "Environment", true, EditorStyles.foldoutHeader);
		    if (!_environmentFoldout) return;

		    EditorGUI.indentLevel++;

	    EnvironmentBuildSettings? selected = _environments
		    .AsValueEnumerable().FirstOrDefault(e => e.Name.Equals(_buildSettingsEditor.Target.Environment.Name));
		    int selectedIndex = Mathf.Max(0, Array.IndexOf(_environments.AsValueEnumerable().ToArray(), selected));

		    EditorGUILayout.BeginHorizontal();
		    int editedIndex = EditorGUILayout.Popup("Environment", selectedIndex, _environmentLabels);

		    if (GUILayout.Button("Refresh", GUILayout.MaxWidth(60)))
		    {
			    _environments = EnvironmentsDownloader.Refresh(Target.EnvironmentsUrl);
		    _environmentLabels = _environments.AsValueEnumerable().Select(e => ZString.Format("  {0}", e.Name)).ToArray();
	    }

	    if (GUILayout.Button("Download", GUILayout.MaxWidth(70)))
	    {
		    _environments = EnvironmentsDownloader.Refresh(Target.EnvironmentsUrl);
		    _environmentLabels = _environments.AsValueEnumerable().Select(e => ZString.Format("  {0}", e.Name)).ToArray();
			    Debug.Log($"Downloaded {_environments.Count} environments from {Target.EnvironmentsUrl}");
		    }

		    EditorGUILayout.EndHorizontal();

		    EditorGUILayout.PropertyField(_serverCommsKeyProp);
		    EditorGUILayout.PropertyField(_userServiceKeyProp);
		    EditorGUILayout.PropertyField(_environmentsUrlProp);

		    if (editedIndex != selectedIndex)
		    {
			    editedIndex = Mathf.Clamp(editedIndex, 0, _environments.Count);
			    _buildSettingsEditor.SetEnvironment(Target, _environments[editedIndex]);
		    }

		    EditorGUI.indentLevel--;
	    }

	    private void DrawBuildOptionsSection()
	    {
		    _buildOptionsFoldout = EditorGUILayout.Foldout(_buildOptionsFoldout, "Build Options", true, EditorStyles.foldoutHeader);
		    if (!_buildOptionsFoldout) return;

		    EditorGUI.indentLevel++;
		    EditorGUILayout.PropertyField(_debugBuildOptionsProp);
		    EditorGUILayout.PropertyField(_releaseBuildOptionsProp);
		    EditorGUI.indentLevel--;
	    }

	    private void DrawAddressablesSection()
	    {
		    _addressablesFoldout = EditorGUILayout.Foldout(_addressablesFoldout, "Addressables", true, EditorStyles.foldoutHeader);
		    if (!_addressablesFoldout) return;

		    EditorGUI.indentLevel++;
		    EditorGUILayout.PropertyField(_buildSettingsEditor.ContainerLogFlagProp);
		    EditorGUILayout.PropertyField(_buildSettingsEditor.UniqueIdProp);
		    EditorGUILayout.PropertyField(_buildSettingsEditor.AssetIdProp);
		    EditorGUI.indentLevel--;
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
	    _environmentLabels = _environments.AsValueEnumerable().Select(e => ZString.Format("  {0}", e.Name)).ToArray();
    }
    }
}
#nullable disable
