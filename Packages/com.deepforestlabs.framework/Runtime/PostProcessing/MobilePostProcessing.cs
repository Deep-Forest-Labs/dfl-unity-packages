#nullable enable
using Unity.Collections;
using UnityEngine;

namespace DeepForestLabs.PostProcessing
{
	[ExecuteInEditMode]
	public sealed class MobilePostProcessing : MonoBehaviour
	{
		[SerializeField] public bool Blur = false;
		[Range(0, 1)]
		[SerializeField] public float BlurAmount = 1f;
		[SerializeField] public Texture2D? BlurMask = default!;
		[SerializeField] public bool Bloom = false;
		[SerializeField] public Color BloomColor = Color.white;
		[Range(0, 5)]
		[SerializeField] public float BloomAmount = 1f;
		[Range(0, 1)]
		[SerializeField] public float BloomDiffuse = 1f;
		[Range(0, 1)]
		[SerializeField] public float BloomThreshold = 0f;
		[Range(0, 1)]
		[SerializeField] public float BloomSoftness = 0f;

		[SerializeField] public bool LUT = false;
		[Range(0, 1)]
		[SerializeField] public float LutAmount = 0.0f;
		[SerializeField] public Texture2D? SourceLut = default;

		[SerializeField] public bool ImageFiltering = false;
		[SerializeField] public Color Color = Color.white;
		[Range(0, 1)]
		[SerializeField] public float Contrast = 0f;
		[Range(-1, 1)]
		[SerializeField] public float Brightness = 0f;
		[Range(-1, 1)]
		[SerializeField] public float Saturation = 0f;
		[Range(-1, 1)]
		[SerializeField] public float Exposure = 0f;
		[Range(-1, 1)]
		[SerializeField] public float Gamma = 0f;
		[Range(0, 1)]
		[SerializeField] public float Sharpness = 0f;

		[SerializeField] public bool ChromaticAberration = false;
		[SerializeField] public float Offset = 0;
		[Range(-1, 1)]
		[SerializeField] public float FishEyeDistortion = 0;
		[Range(0, 1)]
		[SerializeField] public float GlitchAmount = 0;

		[SerializeField] public bool Distortion = false;
		[Range(0, 1)]
		[SerializeField] public float LensDistortion = 0;

		[SerializeField] public bool Vignette = false;
		[SerializeField] public Color VignetteColor = Color.black;
		[Range(0, 1)]
		[SerializeField] public float VignetteAmount = 0f;
		[Range(0.001f, 1)]
		[SerializeField] public float VignetteSoftness = 0.0001f;

		[SerializeField] private Material _material = null!;
		[HideInInspector] [SerializeField] public PostProcessingProfile? Profile = null;
		public Material? Material { get; set; }

		private Texture2D? previous;
		private Texture3D? converted3D = null;
		private float t, a, knee;
		private int numberOfPasses = 3;

		[SerializeField]
		private bool _screenCam = false;

		private void Awake()
		{
			if (_material != null)
			{
				Material = new Material(_material);
			}
		}

		private void Start()
		{
			if (_screenCam)
			{
				SetBlurMask(BlurMask);
			}
		}

		public void Update()
		{
			if (SourceLut != previous)
			{
				previous = SourceLut;
				Convert3D(SourceLut);
			}
		}

		private void OnDestroy()
		{
			DestroyImmediate(Material);
			
			if (converted3D != null)
			{
				DestroyImmediate(converted3D);
			}
			converted3D = null;
		}

