#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace DeepForestLabs.PostProcessing
{
    public sealed class RenderController : MonoBehaviour, IInitializable, IDisposable
    {
        [Dependency] private readonly CancellationToken _scope = default!;

        [SerializeField] private MobilePostProcessing _postProcessing = default!;
        [SerializeField] private PostProcessingProfile? _defaultPPProfile = null;
        
        public PostProcessingProfile? DefaultPPProfile => _defaultPPProfile;
        private RenderSettingsProfile? DefaultRsProfile { get; set; } = null;

        private bool _revertFogState = false;

        private bool _transitioningPostProcessing = false;
        private bool _transitioningRenderSettings = false;

        #region Initialization
        public async UniTask Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            PostProcessingStateListener.RenderController = this;
            RenderSettingsStateListener.RenderController = this;
            InitRenderSettings();

            PostProcessingProfile? profile = DefaultPPProfile;
            if (_postProcessing == null)
            {
                return;
            }

            if (profile == null)
            {
                profile = DefaultPPProfile;
            }

            if (profile != null)
            {
                LoadPostProcessingMaterial(profile, profile.Vignette);
                await LoadPostProcessingProfileAsync(profile, token);
            }
        }
        
        public void Dispose()
        {
            PostProcessingStateListener.RenderController = this;
            RenderSettingsStateListener.RenderController = null;
            if (_postProcessing != null)
            {
                _postProcessing.gameObject.SetActive(false);
            }
        }

        private void OnPreRender()
        {
            _revertFogState = RenderSettings.fog;
            RenderSettings.fog = enabled;
        }

        private void OnPostRender()
        {
            RenderSettings.fog = _revertFogState;
        }
        #endregion

        #region Render Settings
        public void InitRenderSettings(RenderSettingsProfile? profile = null)
        {
            DefaultRsProfile = profile;
            if (DefaultRsProfile == null)
            {
                DefaultRsProfile = (RenderSettingsProfile)ScriptableObject.CreateInstance(typeof(RenderSettingsProfile));
                DefaultRsProfile.Init();
            }

            LoadRenderSettingsProfile(DefaultRsProfile);
        }

        public void ResetRenderSettings(float duration = 0, RenderSettingsProfile? useProfile = null)
        {
            RenderSettingsProfile? profile = DefaultRsProfile;
            if (useProfile != null)
            {
                profile = useProfile;
            }

            if (profile == null)
            {
                return;
            }

            if (duration == 0)
            {
                LoadRenderSettingsProfile(profile);
            }
            else
            {
                // TODO - Replace token with proper container token
                // RenderController should live in the BoardContainer
                // It currently persists across boards, but gets re-initialized on each board via a BoardInitializer
                TransitionRenderSettingsAsync(profile, duration, _scope).Forget();
            }
        }

        public void LoadRenderSettingsProfile(RenderSettingsProfile profile)
        {
            if (profile == null)
            {
                return;
            }
	
            RenderSettings.ambientMode = profile.AmbientMode;
            if (RenderSettings.ambientMode == AmbientMode.Flat)
            {
                RenderSettings.ambientLight = profile.AmbientColor;
            }
            else if (RenderSettings.ambientMode == AmbientMode.Trilight)
            {
                RenderSettings.ambientSkyColor = profile.AmbientSkyColor;
                RenderSettings.ambientEquatorColor = profile.AmbientEquatorColor;
                RenderSettings.ambientGroundColor = profile.AmbientGroundColor;
            }

            RenderSettings.fogColor = profile.FogColor;
            RenderSettings.fogMode = profile.FogMode;
            RenderSettings.fogStartDistance = profile.FogStartDistance;
            RenderSettings.fogEndDistance = profile.FogEndDistance;
            RenderSettings.fogDensity = profile.FogDensity;
        }

        public void TransitionRenderSettings(RenderSettingsProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            if (profile.TransitionDuration <= 0)
            {
                LoadRenderSettingsProfile(profile);
            }
            else
            {
                TransitionRenderSettingsAsync(profile, profile.TransitionDuration, _scope).Forget();
            }
        }

        private async UniTask TransitionRenderSettingsAsync(RenderSettingsProfile targetProfile, float duration, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_transitioningRenderSettings)
            {
                return;
            }

            if (targetProfile == null)
            {
                return;
            }

            _transitioningRenderSettings = true;

            RenderSettingsProfile currProfile = (RenderSettingsProfile)ScriptableObject.CreateInstance(typeof(RenderSettingsProfile));
            currProfile.Init();

            RenderSettings.fogMode = targetProfile.FogMode;
            RenderSettings.ambientMode = targetProfile.AmbientMode;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (currProfile == null || targetProfile == null)
                {
                    _transitioningRenderSettings = false;
                    return;
                }

                float t = elapsed / duration;

                if (RenderSettings.ambientMode == AmbientMode.Flat)
                {
                    RenderSettings.ambientLight = Color.Lerp(currProfile.AmbientColor, targetProfile.AmbientColor, t);
                }
                else if (RenderSettings.ambientMode == AmbientMode.Trilight)
                {
                    RenderSettings.ambientSkyColor = Color.Lerp(currProfile.AmbientSkyColor, targetProfile.AmbientSkyColor, t);
                    RenderSettings.ambientEquatorColor = Color.Lerp(currProfile.AmbientEquatorColor, targetProfile.AmbientEquatorColor, t);
                    RenderSettings.ambientGroundColor = Color.Lerp(currProfile.AmbientGroundColor, targetProfile.AmbientGroundColor, t);
                }

                RenderSettings.fogColor = Color.Lerp(currProfile.FogColor, targetProfile.FogColor, t);
                RenderSettings.fogStartDistance = Mathf.Lerp(currProfile.FogStartDistance, targetProfile.FogStartDistance, t);
                RenderSettings.fogEndDistance = Mathf.Lerp(currProfile.FogEndDistance, targetProfile.FogEndDistance, t);
                RenderSettings.fogDensity = Mathf.Lerp(currProfile.FogDensity, targetProfile.FogDensity, t);

                elapsed += Time.deltaTime;
                await UniTask.NextFrame(token);
            }

            LoadRenderSettingsProfile(targetProfile);
            _transitioningRenderSettings = false;
        }
        #endregion

        #region Post Processing
        private void LoadPostProcessingMaterial(PostProcessingProfile? profile, bool vignette)
        {
            if (_postProcessing != null && profile != null)
            {
                _postProcessing.Blur = profile.Blur;
                _postProcessing.Bloom = profile.Bloom;
                _postProcessing.LUT = false;
                _postProcessing.ImageFiltering = profile.ImageFiltering;
                _postProcessing.ChromaticAberration = profile.ChromaticAberration;
                _postProcessing.Distortion = profile.Distortion;

                _postProcessing.Vignette = vignette;
            }
        }

        public void ResetPostProcessing(float duration = 0, PostProcessingProfile? useProfile = default)
        {
            PostProcessingProfile? profile = DefaultPPProfile;
            if (useProfile != null)
            {
                profile = useProfile;
            }
            if (profile == null)
            {
                return;
            }
            
            if (duration == 0)
            {
                LoadPostProcessingProfile(profile);
            }
            else
            {
                TransitionPostProcessingAsync(profile, duration, _scope).Forget();
            }
        }

        public void LoadPostProcessingProfile(PostProcessingProfile profile)
        {
            if (profile == null || _postProcessing == null)
            {
                return;
            }

            _postProcessing.Blur = profile.Blur;
            _postProcessing.Bloom = profile.Bloom;
            _postProcessing.ImageFiltering = profile.ImageFiltering;
            _postProcessing.ChromaticAberration = profile.ChromaticAberration;
            _postProcessing.Distortion = profile.Distortion;
            _postProcessing.Vignette = profile.Vignette;

            _postProcessing.BlurAmount = profile.BlurAmount;
            _postProcessing.SetBlurMask(profile.BlurMask);
            _postProcessing.BloomColor = profile.BloomColor;
            _postProcessing.BloomAmount = profile.BloomAmount;
            _postProcessing.BloomDiffuse = profile.BloomDiffuse;
            _postProcessing.BloomThreshold = profile.BloomThreshold;
            _postProcessing.BloomSoftness = profile.BloomSoftness;
            _postProcessing.Color = profile.Color;
            _postProcessing.Contrast = profile.Contrast;
            _postProcessing.Brightness = profile.Brightness;
            _postProcessing.Saturation = profile.Saturation;
            _postProcessing.Exposure = profile.Exposure;
            _postProcessing.Gamma = profile.Gamma;
            _postProcessing.Sharpness = profile.Sharpness;
            _postProcessing.Offset = profile.Offset;
            _postProcessing.FishEyeDistortion = profile.FishEyeDistortion;
            _postProcessing.GlitchAmount = profile.GlitchAmount;
            _postProcessing.LensDistortion = profile.LensDistortion;
            _postProcessing.VignetteColor = profile.VignetteColor;
            _postProcessing.VignetteAmount = profile.VignetteAmount;
            _postProcessing.VignetteSoftness = profile.VignetteSoftness;

            _postProcessing.Profile = profile;
        }

        public async UniTask LoadPostProcessingProfileAsync(PostProcessingProfile profile, CancellationToken token, bool initFrame = true)
        {
            token.ThrowIfCancellationRequested();

            if (profile == null || _postProcessing == null)
            {
                return;
            }

            if (initFrame)
            {
                _postProcessing.Blur = profile.Blur;
                _postProcessing.Bloom = profile.Bloom;
                _postProcessing.ImageFiltering = profile.ImageFiltering;
                _postProcessing.ChromaticAberration = profile.ChromaticAberration;
                _postProcessing.Distortion = profile.Distortion;
                _postProcessing.Vignette = true;
                _postProcessing.VignetteAmount = 0;
                _postProcessing.VignetteSoftness = 1;

                await UniTask.NextFrame(token);
            }

            _postProcessing.BlurAmount = profile.BlurAmount;
            _postProcessing.SetBlurMask(profile.BlurMask);
            _postProcessing.BloomColor = profile.BloomColor;
            _postProcessing.BloomAmount = profile.BloomAmount;
            _postProcessing.BloomDiffuse = profile.BloomDiffuse;
            _postProcessing.BloomThreshold = profile.BloomThreshold;
            _postProcessing.BloomSoftness = profile.BloomSoftness;
            _postProcessing.Color = profile.Color;
            _postProcessing.Contrast = profile.Contrast;
            _postProcessing.Brightness = profile.Brightness;
            _postProcessing.Saturation = profile.Saturation;
            _postProcessing.Exposure = profile.Exposure;
            _postProcessing.Gamma = profile.Gamma;
            _postProcessing.Sharpness = profile.Sharpness;
            _postProcessing.Offset = profile.Offset;
            _postProcessing.FishEyeDistortion = profile.FishEyeDistortion;
            _postProcessing.GlitchAmount = profile.GlitchAmount;
            _postProcessing.LensDistortion = profile.LensDistortion;
            _postProcessing.VignetteColor = profile.VignetteColor;
            _postProcessing.VignetteAmount = profile.VignetteAmount;
            _postProcessing.VignetteSoftness = profile.VignetteSoftness;

            _postProcessing.Profile = profile;
        }

        public void TransitionPostProcessing(PostProcessingProfile profile)
        {
            if (profile == null || _postProcessing == null)
            {
                return;
            }

            if (profile.TransitionDuration <= 0)
            {
                LoadPostProcessingProfile(profile);
            }
            else
            {
                TransitionPostProcessingAsync(profile, profile.TransitionDuration, _scope).Forget();
            }
        }

        private async UniTask TransitionPostProcessingAsync(PostProcessingProfile targetProfile, float duration, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_transitioningPostProcessing)
            {
                return;
            }

            if (targetProfile == null || _postProcessing == null)
            {
                return;
            }

            _transitioningPostProcessing = true;
            if (targetProfile.BlurMask != null)
            {
                _postProcessing.SetBlurMask(targetProfile.BlurMask);
            }

            PostProcessingProfile currProfile = (PostProcessingProfile)ScriptableObject.CreateInstance(typeof(PostProcessingProfile));
            currProfile.Init(_postProcessing);

            _postProcessing.ImageFiltering = targetProfile.ImageFiltering;

            if (targetProfile.Blur)
            {
                _postProcessing.Blur = targetProfile.Blur;
            }

            if (targetProfile.Bloom)
            {
                _postProcessing.Bloom = targetProfile.Bloom;
            }

            if (targetProfile.ChromaticAberration)
            {
                _postProcessing.ChromaticAberration = targetProfile.ChromaticAberration;
            }

            if (targetProfile.Distortion)
            {
                _postProcessing.Distortion = targetProfile.Distortion;
            }

            if (targetProfile.Vignette)
            {
                _postProcessing.Vignette = targetProfile.Vignette;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (currProfile == null || targetProfile == null || _postProcessing == null)
                {
                    _transitioningPostProcessing = false;
                    return;
                }

                float t = elapsed / duration;
                _postProcessing.BlurAmount = Mathf.Lerp(currProfile.BlurAmount, targetProfile.BlurAmount, t);
                _postProcessing.BloomColor = Color.Lerp(currProfile.BloomColor, targetProfile.BloomColor, t);
                _postProcessing.BloomAmount = Mathf.Lerp(currProfile.BloomAmount, targetProfile.BloomAmount, t);
                _postProcessing.BloomDiffuse = Mathf.Lerp(currProfile.BloomDiffuse, targetProfile.BloomDiffuse, t);
                _postProcessing.BloomThreshold = Mathf.Lerp(currProfile.BloomThreshold, targetProfile.BloomThreshold, t);
                _postProcessing.BloomSoftness = Mathf.Lerp(currProfile.BloomSoftness, targetProfile.BloomSoftness, t);
                _postProcessing.Color = Color.Lerp(currProfile.Color, targetProfile.Color, t);
                _postProcessing.Contrast = Mathf.Lerp(currProfile.Contrast, targetProfile.Contrast, t);
                _postProcessing.Brightness = Mathf.Lerp(currProfile.Brightness, targetProfile.Brightness, t);
                _postProcessing.Saturation = Mathf.Lerp(currProfile.Saturation, targetProfile.Saturation, t);
                _postProcessing.Exposure = Mathf.Lerp(currProfile.Exposure, targetProfile.Exposure, t);
                _postProcessing.Gamma = Mathf.Lerp(currProfile.Gamma, targetProfile.Gamma, t);
                _postProcessing.Sharpness = Mathf.Lerp(currProfile.Sharpness, targetProfile.Sharpness, t);
                _postProcessing.Offset = Mathf.Lerp(currProfile.Offset, targetProfile.Offset, t);
                _postProcessing.FishEyeDistortion = Mathf.Lerp(currProfile.FishEyeDistortion, targetProfile.FishEyeDistortion, t);
                _postProcessing.GlitchAmount = Mathf.Lerp(currProfile.GlitchAmount, targetProfile.GlitchAmount, t);
                _postProcessing.LensDistortion = Mathf.Lerp(currProfile.LensDistortion, targetProfile.LensDistortion, t);
                _postProcessing.VignetteColor = Color.Lerp(currProfile.VignetteColor, targetProfile.VignetteColor, t);
                _postProcessing.VignetteAmount = Mathf.Lerp(currProfile.VignetteAmount, targetProfile.VignetteAmount, t);
                _postProcessing.VignetteSoftness = Mathf.Lerp(currProfile.VignetteSoftness, targetProfile.VignetteSoftness, t);

                elapsed += Time.deltaTime;
                await UniTask.NextFrame(token);
            }

            _postProcessing.Blur = targetProfile.Blur;
            _postProcessing.Bloom = targetProfile.Bloom;
            _postProcessing.ChromaticAberration = targetProfile.ChromaticAberration;
            _postProcessing.Distortion = targetProfile.Distortion;
            _postProcessing.Vignette = targetProfile.Vignette;

            await LoadPostProcessingProfileAsync(targetProfile, token, false);
            _transitioningPostProcessing = false;
        }
        #endregion
    }
}
#nullable disable