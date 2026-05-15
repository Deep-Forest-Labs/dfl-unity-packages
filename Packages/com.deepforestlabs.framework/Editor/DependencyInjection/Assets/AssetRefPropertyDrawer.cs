#nullable enable
using System;
using System.Reflection;
using System.Runtime.Serialization;
using DeepForestLabs.Logger;
using DeepForestLabs.Data;
using DeepForestLabs.MVC.Views;
using DeepForestLabs.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace DeepForestLabs.DependencyInjection.Assets
{
    [CustomPropertyDrawer(typeof(SceneAssetRef))]
    public sealed class SceneAssetRefPropertyDrawer : AssetRefPropertyDrawer<SceneAssetRef>
    {
        protected override Type ObjectFieldType => typeof(SceneAsset);
        protected override Type ObjectLoadType => typeof(SceneAsset);
    }

    [CustomPropertyDrawer(typeof(AudioClipAssetRef))]
    public sealed class AudioClipAssetRefPropertyDrawer : AssetRefPropertyDrawer<AudioClipAssetRef>
    {
        protected override Type ObjectFieldType => typeof(AudioClip);
        protected override Type ObjectLoadType => typeof(AudioClip);
    }

    [CustomPropertyDrawer(typeof(MeshAssetRef))]
    public sealed class MeshAssetRefPropertyDrawer : AssetRefPropertyDrawer<MeshAssetRef>
    {
        protected override Type ObjectFieldType => typeof(Mesh);
        protected override Type ObjectLoadType => typeof(Mesh);
    }

    [CustomPropertyDrawer(typeof(SpriteAssetRef))]
    public sealed class SpriteAssetRefPropertyDrawer : AssetRefPropertyDrawer<SpriteAssetRef>
    {
        protected override Type ObjectFieldType => typeof(Sprite);
        protected override Type ObjectLoadType => typeof(Sprite);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float viewWidth = EditorGUIUtility.currentViewWidth;
            if (viewWidth > 64 * 4)
            {
                return Mathf.Max(EditorGUIUtility.singleLineHeight, 64f); 
            }

            return Mathf.Max(EditorGUIUtility.singleLineHeight, viewWidth / 4);
        }
    }

    [CustomPropertyDrawer(typeof(Texture2DAssetRef))]
    public sealed class Texture2DAssetRefPropertyDrawer : AssetRefPropertyDrawer<Texture2DAssetRef>
    {
        protected override Type ObjectFieldType => typeof(Texture2D);
        
        protected override Type ObjectLoadType => typeof(Texture2D);
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float viewWidth = EditorGUIUtility.currentViewWidth;
            if (viewWidth > 64 * 4)
            {
                return Mathf.Max(EditorGUIUtility.singleLineHeight, 64f); 
            }

            return Mathf.Max(EditorGUIUtility.singleLineHeight, viewWidth / 4);
        }
    }
    
    [CustomPropertyDrawer(typeof(SpriteAtlasAssetRef))]
    public sealed class SpriteAtlasAssetRefPropertyDrawer : AssetRefPropertyDrawer<SpriteAtlasAssetRef>
    {
        protected override Type ObjectFieldType => typeof(SpriteAtlas);
        
        protected override Type ObjectLoadType => typeof(SpriteAtlas);
    }

    [CustomPropertyDrawer(typeof(ScriptableObjectAssetRef), true)]
    public class ScriptableObjectAssetRefPropertyDrawer : AssetRefPropertyDrawer<ScriptableObjectAssetRef>
    {
        protected override Type ObjectFieldType
        {
            get
            {
                if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(ScriptableObjectAssetRefT<>))
                {
                    return fieldInfo.FieldType.GetGenericArguments()[0];
                }
                return typeof(ScriptableObject);
            }
        }
        
        protected override Type ObjectLoadType => typeof(ScriptableObject);

        protected override Object? GetAsset(SerializedProperty property, ScriptableObjectAssetRef? assetRef)
        {
            Object? asset = base.GetAsset(property, assetRef);

            Type scriptableObjectType;
            if (fieldInfo.FieldType.IsGenericType &&
                fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(ScriptableObjectAssetRefT<>))
            {
                scriptableObjectType = fieldInfo.FieldType.GetGenericArguments()[0];
            }
            else
            {
                scriptableObjectType = typeof(ScriptableObject);
            }

            if (asset != null && Event.current.type == EventType.Layout)
            {
                Log.Validate(property.serializedObject.targetObject, scriptableObjectType.IsInstanceOfType(asset), "{0} must be of type {1}.", fieldInfo.Name, scriptableObjectType.Name);
            }

            return asset;
        }
    }
    
    [CustomPropertyDrawer(typeof(GameObjectAssetRef), true)]
    public class GameObjectAssetRefPropertyDrawer : AssetRefPropertyDrawer<GameObjectAssetRef>
    {
        protected override Type ObjectFieldType => GetComponentType() ?? typeof(GameObject);

        protected override Type ObjectLoadType => typeof(GameObject);

        protected override Object? GetAsset(SerializedProperty property, GameObjectAssetRef? assetRef)
        {
            GameObject? gameObject = base.GetAsset(property, assetRef) as GameObject;
            if (gameObject == null)
            {
                return null;
            }

            Type? componentType = GetComponentType();

            if (componentType != null && typeof(Component).IsAssignableFrom(componentType))
            {
                gameObject.TryGetComponent(componentType, out Component? component);
                return component;
            }
            
            return gameObject;
        }

        private Type? GetComponentType()
        {
            Type? componentType = null;
            Type? assetRefType = fieldInfo.FieldType;
            while (assetRefType != null)
            {
                if (assetRefType.IsGenericType && assetRefType.GetGenericTypeDefinition() == typeof(GameObjectAssetRefT<>))
                {
                    componentType = assetRefType.GetGenericArguments()[0];
                    break;
                }

                assetRefType = assetRefType.BaseType;
            }

            return componentType;
        }
    }

    public abstract class AssetRefPropertyDrawer<TAssetRef> : PropertyDrawer 
        where TAssetRef : AssetRef
    {
        protected const BindingFlags kFieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        protected abstract Type ObjectFieldType { get; }
        protected abstract Type ObjectLoadType { get; }

        protected bool IsInDevelopment(SerializedProperty property)
        {
            if (property.serializedObject.targetObject is ValidatedData validatedData)
            {
                return AssetDatabaseAuditUtil.IsInDevelopment(validatedData);
            }
            if (property.serializedObject.targetObject is ValidatedBehaviour validatedView)
            {
                return AssetDatabaseAuditUtil.IsInDevelopment(validatedView);
            }

            return false;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TAssetRef? assetRef = GetAssetRef(property);
            Object? asset = GetAsset(property, assetRef);

            if (assetRef != null)
            {
                string suffix = assetRef.Mode switch
                {
                    AssetRefMode.Resources => " (R)",
                    AssetRefMode.Addressables => " (A)",
                    _ => string.Empty
                };
                if (suffix.Length > 0)
                    label = new GUIContent(label.text + suffix, label.tooltip);
            }

            Color color = GUI.color;
            GUI.color = IsValid(assetRef, asset) 
                ? color 
                : IsInDevelopment(property) 
                    ? Color.yellow 
                    : Color.red;
            Object? selectedAsset = OnFieldGUI(position, property, label, assetRef, asset);
            GUI.color = color;
            if (selectedAsset != asset)
            {
                TAssetRef? selectedAssetRef = null;
                Type assetRefType = ResolveAssetRefType(property, fieldInfo);
                Log.Assert(typeof(TAssetRef).IsAssignableFrom(assetRefType), "typeof(TAssetRef).IsAssignableFrom(fieldInfo.FieldType)");

                AssetRefMode mode = default;
                string resourcesPath = string.Empty;
                string guid = string.Empty;
                if (selectedAsset != null)
                {
                    if (TryGetResourcesAddress(selectedAsset, out resourcesPath))
                    {
                        mode = AssetRefMode.Resources;
                        guid = string.Empty;
                        selectedAssetRef = FormatterServices.GetUninitializedObject(assetRefType) as TAssetRef;
                    }
                    else if (TryGetAddressablesAddress(selectedAsset, out guid))
                    {
                        mode = AssetRefMode.Addressables;
                        resourcesPath = string.Empty;
                        selectedAssetRef = FormatterServices.GetUninitializedObject(assetRefType) as TAssetRef;
                    }
                    else
                    {
                        mode = AssetRefMode.Addressables;
                        resourcesPath = string.Empty;
                        guid = string.Empty;
                        selectedAssetRef = null;
                    }
                }

                if (selectedAssetRef != null)
                {
                    assetRefType.GetField("_mode", kFieldFlags)!.SetValue(selectedAssetRef, mode);
                    assetRefType.GetField("_resourcesPath", kFieldFlags)!.SetValue(selectedAssetRef, resourcesPath);
                    assetRefType.GetField("_guid", kFieldFlags)!.SetValue(selectedAssetRef, guid);
                }

                SerializedProperty? modeProp = property.FindPropertyRelative("_mode");
                SerializedProperty? resourcesPathProp = property.FindPropertyRelative("_resourcesPath");
                SerializedProperty? guidProp = property.FindPropertyRelative("_guid");
                if (modeProp != null)
                {
                    modeProp.enumValueFlag = (int)mode;
                }
                if (resourcesPathProp != null)
                {
                    resourcesPathProp.stringValue = resourcesPath;
                }
                if (guidProp != null)
                {
                    guidProp.stringValue = guid;
                }

                OnValueChanged(property, selectedAssetRef, selectedAsset);

                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    property.managedReferenceValue = selectedAssetRef;
                }
            }
        }

        protected virtual TAssetRef? GetAssetRef(SerializedProperty property)
        {
            TAssetRef? assetRef;
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (property.managedReferenceValue == null)
                {
                    assetRef = null;
                }
                else
                {
                    assetRef = property.managedReferenceValue as TAssetRef ?? throw new InvalidCastException();
                }
            }
            else
            {
                Type assetRefType = ResolveAssetRefType(property, fieldInfo);
                Log.Assert(typeof(TAssetRef).IsAssignableFrom(assetRefType), "typeof(TAssetRef).IsAssignableFrom(fieldInfo.FieldType)");
                assetRef = FormatterServices.GetUninitializedObject(assetRefType) as TAssetRef;
                assetRefType.GetField("_mode", kFieldFlags)!.SetValue(assetRef, property.FindPropertyRelative("_mode").enumValueFlag);
                assetRefType.GetField("_resourcesPath", kFieldFlags)!.SetValue(assetRef, property.FindPropertyRelative("_resourcesPath").stringValue);
                assetRefType.GetField("_guid", kFieldFlags)!.SetValue(assetRef, property.FindPropertyRelative("_guid").stringValue);
            }

            return assetRef;
        }

        protected virtual Object? GetAsset(SerializedProperty property, TAssetRef? assetRef)
        {
            if (assetRef == null)
            {
                return null;
            }
            
            Object? asset;
            switch (assetRef.Mode)
            {
                case AssetRefMode.Resources:
                    asset = Resources.Load(assetRef._resourcesPath, ObjectLoadType);
                    break;
                case AssetRefMode.Addressables:
                    asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetRef._guid), ObjectLoadType);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return asset;
        }
        
        protected bool IsOptional()
        {
            return fieldInfo.GetCustomAttribute<OptionalAttribute>() != null;
        }
        
        protected virtual bool IsValid(TAssetRef? assetRef, Object? asset)
        {
            return asset != null || IsOptional();
        }
        
        protected virtual Object? OnFieldGUI(Rect position, SerializedProperty property, GUIContent label, TAssetRef? assetRef, Object? asset)
        {
            return EditorGUI.ObjectField(position, label, asset, ObjectFieldType, false);
        }

        protected virtual void OnValueChanged(SerializedProperty property, TAssetRef? assetRef, Object? asset)
        {
        }
        
        private static Type ResolveAssetRefType(SerializedProperty property, FieldInfo fi)
        {
            // ManagedReference fields are already the exact type
            if (property.propertyType == SerializedPropertyType.ManagedReference)
                return fi.FieldType;

            Type t = fi.FieldType;

            // If we're drawing an element of a List<T> or T[]
            if (property.propertyPath.Contains("Array.data["))
            {
                if (t.IsArray) return t.GetElementType()!;
                if (t.IsGenericType && typeof(System.Collections.IList).IsAssignableFrom(t))
                    return t.GetGenericArguments()[0];
            }

            return t;
        }

        private static bool TryGetAddressablesAddress(Object? obj, out string guid)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath) || !assetPath.Contains("Addressables"))
            {
                guid = string.Empty;
                return false;
            }
            guid = AssetDatabase.AssetPathToGUID(assetPath);

            return true;
        }

        private static bool TryGetResourcesAddress(Object? obj, out string resourcesPath)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || !path.Contains("/Resources/"))
            {
                resourcesPath = string.Empty;
                return false;
            }

            resourcesPath = path.Substring(path.LastIndexOf("/Resources/", StringComparison.Ordinal) + "/Resources/".Length);
            resourcesPath = resourcesPath.Substring(0, resourcesPath.LastIndexOf('.'));

            Object? asset = Resources.Load(resourcesPath);
            if (asset == null)
            {
                resourcesPath = string.Empty;
                return false;
            }
            
            return true;
        }
    }
}
#nullable disable