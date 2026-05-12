#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace DeepForestLabs.Components
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [AddComponentMenu("DeepForestLabs/UICompositeImage")]
    public sealed partial class UICompositeImage : Image
    {
        [SerializeField] private float _borderWidth;
        [SerializeField] private float _falloffDistance = 1f;
        [SerializeField] private Vector4 _radii;
        [SerializeField] private UICompositeImageModifierType _modifier;
        [SerializeField] private UICompositeImageRenderMode _mode = UICompositeImageRenderMode.Baked;
        [SerializeField] private List<UIDefaultImageLayer> _layers = new();

        // Render order for composite image layers
        private static readonly Dictionary<UICompositeImageLayerType, int> _renderOrder = new()
        {
            [UICompositeImageLayerType.DropShadow] = 0,
            [UICompositeImageLayerType.Default] = 1,
            [UICompositeImageLayerType.Outline] = 2,
            [UICompositeImageLayerType.Glow] = 3
        };


        private UIDefaultImageLayer[] GetOrderedLayers()
        {
            return _layers
                .Where(l => l != null && l.Enabled)
                .OrderBy(l => _renderOrder.GetValueOrDefault(l.LayerType, int.MaxValue))
                .ToArray();
        }

        public float BorderWidth
        {
            get => _borderWidth;
            internal set => _borderWidth = value;
        }

        public float FalloffDistance
        {
            get => _falloffDistance;
            internal set => _falloffDistance = value;
        }

        public Vector4 Radii
        {
            get => _radii;
            internal set => _radii = value;
        }

        public UICompositeImageModifierType Modifier
        {
            get => _modifier;
            internal set => _modifier = value;
        }

        public UICompositeImageRenderMode Mode
        {
            get => _mode;
            internal set => _mode = value;
        }

        public List<UIDefaultImageLayer> Layers
        {
            get { return _layers; }
        }

        protected override void Awake()
        {
            base.Awake();
            RefreshSpriteMaterial();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshSpriteMaterial();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            RefreshSpriteMaterial();
        }
#endif

        public void Update()
        {
            if (!Application.isPlaying)
            {
                RefreshSpriteMaterial();
                UpdateGeometry();
            }
        }

        public void RefreshSpriteMaterial()
        {
            switch (_mode)
            {
                case UICompositeImageRenderMode.Dynamic:
                    sprite = GetWhiteSprite();
                    material = GetProceduralMaterial();
                    break;

                case UICompositeImageRenderMode.Baked:
                    if (_layers != null && _layers.Count > 0 && _layers[0]?.Sprite != null)
                    {
                        sprite = _layers[0].Sprite;
                    }
                    else
                    {
                        sprite = null;
                    }
                    material = GetAlphaMaskMaterial();
                    break;

                case UICompositeImageRenderMode.BakedVertex:
                    if (_layers != null && _layers.Count > 0 && _layers[0]?.Sprite != null)
                    {
                        sprite = _layers[0].Sprite;
                    }
                    else
                    {
                        sprite = null;
                    }
                    material = material = GetAlpha8VertexMaterial();
                    break;
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (_mode == UICompositeImageRenderMode.Dynamic)
            {
                RenderDynamic(vh);
            }
            else
            {
                RenderBaked(vh);
            }
        }

        private void RenderBaked(VertexHelper vh)
        {
            if (sprite == null || _layers == null)
            {
                return;
            }

            foreach (UIDefaultImageLayer layer in GetOrderedLayers())
            {
                AddBakedLayer(vh, layer);
            }
        }

        private void AddBakedLayer(VertexHelper vh, IUICompositeLayer layer)
        {
            if (!layer.Enabled || layer.Sprite == null)
            {
                return;
            }

            Color layerColor = layer.Color;
            /*Color layerColor = new Color(
                layer.Color.r * layer.Color.a,
                layer.Color.g * layer.Color.a,
                layer.Color.b * layer.Color.a,
                layer.Color.a
            );*/

            Rect rect = GetPixelAdjustedRect();
            Vector4 outer = DataUtility.GetOuterUV(layer.Sprite!);
            Vector4 inner = DataUtility.GetInnerUV(layer.Sprite!);
            Vector4 border = layer.Sprite!.border;

            switch (type)
            {
                case Type.Sliced:
                    GenerateSlicedSprite(vh, rect, layerColor, outer, inner, border, layer.FillCenter);
                    break;
                case Type.Simple:
                default:
                    GenerateSimple(vh, layerColor, rect, outer);
                    break;
            }

            // Apply BaseMeshEffect vertex modifications (like Gradient2) manually
            BaseMeshEffect[] effects = layer.Effects;
            if (effects != null)
            {
                foreach (var effect in effects)
                {
                    if (effect == null || !effect.enabled) continue;

                    try
                    {
                        effect.ModifyMesh(vh); // Applies vertex color modifications (e.g., gradient)
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"MeshEffect {effect.GetType().Name} failed: {ex.Message}", effect);
                    }
                }
            }
        }

        private static void GenerateSimple(VertexHelper vh, Color layerColor, Rect rect, Vector4 outer)
        {
            // Simple quad
            UIVertex vert = UIVertex.simpleVert;
            vert.color = layerColor;

            vert.position = new Vector2(rect.xMin, rect.yMin);
            vert.uv0 = new Vector2(outer.x, outer.y);
            vh.AddVert(vert);

            vert.position = new Vector2(rect.xMin, rect.yMax);
            vert.uv0 = new Vector2(outer.x, outer.w);
            vh.AddVert(vert);

            vert.position = new Vector2(rect.xMax, rect.yMax);
            vert.uv0 = new Vector2(outer.z, outer.w);
            vh.AddVert(vert);

            vert.position = new Vector2(rect.xMax, rect.yMin);
            vert.uv0 = new Vector2(outer.z, outer.y);
            vh.AddVert(vert);

            int startIndex = vh.currentVertCount - 4;
            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        private void GenerateSlicedSprite(VertexHelper vh, Rect rect, Color col, Vector4 outer, Vector4 inner, Vector4 border, bool fillCenter)
        {
            // Calculate pixel positions
            float left = border.x;
            float right = border.z;
            float top = border.w;
            float bottom = border.y;

            float width = rect.width;
            float height = rect.height;
            float totalBorder = left + right;
            float x0, x1, x2, x3;
            float y0, y1, y2, y3;

            if (width < totalBorder)
            {
                float shortage = totalBorder - width;
                float leftAdjusted = left - shortage * 0.5f;
                float rightAdjusted = right - shortage * 0.5f;

                // Clamp to non-negative
                leftAdjusted = Mathf.Max(0, leftAdjusted);
                rightAdjusted = Mathf.Max(0, rightAdjusted);

                x0 = rect.x;
                x1 = x0 + leftAdjusted;
                x3 = x0 + width;
                x2 = x3 - rightAdjusted;
            }
            else
            {
                x0 = rect.x;
                float gutter = 1f;
                x1 = x0 + Mathf.Max(left, gutter);
                x2 = x0 + width - Mathf.Max(right, gutter);
                x3 = x0 + width;
            }

            y0 = rect.y;
            y1 = y0 + Mathf.Max(bottom, 1f);
            y2 = y0 + height - Mathf.Max(top, 1f);
            y3 = y0 + height;

            // UV coordinates
            float uvLeft = outer.x;
            float uvRight = outer.z;
            float uvTop = outer.w;
            float uvBottom = outer.y;

            float uvInnerLeft = inner.x;
            float uvInnerRight = inner.z;
            float uvInnerTop = inner.w;
            float uvInnerBottom = inner.y;

            // Adjust UVs proportionally if width is too narrow
            if (width < totalBorder)
            {
                float capUWidth = uvInnerLeft - uvLeft;
                float capURightWidth = uvRight - uvInnerRight;

                float leftRatio = Mathf.Clamp01((x1 - x0) / left);
                float rightRatio = Mathf.Clamp01((x3 - x2) / right);

                uvInnerLeft = uvLeft + capUWidth * leftRatio;
                uvInnerRight = uvRight - capURightWidth * rightRatio;
            }

            Vector2[] positions = new Vector2[]
            {
                new Vector2(x0, y0), new Vector2(x1, y0), new Vector2(x2, y0), new Vector2(x3, y0),
                new Vector2(x0, y1), new Vector2(x1, y1), new Vector2(x2, y1), new Vector2(x3, y1),
                new Vector2(x0, y2), new Vector2(x1, y2), new Vector2(x2, y2), new Vector2(x3, y2),
                new Vector2(x0, y3), new Vector2(x1, y3), new Vector2(x2, y3), new Vector2(x3, y3)
            };

            Vector2[] uvs = new Vector2[]
            {
                new Vector2(uvLeft, uvBottom), new Vector2(uvInnerLeft, uvBottom), new Vector2(uvInnerRight, uvBottom), new Vector2(uvRight, uvBottom),
                new Vector2(uvLeft, uvInnerBottom), new Vector2(uvInnerLeft, uvInnerBottom), new Vector2(uvInnerRight, uvInnerBottom), new Vector2(uvRight, uvInnerBottom),
                new Vector2(uvLeft, uvInnerTop), new Vector2(uvInnerLeft, uvInnerTop), new Vector2(uvInnerRight, uvInnerTop), new Vector2(uvRight, uvInnerTop),
                new Vector2(uvLeft, uvTop), new Vector2(uvInnerLeft, uvTop), new Vector2(uvInnerRight, uvTop), new Vector2(uvRight, uvTop)
            };

            int startIndex = vh.currentVertCount;

            for (int i = 0; i < positions.Length; i++)
            {
                UIVertex vert = UIVertex.simpleVert;
                vert.color = col;
                vert.position = positions[i];
                vert.uv0 = uvs[i];
                vh.AddVert(vert);
            }

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    int i0 = startIndex + y * 4 + x;
                    int i1 = startIndex + y * 4 + x + 1;
                    int i2 = startIndex + (y + 1) * 4 + x + 1;
                    int i3 = startIndex + (y + 1) * 4 + x;

                    bool skipCenter = !fillCenter && (x == 1 && y == 1);
                    if (!skipCenter)
                    {
                        vh.AddTriangle(i0, i1, i2);
                        vh.AddTriangle(i2, i3, i0);
                    }
                }
            }
        }
        
        private static Material? _defaultPremultipliedMaterial;

        private static Material GetAlphaMaskMaterial()
        {
            if (_defaultPremultipliedMaterial == null)
            {
                Shader? shader = Shader.Find("UI/Unlit/AlphaMask");
                if (shader == null)
                {
                    throw new Exception("Missing shader: UI/Unlit/AlphaMask");
                }

                _defaultPremultipliedMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            return _defaultPremultipliedMaterial;
        }
        
        private static Material? _alpha8VertexMaterial;

        private static Material GetAlpha8VertexMaterial()
        {
            if (_alpha8VertexMaterial == null)
            {
                Shader shader = Shader.Find("UI/Unlit/Alpha8Vertex");
                if (shader == null)
                    throw new Exception("Missing shader: UI/Unlit/Alpha8Vertex");

                _alpha8VertexMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            return _alpha8VertexMaterial;
        }
    }
}
