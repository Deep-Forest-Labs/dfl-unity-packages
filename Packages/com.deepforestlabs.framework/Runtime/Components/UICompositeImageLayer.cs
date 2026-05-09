#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace DeepForestLabs.Components
{
    public enum UICompositeImageLayerType
    {
        DropShadow,
        Default,
        Outline,
        Glow
    }
    
    /// <summary>
    /// The mode to use for rendering. Default casts the shadow outside, Inset casts the shadow inside.
    /// </summary>
    public enum DropShadowMode
    {
        /// <summary>
        /// Default casts the shadow outside.
        /// </summary>
        Default,
        /// <summary>
        /// Inset casts the shadow inside.
        /// </summary>
        Inset,
        /// <summary>
        /// Glow mode.
        /// </summary>
        Glow,
        /// <summary>
        /// Cutout mode.
        /// </summary>
        Cutout,
    }

    /// <summary>
    /// The algorithm to use for generating the outline.
    /// </summary>
    public enum OutlineMethod
    {
        /// <summary>
        /// Use a signed distance map for the outline.
        /// </summary>
        DistanceMap,
        /// <summary>
        /// Use a dilate/erode operation for the outline.
        /// </summary>
        Dilate,
    }

    /// <summary>
    /// The direction in which the outline grows from the edge.
    /// </summary>
    public enum OutlineDirection
    {
        /// <summary>
        /// Grow the outline from the edge both inside and outside.
        /// </summary>
        Both,
        /// <summary>
        /// Grow the outline from the edge only inside.
        /// </summary>
        Inside,
        /// <summary>
        /// Grow the outline from the edge only outside.
        /// </summary>
        Outside,
    }

    /// <summary>
    /// The mode for the glow falloff.
    /// </summary>
    public enum GlowFalloffMode
    {
        /// <summary>
        /// Exponential falloff.
        /// </summary>
        Exponential,
        /// <summary>
        /// Curve-based falloff.
        /// </summary>
        Curve,
    }

    /// <summary>
    /// The fill mode for the glow.
    /// </summary>
    public enum GlowFillMode
    {
        /// <summary>
        /// Fill with a solid color.
        /// </summary>
        Color,
        /// <summary>
        /// Fill with a texture.
        /// </summary>
        Texture,
        /// <summary>
        /// Fill with a gradient.
        /// </summary>
        Gradient,
    }
    
    public interface IUICompositeLayer
    {
        UICompositeImageLayerType Type { get; }
        bool Enabled { get; set; }
        Color Color { get; set; }
        Sprite? Sprite { get; set; }
        bool FillCenter { get; set; }
        Material? Material { get; }
        UICompositeImageLayerType LayerType { get; }
        BaseMeshEffect[] Effects { get; }
    }

    [System.Serializable]
    public sealed class UIDefaultImageLayer : IUICompositeLayer
    {
        [SerializeField] private bool _enabled = true;
        [SerializeField] private Sprite? _sprite = null;
        [SerializeField] [HideInInspector] private Material? _material = null;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private bool _fillCenter = true;
        [SerializeField] private BaseMeshEffect[] _effects = default!;

        public UICompositeImageLayerType Type => UICompositeImageLayerType.Default;
        public bool Enabled { get => _enabled; set => _enabled = value; }
        public Sprite? Sprite { get => _sprite; set => _sprite = value; }
        public Material? Material { get => _material; set => _material = value; }
        public Color Color
        {
            get => _color;
            set => _color = value;
        }
        public bool FillCenter { get => _fillCenter; set => _fillCenter = value; }
        public UICompositeImageLayerType LayerType => UICompositeImageLayerType.Default;
        public BaseMeshEffect[] Effects { get => _effects; set => _effects = value; }
    }
}