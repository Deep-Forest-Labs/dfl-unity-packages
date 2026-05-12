#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Text;
using ZLinq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace DeepForestLabs.DependencyInjection.Assets
{
    [CustomPropertyDrawer(typeof(AtlasedSpriteAssetRef))]
    public sealed class AtlasedSpriteAssetRefPropertyDrawer : AssetRefPropertyDrawer<AtlasedSpriteAssetRef>
    {
        private static readonly Dictionary<SpriteAtlas, Sprite[]> s_PackableSpritesCache = new();
        
        protected override Type ObjectFieldType => throw new NotSupportedException(); 
        protected override Type ObjectLoadType => typeof(SpriteAtlas);
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float viewWidth = EditorGUIUtility.currentViewWidth;
            if (viewWidth > 64 * 4)
            {
                return Mathf.Max(EditorGUIUtility.singleLineHeight, 64f); 
            }

            return Mathf.Max(EditorGUIUtility.singleLineHeight, viewWidth / 4);
        }
        
        protected override AtlasedSpriteAssetRef? GetAssetRef(SerializedProperty property)
        {
            AtlasedSpriteAssetRef? assetRef = base.GetAssetRef(property);

            if (assetRef != null && property.propertyType != SerializedPropertyType.ManagedReference)
            {
                typeof(AtlasedSpriteAssetRef).GetField("_spriteName", kFieldFlags)!.SetValue(assetRef, property.FindPropertyRelative("_spriteName").stringValue);
            }

            return assetRef;
        }
        
        protected override Object? OnFieldGUI(Rect position, SerializedProperty property, GUIContent label, AtlasedSpriteAssetRef? assetRef, Object? asset)
        {
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            SpriteAtlas? currentAtlas = asset as SpriteAtlas;
            Sprite[] sprites = GetSpritesFromAtlas(currentAtlas);
            Sprite? currentSprite = currentAtlas == null || assetRef == null || string.IsNullOrEmpty(assetRef._spriteName)
                ? null
                : sprites.AsValueEnumerable().FirstOrDefault(s => s.name == assetRef._spriteName);

            Rect atlasRect = new Rect(position.x, position.y, position.width - position.height, EditorGUIUtility.singleLineHeight);
            Rect spriteRect = new Rect(position.x + position.width - position.height, position.y, position.height, position.height);
            Rect pickerRect = new Rect(spriteRect.xMax - 32f, position.yMax - EditorGUIUtility.singleLineHeight , 32f, EditorGUIUtility.singleLineHeight);

            SpriteAtlas? selectedAtlas = EditorGUI.ObjectField(atlasRect, currentAtlas, typeof(SpriteAtlas), false) as SpriteAtlas;
            GUI.enabled = currentAtlas != null;
            
            // Draw custom picker button
            if (GUI.Button(pickerRect, GUIContent.none, GUIStyle.none))
            {
                // Open an EditorWindow so we get native window chrome/buttons
                Vector2 mouse = GUIUtility.GUIToScreenPoint(new Vector2(pickerRect.x, pickerRect.yMax));
                SpriteSelectorWindow.Open(sprites, sprite =>
                {
                    UpdateSprite(property, assetRef, sprite);
                }, new Rect(mouse.x - 420f, mouse.y, 420f, 520f));
            }
                
            //GUI.enabled = false;
            Sprite? dragAndDroppedSprite = EditorGUI.ObjectField(spriteRect, currentSprite, typeof(Sprite), false) as Sprite;
            if (currentAtlas != null && dragAndDroppedSprite != currentSprite)
            {
                if (dragAndDroppedSprite == null)
                {
                    UpdateSprite(property, assetRef, null);
                }
                else if (currentAtlas.GetSprite(dragAndDroppedSprite.name) == dragAndDroppedSprite)
                {
                    UpdateSprite(property, assetRef, dragAndDroppedSprite);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Sprite",
                        ZString.Format("Sprite `{0}` is not part of atlas `{1}`.", dragAndDroppedSprite.name,
                            currentAtlas.name), "ok");
                }
            }
            GUI.enabled = true;

            return selectedAtlas;
        }

        private void UpdateSprite(SerializedProperty property, AtlasedSpriteAssetRef? assetRef, Sprite? selectedSprite)
        {
            string spriteName = selectedSprite == null
                ? string.Empty
                : selectedSprite.name;
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (assetRef != null)
                {
                    typeof(AtlasedSpriteAssetRef).GetField("_spriteName", kFieldFlags)!.SetValue(assetRef, spriteName);
                    property.managedReferenceValue = assetRef;
                }
            }
            else
            {
                property.FindPropertyRelative("_spriteName")!.stringValue = spriteName;
            }
            property.serializedObject.ApplyModifiedProperties();
        }

        protected override bool IsValid(AtlasedSpriteAssetRef? assetRef, Object? asset)
        {
            if (IsOptional())
            {
                if (base.IsValid(assetRef, asset) && assetRef != null &&
                    asset is SpriteAtlas atlas && atlas.GetSprite(assetRef._spriteName) == null)
                {
                    return false;
                }

                return true;
            }
            else
            {
                return base.IsValid(assetRef, asset) && assetRef != null &&
                       asset is SpriteAtlas atlas && atlas.GetSprite(assetRef._spriteName) != null;    
            }
        }

        private static Sprite[] GetSpritesFromAtlas(SpriteAtlas? atlas)
        {
            if (atlas == null)
            {
                return Array.Empty<Sprite>();
            }
            
            if (s_PackableSpritesCache.TryGetValue(atlas, out Sprite[]? cached) && cached != null)
            {
                return cached;
            }

            // De-duplicate by asset path + name (protects against same-named sprites from different textures)
            Sprite[] unique = atlas.GetPackableSprites()
                .AsValueEnumerable()
                .Where(s => s != null)
                .GroupBy(s => AssetDatabase.GetAssetPath(s) + "|" + s.name, StringComparer.Ordinal)
                .Select(g => g.AsValueEnumerable().First())
                .ToArray();

            s_PackableSpritesCache[atlas] = unique;

            return unique;
        }
        
        
        public static void InvalidateAtlasCache(SpriteAtlas atlas)
        {
            if (atlas != null)
            {
                s_PackableSpritesCache.Remove(atlas);
            }
        }
        
                // EditorWindow for sprite selection with toolbar and preview
        private sealed class SpriteSelectorWindow : EditorWindow
        {
            private Sprite[] _allSprites = Array.Empty<Sprite>();
            private Action<Sprite>? _onSelected;

            private Vector2 _scroll;
            private string _search = string.Empty;
            private int _activeIndex = -1;
            private float _tile = 64f;
            private const float kTilePadding = 6f;
            private GUIStyle? _nameStyle;

            private List<Sprite> _view = new();

            public static void Open(Sprite[] sprites, Action<Sprite> onSelected, Rect screenRect)
            {
                SpriteSelectorWindow? win = CreateInstance<SpriteSelectorWindow>();
                win.titleContent = new GUIContent("Select Sprite");
                win._allSprites = sprites;
                win._onSelected = onSelected;
                win._nameStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.UpperCenter,
                    wordWrap = true,
                    clipping = TextClipping.Clip
                };
                win.position = screenRect;
                win.minSize = new Vector2(360f, 360f);
                win.ShowUtility();
                win.Focus();
                win.RebuildView();
            }

            private void OnEnable()
            {
                wantsMouseMove = true;
            }

            private void RebuildView()
            {
                if (_allSprites.Length == 0)
                {
                    _view = new List<Sprite>(0);
                    return;
                }

                if (string.IsNullOrEmpty(_search))
                {
                    _view = _allSprites.AsValueEnumerable().ToList();
                }
                else
                {
                    string q = _search.Trim();
                    _view = _allSprites.AsValueEnumerable().Where(s => s != null && s.name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }
                if (_activeIndex >= _view.Count)
                {
                    _activeIndex = _view.Count - 1;
                }

                if (_activeIndex < 0 && _view.Count > 0)
                {
                    _activeIndex = 0;
                }
            }

            private void OnGUI()
            {
                // Header (toolbar-like, with explicit layout so search gets most width)
                Rect hr = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                if (Event.current.type == EventType.Repaint)
                    EditorStyles.toolbar.Draw(hr, GUIContent.none, false, false, false, false);

                float pad = 6f;
                float iconW = 16f;
                float sliderW = 140f;
                float countW = 40f;
                Rect searchRect = new Rect(hr.x + 4, hr.y + 0, hr.width - (4 + pad + iconW + 4 + sliderW + 8 + countW), hr.height);
                Rect iconRect   = new Rect(searchRect.xMax + pad, hr.y + 1, iconW, hr.height - 2);
                Rect sliderRect = new Rect(iconRect.xMax + 4, hr.y + 2, sliderW, hr.height - 4);
                Rect countRect  = new Rect(hr.xMax - countW, hr.y, countW, hr.height);

                GUI.SetNextControlName("SpriteSearch");
                string newSearch = EditorGUI.TextField(searchRect, _search, EditorStyles.toolbarSearchField);
                GUI.Label(iconRect, EditorGUIUtility.IconContent("d_PreMatCube"));
                float newTile = Mathf.Round(GUI.HorizontalSlider(sliderRect, _tile, 0f, 128f));
                GUI.Label(countRect, _view.Count.ToString(), EditorStyles.toolbarButton);

                if (Event.current.type == EventType.Repaint && GUI.GetNameOfFocusedControl() == string.Empty)
                    GUI.FocusControl("SpriteSearch");

                if (newSearch != _search) { _search = newSearch; RebuildView(); }
                if (!Mathf.Approximately(newTile, _tile)) { _tile = newTile; }

                // Middle: list/grid
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                if (_tile <= 0.5f)
                {
                    DrawList(_view);
                }
                else
                {
                    DrawGrid(_view, _tile);
                }

                EditorGUILayout.EndScrollView();

                // Bottom preview panel (boxed), like Unity picker
                Rect pr = GUILayoutUtility.GetRect(10, 96, GUILayout.ExpandWidth(true));
                if (Event.current.type == EventType.Repaint)
                    EditorStyles.helpBox.Draw(pr, GUIContent.none, false, false, false, false);
                pr = new Rect(pr.x + 4, pr.y + 4, pr.width - 8, pr.height - 8);
                DrawSelectionPreview(_activeIndex >= 0 && _activeIndex < _view.Count ? _view[_activeIndex] : null, pr);

                HandleKeyboard(_view);
            }

            private void DrawList(List<Sprite> sprites)
            {
                const float rowH = 20f;
                const float iconSize = 16f;
                const float iconPad = 4f;
                for (int i = 0; i < sprites.Count; i++)
                {
                    Sprite s = sprites[i];
                    Rect row = GUILayoutUtility.GetRect(10, rowH, GUILayout.ExpandWidth(true));

                    if (Event.current.type == EventType.Repaint && i == _activeIndex)
                        EditorGUI.DrawRect(row, new Color(0.24f, 0.48f, 0.90f, 0.20f));

                    // Icon on the left
                    Rect iconRect = new Rect(row.x + iconPad, row.y + (rowH - iconSize) * 0.5f, iconSize, iconSize);
                    Texture thumb = AssetPreview.GetMiniThumbnail(s);
                    if (thumb != null)
                        GUI.DrawTexture(iconRect, thumb, ScaleMode.ScaleToFit, true);

                    // Name label next to icon
                    Rect labelRect = new Rect(iconRect.xMax + 6f, row.y, row.width - (iconRect.width + 12f), rowH);
                    EditorGUI.LabelField(labelRect, s.name);

                    // Click selects
                    if (GUI.Button(row, GUIContent.none, GUIStyle.none))
                        Commit(s);

                    // Hover/Context
                    if (row.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseMove) { _activeIndex = i; Repaint(); }
                        if (Event.current.type == EventType.ContextClick) { ShowContext(s); Event.current.Use(); }
                    }
                }
            }

            private void DrawGrid(List<Sprite> sprites, float tile)
            {
                if (sprites.Count == 0)
                {
                    return;
                }

                float w = position.width - 24f;
                int cols = Mathf.Max(1, Mathf.FloorToInt((w + kTilePadding) / (tile + kTilePadding)));
                float lineH = tile + 28f;

                for (int start = 0; start < sprites.Count; start += cols)
                {
                    Rect row = GUILayoutUtility.GetRect(10, lineH, GUILayout.ExpandWidth(true));
                    float x = row.x; int end = Mathf.Min(start + cols, sprites.Count);
                    for (int i = start; i < end; i++)
                    {
                        Sprite? s = sprites[i];
                        Rect cellRect = new Rect(x, row.y, tile, tile);
                        Rect nameRect = new Rect(x - 8, row.y + tile + 2, tile + 16, 20);

                        if (Event.current.type == EventType.Repaint && i == _activeIndex)
                        {
                            GUI.Box(new Rect(cellRect.x - 2, cellRect.y - 2, cellRect.width + 4, cellRect.height + 4), GUIContent.none, EditorStyles.helpBox);
                        }

                        Texture tex = GetPreview(s);
                        if (tex)
                        {
                            GUI.DrawTexture(cellRect, tex, ScaleMode.ScaleToFit, true);
                        }

                        GUI.Label(nameRect, s.name, _nameStyle);

                        if (GUI.Button(new Rect(cellRect.x - 2, cellRect.y - 2, cellRect.width + 4, cellRect.height + 26), GUIContent.none, GUIStyle.none))
                        {
                            Commit(s);
                        }

                        if (cellRect.Contains(Event.current.mousePosition))
                        {
                            if (Event.current.type == EventType.MouseMove) { _activeIndex = i; Repaint(); }
                            if (Event.current.type == EventType.ContextClick) { ShowContext(s); Event.current.Use(); }
                        }

                        x += tile + kTilePadding;
                    }
                }
            }

            private static Texture GetPreview(Sprite s)
            {
                Texture2D? tex = AssetPreview.GetAssetPreview(s);
                return tex != null ? tex : AssetPreview.GetMiniThumbnail(s);
            }

            private void DrawSelectionPreview(Sprite? s, Rect r)
            {
                if (s == null)
                {
                    EditorGUI.LabelField(r, "");
                    return;
                }

                Texture tex = GetPreview(s);
                Rect left = new Rect(r.x + 4, r.y + 4, 72, 72);
                Rect right = new Rect(left.xMax + 6, r.y + 6, r.width - left.width - 16, r.height - 12);
                if (tex)
                {
                    GUI.DrawTexture(left, tex, ScaleMode.ScaleToFit, true);
                }

                string path = AssetDatabase.GetAssetPath(s);
                string size = $"({Mathf.RoundToInt(s.rect.width)}x{Mathf.RoundToInt(s.rect.height)})";
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.LabelField(right, new GUIContent($"{s.name}\nSprite\n{size}\n{path}"));
                }
            }

            private void ShowContext(Sprite s)
            {
                GenericMenu m = new GenericMenu();
                m.AddItem(new GUIContent("Ping"), false, () => { EditorGUIUtility.PingObject(s); Selection.activeObject = s; });
                m.ShowAsContext();
            }

            private void HandleKeyboard(List<Sprite> list)
            {
                Event e = Event.current;
                if (e.type != EventType.KeyDown || list.Count == 0)
                {
                    return;
                }

                int cols = (_tile <= 0.5f) ? 1 : Mathf.Max(1, Mathf.FloorToInt((position.width - 24f + kTilePadding) / (_tile + kTilePadding)));
                if (_activeIndex < 0)
                {
                    _activeIndex = 0;
                }

                switch (e.keyCode)
                {
                    case KeyCode.RightArrow: _activeIndex = Mathf.Clamp(_activeIndex + 1, 0, list.Count - 1); e.Use(); Repaint(); break;
                    case KeyCode.LeftArrow:  _activeIndex = Mathf.Clamp(_activeIndex - 1, 0, list.Count - 1); e.Use(); Repaint(); break;
                    case KeyCode.DownArrow:  _activeIndex = Mathf.Clamp(_activeIndex + cols, 0, list.Count - 1); e.Use(); Repaint(); break;
                    case KeyCode.UpArrow:    _activeIndex = Mathf.Clamp(_activeIndex - cols, 0, list.Count - 1); e.Use(); Repaint(); break;
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        if (_activeIndex >= 0 && _activeIndex < list.Count)
                        {
                            Commit(list[_activeIndex]);
                        }

                        e.Use();
                        break;
                    case KeyCode.Escape:
                        Close();
                        e.Use();
                        break;
                }
            }

            private void Commit(Sprite s)
            {
                _onSelected?.Invoke(s);
                Close();
            }
 
            private void OnLostFocus()
            {
                Close();
            }
        }
    }
}