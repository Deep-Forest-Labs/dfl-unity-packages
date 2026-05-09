#nullable enable
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.U2D
{
    public static class SpriteAtlasUtils
    {
        public static IEnumerable<Sprite> GetPackableSprites(this SpriteAtlas atlas)
        {
            // Enumerate packables: can be Sprites, Texture2D, or folders (DefaultAsset)
            Object[]? packables = UnityEditor.U2D.SpriteAtlasExtensions.GetPackables(atlas);
            foreach (Object? obj in packables)
            {
                if (obj == null)
                {
                    continue;
                }

                switch (obj)
                {
                    case Sprite s:
                    {
                        if (EditorUtility.IsPersistent(s))
                        {
                            yield return s;
                        }
                        else
                        {
                            string? p = AssetDatabase.GetAssetPath(s);
                            if (!string.IsNullOrEmpty(p))
                            {
                                Object[]? subs = AssetDatabase.LoadAllAssetsAtPath(p);
                                foreach (Object sub in subs)
                                {
                                    if (sub is Sprite ss)
                                    {
                                        yield return ss;
                                    }
                                }
                            }
                        }
                        break;
                    }

                    case Texture2D tex:
                    {
                        string? p = AssetDatabase.GetAssetPath(tex);
                        if (string.IsNullOrEmpty(p))
                        {
                            break;
                        }

                        Object[]? subs = AssetDatabase.LoadAllAssetsAtPath(p);
                        foreach (Object sub in subs)
                        {
                            if (sub is Sprite ss)
                            {
                                yield return ss;
                            }
                        }

                        break;
                    }

                    case DefaultAsset folder:
                    {
                        string? folderPath = AssetDatabase.GetAssetPath(folder);
                        if (string.IsNullOrEmpty(folderPath))
                        {
                            break;
                        }

                        string[]? guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
                        foreach (string guid in guids)
                        {
                            string? p = AssetDatabase.GUIDToAssetPath(guid);
                            Object[]? subs = AssetDatabase.LoadAllAssetsAtPath(p);
                            foreach (Object sub in subs)
                            {
                                if (sub is Sprite ss)
                                {
                                    yield return ss;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
        public static Sprite? GetPackableSprite(this SpriteAtlas atlas, string spriteName)
        {
            foreach (Sprite sprite in GetPackableSprites(atlas))
            {
                if (sprite.name == spriteName)
                {
                    return sprite;
                }
            }

            return null;
        }
    }
}