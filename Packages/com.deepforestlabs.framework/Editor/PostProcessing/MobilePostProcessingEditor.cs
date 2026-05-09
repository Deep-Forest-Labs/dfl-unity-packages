#nullable enable
using System.IO;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.PostProcessing
{
    [CustomEditor(typeof(MobilePostProcessing))]
    public class MobilePostProcessingEditor : Editor
    {
        private const string _fallbackName = "New_Profile";
        private string _newProfileName = _fallbackName;
        private RenderController? _renderController;
        private SerializedProperty? _profileProp;
        private PostProcessingProfile? _profile;


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            MobilePostProcessing mpp = (target as MobilePostProcessing)!;
            serializedObject.Update();

            GUILayout.Space(20f);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(20f);

            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 300,
                stretchWidth = true
            };

            EditorGUILayout.BeginHorizontal();
            _newProfileName = GUILayout.TextField(_newProfileName);

            if (GUILayout.Button("Save New Profile"))
            {
                RenderController renderController = mpp.GetComponent<RenderController>();
                if (renderController == null)
                {
                    Log.Editor("No RenderController found - Add a RenderController component to the Camera GameObject");
                    return;
                }

                if (renderController.DefaultPPProfile == null)
                {
                    Log.Editor("No Default Profile found - Assign a profile to Default PP Profile on the RenderController");
                    return;
                }

                mpp.Profile = CreateInstance(typeof(PostProcessingProfile)) as PostProcessingProfile;
                Log.Assert(mpp.Profile != null, "mpp.Profile != null");
                
                mpp.Profile.Init(mpp);

                string path = AssetDatabase.GetAssetPath((Object)renderController.DefaultPPProfile);
                path = Path.GetDirectoryName(path) + "/";
                path += string.IsNullOrEmpty(_newProfileName) ? _fallbackName : _newProfileName;
                path += ".asset";

                AssetDatabase.CreateAsset(mpp.Profile, path);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();

            _profileProp = serializedObject.FindProperty("Profile");
            _profile = EditorGUILayout.ObjectField("Profile", _profileProp.objectReferenceValue,
                typeof(PostProcessingProfile), false) as PostProcessingProfile;
            _profileProp.objectReferenceValue = _profile;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load Profile", style))
            {
                if (mpp.Profile == null)
                {
                    Log.Editor("No Profile found - Assign a PostProcessingProfile to the Profile field");
                    return;
                }

                _renderController = mpp.GetComponent<RenderController>();
                if (_renderController == null)
                {
                    Log.Editor("No RenderController found - Add a RenderController component to the Camera GameObject");
                    return;
                }

                Undo.RecordObject(mpp, "MobilePostProcessing : PostProcessingProfile " + mpp.Profile.name + " Loaded.");
                if (EditorApplication.isPlayingOrWillChangePlaymode
                    || EditorApplication.isPaused)
                {
                    _renderController.LoadPostProcessingProfile(mpp.Profile);
                }
                else
                {
                    LoadPostProcessingProfileEditor(mpp);
                }
                EditorUtility.SetDirty(mpp);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Overwrite Profile", style))
            {
                if (mpp.Profile == null)
                {
                    Log.Editor("No Profile found - Assign a PostProcessingProfile to the Profile field");
                    return;
                }

                if (EditorUtility.DisplayDialog("Overwrite Profile", "Are you sure you want to save over the existing profile?", "Overwrite " + mpp.Profile.name, "Cancel"))
                {
                    mpp.Profile.Init(mpp);
                    EditorUtility.SetDirty(mpp.Profile);
                    AssetDatabase.SaveAssets();
                }
            }
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void LoadPostProcessingProfileEditor(MobilePostProcessing mpp)
        {
            if (mpp.Profile == null)
            {
                return;
            }
            mpp.Blur = mpp.Profile.Blur;
            mpp.Bloom = mpp.Profile.Bloom;
            mpp.ImageFiltering = mpp.Profile.ImageFiltering;
            mpp.ChromaticAberration = mpp.Profile.ChromaticAberration;
            mpp.Distortion = mpp.Profile.Distortion;
            mpp.Vignette = mpp.Profile.Vignette;
				
            mpp.BlurAmount = mpp.Profile.BlurAmount;
            mpp.SetBlurMask(mpp.Profile.BlurMask);
            mpp.BloomColor = mpp.Profile.BloomColor;
            mpp.BloomAmount = mpp.Profile.BloomAmount;
            mpp.BloomDiffuse = mpp.Profile.BloomDiffuse;
            mpp.BloomThreshold = mpp.Profile.BloomThreshold;
            mpp.BloomSoftness = mpp.Profile.BloomSoftness;
            mpp.Color = mpp.Profile.Color;
            mpp.Contrast = mpp.Profile.Contrast;
            mpp.Brightness = mpp.Profile.Brightness;
            mpp.Saturation = mpp.Profile.Saturation;
            mpp.Exposure = mpp.Profile.Exposure;
            mpp.Gamma = mpp.Profile.Gamma;
            mpp.Sharpness = mpp.Profile.Sharpness;
            mpp.Offset = mpp.Profile.Offset;
            mpp.FishEyeDistortion = mpp.Profile.FishEyeDistortion;
            mpp.GlitchAmount = mpp.Profile.GlitchAmount;
            mpp.LensDistortion = mpp.Profile.LensDistortion;
            mpp.VignetteColor = mpp.Profile.VignetteColor;
            mpp.VignetteAmount = mpp.Profile.VignetteAmount;
            mpp.VignetteSoftness = mpp.Profile.VignetteSoftness;
        }
    }
}
#nullable disable