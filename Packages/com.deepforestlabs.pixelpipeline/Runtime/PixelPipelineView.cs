#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.MVC.Views;
using UnityEngine;

namespace DeepForestLabs.PixelPipeline
{
    /// <summary>
    /// Owns the Far, Near, and Viewmodel render textures, assigns them to offscreen
    /// cameras, and publishes them (plus fade parameters) as global shader values so
    /// the renderer features can read them without per-material wiring.
    ///
    /// Event-driven: field changes go through public property setters, screen
    /// resizes come from ScreenSizeWatcher, and no per-frame work runs.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [ExecuteAlways]
    public class PixelPipelineView : MonoBehaviour, IView
    {
        [Header("Cameras")]
        [SerializeField] private Camera farCamera;
        [SerializeField] private Camera nearCamera;
        [SerializeField] private Camera compositeCamera;
        [SerializeField] private Camera viewmodelCamera;

        [Header("Viewmodel Anchors")]
        [SerializeField] private Transform primaryWeaponAnchor;
        [SerializeField] private Transform secondaryWeaponAnchor;
        [SerializeField] private Transform spellHandAnchor;

        public Camera FarCamera => farCamera;
        public Camera CompositeCamera => compositeCamera;
        public Transform PrimaryWeaponAnchor => primaryWeaponAnchor;
        public Transform SecondaryWeaponAnchor => secondaryWeaponAnchor;
        public Transform SpellHandAnchor => spellHandAnchor;

        [Header("Screen Size Source")]
        [SerializeField] private ScreenSizeWatcher screenSizeWatcher;

        [Header("Pixel Size")]
        [SerializeField, Range(1, 8)] private int pixelScale = 3;
        [SerializeField, Range(1, 4)] private int farBandMultiplier = 2;

        [Header("Camera")]
        [SerializeField, Range(30f, 110f)] private float fieldOfView = 60f;
        [SerializeField, Range(30f, 110f)] private float viewmodelFieldOfView = 50f;

        [Header("Simulated Display")]
        [SerializeField] private Vector2Int simulatedResolution = Vector2Int.zero;

        [Header("Dither Fade (eye-space meters)")]
        [SerializeField] private float fadeStart = 25f;
        [SerializeField] private float fadeEnd = 35f;

        [Header("Far Fog (Retro Dither)")]
        [SerializeField] private bool farFogEnabled = true;
        [SerializeField] private float farFogStart = 30f;
        [SerializeField] private float farFogEnd = 50f;
        [SerializeField] private Color farFogColorLight = new(0.08f, 0.05f, 0.12f);
        [SerializeField] private Color farFogColorDark = new(0.02f, 0.01f, 0.04f);

        [Header("Far Color Shift")]
        [SerializeField] private bool farColorShiftEnabled = true;
        [SerializeField] private float farColorShiftStart = 20f;
        [SerializeField] private float farColorShiftEnd = 35f;
        [SerializeField] private Color farColorShiftTint = new(0.6f, 0.6f, 0.8f, 1f);
        [SerializeField, Range(0f, 1f)] private float farColorShiftDesaturation = 0.7f;

        [Header("Color Quantization")]
        [SerializeField] private float colorQuantizationLevels;
        [SerializeField] private bool quantizeViewmodel;

        private RenderTexture farRT;
        private RenderTexture nearRT;
        private RenderTexture viewmodelRT;
        private bool subscribedToWatcher;

        private static readonly int FarRTId = Shader.PropertyToID("_FarRT");
        private static readonly int NearRTId = Shader.PropertyToID("_NearRT");
        private static readonly int ViewmodelRTId = Shader.PropertyToID("_ViewmodelRT");
        private static readonly int FadeStartId = Shader.PropertyToID("_PixelPipelineFadeStart");
        private static readonly int FadeEndId = Shader.PropertyToID("_PixelPipelineFadeEnd");

        private static readonly int FarFogEnabledId = Shader.PropertyToID("_FarFogEnabled");
        private static readonly int FarFogStartId = Shader.PropertyToID("_FarFogStart");
        private static readonly int FarFogEndId = Shader.PropertyToID("_FarFogEnd");
        private static readonly int FarFogColorLightId = Shader.PropertyToID("_FarFogColorLight");
        private static readonly int FarFogColorDarkId = Shader.PropertyToID("_FarFogColorDark");

        private static readonly int FarColorShiftEnabledId = Shader.PropertyToID("_FarColorShiftEnabled");
        private static readonly int FarColorShiftStartId = Shader.PropertyToID("_FarColorShiftStart");
        private static readonly int FarColorShiftEndId = Shader.PropertyToID("_FarColorShiftEnd");
        private static readonly int FarColorShiftTintId = Shader.PropertyToID("_FarColorShiftTint");
        private static readonly int FarColorShiftDesaturationId = Shader.PropertyToID("_FarColorShiftDesaturation");

