#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace DeepForestLabs.DependencyInjection
{
    [TestFixture]
    public sealed class IContainerBuilderExtensionsTests
    {
        private sealed class MockView : MonoBehaviour
        {
        }

        private sealed class MockTypedPrefabRef : GameObjectAssetRefT<MockView>
        {
            public MockTypedPrefabRef()
                : base(AssetRefMode.Resources, "Test/MockView", string.Empty)
            {
            }
        }

        private sealed class MockUntypedPrefabRef : GameObjectAssetRef
        {
            public MockUntypedPrefabRef()
                : base(AssetRefMode.Resources, "Test/MockView", string.Empty)
            {
            }
        }

        [Test]
        public void AddGameObjectManager_GameObjectAssetRefT_RegistersIGameObjectManagerT()
        {
            using CancellationTokenSource cts = new();
            IContainerBuilder builder = Container.CreateMain(nameof(IContainerBuilderExtensionsTests), cts.Token);
            MockTypedPrefabRef prefab = new();

            builder.AddGameObjectManager(prefab, GameObjectManagerOptions.OnDemand);

            List<Type> registeredTypes = GetAsyncSingletonResolverTypes((ContainerBuilder)builder);
            Assert.IsTrue(registeredTypes.Contains(typeof(IGameObjectManagerT<MockView>)));
            Assert.IsFalse(registeredTypes.Contains(typeof(IGameObjectManager)));
        }

        [Test]
        public void AddGameObjectManager_GameObjectAssetRef_RegistersIGameObjectManager()
        {
            using CancellationTokenSource cts = new();
            IContainerBuilder builder = Container.CreateMain(nameof(IContainerBuilderExtensionsTests), cts.Token);
            MockUntypedPrefabRef prefab = new();

            builder.AddGameObjectManager(prefab, GameObjectManagerOptions.OnDemand);

            List<Type> registeredTypes = GetAsyncSingletonResolverTypes((ContainerBuilder)builder);
            Assert.IsTrue(registeredTypes.Contains(typeof(IGameObjectManager)));
            Assert.IsFalse(registeredTypes.Contains(typeof(IGameObjectManagerT<MockView>)));
        }

        private static List<Type> GetAsyncSingletonResolverTypes(ContainerBuilder builder)
        {
            FieldInfo? field = typeof(ContainerBuilder).GetField("_asyncSingletonResolvers",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            var resolvers = (Dictionary<Type, object>?)field.GetValue(builder);
            Assert.NotNull(resolvers);
            return resolvers.Keys.ToList();
        }
    }
}
#nullable disable
