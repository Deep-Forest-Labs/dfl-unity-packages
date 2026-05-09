#nullable enable
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DeepForestLabs.Components
{
    public sealed partial class UICompositeImage
    {
        private static Material? _defaultProceduralMaterial;
        private static Sprite? _defaultWhiteSprite;

        private static Material GetProceduralMaterial()
        {
            if (_defaultProceduralMaterial == null)
            {
                Shader? shader = Shader.Find("UI/UIComposite_Procedural"); // updated to use the custom debug shader
                if (shader == null)
                {
                    throw new Exception("UI/UIComposite_Procedural shader not found. Please ensure it is included in the project.");
                }
                _defaultProceduralMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            return _defaultProceduralMaterial;
        }

        private static Sprite GetWhiteSprite()
        {
            if (_defaultWhiteSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _defaultWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                _defaultWhiteSprite.name = nameof(UICompositeImage);
                _defaultWhiteSprite.hideFlags = HideFlags.HideAndDontSave;
            }
            return _defaultWhiteSprite;
        }

        private void RenderDynamic(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);

            Rect rect = GetPixelAdjustedRect();
            float size = Mathf.Min(rect.width, rect.height);
            // DON'T force square rendering - this breaks nine-slice!

            // Use full, unclamped, unencoded radii, but normalize so that adjacent corners never overlap
            Vector4 radii = _radii;
            float totalTop = radii.x + radii.y;
            if (totalTop > size)
            {
                float scale = size / totalTop;
                radii.x *= scale;
                radii.y *= scale;
            }
            float totalBottom = radii.w + radii.z;
            if (totalBottom > size)
            {
                float scale = size / totalBottom;
                radii.w *= scale;
                radii.z *= scale;
            }
            Vector4 normalizedRadii = radii / size;

            Vector2 uv1 = new Vector2(rect.width + _falloffDistance, rect.height + _falloffDistance);
            Vector2 uv3 = new Vector2(
                _borderWidth == 0f ? 1f : Mathf.Clamp01(_borderWidth),
                1f / Mathf.Max(0.0001f, _falloffDistance)
            );

#if !RELEASE_MODE
            // Ensure canvas has additional shader channels for procedural preview
            Canvas? canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                AdditionalCanvasShaderChannels requiredChannels =
                    AdditionalCanvasShaderChannels.Normal |
                    AdditionalCanvasShaderChannels.Tangent |
                    AdditionalCanvasShaderChannels.TexCoord1 |
                    AdditionalCanvasShaderChannels.TexCoord3;

                if ((canvas.additionalShaderChannels & requiredChannels) != requiredChannels)
                {
                    canvas.additionalShaderChannels |= requiredChannels;
                }
            }
#endif

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                UIVertex vert = new UIVertex();
                vh.PopulateUIVertex(ref vert, i);

                vert.uv1 = uv1;
                vert.uv3 = uv3;

                vert.normal = new Vector3(normalizedRadii.x, normalizedRadii.y, normalizedRadii.z);
                vert.tangent = new Vector4(normalizedRadii.w, 0f, 0f, 1f);

                vh.SetUIVertex(vert, i);
            }
        }
    }
}
