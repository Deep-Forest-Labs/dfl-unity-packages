#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using DeepForestLabs.Logger;
using Cysharp.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace DeepForestLabs
{
    public enum AssetRefMode
    {
        Addressables,
        Resources
    }
    
    [Serializable]
    public abstract class AssetRef : IEquatable<AssetRef>
    {
        [SerializeField] protected internal AssetRefMode _mode;
        [SerializeField] protected internal string _resourcesPath;
        [FormerlySerializedAs("m_AssetGUID")]
        [FormerlySerializedAs("m_assetGUID")]
        [SerializeField] protected internal string _guid;

        public AssetRefMode Mode => _mode;

        public string Address
        {
            get
            {
                switch (_mode)
                {
                    case AssetRefMode.Addressables:
                        Log.Assert(!string.IsNullOrEmpty(_guid), "!string.IsNullOrEmpty(_guid)");
                        Log.Assert(Guid.TryParse(_guid, out _), "Guid.TryParse(_guid, out _)");
                        return _guid;
                    case AssetRefMode.Resources:
                        Log.Assert(!string.IsNullOrEmpty(_resourcesPath), "!string.IsNullOrEmpty(_resourcesPath)");
                        return _resourcesPath;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        protected AssetRef(AssetRefMode mode, string resourcesPath, string guid)
        {
            _mode = mode;
            _resourcesPath = resourcesPath;
            _guid = guid;
        }

        protected internal virtual bool IsValid()
        {
            switch (_mode)
            {
                case AssetRefMode.Addressables:
                    return !string.IsNullOrEmpty(_guid) && Guid.TryParse(_guid, out Guid _);
                case AssetRefMode.Resources:
                    return !string.IsNullOrEmpty(_resourcesPath);
                default:
                    throw new NotSupportedException();
            } 
        }

#if UNITY_EDITOR
        protected internal virtual bool IsEditorValid()
        {
            return IsValid();
        }
#endif

        public bool Equals(AssetRef? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (_mode != other._mode) return false;
            return _mode == AssetRefMode.Addressables
                ? string.Equals(_guid, other._guid, StringComparison.Ordinal)
                : string.Equals(_resourcesPath, other._resourcesPath, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is AssetRef assetRefBase)
                return Equals(assetRefBase);
            return false;
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return _mode == AssetRefMode.Addressables
                ? HashCode.Combine((int)_mode, StringComparer.Ordinal.GetHashCode(_guid))
                : HashCode.Combine((int)_mode, StringComparer.Ordinal.GetHashCode(_resourcesPath));
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
        
        public override string ToString()
        {
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return string.IsNullOrEmpty(_resourcesPath) ? base.ToString() : _resourcesPath;
                case AssetRefMode.Addressables:
#if UNITY_EDITOR
                    return string.IsNullOrEmpty(_guid) ? base.ToString() : AssetDatabase.GUIDToAssetPath(_guid);
#else
                    return string.IsNullOrEmpty(_guid) ? base.ToString() : _guid;
#endif
                default:
                    throw new NotSupportedException();
            }
        }
        
        public static bool operator ==(AssetRef? a, AssetRef? b) => Equals(a, b);
        public static bool operator !=(AssetRef? a, AssetRef? b) => !Equals(a, b);
        
        protected static string NormalizeGuid(string guid) => guid.Trim().ToLowerInvariant();
		
        protected  static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            path = path.Trim().Replace('\\','/');
            if (path.StartsWith("/")) path = path[1..];
            if (path.EndsWith("/")) path = path[..^1];
            return path;
        }
        
#if UNITY_EDITOR
        internal static bool IsInAddressablesFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path.IndexOf("Addressable", StringComparison.OrdinalIgnoreCase) >= 0;
        }
#endif
    }
    
    [Serializable]
    public abstract class AssetRefT<T> : AssetRef, IEquatable<AssetRefT<T>> where T : Object
    {
        protected AssetRefT(AssetRefMode mode, string resourcesPath, string guid) : base(mode, resourcesPath, guid)
        {
        }

        public bool Equals(AssetRefT<T>? other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is AssetRefT<T> assetRefT)
                return base.Equals(assetRefT);
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public static bool operator ==(AssetRefT<T>? a, AssetRefT<T>? b) => Equals(a, b);
        public static bool operator !=(AssetRefT<T>? a, AssetRefT<T>? b) => !Equals(a, b);
    }

    [Serializable]
    public sealed class SceneAssetRef : AssetRef, IEquatable<SceneAssetRef>
    {
        private SceneAssetRef(AssetRefMode mode, string resourcesPath, string guid) : base(mode, resourcesPath, guid)
        {
        }
        
#if UNITY_EDITOR
        protected internal override bool IsEditorValid()
        {
            if (!base.IsEditorValid())
            {
                return false;
            }
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return IsResourcesScene();
                case AssetRefMode.Addressables:
                    return IsAddressableScene();
                default:
                    throw new NotSupportedException();
            }
        }
        
        private bool IsAddressableScene()
        {
            string guid = _guid;
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            string? path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            if (!IsInAddressablesFolder(path))
            {
                return false;
            }

            return AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(SceneAsset);
        }
        
        private bool IsResourcesScene()
        {
            if (string.IsNullOrEmpty(_resourcesPath))
            {
                return false;
            }

            // Strip any ".unity" extension if it was included
            string cleanPath = _resourcesPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
                ? _resourcesPath.Substring(0, _resourcesPath.Length - 6)
                : _resourcesPath;

            // Load the scene asset from Resources
            SceneAsset? sceneAsset = Resources.Load<SceneAsset>(cleanPath);
            return sceneAsset != null;
        }
#endif

        public bool Equals(SceneAssetRef? other)
        {
            return base.Equals(other);
        }
       
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is SceneAssetRef sceneAssetRef)
                return Equals(sceneAssetRef);
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public static SceneAssetRef FromResources(string resourcesPath) =>
            new(AssetRefMode.Resources, NormalizePath(resourcesPath), string.Empty);
        
        public static SceneAssetRef FromAddressables(string guid) =>
            new(AssetRefMode.Addressables, string.Empty, NormalizeGuid(guid));
        
        public static bool operator ==(SceneAssetRef? a, SceneAssetRef? b) => Equals(a, b);
        public static bool operator !=(SceneAssetRef? a, SceneAssetRef? b) => !Equals(a, b);
    }
    
    [Serializable]
    public class AudioClipAssetRef : AssetRefT<AudioClip>, IEquatable<AudioClipAssetRef>
    {
        protected AudioClipAssetRef(AssetRefMode mode, string resourcesPath, string guid) : base(mode, resourcesPath, guid)
        {
        }
        
#if UNITY_EDITOR
        protected internal override bool IsEditorValid()
        {
            if (!base.IsEditorValid())
            {
                return false;
            }
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return IsResourcesAudioClip();
                case AssetRefMode.Addressables:
                    return IsAddressableAudioClip();
                default:
                    throw new NotSupportedException();
            }
        }
        
        private bool IsAddressableAudioClip()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return false;
            }

            string? path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!IsInAddressablesFolder(path))
            {
                return false;
            }

            return AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(AudioClip);
        }

        private bool IsResourcesAudioClip()
        {
            if (string.IsNullOrEmpty(_resourcesPath))
            {
                return false;
            }

            string? cleanPath = _resourcesPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                                _resourcesPath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                                _resourcesPath.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)
                ? System.IO.Path.ChangeExtension(_resourcesPath, null)
                : _resourcesPath;

            AudioClip? clip = Resources.Load<AudioClip>(cleanPath);
            return clip != null;
        }
