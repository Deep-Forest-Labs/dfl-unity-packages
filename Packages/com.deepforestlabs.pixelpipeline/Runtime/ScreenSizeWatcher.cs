#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeepForestLabs.PixelPipeline
{
    /// <summary>
    /// Rebroadcasts OnRectTransformDimensionsChange as a plain C# event.
    /// Attach to a GameObject with a Canvas in Screen Space Overlay mode so its
    /// RectTransform always matches the screen; subscribers get a deterministic
    /// notification on resize without any per-frame polling.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ScreenSizeWatcher : UIBehaviour
    {
        public event Action ScreenSizeChanged;

        public int CachedWidth { get; private set; }
        public int CachedHeight { get; private set; }
        public bool HasValidSize => CachedWidth > 0 && CachedHeight > 0;

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateCachedSize();
            ScreenSizeChanged?.Invoke();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateCachedSize();
            ScreenSizeChanged?.Invoke();
        }

        private void UpdateCachedSize()
        {
            RectTransform rt = transform as RectTransform;
            if (rt == null) return;
            Vector2 size = rt.rect.size;
            CachedWidth = Mathf.RoundToInt(size.x);
            CachedHeight = Mathf.RoundToInt(size.y);
        }
    }
}
#nullable disable
