using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.PostProcessing
{
	[CustomEditor(typeof(RenderSettingsStateListener))]
	public class RenderSettingsStateListenerEditor : Editor
	{
		private SerializedProperty _onEnterProp;
		private SerializedProperty _enterStyleProp;
		private SerializedProperty _enterProfileProp;
		private RenderSettingsProfile _enterProfile;
		private TransitionStyle _enterStyle;

		private SerializedProperty _onExitProp;
		private SerializedProperty _exitStyleProp;
		private SerializedProperty _exitProfileProp;
		private RenderSettingsProfile _exitProfile;
		private TransitionStyle _exitStyle;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			RenderSettingsStateListener listener = target as RenderSettingsStateListener;
			serializedObject.Update();

			_onEnterProp = serializedObject.FindProperty("_onEnter");
			_enterStyleProp = serializedObject.FindProperty("_enterStyle");
			_enterProfileProp = serializedObject.FindProperty("_enterProfile");

			_onExitProp = serializedObject.FindProperty("_onExit");
			_exitStyleProp = serializedObject.FindProperty("_exitStyle");
			_exitProfileProp = serializedObject.FindProperty("_exitProfile");

			_onEnterProp.boolValue = EditorGUILayout.Toggle("On Enter", _onEnterProp.boolValue);
			if (_onEnterProp.boolValue)
			{
				_enterStyle = (TransitionStyle)EditorGUILayout.EnumPopup("Transition Style",
					(TransitionStyle)_enterStyleProp.enumValueIndex);
				_enterStyleProp.enumValueIndex = (int)_enterStyle;

				_enterProfile = EditorGUILayout.ObjectField("Profile", _enterProfileProp.objectReferenceValue,
					typeof(RenderSettingsProfile), false) as RenderSettingsProfile;

				_enterProfileProp.objectReferenceValue = _enterProfile;
			}

			EditorGUILayout.Space();

			_onExitProp.boolValue = EditorGUILayout.Toggle("On Exit", _onExitProp.boolValue);
			if (_onExitProp.boolValue)
			{
				_exitStyle = (TransitionStyle)EditorGUILayout.EnumPopup("Transition Style",
					(TransitionStyle)_exitStyleProp.enumValueIndex);
				_exitStyleProp.enumValueIndex = (int)_exitStyle;

				_exitProfile = EditorGUILayout.ObjectField("Profile", _exitProfileProp.objectReferenceValue,
					typeof(RenderSettingsProfile), false) as RenderSettingsProfile;

				_exitProfileProp.objectReferenceValue = _exitProfile;
			}

			serializedObject.ApplyModifiedProperties();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(listener);
			}
		}
	}
}
