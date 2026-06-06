#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Factories;
using UnityEngine;

namespace DeepForestLabs
{
    /// <summary>
    /// Routes generic DI registrations through concrete <see cref="ContainerBuilder"/> calls.
    /// IL2CPP cannot reliably dispatch generic methods on <see cref="IContainerBuilder"/>.
    /// </summary>
    public static class IContainerBuilderExtensions
    {
        private static ContainerBuilder Concrete(IContainerBuilder builder) => (ContainerBuilder)builder;

        private static ContainerBuilder Concrete(IParentContainerBuilder builder) => (ContainerBuilder)builder;

        public static IContainerBuilder AddScoped<T>(this IContainerBuilder builder)
            where T : new()
            => Concrete(builder).AddScoped<T>();

        public static IContainerBuilder AddScoped<T, U>(this IContainerBuilder builder)
            where U : T, new()
            => Concrete(builder).AddScoped<T, U>();

        public static IContainerBuilder AddScoped<T>(this IContainerBuilder builder, Func<IDiCollection, T> factory)
            => Concrete(builder).AddScoped(factory);

        public static IContainerBuilder AddScoped<T>(this IContainerBuilder builder,
            Func<IDiCollection, CancellationToken, UniTask<T>> asyncFactory)
            => Concrete(builder).AddScoped(asyncFactory);

        public static IContainerBuilder AddSingleton<T>(this IContainerBuilder builder, T instance)
            => Concrete(builder).AddSingleton(instance);

        public static IContainerBuilder AddAlias<A, T>(this IContainerBuilder builder)
            where T : A
            => Concrete(builder).AddAlias<A, T>();

        public static IContainerBuilder PromoteFrom<T>(this IParentContainerBuilder builder, IContainerBuilder child)
            => Concrete(builder).PromoteFrom<T>(child);

        public static IContainerBuilder Promote<T>(this IParentContainerBuilder builder, T instance)
            => Concrete(builder).Promote(instance);

        public static IContainerBuilder AddScopedComponent<TComponent>(this IContainerBuilder builder)
            where TComponent : Component
            => Concrete(builder).AddScopedComponent<TComponent>();

        public static IContainerBuilder AddTransient<T>(this IContainerBuilder builder)
            where T : new()
            => Concrete(builder).AddTransient<T>();

        public static IContainerBuilder AddTransient<T, U>(this IContainerBuilder builder)
            where U : T, new()
            => Concrete(builder).AddTransient<T, U>();

        public static IContainerBuilder AddTransient<T>(this IContainerBuilder builder, Func<IDiCollection, T> factory)
            => Concrete(builder).AddTransient(factory);

        public static IContainerBuilder AddTransient<T>(this IContainerBuilder builder,
            Func<IDiCollection, CancellationToken, UniTask<T>> factory)
            => Concrete(builder).AddTransient(factory);

        public static IContainerBuilder AddDownload<T>(this IContainerBuilder builder,
            ScriptableObjectAssetRefT<T> assetRef) where T : ScriptableObject
            => Concrete(builder).AddDownload(assetRef);

        public static IContainerBuilder AddDownload<T>(this IContainerBuilder builder, GameObjectAssetRefT<T> assetRef)
            where T : Component
            => Concrete(builder).AddDownload(assetRef);

        public static IContainerBuilder AddScene<T>(this IContainerBuilder builder, SceneAssetRef key)
            where T : class
            => Concrete(builder).AddScene<T>(key);

        public static IContainerBuilder AddScene<T1, T2>(this IContainerBuilder builder, SceneAssetRef assetRef)
            where T1 : class where T2 : class
            => Concrete(builder).AddScene<T1, T2>(assetRef);

        public static IContainerBuilder AddScriptableObject<T>(this IContainerBuilder builder,
            ScriptableObjectAssetRefT<T> assetRef) where T : ScriptableObject
            => Concrete(builder).AddScriptableObject(assetRef);

        public static IContainerBuilder AddGameObjectManager<T>(this IContainerBuilder builder,
            GameObjectAssetRefT<T> assetRef, GameObjectManagerOptions option) where T : Component
            => Concrete(builder).AddGameObjectManager(assetRef, option);

        public static IContainerBuilder AddScopedGameObject<T>(this IContainerBuilder builder,
            GameObjectAssetRefT<T> assetRef, Transform? parent, bool worldPositionStays = true) where T : Component
            => Concrete(builder).AddScopedGameObject(assetRef, parent, worldPositionStays);

        public static IContainerBuilder AddFromBuilder<T>(this IContainerBuilder builder,
            ScriptableObjectAssetRefT<T> assetRef) where T : ContainerBuilderFactory
            => Concrete(builder).AddFromBuilder(assetRef);

        public static IContainerBuilder AddFromBuilder<TArg>(this IContainerBuilder builder,
            ContainerBuilderFactory<TArg> factory, TArg arg)
            => Concrete(builder).AddFromBuilder(factory, arg);

        public static IContainerBuilder AddFromBuilder<TArg1, TArg2>(this IContainerBuilder builder,
            ContainerBuilderFactory<TArg1, TArg2> factory, TArg1 arg1, TArg2 arg2)
            => Concrete(builder).AddFromBuilder(factory, arg1, arg2);

        public static IContainerBuilder AddChild<T>(this IContainerBuilder builder,
            ScriptableObjectAssetRefT<T> assetRef) where T : ContainerFactory
            => Concrete(builder).AddChild(assetRef);
    }
}
#nullable disable