#endif
        
        public bool Equals(AudioClipAssetRef? other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            return obj is AudioClipAssetRef other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public static AudioClipAssetRef FromResources(string resourcesPath) =>
            new(AssetRefMode.Resources, NormalizePath(resourcesPath), string.Empty);
        
        public static AudioClipAssetRef FromAddressables(string guid) =>
            new(AssetRefMode.Addressables, string.Empty, NormalizeGuid(guid));
        
        public static bool operator ==(AudioClipAssetRef? a, AudioClipAssetRef? b) => Equals(a, b);
        public static bool operator !=(AudioClipAssetRef? a, AudioClipAssetRef? b) => !Equals(a, b);
    }
    
    [Serializable]
    public sealed class MeshAssetRef : AssetRefT<Mesh>, IEquatable<MeshAssetRef>
    {
        private MeshAssetRef(AssetRefMode mode, string resourcesPath, string guid) : base(mode, resourcesPath, guid)
        {
        }
        
#if UNITY_EDITOR
        protected internal override bool IsEditorValid()
        {
            if (!base.IsEditorValid())
            {
                return false;
            }
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return IsResourcesMesh();
                case AssetRefMode.Addressables:
                    return IsAddressableMesh();
                default:
                    throw new NotSupportedException();
            }
        }
        
        private bool IsAddressableMesh()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return false;
            }

            string? path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!IsInAddressablesFolder(path))
            {
                return false;
            }

            Type? mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (mainType == typeof(Mesh))
            {
                return true;
            }

            if (mainType == typeof(GameObject))
            {
                Object[]? subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                if (subs != null)
                {
                    foreach (Object sub in subs)
                    {
                        if (sub is Mesh)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsResourcesMesh()
        {
            if (string.IsNullOrEmpty(_resourcesPath))
            {
                return false;
            }

            string cleanPath = _resourcesPath;
            // Meshes are often sub-objects; try direct, then all
            Mesh? mesh = Resources.Load<Mesh>(cleanPath);
            if (mesh != null)
            {
                return true;
            }

            Mesh[]? meshes = Resources.LoadAll<Mesh>(cleanPath);
            return meshes != null && meshes.Length > 0;
        }
#endif
        
        public bool Equals(MeshAssetRef? other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            return obj is MeshAssetRef other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public static MeshAssetRef FromResources(string resourcesPath) =>
            new(AssetRefMode.Resources, NormalizePath(resourcesPath), string.Empty);
        
        public static MeshAssetRef FromAddressables(string guid) =>
            new(AssetRefMode.Addressables, string.Empty, NormalizeGuid(guid));
        
        public static bool operator ==(MeshAssetRef? a, MeshAssetRef? b) => Equals(a, b);
        public static bool operator !=(MeshAssetRef? a, MeshAssetRef? b) => !Equals(a, b);
    }
    
    [Serializable]
    public sealed class SpriteAssetRef : AssetRefT<Sprite>, IEquatable<SpriteAssetRef>
    {
        private SpriteAssetRef(AssetRefMode mode, string resourcesPath, string guid) : base(mode, resourcesPath, guid)
        {
        }
        
#if UNITY_EDITOR
        protected internal override bool IsEditorValid()
        {
            if (!base.IsEditorValid())
            {
                return false;
            }
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return IsResourcesSprite();
                case AssetRefMode.Addressables:
                    return IsAddressableSprite();
                default:
                    throw new NotSupportedException();
            }
        }
        
        private bool IsAddressableSprite()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return false;
            }

            string? path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!IsInAddressablesFolder(path))
            {
                return false;
            }

            Type? mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (mainType == typeof(Sprite))
            {
                return true;
            }

            Object[]? reps = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            if (reps != null)
            {
                foreach (Object rep in reps)
                {
                    if (rep is Sprite)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsResourcesSprite()
        {
            if (string.IsNullOrEmpty(_resourcesPath))
            {
                return false;
            }

            string? cleanPath = _resourcesPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                _resourcesPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                _resourcesPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                ? System.IO.Path.ChangeExtension(_resourcesPath, null)
                : _resourcesPath;
            Sprite? sprite = Resources.Load<Sprite>(cleanPath);
            if (sprite != null)
            {
                return true;
            }

            Sprite[]? sprites = Resources.LoadAll<Sprite>(cleanPath);
            return sprites != null && sprites.Length > 0;
        }
#endif
        
        public bool Equals(SpriteAssetRef? other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            return obj is SpriteAssetRef other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public static SpriteAssetRef FromResources(string resourcesPath) =>
            new(AssetRefMode.Resources, NormalizePath(resourcesPath), string.Empty);
        
        public static SpriteAssetRef FromAddressables(string guid) =>
            new(AssetRefMode.Addressables, string.Empty, NormalizeGuid(guid));
        
        public static bool operator ==(SpriteAssetRef? a, SpriteAssetRef? b) => Equals(a, b);
        public static bool operator !=(SpriteAssetRef? a, SpriteAssetRef? b) => !Equals(a, b);
    }
    
    [Serializable]
    public sealed class AtlasedSpriteAssetRef : SpriteAtlasAssetRef, IEquatable<AtlasedSpriteAssetRef>
    {
        [FormerlySerializedAs("m_SubObjectName")]
        [SerializeField] internal string _spriteName;

        private AtlasedSpriteAssetRef(AssetRefMode mode, string resourcesPath, string guid, string spriteName)
            : base(mode, resourcesPath, guid)
        {
            _spriteName = spriteName;
        }

        protected internal override bool IsValid()
        {
            return base.IsValid() && !string.IsNullOrEmpty(_spriteName);
        }

#if UNITY_EDITOR
        protected internal override bool IsEditorValid()
        {
            if (!base.IsEditorValid())
            {
                return false;
            }
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return IsResourcesAtlasedSprite();
                case AssetRefMode.Addressables:
                    return IsAddressableAtlasedSprite();
                default:
                    throw new NotSupportedException();
            }
        }
        
        private bool IsAddressableAtlasedSprite()
        {
            if (string.IsNullOrEmpty(_guid) || string.IsNullOrEmpty(_spriteName))
            {
                return false;
            }

            string? path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!IsInAddressablesFolder(path))
            {
                return false;
            }

            SpriteAtlas? spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            if (spriteAtlas == null)
            {
                return false;
            }

            return spriteAtlas.GetSprite(_spriteName) != null;
        }

        private bool IsResourcesAtlasedSprite()
        {
            if (string.IsNullOrEmpty(_resourcesPath) || string.IsNullOrEmpty(_spriteName))
            {
                return false;
            }

            string? cleanPath = _resourcesPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                _resourcesPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                _resourcesPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                ? System.IO.Path.ChangeExtension(_resourcesPath, null)
                : _resourcesPath;

            Sprite[]? sprites = Resources.LoadAll<Sprite>(cleanPath);
            if (sprites == null || sprites.Length == 0)
            {
                return false;
            }

            foreach (Sprite sprite in sprites)
            {
                if (string.Equals(sprite.name, _spriteName, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
#endif
        
        public override string ToString()
        {
            //DO NOT USE ZString, will cause NestedStringBuilderCreationException
            return $"{base.ToString()}[{_spriteName}]";
        }
        
        public bool Equals(AtlasedSpriteAssetRef? other)
        {
            return base.Equals(other) && string.Equals(_spriteName, other._spriteName, StringComparison.Ordinal); 
        }

        public override bool Equals(object? obj)
        {
            return obj is AtlasedSpriteAssetRef other && Equals(other);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return _mode == AssetRefMode.Addressables
                ? HashCode.Combine((int)_mode, StringComparer.Ordinal.GetHashCode(_guid), StringComparer.Ordinal.GetHashCode(_spriteName))
                : HashCode.Combine((int)_mode, StringComparer.Ordinal.GetHashCode(_resourcesPath), StringComparer.Ordinal.GetHashCode(_spriteName));
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
        
        public static AtlasedSpriteAssetRef FromAtlas(SpriteAtlasAssetRef atlasAssetRef, string spriteName)
        {
            switch (atlasAssetRef._mode)
            {
                case AssetRefMode.Addressables:
                    return new AtlasedSpriteAssetRef(AssetRefMode.Addressables, string.Empty, atlasAssetRef._guid, spriteName);
                case AssetRefMode.Resources:
                    return new AtlasedSpriteAssetRef(AssetRefMode.Resources, atlasAssetRef._resourcesPath, spriteName, string.Empty);
                default:
                    throw new NotSupportedException();
            }
        }

        public static bool operator ==(AtlasedSpriteAssetRef? a, AtlasedSpriteAssetRef? b) => Equals(a, b);
        public static bool operator !=(AtlasedSpriteAssetRef? a, AtlasedSpriteAssetRef? b) => !Equals(a, b);
    }
    
    [Serializable]
    public class SpriteAtlasAssetRef : AssetRefT<SpriteAtlas>, IEquatable<SpriteAtlasAssetRef>
    {
        protected SpriteAtlasAssetRef(AssetRefMode mode, string resourcesPath, string guid)
            : base(mode, resourcesPath, guid)
        {
        }
        
#if UNITY_EDITOR
        protected internal override bool IsEditorValid()
        {
            if (!base.IsEditorValid())
            {
                return false;
            }
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return IsResourcesSpriteAtlas();
                case AssetRefMode.Addressables:
                    return IsAddressableSpriteAtlas();
                default:
                    throw new NotSupportedException();
            }
        }

        private bool IsAddressableSpriteAtlas()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return false;
            }

            string? path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!IsInAddressablesFolder(path))
            {
                return false;
            }

            return AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(SpriteAtlas);
        }

        private bool IsResourcesSpriteAtlas()
        {
            if (string.IsNullOrEmpty(_resourcesPath))
            {
                return false;
            }

            string? cleanPath = _resourcesPath.EndsWith(".spriteatlas", StringComparison.OrdinalIgnoreCase) ||
                                _resourcesPath.EndsWith(".spriteatlasV2", StringComparison.OrdinalIgnoreCase)
                ? System.IO.Path.ChangeExtension(_resourcesPath, null)
                : _resourcesPath;
            SpriteAtlas? spriteAtlas = Resources.Load<SpriteAtlas>(cleanPath);
            return spriteAtlas != null;
        }
#endif
        
        public bool Equals(SpriteAtlasAssetRef? other)
        {
            return base.Equals(other); 
        }

        public override bool Equals(object? obj)
        {
            return obj is SpriteAtlasAssetRef other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static SpriteAtlasAssetRef FromResources(string resourcesPath) =>
            new(AssetRefMode.Resources, NormalizePath(resourcesPath), string.Empty);
        
        public static SpriteAtlasAssetRef FromAddressables(string guid) =>
            new(AssetRefMode.Addressables, string.Empty, NormalizeGuid(guid));
        
        public static bool operator ==(SpriteAtlasAssetRef? a, SpriteAtlasAssetRef? b) => Equals(a, b);
        public static bool operator !=(SpriteAtlasAssetRef? a, SpriteAtlasAssetRef? b) => !Equals(a, b);
    }
    
    [Serializable]
    public sealed class Texture2DAssetRef : AssetRefT<Texture2D>, IEquatable<Texture2DAssetRef>
    {
        private Texture2DAssetRef(AssetRefMode mode, string resourcesPath, string guid) : base(mode, resourcesPath, guid)
        {
        }

#if UNITY_EDITOR
        protected internal override bool IsEditorValid()
        {
            if (!base.IsEditorValid())
            {
                return false;
            }
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return IsResourcesTexture2D();
                case AssetRefMode.Addressables:
                    return IsAddressableTexture2D();
                default:
                    throw new NotSupportedException();
            }
        }
        
        private bool IsAddressableTexture2D()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return false;
            }

            string? path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!IsInAddressablesFolder(path))
            {
                return false;
            }

            return AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(Texture2D);
        }

        private bool IsResourcesTexture2D()
        {
            if (string.IsNullOrEmpty(_resourcesPath))
            {
                return false;
            }

            string? cleanPath = _resourcesPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                _resourcesPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                _resourcesPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                ? System.IO.Path.ChangeExtension(_resourcesPath, null)
                : _resourcesPath;
            Texture2D? tex = Resources.Load<Texture2D>(cleanPath);
            return tex != null;
        }
