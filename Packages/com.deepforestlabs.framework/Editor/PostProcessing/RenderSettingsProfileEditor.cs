using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DeepForestLabs.PostProcessing
{
	[CustomEditor(typeof(RenderSettingsProfile))]
	public class RenderSettingsProfileEditor : Editor
	{
		private enum AmbientColorMode
		{
			Flat = 0,
			Trilight = 1
		}

		private AmbientColorMode _ambientColorMode = AmbientColorMode.Flat;
		private SerializedProperty _ambientModeProp = null;
		private SerializedProperty _ambientFlatColorProp = null;
		private SerializedProperty _ambientSkyColorProp = null;
		private SerializedProperty _ambientEquatorColorProp = null;
		private SerializedProperty _ambientGroundColorProp = null;

		private void OnEnable()
		{
			_ambientModeProp = serializedObject.FindProperty("_ambientMode");
			_ambientFlatColorProp = serializedObject.FindProperty("_ambientColor");
			_ambientSkyColorProp = serializedObject.FindProperty("_ambientSkyColor");
			_ambientEquatorColorProp = serializedObject.FindProperty("_ambientEquatorColor");
			_ambientGroundColorProp = serializedObject.FindProperty("_ambientGroundColor");
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			serializedObject.Update();

			_ambientColorMode = (int)AmbientMode.Flat == _ambientModeProp.enumValueIndex ? AmbientColorMode.Flat : AmbientColorMode.Trilight;
			_ambientColorMode = (AmbientColorMode)EditorGUILayout.EnumPopup("Ambient Mode", _ambientColorMode);

			switch (_ambientColorMode)
			{
				case AmbientColorMode.Flat:
					_ambientModeProp.enumValueIndex = (int)AmbientMode.Flat;
					break;
				case AmbientColorMode.Trilight:
					_ambientModeProp.enumValueIndex = (int)AmbientMode.Trilight;
					break;
			}

			if (_ambientModeProp.enumValueIndex == (int)AmbientMode.Flat)
			{
				_ambientFlatColorProp.colorValue = EditorGUILayout.ColorField(new GUIContent(text: "Ambient Color"),
					_ambientFlatColorProp.colorValue, true, true, true);
			}
			else if (_ambientModeProp.enumValueIndex == (int)AmbientMode.Trilight)
			{
				_ambientSkyColorProp.colorValue = EditorGUILayout.ColorField(new GUIContent("Ambient Sky Color"),
					_ambientSkyColorProp.colorValue, true, true, true);
				_ambientEquatorColorProp.colorValue = EditorGUILayout.ColorField(new GUIContent("Ambient Equator Color"),
					_ambientEquatorColorProp.colorValue, true, true, true);
				_ambientGroundColorProp.colorValue = EditorGUILayout.ColorField(new GUIContent("Ambient Ground Color"),
					_ambientGroundColorProp.colorValue, true, true, true);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
