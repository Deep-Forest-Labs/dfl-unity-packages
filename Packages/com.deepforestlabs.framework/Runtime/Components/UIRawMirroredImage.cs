namespace DeepForestLabs.Components
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("UI/RawImage With Mirror")]
    public sealed class UIRawImageMirror : RawImage
    {
        [SerializeField] private bool _horizontal;
        [SerializeField] private bool _vertical;
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (texture == null)
            {
                base.OnPopulateMesh(vh);
                return;
            }

            vh.Clear();

            if (_horizontal && _vertical)
            {
                OnPopulateBoth(vh);
            }
            else if (_horizontal)
            {
                OnPopulateHorizontal(vh);
            }
            else if (_vertical)
            {
                OnPopulateVertical(vh);
            }
            else
            {
                base.OnPopulateMesh(vh);
            }
        }

        private void OnPopulateBoth(VertexHelper vh)
        {
            Rect rect = GetPixelAdjustedRect();
            Color color32 = color;

            Rect uv = uvRect;
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

        private void OnPopulateHorizontal(VertexHelper vh)
        {
            Rect rect = GetPixelAdjustedRect();
            Color color32 = color;

            Rect uv = uvRect;
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

        private void OnPopulateVertical(VertexHelper vh)
        {
            Rect rect = GetPixelAdjustedRect();
            Color color32 = color;

            Rect uv = uvRect;
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
    }
}