        private static readonly int ColorQuantizationLevelsId = Shader.PropertyToID("_ColorQuantizationLevels");
        private static readonly int QuantizeViewmodelId = Shader.PropertyToID("_QuantizeViewmodel");
        private static readonly int PixelScaleId = Shader.PropertyToID("_PixelPipelinePixelScale");

        public int PixelScale
        {
            get => pixelScale;
            set
            {
                int clamped = Mathf.Clamp(value, 1, 8);
                if (pixelScale == clamped) return;
                pixelScale = clamped;
                ReallocateRenderTextures();
            }
        }

        public int FarBandMultiplier
        {
            get => farBandMultiplier;
            set
            {
                int clamped = Mathf.Clamp(value, 1, 4);
                if (farBandMultiplier == clamped) return;
                farBandMultiplier = clamped;
                ReallocateRenderTextures();
            }
        }

        public float FieldOfView
        {
            get => fieldOfView;
            set
            {
                float clamped = Mathf.Clamp(value, 30f, 110f);
                if (Mathf.Approximately(fieldOfView, clamped)) return;
                fieldOfView = clamped;
                ApplyFieldOfView();
            }
        }

        public float ViewmodelFieldOfView
        {
            get => viewmodelFieldOfView;
            set
            {
                float clamped = Mathf.Clamp(value, 30f, 110f);
                if (Mathf.Approximately(viewmodelFieldOfView, clamped)) return;
                viewmodelFieldOfView = clamped;
                ApplyFieldOfView();
            }
        }

        private float worldZoom = 1f;

        /// <summary>
        /// Multiplier applied to <see cref="FieldOfView"/>. Effective world camera
        /// FOV = fieldOfView / WorldZoom. A value of 1 means no zoom.
        /// </summary>
        public float WorldZoom
        {
            get => worldZoom;
            set
            {
                float clamped = Mathf.Max(0.2f, value);
                if (Mathf.Approximately(worldZoom, clamped)) return;
                worldZoom = clamped;
                ApplyFieldOfView();
            }
        }

        private float viewmodelZoom = 1f;

        /// <summary>
        /// Multiplier applied to <see cref="ViewmodelFieldOfView"/>.
        /// Effective viewmodel FOV = viewmodelFieldOfView / ViewmodelZoom.
        /// </summary>
        public float ViewmodelZoom
        {
            get => viewmodelZoom;
            set
            {
                float clamped = Mathf.Max(0.2f, value);
                if (Mathf.Approximately(viewmodelZoom, clamped)) return;
                viewmodelZoom = clamped;
                ApplyFieldOfView();
            }
        }

        public Vector2Int SimulatedResolution
        {
            get => simulatedResolution;
            set
            {
                Vector2Int clamped = new Vector2Int(Mathf.Max(0, value.x), Mathf.Max(0, value.y));
                if (simulatedResolution == clamped) return;
                simulatedResolution = clamped;
                ApplySimulatedResolution();
            }
        }

        public bool FarFogEnabled
        {
            get => farFogEnabled;
            set
            {
                if (farFogEnabled == value) return;
                farFogEnabled = value;
                PublishFogGlobals();
            }
        }

        public float FarFogStart
        {
            get => farFogStart;
            set
            {
                if (Mathf.Approximately(farFogStart, value)) return;
                farFogStart = value;
                PublishFogGlobals();
            }
        }

        public float FarFogEnd
        {
            get => farFogEnd;
            set
            {
                if (Mathf.Approximately(farFogEnd, value)) return;
                farFogEnd = value;
                PublishFogGlobals();
            }
        }

        public Color FarFogColorLight
        {
            get => farFogColorLight;
            set
            {
                if (farFogColorLight == value) return;
                farFogColorLight = value;
                PublishFogGlobals();
            }
        }

        public Color FarFogColorDark
        {
            get => farFogColorDark;
            set
            {
                if (farFogColorDark == value) return;
                farFogColorDark = value;
                PublishFogGlobals();
            }
        }

        public bool FarColorShiftEnabled
        {
            get => farColorShiftEnabled;
            set
            {
                if (farColorShiftEnabled == value) return;
                farColorShiftEnabled = value;
                PublishFogGlobals();
            }
        }

        public float FarColorShiftStart
        {
            get => farColorShiftStart;
            set
            {
                if (Mathf.Approximately(farColorShiftStart, value)) return;
                farColorShiftStart = value;
                PublishFogGlobals();
            }
        }

