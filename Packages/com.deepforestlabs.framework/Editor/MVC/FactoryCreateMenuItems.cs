#nullable enable
using System;
using System.IO;
using System.Reflection;
using ZLinq;
using Cysharp.Text;
using DeepForestLabs.Factories;
using DeepForestLabs.MVC.Factory;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace DeepForestLabs.MVC
{
    [InitializeOnLoad]
    public static class FactoryCreateMenuItems
    {
        private const string CREATE_MENU_PATH_PREFIX = "Assets/Deep Forest Labs/Create";
        private const string EDIT_MENU_PATH_PREFIX = "Assets/Deep Forest Labs/Edit";
        private static bool HAS_CREATED_MENU_ITEMS = false;
        private static bool HAS_COMPILE_ERRORS = false;
        private const string BASE_CREATE_PATH = "Assets/Data/_AssetData/Addressables";
            
        static FactoryCreateMenuItems()
        {
            CompilationPipeline.compilationStarted += _ =>
            {
                HAS_COMPILE_ERRORS = false;
                HAS_CREATED_MENU_ITEMS = false;
            };
            CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;
            AssemblyReloadEvents.afterAssemblyReload += CreateMenus;
        }

        private static void OnCompilationFinished(string outputPath, CompilerMessage[] messages)
        {
            HAS_COMPILE_ERRORS |= messages.AsValueEnumerable().Count(m => m.type == CompilerMessageType.Error) != 0;
        }

        private static void CreateMenus()
        {
            if (HAS_COMPILE_ERRORS || HAS_CREATED_MENU_ITEMS || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            HAS_CREATED_MENU_ITEMS = true;
            
            foreach (Assembly? assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type?[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    Debug.LogWarning($"[FactoryCreateMenuItems] Failed to load types from {assembly.FullName}: {e.Message}");
                    types = e.Types;
                }

                foreach (Type? type in types)
                {
                    if (type == null || !type.IsClass || type.IsAbstract || !typeof(ScriptableObject).IsAssignableFrom(type))
                    {
                        continue;
                    }
                    
                    if ((!type.IsGenericType && typeof(Factories.ContainerFactory).IsAssignableFrom(type)) ||
                         (!type.IsGenericType && typeof(ContainerBuilderFactory).IsAssignableFrom(type)))
                    {
                        AddCreateMenuItem(type);
                        AddEditMenuItem(type);
                    }
                    else if ((type.IsGenericType && type.GetGenericTypeDefinition() ==  typeof(ContainerBuilderFactory<>)) ||
                        (type.IsGenericType && type.GetGenericTypeDefinition() ==  typeof(ContainerBuilderFactory<,>)))
                    {
                        AddCreateMenuItem(type);
                        AddEditMenuItem(type);
                    }
                }
            }
        }

        private static void AddCreateMenuItem(Type type)
        {
            GetMenuItemInfo(type, CREATE_MENU_PATH_PREFIX, out string name, out string path, out string shortcut, out int priority);

            if (!MenuExternal.MenuItemExists(path))
            {
                MenuExternal.AddMenuItem(path, shortcut, false, priority, () => CreateAsset(type, path),
                    () => true);
            }
        }
        
        private static void AddEditMenuItem(Type type)
        {
            GetMenuItemInfo(type, EDIT_MENU_PATH_PREFIX, out string name, out string path, out string shortcut, out int priority);

            if (!MenuExternal.MenuItemExists(path))
            {
                MenuExternal.AddMenuItem(path, shortcut, false, priority, () => EditAsset(path),
                    () => true);
            }
        }
        
        private static void CreateAsset(Type type, string path)
        {
            path = path.Replace('.', '/');
            path = path.Replace(CREATE_MENU_PATH_PREFIX, BASE_CREATE_PATH);
            path = Path.ChangeExtension(path, "asset");
            
            if (path.EndsWith("MainArgs.asset"))
            {
                path = $"Assets/Resources/{Main.MAIN_ARGS_RESOURCES_PATH}.asset";
            }
            
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Directory!.Exists)
            {
                fileInfo.Directory.Create();
            }
            
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            fileInfo = new FileInfo(path);
            if (!fileInfo.Directory!.Exists)
            {
                fileInfo.Directory.Create();
            }

            ScriptableObject? instance = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(instance, path);
            Selection.activeObject = instance;
        }
        
        private static void EditAsset(string path)
        {
            if (path.EndsWith("MainArgs"))
            {
                path = $"Assets/Resources/{Main.MAIN_ARGS_RESOURCES_PATH}";
            }

            path = path.Replace(EDIT_MENU_PATH_PREFIX, BASE_CREATE_PATH);
            path = Path.ChangeExtension(path, "asset");
            
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Directory!.Exists)
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            }
        }
        
        private static void GetMenuItemInfo(Type type, string menuPrefix, out string name, out string path, out string shortcut, out int priority)
        {
            //HACK - for PK - get 2.5.0 out quicker than configurable solution
            name = type.Name.Replace("Factory", string.Empty);
            path = type.Namespace?.Replace(".Factories", "")?.Replace('.', '/')
                ?? throw new InvalidOperationException(
                    $"Factory type '{type.FullName ?? type.Name}' must be declared inside a namespace. " +
                    $"Add a namespace to the file containing '{type.Name}' (e.g. 'namespace MyGame.Factories').");
            shortcut = string.Empty;
            priority = default;
            FactoryMenuItemAttribute? menuItem = type.GetCustomAttribute<FactoryMenuItemAttribute>();
            if (menuItem != null)
            {
                name = menuItem.Name ?? name;
                shortcut = menuItem.Shortcut ?? shortcut;
                path = menuItem.Path ?? path;
            }

            if (path != null)
            {
                path = ZString.Concat(menuPrefix, '/', path, '/', name);
            }
            else
            {
                path = ZString.Concat(menuPrefix,'/', name);
            }
        }
    }
}
#nullable disable