#endif

        public bool Equals(Texture2DAssetRef? other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            return obj is Texture2DAssetRef other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public static Texture2DAssetRef FromResources(string resourcesPath) =>
            new(AssetRefMode.Resources, NormalizePath(resourcesPath), string.Empty);
        
        public static Texture2DAssetRef FromAddressables(string guid) =>
            new(AssetRefMode.Addressables, string.Empty, NormalizeGuid(guid));
        
        public static bool operator ==(Texture2DAssetRef? a, Texture2DAssetRef? b) => Equals(a, b);
        public static bool operator !=(Texture2DAssetRef? a, Texture2DAssetRef? b) => !Equals(a, b);
    }
    
    [Serializable]
    public class ScriptableObjectAssetRef : AssetRefT<ScriptableObject>, IEquatable<ScriptableObjectAssetRef>
    {
        protected ScriptableObjectAssetRef(AssetRefMode mode, string resourcesPath, string guid) : base(mode, resourcesPath, guid)
        {
        }
        
        public bool Equals(ScriptableObjectAssetRef? other)
        {
            return base.Equals(other);
        }

#if UNITY_EDITOR
        protected internal override bool IsEditorValid()
        {
            if (!base.IsEditorValid())
            {
                return false;
            }
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return IsResourcesScriptableObject();
                case AssetRefMode.Addressables:
                    return IsAddressableScriptableObject();
                default:
                    throw new NotSupportedException();
            }
        }

        private bool IsAddressableScriptableObject()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return false;
            }

            string? path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!IsInAddressablesFolder(path))
            {
                return false;
            }

            ScriptableObject? asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            return asset != null;
        }

        private bool IsResourcesScriptableObject()
        {
            if (string.IsNullOrEmpty(_resourcesPath))
            {
                return false;
            }

            string? cleanPath = _resourcesPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)
                ? System.IO.Path.ChangeExtension(_resourcesPath, null)
                : _resourcesPath;
            ScriptableObject? so = Resources.Load<ScriptableObject>(cleanPath);
            return so != null;
        }
