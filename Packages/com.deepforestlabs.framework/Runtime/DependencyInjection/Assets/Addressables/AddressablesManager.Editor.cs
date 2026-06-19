#nullable enable
using System;
using DeepForestLabs.BuildSystems;
using Cysharp.Text;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;
using AddressablesImpl = UnityEngine.AddressableAssets.Addressables;

namespace DeepForestLabs.Assets.Addressables
{
    internal sealed partial class AddressablesManager
    {
        #if UNITY_EDITOR
        public static Func<string, UnityEditor.SceneAsset?>? _sceneAssetRefContextProvider;
        #else
        public static Func<string, UnityEngine.Object?>? _sceneAssetRefContextProvider;
        #endif
        public static Func<string, AudioClip?>? _audioClipAssetRefContextProvider;
        public static Func<string, Mesh?>? _meshAssetRefContextProvider;
        public static Func<string, RuntimeAnimatorController?>? _runtimeAnimatorControllerAssetRefContextProvider;
        public static Func<string, Sprite?>? _spriteAssetRefContextProvider;
        public static Func<string, string, Sprite?>? _atlasedSpriteAssetRefContextProvider;
        public static Func<string, SpriteAtlas?>? _spriteAtlasAssetRefContextProvider;
        public static Func<string, Texture2D?>? _texture2DAssetRefContextProvider;
        public static Func<string, ScriptableObject?>? _scriptableObjectAssetRefContextProvider;
        public static Func<string, GameObject?>? _gameObjectAssetRefContextProvider;
    }
}
