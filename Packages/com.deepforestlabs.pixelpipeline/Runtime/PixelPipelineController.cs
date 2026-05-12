#nullable enable
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
    public class PixelPipelineController : MonoBehaviour
    {
        [Header("Cameras")]
        [SerializeField] private Camera farCamera;
        [SerializeField] private Camera nearCamera;
        [SerializeField] private Camera compositeCamera;
        [SerializeField] private Camera viewmodelCamera;

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

        private RenderTexture farRT;
        private RenderTexture nearRT;
        private RenderTexture viewmodelRT;
        private bool subscribedToWatcher;

        private static readonly int FarRTId = Shader.PropertyToID("_FarRT");
        private static readonly int NearRTId = Shader.PropertyToID("_NearRT");
        private static readonly int ViewmodelRTId = Shader.PropertyToID("_ViewmodelRT");
        private static readonly int FadeStartId = Shader.PropertyToID("_PixelPipelineFadeStart");
        private static readonly int FadeEndId = Shader.PropertyToID("_PixelPipelineFadeEnd");

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
    }
}
#nullable disable
