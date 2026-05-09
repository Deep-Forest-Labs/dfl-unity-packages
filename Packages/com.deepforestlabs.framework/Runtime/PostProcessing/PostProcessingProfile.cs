#nullable enable
using DeepForestLabs.Data;
using UnityEngine;

namespace DeepForestLabs.PostProcessing
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PostProcessingProfile", order = 1)]
    public sealed class PostProcessingProfile : ValidatedData
    {
        [SerializeField]
        private float _transitionDuration = 0f;
        public float TransitionDuration
        {
            get { return _transitionDuration; }
            private set { _transitionDuration = value; }
        }

        [Space(10)]

        #region Blur

        [SerializeField]
        private bool _blur = false;
        public bool Blur
        {
            get { return _blur; }
            private set { _blur = value; }
        }

        [Range(0, 1)]
        [SerializeField]
        private float _blurAmount = 1f;
        public float BlurAmount
        {
            get { return _blurAmount; }
            private set { _blurAmount = value; }
        }

        [SerializeField] [Optional]
        private Texture2D? _blurMask = null;
        public Texture2D? BlurMask
        {
            get { return _blurMask; }
            private set { _blurMask = value; }
        }

        #endregion

        [Space(10)]

        #region Bloom

        [SerializeField]
        private bool _bloom = false;
        public bool Bloom
        {
            get { return _bloom; }
            private set { _bloom = value; }
        }

        [SerializeField]
        private Color _bloomColor = Color.white;
        public Color BloomColor
        {
            get { return _bloomColor; }
            private set { _bloomColor = value; }
        }

        [Range(0, 5)]
        [SerializeField]
        private float _bloomAmount = 1f;
        public float BloomAmount
        {
            get { return _bloomAmount; }
            private set { _bloomAmount = value; }
        }

        [Range(0, 1)]
        [SerializeField]
        private float _bloomDiffuse = 1f;
        public float BloomDiffuse
        {
            get { return _bloomDiffuse; }
            private set { _bloomDiffuse = value; }
        }

        [Range(0, 1)]
        [SerializeField]
        private float _bloomThreshold = 0f;
        public float BloomThreshold
        {
            get { return _bloomThreshold; }
            private set { _bloomThreshold = value; }
        }

        [Range(0, 1)]
        [SerializeField]
        private float _bloomSoftness = 0f;
        public float BloomSoftness
        {
            get { return _bloomSoftness; }
            private set { _bloomSoftness = value; }
        }

        #endregion

        [Space(10)]

        #region Image Filtering

        [SerializeField]
        private bool _imageFiltering = true;
        public bool ImageFiltering
        {
            get { return _imageFiltering; }
            private set { _imageFiltering = value; }
        }

        [SerializeField]
        private Color _color = Color.white;
        public Color Color
        {
            get { return _color; }
            private set { _color = value; }
        }

        [Range(0, 1)]
        [SerializeField]
        private float _contrast = 0.15f;
        public float Contrast
        {
            get { return _contrast; }
            private set { _contrast = value; }
        }

        [Range(-1, 1)]
        [SerializeField]
        private float _brightness = 0f;
        public float Brightness
        {
            get { return _brightness; }
            private set { _brightness = value; }
        }

        [Range(-1, 1)]
        [SerializeField]
        private float _saturation = 0.1f;
        public float Saturation
        {
            get { return _saturation; }
            private set { _saturation = value; }
        }

        [Range(-1, 1)]
        [SerializeField]
        private float _exposure = 0.35f;
        public float Exposure
        {
            get { return _exposure; }
            private set { _exposure = value; }
        }

        [Range(-1, 1)]
        [SerializeField]
        private float _gamma = 0.15f;
        public float Gamma
        {
            get { return _gamma; }
            private set { _gamma = value; }
        }

        [Range(0, 1)]
        [SerializeField]
        private float _sharpness = 0f;
        public float Sharpness
        {
            get { return _sharpness; }
            private set { _sharpness = value; }
        }

        #endregion

        [Space(10)]

        #region Chromatic Abberation

        [SerializeField]
        private bool _chromaticAberation = false;
        public bool ChromaticAberration
        {
            get { return _chromaticAberation; }
            private set { _chromaticAberation = value; }
        }

        [SerializeField]
        private float _offset = 0f;
        public float Offset
        {
            get { return _offset; }
            private set { _offset = value; }
        }

        [Range(-1, 1)]
        [SerializeField]
        private float _fishEyeDistortion = 0f;
        public float FishEyeDistortion
        {
            get { return _fishEyeDistortion; }
            private set { _fishEyeDistortion = value; }
        }

        [Range(0, 1)]
        [SerializeField]
        private float _glitchAmount = 0f;
        public float GlitchAmount
        {
            get { return _glitchAmount; }
            private set { _glitchAmount = value; }
        }

        #endregion

        [Space(10)]

        #region Distortion

        [SerializeField]
        private bool _distortion = false;
        public bool Distortion
        {
            get { return _distortion; }
            private set { _distortion = value; }
        }

        [Range(0, 1)]
        [SerializeField]
        private float _lensDistortion = 0f;
        public float LensDistortion
        {
            get { return _lensDistortion; }
            private set { _lensDistortion = value; }
        }

        #endregion

        [Space(10)]

        #region Vignette

        [SerializeField]
        private bool _vignette = false;
        public bool Vignette
        {
            get { return _vignette; }
            private set { _vignette = value; }
        }

        [SerializeField]
        private Color _vignetteColor = Color.black;
        public Color VignetteColor
        {
            get { return _vignetteColor; }
            private set { _vignetteColor = value; }
        }

        [Range(0, 1)]
        [SerializeField]
        private float _vignetteAmount = 0f;
        public float VignetteAmount
        {
            get { return _vignetteAmount; }
            private set { _vignetteAmount = value; }
        }

        [Range(0.001f, 1)]
        [SerializeField]
        private float _vignetteSoftness = 0.0001f;

        public float VignetteSoftness
        {
            get { return _vignetteSoftness; }
            private set { _vignetteSoftness = value; }
        }

        #endregion

        public void Init(MobilePostProcessing postProcessing)
        {
            if (postProcessing == null)
            {
                return;
            }

            Blur = postProcessing.Blur;
            BlurAmount = postProcessing.BlurAmount;
            BlurMask = postProcessing.BlurMask;
            Bloom = postProcessing.Bloom;
            BloomColor = postProcessing.BloomColor;
            BloomAmount = postProcessing.BloomAmount;
            BloomDiffuse = postProcessing.BloomDiffuse;
            BloomThreshold = postProcessing.BloomThreshold;
            BloomSoftness = postProcessing.BloomSoftness;
            ImageFiltering = postProcessing.ImageFiltering;
            Color = postProcessing.Color;
            Contrast = postProcessing.Contrast;
            Brightness = postProcessing.Brightness;
            Saturation = postProcessing.Saturation;
            Exposure = postProcessing.Exposure;
            Gamma = postProcessing.Gamma;
            Sharpness = postProcessing.Sharpness;
            ChromaticAberration = postProcessing.ChromaticAberration;
            Offset = postProcessing.Offset;
            FishEyeDistortion = postProcessing.FishEyeDistortion;
            GlitchAmount = postProcessing.GlitchAmount;
            Distortion = postProcessing.Distortion;
            LensDistortion = postProcessing.LensDistortion;
            Vignette = postProcessing.Vignette;
            VignetteColor = postProcessing.VignetteColor;
            VignetteAmount = postProcessing.VignetteAmount;
            VignetteSoftness = postProcessing.VignetteSoftness;
        }
    }
}
#nullable disable