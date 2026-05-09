using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DeepForestLabs.PostProcessing
{
	public class RenderSettingsProfileGenerator
	{
		[MenuItem("Tools/Generate RenderSettingsProfile From Scene")]
		public static void Generate()
		{
			RenderSettingsProfile newProfile = (RenderSettingsProfile)ScriptableObject.CreateInstance(typeof(RenderSettingsProfile));

			newProfile.Init();

			if (!AssetDatabase.IsValidFolder("Assets/A_RenderSettingsProfiles"))
			{
				AssetDatabase.CreateFolder("Assets", "A_RenderSettingsProfiles");
			}

			string sceneName = EditorSceneManager.GetActiveScene().name;
			AssetDatabase.CreateAsset(newProfile, "Assets/A_RenderSettingsProfiles/" + sceneName + "_RP.asset");
			AssetDatabase.SaveAssets();
		}
	}
}
