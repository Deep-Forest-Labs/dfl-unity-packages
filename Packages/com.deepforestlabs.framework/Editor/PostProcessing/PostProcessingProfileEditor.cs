#nullable enable
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.PostProcessing
{
	[CustomEditor(typeof(PostProcessingProfile))]
	public class PostProcessingProfileEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			PostProcessingProfile? profile = target as PostProcessingProfile;

			GUILayout.Space(20f);
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Space(20f);

			if (GUILayout.Button("Create Material"))
			{
				Material? defaultMat = AssetDatabase.LoadAssetAtPath("Assets/MobilePostProcess/MobilePPS_Default.mat", typeof(Material)) as Material;

				if (defaultMat == null)
				{
					Log.Editor("PostProcessingProfile - Create Material: No default material found");
					return;
				}

				if (profile == null)
				{
					Log.Editor("PostProcessingProfile - Create Material: No profile assigned");
					return;
				}

				Material newMat = Instantiate(defaultMat);

				if (profile.Bloom)
				{
					newMat.EnableKeyword(MobilePostProcessing.BloomKeyword);
				}
				else
				{
					newMat.DisableKeyword(MobilePostProcessing.BloomKeyword);
				}

				if (profile.Blur)
				{
					newMat.EnableKeyword(MobilePostProcessing.BlurKeyword);
				}
				else
				{
					newMat.DisableKeyword(MobilePostProcessing.BlurKeyword);
				}

				if (profile.ChromaticAberration)
				{
					newMat.EnableKeyword(MobilePostProcessing.ChromaKeyword);
				}
				else
				{
					newMat.DisableKeyword(MobilePostProcessing.ChromaKeyword);
				}

				if (profile.ImageFiltering)
				{
					newMat.EnableKeyword(MobilePostProcessing.FilterKeyword);
				}
				else
				{
					newMat.DisableKeyword(MobilePostProcessing.FilterKeyword);
				}

				if (profile.Sharpness > 0)
				{
					newMat.EnableKeyword(MobilePostProcessing.SharpenKeyword);
				}
				else
				{
					newMat.DisableKeyword(MobilePostProcessing.SharpenKeyword);
				}

				if (profile.Distortion)
				{
					newMat.EnableKeyword(MobilePostProcessing.DistortionKeyword);
				}
				else
				{
					newMat.DisableKeyword(MobilePostProcessing.DistortionKeyword);
				}

				newMat.DisableKeyword(MobilePostProcessing.LutKeyword);
				string path = AssetDatabase.GetAssetPath(profile);
				path = System.IO.Path.ChangeExtension(path, ".mat");
				AssetDatabase.CreateAsset(newMat, path);
				AssetDatabase.SaveAssets();
			}
		}
	}
}
#nullable disable