        public float FarColorShiftEnd
        {
            get => farColorShiftEnd;
            set
            {
                if (Mathf.Approximately(farColorShiftEnd, value)) return;
                farColorShiftEnd = value;
                PublishFogGlobals();
            }
        }

        public Color FarColorShiftTint
        {
            get => farColorShiftTint;
            set
            {
                if (farColorShiftTint == value) return;
                farColorShiftTint = value;
                PublishFogGlobals();
            }
        }

        public float FarColorShiftDesaturation
        {
            get => farColorShiftDesaturation;
            set
            {
                float clamped = Mathf.Clamp01(value);
                if (Mathf.Approximately(farColorShiftDesaturation, clamped)) return;
                farColorShiftDesaturation = clamped;
                PublishFogGlobals();
            }
        }

        public float ColorQuantizationLevels
        {
            get => colorQuantizationLevels;
            set
            {
                if (Mathf.Approximately(colorQuantizationLevels, value)) return;
                colorQuantizationLevels = value;
                PublishQuantizationGlobals();
            }
        }

        public bool QuantizeViewmodel
        {
            get => quantizeViewmodel;
            set
            {
                if (quantizeViewmodel == value) return;
                quantizeViewmodel = value;
                PublishQuantizationGlobals();
            }
        }

        private void OnEnable()
        {
            SubscribeToWatcher();
            ApplyAll();
        }

        private void OnDisable()
        {
            UnsubscribeFromWatcher();
            DetachRenderTextures();
        }

        private void OnValidate()
        {
            if (fadeEnd <= fadeStart) fadeEnd = fadeStart + 0.01f;
            if (farFogEnd <= farFogStart) farFogEnd = farFogStart + 0.01f;
            if (farColorShiftEnd <= farColorShiftStart) farColorShiftEnd = farColorShiftStart + 0.01f;
            if (simulatedResolution.x < 0) simulatedResolution.x = 0;
            if (simulatedResolution.y < 0) simulatedResolution.y = 0;

            if (!isActiveAndEnabled) return;
            if (!subscribedToWatcher) SubscribeToWatcher();
            ApplyAll();
        }

        private void OnScreenSizeChanged()
        {
            ReallocateRenderTextures();
        }

        private void SubscribeToWatcher()
        {
            if (subscribedToWatcher || screenSizeWatcher == null) return;
            screenSizeWatcher.ScreenSizeChanged += OnScreenSizeChanged;
            subscribedToWatcher = true;
        }

        private void UnsubscribeFromWatcher()
        {
            if (!subscribedToWatcher || screenSizeWatcher == null)
            {
                subscribedToWatcher = false;
                return;
            }
            screenSizeWatcher.ScreenSizeChanged -= OnScreenSizeChanged;
            subscribedToWatcher = false;
        }

        private void ApplyAll()
        {
            ApplySimulatedResolution();
            ReallocateRenderTextures();
            ApplyFieldOfView();
            PublishFadeGlobals();
            PublishFogGlobals();
            PublishQuantizationGlobals();
        }

        private void ApplySimulatedResolution()
        {
            if (!Application.isPlaying) return;
            if (simulatedResolution.x <= 0 || simulatedResolution.y <= 0) return;
            if (Screen.width == simulatedResolution.x && Screen.height == simulatedResolution.y) return;
            Screen.SetResolution(simulatedResolution.x, simulatedResolution.y, Screen.fullScreenMode);
        }

        private void ApplyFieldOfView()
        {
            float worldFov = fieldOfView / Mathf.Max(0.2f, worldZoom);
            if (farCamera != null) farCamera.fieldOfView = worldFov;
            if (nearCamera != null) nearCamera.fieldOfView = worldFov;
            if (viewmodelCamera != null)
                viewmodelCamera.fieldOfView = viewmodelFieldOfView / Mathf.Max(0.2f, viewmodelZoom);
        }

        private void GetEffectiveScreenSize(out int screenW, out int screenH)
        {
            if (screenSizeWatcher != null && screenSizeWatcher.HasValidSize)
            {
                screenW = Mathf.Max(1, screenSizeWatcher.CachedWidth);
                screenH = Mathf.Max(1, screenSizeWatcher.CachedHeight);
                return;
            }
            screenW = Mathf.Max(1, Screen.width);
            screenH = Mathf.Max(1, Screen.height);
        }