#endif

        public override bool Equals(object? obj)
        {
            return obj is ScriptableObjectAssetRef other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public static bool operator ==(ScriptableObjectAssetRef? a, ScriptableObjectAssetRef? b) => Equals(a, b);
        public static bool operator !=(ScriptableObjectAssetRef? a, ScriptableObjectAssetRef? b) => !Equals(a, b);
    }
    
    [Serializable]
    public sealed class ScriptableObjectAssetRefT<T> : ScriptableObjectAssetRef, IEquatable<ScriptableObjectAssetRefT<T>>
        where T : ScriptableObject
    {
        private ScriptableObjectAssetRefT(AssetRefMode mode, string resourcesPath, string guid) : base(mode, resourcesPath, guid)
        {
        }
        
#if UNITY_EDITOR
        protected internal override bool IsEditorValid()
        {
            if (!base.IsEditorValid())
            {
                return false;
            }
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return IsResourcesScriptableObject();
                case AssetRefMode.Addressables:
                    return IsAddressableScriptableObject();
                default:
                    throw new NotSupportedException();
            }
        }

        private bool IsAddressableScriptableObject()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return false;
            }

            string? path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!IsInAddressablesFolder(path))
            {
                return false;
            }

            T? asset = AssetDatabase.LoadAssetAtPath<T>(path);
            return asset != null;
        }

        private bool IsResourcesScriptableObject()
        {
            if (string.IsNullOrEmpty(_resourcesPath))
            {
                return false;
            }

            string? cleanPath = _resourcesPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)
                ? System.IO.Path.ChangeExtension(_resourcesPath, null)
                : _resourcesPath;
            T? so = Resources.Load<T>(cleanPath);
            return so != null;
        }
