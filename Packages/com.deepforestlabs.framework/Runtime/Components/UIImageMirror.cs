using DeepForestLabs.Logger;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace DeepForestLabs.Components
{
    [AddComponentMenu("UI/Image With Mirror")]
    public sealed class UIImageMirror : Image
    {
        [SerializeField] private bool _horizontal;
        [SerializeField] private bool _vertical;
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (sprite == null)
            {
                base.OnPopulateMesh(vh);
                return;
            }
            
            Log.Assert(type == Type.Simple, "type == Type.Simple");
            vh.Clear();

            Rect rect = preserveAspect ? GetPreservedRect() : GetPixelAdjustedRect();

            if (_horizontal && _vertical)
            {
                OnPopulateBoth(vh, rect);
            }
            else if (_horizontal)
            {
                OnPopulateHorizontal(vh, rect);
            }
            else if (_vertical)
            {
                OnPopulateVertical(vh, rect);
            }
            else
            {
                base.OnPopulateMesh(vh);
            }
        }

        private void OnPopulateBoth(VertexHelper vh, Rect rect)
        {
            Color color32 = color;

            float uMin = uv.xMin;
            float vMin = uv.yMin;
            float uMax = uv.xMax;
            float vMax = uv.yMax;

            float xMid = (rect.xMin + rect.xMax) * 0.5f;
            float yMid = (rect.yMin + rect.yMax) * 0.5f;

            // Top-left: normal
            vh.AddVert(new Vector3(rect.xMin, yMid), color32, new Vector2(uMin, vMin));
            vh.AddVert(new Vector3(rect.xMin, rect.yMax), color32, new Vector2(uMin, vMax));
            vh.AddVert(new Vector3(xMid, rect.yMax), color32, new Vector2(uMax, vMax));
            vh.AddVert(new Vector3(xMid, yMid), color32, new Vector2(uMax, vMin));

            // Top-right: mirrored horizontally
            vh.AddVert(new Vector3(xMid, yMid), color32, new Vector2(uMax, vMin));
            vh.AddVert(new Vector3(xMid, rect.yMax), color32, new Vector2(uMax, vMax));
            vh.AddVert(new Vector3(rect.xMax, rect.yMax), color32, new Vector2(uMin, vMax));
            vh.AddVert(new Vector3(rect.xMax, yMid), color32, new Vector2(uMin, vMin));

            // Bottom-left: mirrored vertically
            vh.AddVert(new Vector3(rect.xMin, rect.yMin), color32, new Vector2(uMin, vMax));
            vh.AddVert(new Vector3(rect.xMin, yMid),      color32, new Vector2(uMin, vMin));
            vh.AddVert(new Vector3(xMid, yMid),           color32, new Vector2(uMax, vMin));
            vh.AddVert(new Vector3(xMid, rect.yMin),      color32, new Vector2(uMax, vMax));

            // Bottom-right: mirrored both axes
            vh.AddVert(new Vector3(xMid, rect.yMin),      color32, new Vector2(uMax, vMax));
            vh.AddVert(new Vector3(xMid, yMid),           color32, new Vector2(uMax, vMin));
            vh.AddVert(new Vector3(rect.xMax, yMid),      color32, new Vector2(uMin, vMin));
            vh.AddVert(new Vector3(rect.xMax, rect.yMin), color32, new Vector2(uMin, vMax));

            // Triangles
            for (int i = 0; i < 4; i++)
            {
                int idx = i * 4;
                vh.AddTriangle(idx + 0, idx + 1, idx + 2);
                vh.AddTriangle(idx + 2, idx + 3, idx + 0);
            }
        }

        private void OnPopulateHorizontal(VertexHelper vh, Rect rect)
        {
            Color color32 = color;

            float uMin = uv.xMin;
            float vMin = uv.yMin;
            float uMax = uv.xMax;
            float vMax = uv.yMax;

            float xMid = (rect.xMin + rect.xMax) * 0.5f;

            // Left side - normal
            vh.AddVert(new Vector3(rect.xMin, rect.yMin), color32, new Vector2(uMin, vMin));
            vh.AddVert(new Vector3(rect.xMin, rect.yMax), color32, new Vector2(uMin, vMax));
            vh.AddVert(new Vector3(xMid,      rect.yMax), color32, new Vector2(uMax, vMax));
            vh.AddVert(new Vector3(xMid,      rect.yMin), color32, new Vector2(uMax, vMin));

            // Right side - mirrored
            vh.AddVert(new Vector3(xMid,      rect.yMin), color32, new Vector2(uMax, vMin));
            vh.AddVert(new Vector3(xMid,      rect.yMax), color32, new Vector2(uMax, vMax));
            vh.AddVert(new Vector3(rect.xMax, rect.yMax), color32, new Vector2(uMin, vMax));
            vh.AddVert(new Vector3(rect.xMax, rect.yMin), color32, new Vector2(uMin, vMin));

            vh.AddTriangle(0, 1, 2); vh.AddTriangle(2, 3, 0);
            vh.AddTriangle(4, 5, 6); vh.AddTriangle(6, 7, 4);
        }

        private void OnPopulateVertical(VertexHelper vh, Rect rect)
        {
            Color color32 = color;
            
            float uMin = uv.xMin;
            float vMin = uv.yMin;
            float uMax = uv.xMax;
            float vMax = uv.yMax;

            float yMid = (rect.yMin + rect.yMax) * 0.5f;

            // Top side - normal
            vh.AddVert(new Vector3(rect.xMin, yMid), color32, new Vector2(uMin, vMin));
            vh.AddVert(new Vector3(rect.xMin, rect.yMax), color32, new Vector2(uMin, vMax));
            vh.AddVert(new Vector3(rect.xMax, rect.yMax), color32, new Vector2(uMax, vMax));
            vh.AddVert(new Vector3(rect.xMax, yMid), color32, new Vector2(uMax, vMin));

            // Bottom side - mirrored vertically
            vh.AddVert(new Vector3(rect.xMin, rect.yMin), color32, new Vector2(uMin, vMax));
            vh.AddVert(new Vector3(rect.xMin, yMid),      color32, new Vector2(uMin, vMin));
            vh.AddVert(new Vector3(rect.xMax, yMid),      color32, new Vector2(uMax, vMin));
            vh.AddVert(new Vector3(rect.xMax, rect.yMin), color32, new Vector2(uMax, vMax));

            vh.AddTriangle(0, 1, 2); vh.AddTriangle(2, 3, 0);
            vh.AddTriangle(4, 5, 6); vh.AddTriangle(6, 7, 4);
        }
        
        private Rect uv
        {
            get
            {
                Texture tex = sprite.texture;
                Rect texRect = sprite.textureRect;

                float uMin = texRect.xMin / tex.width;
                float vMin = texRect.yMin / tex.height;
                float uMax = texRect.xMax / tex.width;
                float vMax = texRect.yMax / tex.height;

                return Rect.MinMaxRect(uMin, vMin, uMax, vMax);
            }
        }

        private Rect GetPreservedRect()
        {
            Rect rect = GetPixelAdjustedRect();
            if (sprite == null)
                return rect;

            float spriteRatio = sprite.rect.width / sprite.rect.height;

            // Determine how many times the sprite will appear in each direction
            float mirrorXMultiplier = _horizontal ? 2f : 1f;
            float mirrorYMultiplier = _vertical ? 2f : 1f;

            // Adjusted rect size that will contain the mirrored content
            float targetWidth = rect.width / mirrorXMultiplier;
            float targetHeight = rect.height / mirrorYMultiplier;

            // Now fit the sprite into this adjusted rect while preserving aspect
            float adjustedWidth, adjustedHeight;
            if (spriteRatio > (targetWidth / targetHeight))
            {
                // Fit by width
                adjustedWidth = targetWidth;
                adjustedHeight = targetWidth / spriteRatio;
            }
            else
            {
                // Fit by height
                adjustedHeight = targetHeight;
                adjustedWidth = targetHeight * spriteRatio;
            }

            // Expand back to full rect by applying mirror multipliers
            adjustedWidth *= mirrorXMultiplier;
            adjustedHeight *= mirrorYMultiplier;

            float x = rect.x + (rect.width - adjustedWidth) * 0.5f;
            float y = rect.y + (rect.height - adjustedHeight) * 0.5f;

            return new Rect(x, y, adjustedWidth, adjustedHeight);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            Log.Assert(type == Type.Simple, "type == Type.Simple");
        }
#endif
    }
}