        private void ReallocateRenderTextures()
        {
            GetEffectiveScreenSize(out int screenW, out int screenH);

            int scale = Mathf.Max(1, pixelScale);
            int bandMul = Mathf.Max(1, farBandMultiplier);

            int farW = Mathf.Max(16, screenW / scale);
            int farH = Mathf.Max(16, screenH / scale);
            int nearW = Mathf.Max(8, farW / bandMul);
            int nearH = Mathf.Max(8, farH / bandMul);

            EnsureRenderTexture(ref farRT, farW, farH, RenderTextureFormat.ARGB32, 16, "PixelPipeline_FarRT");
            EnsureRenderTexture(ref nearRT, nearW, nearH, RenderTextureFormat.ARGB32, 16, "PixelPipeline_NearRT");
            EnsureRenderTexture(ref viewmodelRT, farW, farH, RenderTextureFormat.ARGB32, 16, "PixelPipeline_ViewmodelRT");

            if (farCamera != null) farCamera.targetTexture = farRT;
            if (nearCamera != null) nearCamera.targetTexture = nearRT;
            if (viewmodelCamera != null) viewmodelCamera.targetTexture = viewmodelRT;

            PublishTextureGlobals();
        }

        private void DetachRenderTextures()
        {
            if (farCamera != null && farCamera.targetTexture == farRT) farCamera.targetTexture = null;
            if (nearCamera != null && nearCamera.targetTexture == nearRT) nearCamera.targetTexture = null;
            if (viewmodelCamera != null && viewmodelCamera.targetTexture == viewmodelRT) viewmodelCamera.targetTexture = null;

            ReleaseTexture(ref farRT);
            ReleaseTexture(ref nearRT);
            ReleaseTexture(ref viewmodelRT);
        }

        private static void EnsureRenderTexture(ref RenderTexture rt, int width, int height,
            RenderTextureFormat format, int depth, string rtName)
        {
            if (rt != null && rt.width == width && rt.height == height && rt.format == format && rt.depth == depth)
                return;

            ReleaseTexture(ref rt);

            rt = new RenderTexture(width, height, depth, format, RenderTextureReadWrite.sRGB)
            {
                name = rtName,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0,
                useMipMap = false,
                autoGenerateMips = false
            };
            rt.Create();
        }

        private static void ReleaseTexture(ref RenderTexture rt)
        {
            if (rt == null) return;
            if (rt.IsCreated()) rt.Release();

            if (Application.isPlaying)
                Destroy(rt);
            else
                DestroyImmediate(rt);

            rt = null;
        }

        private void PublishTextureGlobals()
        {
            if (farRT != null) Shader.SetGlobalTexture(FarRTId, farRT);
            if (nearRT != null) Shader.SetGlobalTexture(NearRTId, nearRT);
            if (viewmodelRT != null) Shader.SetGlobalTexture(ViewmodelRTId, viewmodelRT);
        }

        private void PublishFadeGlobals()
        {
            Shader.SetGlobalFloat(FadeStartId, fadeStart);
            Shader.SetGlobalFloat(FadeEndId, fadeEnd);
        }

        private void PublishFogGlobals()
        {
            Shader.SetGlobalFloat(FarFogEnabledId, farFogEnabled ? 1f : 0f);
            Shader.SetGlobalFloat(FarFogStartId, farFogStart);
            Shader.SetGlobalFloat(FarFogEndId, Mathf.Max(farFogEnd, farFogStart + 0.01f));
            Shader.SetGlobalColor(FarFogColorLightId, farFogColorLight);
            Shader.SetGlobalColor(FarFogColorDarkId, farFogColorDark);

            Shader.SetGlobalFloat(FarColorShiftEnabledId, farColorShiftEnabled ? 1f : 0f);
            Shader.SetGlobalFloat(FarColorShiftStartId, farColorShiftStart);
            Shader.SetGlobalFloat(FarColorShiftEndId, Mathf.Max(farColorShiftEnd, farColorShiftStart + 0.01f));
            Shader.SetGlobalColor(FarColorShiftTintId, farColorShiftTint);
            Shader.SetGlobalFloat(FarColorShiftDesaturationId, farColorShiftDesaturation);
        }

        private void PublishQuantizationGlobals()
        {
            Shader.SetGlobalFloat(ColorQuantizationLevelsId, colorQuantizationLevels);
            Shader.SetGlobalFloat(QuantizeViewmodelId, quantizeViewmodel ? 1f : 0f);
            Shader.SetGlobalFloat(PixelScaleId, pixelScale);
        }

        public void ApplyKickOffset(float pitchDeg)
        {
            if (viewmodelCamera != null)
                viewmodelCamera.transform.localRotation = Quaternion.Euler(-pitchDeg, 0f, 0f);
        }

        public UniTask OpenAnimation(CancellationToken token) => UniTask.CompletedTask;
        public void OpenAnimationFinished() { }
        public UniTask CloseAnimation(CancellationToken token) => UniTask.CompletedTask;
        public void CloseAnimationFinished() { }
    }
}
#nullable disable