#endif
        
        public bool Equals(ScriptableObjectAssetRefT<T>? other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            return obj is ScriptableObjectAssetRefT<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public static ScriptableObjectAssetRefT<T> FromResources(string resourcesPath) =>
            new(AssetRefMode.Resources, NormalizePath(resourcesPath), string.Empty);
        
        public static ScriptableObjectAssetRefT<T> FromAddressables(string guid) =>
            new(AssetRefMode.Addressables, string.Empty, NormalizeGuid(guid));
        
        public static bool operator ==(ScriptableObjectAssetRefT<T>? a, ScriptableObjectAssetRefT<T>? b) => Equals(a, b);
        public static bool operator !=(ScriptableObjectAssetRefT<T>? a, ScriptableObjectAssetRefT<T>? b) => !Equals(a, b);
    }

    [Serializable]
    public class GameObjectAssetRef : AssetRefT<GameObject>, IEquatable<GameObjectAssetRef>
    {
        protected GameObjectAssetRef(AssetRefMode mode, string resourcesPath, string guid) : base(mode, resourcesPath, guid)
        {
        }

#if UNITY_EDITOR
        protected internal override bool IsEditorValid()
        {
            if (!base.IsEditorValid())
            {
                return false;
            }
            switch (_mode)
            {
                case AssetRefMode.Resources:
                    return IsResourcesGameObject(out _);
                case AssetRefMode.Addressables:
                    return IsAddressableGameObject(out _);
                default:
                    throw new NotSupportedException();
            }
        }
        
        protected virtual bool IsAddressableGameObject([NotNullWhen(true)] out GameObject? go)
        {
            if (string.IsNullOrEmpty(_guid))
            {
                go = null;
                return false;
            }

            string? path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                go = null;
                return false;
            }

            if (!IsInAddressablesFolder(path))
            {
                go = null;
                return false;
            }

            go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return go != null;
        }

        protected virtual bool IsResourcesGameObject([NotNullWhen(true)] out GameObject? go)
        {
            if (string.IsNullOrEmpty(_resourcesPath))
            {
                go = null;
                return false;
            }

            string? cleanPath = _resourcesPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                ? System.IO.Path.ChangeExtension(_resourcesPath, null)
                : _resourcesPath;
            go = Resources.Load<GameObject>(cleanPath);

            return go != null;
        }
