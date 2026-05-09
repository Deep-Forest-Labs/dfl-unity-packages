#nullable enable
using DeepForestLabs.Assets.Addressables;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace DeepForestLabs.AddressableManagement
{
    [InitializeOnLoad]
    public sealed class AddressablesAssetDatabase
    {
        // ReSharper disable once NotAccessedField.Local
        private static AddressablesAssetDatabase _instance;
        
        static AddressablesAssetDatabase() => _instance = new AddressablesAssetDatabase();

        private AddressablesAssetDatabase()
        {
            AddressablesManager._sceneAssetRefContextProvider = GetSceneContext;
            AddressablesManager._audioClipAssetRefContextProvider = GetAudioClipContext;
            AddressablesManager._meshAssetRefContextProvider = GetMeshContext;
            AddressablesManager._spriteAssetRefContextProvider = GetSpriteContext;
            AddressablesManager._atlasedSpriteAssetRefContextProvider = GetAtlasedSpriteContext;
            AddressablesManager._spriteAtlasAssetRefContextProvider = GetSpriteAtlasContext;
            AddressablesManager._texture2DAssetRefContextProvider = GetTexture2DContext;
            AddressablesManager._scriptableObjectAssetRefContextProvider = GetScriptableObjectContext;
            AddressablesManager._gameObjectAssetRefContextProvider = GetGameObjectContext;
        }
        
        private SceneAsset? GetSceneContext(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(guid));
        }
        
        private AudioClip? GetAudioClipContext(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
        }
        
        private Mesh? GetMeshContext(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(guid));
        }
        
        private Sprite? GetSpriteContext(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
        }

        private Sprite? GetAtlasedSpriteContext(string guid, string spriteName)
        {
            SpriteAtlas? atlas = GetSpriteAtlasContext(guid);
            if (atlas == null)
            {
                return null;
            }

            Sprite? sprite = atlas.GetSprite(spriteName);
            if (sprite == null)
            {
                return null;
            }

            if (sprite.packed)
            {
                return atlas.GetPackableSprite(spriteName);
            }

            return sprite;
        }

        private Texture2D? GetTexture2DContext(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
        }
        
        private ScriptableObject? GetScriptableObjectContext(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
        }
        
        private GameObject? GetGameObjectContext(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
        }
        
        private SpriteAtlas? GetSpriteAtlasContext(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}