		private void Convert3D(Texture2D? temp3DTex)
		{
			if (temp3DTex == null)
			{
				return;
			}
			
			Color[]? color = temp3DTex.GetPixels();
			Color[] newCol = new Color[color.Length];

			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					for (int k = 0; k < 16; k++)
					{
						int val = 16 - j - 1;
						newCol[i + (j * 16) + (k * 256)] = color[k * 16 + i + val * 256];
					}
				}
			}
			if (converted3D)
			{
				DestroyImmediate(converted3D);
			}
			converted3D = new Texture3D(16, 16, 16, TextureFormat.ARGB32, false);
			converted3D.SetPixels(newCol);
			converted3D.Apply();
			converted3D.wrapMode = TextureWrapMode.Clamp;
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Material == null)
			{
				if (Application.isEditor || !Application.isPlaying)
				{
					Graphics.Blit(Texture2D.blackTexture, destination);
					GL.Clear(true, true, Color.black);
				}
				else
				{
					Graphics.Blit(source, destination);
				}
				return;
			}
			
			if (Blur || Bloom)
			{
				Material.DisableKeyword(BlurKeyword);
				Material.DisableKeyword(BloomKeyword);
				if (Bloom)
				{
					Material.EnableKeyword(BloomKeyword);
					Material.SetColor(bloomColorString, BloomColor * BloomAmount);
					Material.SetFloat(blDiffuseString, BloomDiffuse);
					numberOfPasses = Mathf.Max(Mathf.CeilToInt(BloomDiffuse * 4), 1);
					Material.SetFloat(blDiffuseString, numberOfPasses > 1 ? (BloomDiffuse * 4 - Mathf.FloorToInt(BloomDiffuse * 4 - 0.001f)) * 0.5f + 0.5f : BloomDiffuse * 4);
					knee = BloomThreshold * BloomSoftness;
					Material.SetVector(blDataString, new Vector4(BloomThreshold, BloomThreshold - knee, 2f * knee, 1f / (4f * knee + 0.00001f)));
				}
				if (Blur) 
				{
					Material.EnableKeyword(BlurKeyword);
					numberOfPasses = Mathf.Max(Mathf.CeilToInt(BlurAmount * 4), 1);
					Material.SetFloat(blurAmountString, numberOfPasses > 1 ? (BlurAmount * 4 - Mathf.FloorToInt(BlurAmount * 4 - 0.001f)) * 0.5f + 0.5f : BlurAmount * 4);
				}

				if (BlurAmount > 0 || !Blur)
				{
					RenderTexture? blurTex = null;

					if (numberOfPasses == 1)
					{
						blurTex = RenderTexture.GetTemporary(Screen.width / 2, Screen.height / 2, 0, source.format);
						Graphics.Blit(source, blurTex, Material, 0);
					}
					else if (numberOfPasses == 2)
					{
						blurTex = RenderTexture.GetTemporary(Screen.width / 2, Screen.height / 2, 0, source.format);
						RenderTexture? temp1 = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
						Graphics.Blit(source, temp1, Material, 0);
						Graphics.Blit(temp1, blurTex, Material, 0);
						RenderTexture.ReleaseTemporary(temp1);
					}
					else if (numberOfPasses == 3)
					{
						blurTex = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
						RenderTexture? temp1 = RenderTexture.GetTemporary(Screen.width / 8, Screen.height / 8, 0, source.format);
						Graphics.Blit(source, blurTex, Material, 0);
						Graphics.Blit(blurTex, temp1, Material, 0);
						Graphics.Blit(temp1, blurTex, Material, 0);
						RenderTexture.ReleaseTemporary(temp1);
					}              
					else if (numberOfPasses == 4)
					{
						blurTex = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
						RenderTexture? temp1 = RenderTexture.GetTemporary(Screen.width / 8, Screen.height / 8, 0, source.format);
						RenderTexture? temp2 = RenderTexture.GetTemporary(Screen.width / 16, Screen.height / 16, 0, source.format);
						Graphics.Blit(source, blurTex, Material, 0);
						Graphics.Blit(blurTex, temp1, Material, 0);
						Graphics.Blit(temp1, temp2, Material, 0);
						Graphics.Blit(temp2, temp1, Material, 0);
						Graphics.Blit(temp1, blurTex, Material, 0);
						RenderTexture.ReleaseTemporary(temp1);
						RenderTexture.ReleaseTemporary(temp2);
					}

					Material.SetTexture(blurTexString, blurTex);
					RenderTexture.ReleaseTemporary(blurTex);
				}
				else
				{
					Material.SetTexture(blurTexString, source);
				}
			}
			else
			{
				Material.DisableKeyword(BlurKeyword);
				Material.DisableKeyword(BloomKeyword);
			}

			if (LUT)
			{
				Material.EnableKeyword(LutKeyword);
				Material.SetFloat(lutAmountString, LutAmount);
				Material.SetTexture(lutTextureString, converted3D);
			}
			else
			{
				Material.DisableKeyword(LutKeyword);
			}

			if (ImageFiltering)
			{
				Material.EnableKeyword(FilterKeyword);
				Material.SetColor(colorString, (Mathf.Pow(2, Exposure) - Gamma) * Color);
				Material.SetFloat(contrastString, Contrast + 1f);
				Material.SetFloat(brightnessString, Brightness * 0.5f - Contrast);
				Material.SetFloat(saturationString, Saturation + 1f);

				if (Sharpness > 0)
				{
					Material.EnableKeyword(SharpenKeyword);
					Material.SetFloat(centralFactorString, 1.0f + (3.2f * Sharpness));
					Material.SetFloat(sideFactorString, 0.8f * Sharpness);
				}
				else
				{
					Material.DisableKeyword(SharpenKeyword);
				}
			}
			else
			{
				Material.DisableKeyword(FilterKeyword);
				Material.DisableKeyword(SharpenKeyword);
			}

			if (ChromaticAberration)
			{
				Material.EnableKeyword(ChromaKeyword);
				if (GlitchAmount > 0)
				{
					t = Time.realtimeSinceStartup;
					a = (1.0f + Mathf.Sin(t * 6.0f)) * ((0.5f + Mathf.Sin(t * 16.0f) * 0.25f)) * (0.5f + Mathf.Sin(t * 19.0f) * 0.25f) * (0.5f + Mathf.Sin(t * 27.0f) * 0.25f);
					Material.SetFloat(offsetString, 10 * Offset + GlitchAmount * Mathf.Pow(a, 3.0f) * 200);
				}
				else
				{
					Material.SetFloat(offsetString, 10 * Offset);
				}

				Material.SetFloat(fishEyeString, 0.1f * FishEyeDistortion);
			}
			else
			{
				Material.DisableKeyword(ChromaKeyword);
			}

			if (Distortion)
			{
				Material.SetFloat(lensdistortionString, -LensDistortion);
				Material.EnableKeyword(DistortionKeyword);
			}
			else
			{
				Material.DisableKeyword(DistortionKeyword);
			}

			if (Vignette)
			{
				Material.SetColor(vignetteColorString, VignetteColor);
				Material.SetFloat(vignetteAmountString, 1 - VignetteAmount);
				Material.SetFloat(vignetteSoftnessString, 1 - VignetteSoftness - VignetteAmount);
			}
			else
			{
				Material.SetFloat(vignetteAmountString, 1f);
				Material.SetFloat(vignetteSoftnessString, 0.999f);
			}

			Graphics.Blit(source, destination, Material, 1);
		}

		public void SetBlurMask(Texture2D? texture)
		{
			BlurMask = texture;
			if (BlurMask == null)
			{
				Shader.SetGlobalTexture(_screenCam ? maskTextureStringScreenCam : maskTextureString, Texture2D.whiteTexture);
			}
			else
			{
				Shader.SetGlobalTexture(_screenCam ? maskTextureStringScreenCam : maskTextureString, BlurMask);
			}
		}

		private static readonly int blurTexString = Shader.PropertyToID("_BlurTex");
		private static readonly int maskTextureString = Shader.PropertyToID("_MaskTex");
		private static readonly int maskTextureStringScreenCam = Shader.PropertyToID("_ScreenCamMaskTex");
		private static readonly int blurAmountString = Shader.PropertyToID("_BlurAmount");
		private static readonly int bloomColorString = Shader.PropertyToID("_BloomColor");
		private static readonly int blDiffuseString = Shader.PropertyToID("_BloomDiffuse");
		private static readonly int blDataString = Shader.PropertyToID("_BloomData");
		private static readonly int lutTextureString = Shader.PropertyToID("_LutTex");
		private static readonly int lutAmountString = Shader.PropertyToID("_LutAmount");
		private static readonly int colorString = Shader.PropertyToID("_Color");
		private static readonly int contrastString = Shader.PropertyToID("_Contrast");
		private static readonly int brightnessString = Shader.PropertyToID("_Brightness");
		private static readonly int saturationString = Shader.PropertyToID("_Saturation");
		private static readonly int centralFactorString = Shader.PropertyToID("_CentralFactor");
		private static readonly int sideFactorString = Shader.PropertyToID("_SideFactor");
		private static readonly int offsetString = Shader.PropertyToID("_Offset");
		private static readonly int fishEyeString = Shader.PropertyToID("_FishEye");
		private static readonly int lensdistortionString = Shader.PropertyToID("_LensDistortion");
		private static readonly int vignetteColorString = Shader.PropertyToID("_VignetteColor");
		private static readonly int vignetteAmountString = Shader.PropertyToID("_VignetteAmount");
		private static readonly int vignetteSoftnessString = Shader.PropertyToID("_VignetteSoftness");

		public static readonly string BloomKeyword = "BLOOM";
		public static readonly string BlurKeyword = "BLUR";
		public static readonly string ChromaKeyword = "CHROMA";
		public static readonly string LutKeyword = "LUT";
		public static readonly string FilterKeyword = "FILTER";
		public static readonly string SharpenKeyword = "SHARPEN";
		public static readonly string DistortionKeyword = "DISTORTION";
	}
}
#nullable disable