#endif

        public bool Equals(GameObjectAssetRef? other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            return obj is GameObjectAssetRef other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public static GameObjectAssetRef FromResources(string resourcesPath) =>
            new(AssetRefMode.Resources, NormalizePath(resourcesPath), string.Empty);
                
        public static GameObjectAssetRef FromAddressables(string guid) =>
            new(AssetRefMode.Addressables, string.Empty, NormalizeGuid(guid));
        
        public static bool operator ==(GameObjectAssetRef? a, GameObjectAssetRef? b) => Equals(a, b);
        public static bool operator !=(GameObjectAssetRef? a, GameObjectAssetRef? b) => !Equals(a, b);
    }
    
    [Serializable]
    public class GameObjectAssetRefT<T> : GameObjectAssetRef, IEquatable<GameObjectAssetRefT<T>>
        where T : Component
    {
        protected GameObjectAssetRefT(AssetRefMode mode, string resourcesPath, string guid) : base(mode, resourcesPath, guid)
        {
        }
        
#if UNITY_EDITOR
        protected override bool IsAddressableGameObject([NotNullWhen(true)] out GameObject? go)
        {
            return base.IsAddressableGameObject(out go) && go.TryGetComponent(out T? _);
        }

        protected override bool IsResourcesGameObject([NotNullWhen(true)] out GameObject? go)
        {
            return base.IsResourcesGameObject(out go) && go.TryGetComponent(out T? _);
        }
#endif

        public bool Equals(GameObjectAssetRefT<T>? other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            return obj is GameObjectAssetRefT<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public new static GameObjectAssetRefT<T> FromResources(string resourcesPath) =>
            new(AssetRefMode.Resources, NormalizePath(resourcesPath), string.Empty);
                
        public new static GameObjectAssetRefT<T> FromAddressables(string guid) =>
            new(AssetRefMode.Addressables, string.Empty, NormalizeGuid(guid));
        
        public static bool operator ==(GameObjectAssetRefT<T>? a, GameObjectAssetRefT<T>? b) => Equals(a, b);
        public static bool operator !=(GameObjectAssetRefT<T>? a, GameObjectAssetRefT<T>? b) => !Equals(a, b);
    }
}
#nullable disable