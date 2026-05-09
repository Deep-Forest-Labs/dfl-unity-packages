// using System;
// using Cysharp.Text;
// using DeepForestLabs.DependencyInjection;
// using UnityEditor;
// using UnityEngine;
// using Object = UnityEngine.Object;
//
// namespace DeepForestLabs.DependencyInjection
// {
//     public static class ContainerExtensions
//     {
//         public static IContainerBuilder AddAssetFromEditor<TAsset>(this IContainerBuilder container, string assetDatabasePath)
//             where TAsset : Object
//         {
//             return container.AddSingleton<TAsset>(collection 
//                 => AssetDatabase.LoadAssetAtPath(assetDatabasePath, typeof(TAsset)) as TAsset ??
//                    throw new InvalidCastException(ZString.Format("Asset at path '{0}' is not type '{1}'", assetDatabasePath, typeof(TAsset).Name)));
//         }
//         
//         public static IContainerBuilder AddPrefabFromEditor<TBehaviour>(this IContainerBuilder container, string assetDatabasePath)
//             where TBehaviour : MonoBehaviour
//         {
//             return container.AddSingleton<TBehaviour>(_ 
//                 => (AssetDatabase.LoadAssetAtPath(assetDatabasePath, typeof(GameObject)) as GameObject ??
//                     throw new InvalidCastException(ZString.Format("Asset at path '{0}' is not type '{1}'", assetDatabasePath, nameof(GameObject)))).GetComponent<TBehaviour>());
//         }
//     }
// }