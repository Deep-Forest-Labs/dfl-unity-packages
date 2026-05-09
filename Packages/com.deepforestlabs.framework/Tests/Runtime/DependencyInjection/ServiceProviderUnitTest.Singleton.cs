#nullable enable
using System.Collections;
using Cysharp.Threading.Tasks;
using DeepForestLabs.DependencyInjection.Mocks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace DeepForestLabs.DependencyInjection
{
    public sealed partial class ServiceProviderUnitTest
    {
        [UnityTest]
        public IEnumerator AddSingleton_Extension_GetNotNull() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            _unit.AddSingletonFeature();
            IContainer container = await _unit.Build(_scope);
            IFeature feature = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature);
        });
        
        [UnityTest]
        public IEnumerator AddSingleton_TFeature_GetNotNull() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            _unit.AddSingleton(new Feature());
            IContainer container = await _unit.Build(_scope);
            Feature feature = container.Get<Feature>();

            // Assert
            Assert.NotNull(feature);
        });

        [UnityTest]
        public IEnumerator AddSingleton_IFeatureTFeature_GetNotNull() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            _unit.AddSingleton<IFeature>(new Feature());
            IContainer container = await _unit.Build(_scope);
            IFeature feature = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature);
        });
        
        [UnityTest]
        public IEnumerator AddSingleton_Instance_EqualToService() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            IFeature feature = new Feature();

            // Act
            _unit.AddSingleton(feature);
            IContainer container = await _unit.Build(_scope);
            IFeature serviceFeature = container.Get<IFeature>();

            // Assert
            Assert.AreEqual(feature, serviceFeature);
        });

        [UnityTest]
        public IEnumerator AddSingleton_TLambda_GetNotNull() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            _unit.AddSingleton<IFeature>(new Feature());
            IContainer container = await _unit.Build(_scope);
            IFeature feature = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature);
        });

        [UnityTest]
        public IEnumerator AddSingletonAsyncFactory_TNewFactory_GetNotNull() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            _unit.AddScopedPlatformFeature();
            IContainer container = await _unit.Build(_scope);
            IFeature feature = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature);
        });

        [Test]
        public void AddSingleton_DuplicateScoped_ThrowsException()
        {
            // Arrange

            // Act
            _unit.AddScopedFeature();

            // Assert
            Assert.Throws<DiException>(() => _unit.AddSingletonFeature());
        }
        
        [Test]
        public void AddSingleton_DuplicateSingleton_ThrowsException()
        {
            // Arrange

            // Act
            _unit.AddSingletonFeature();

            // Assert
            Assert.Throws<DiException>(() => _unit.AddSingletonFeature());
        }
        
        [Test]
        public void AddSingleton_DuplicateTransient_ThrowsException()
        {
            // Arrange

            // Act
            _unit.AddTransientFeature();

            // Assert
            Assert.Throws<DiException>(() => _unit.AddSingletonFeature());
        }
        
        [UnityTest]
        public IEnumerator AddSingleton_OverrideParent_NotEqualToParent() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            _unit.AddSingletonFeature();
            IContainer parentContainer = await _unit.Build(_scope);
            IFeature parentFeature = parentContainer.Get<IFeature>();
            
            // Act
            await using IContainer childContainer = await parentContainer.CreateChild("UnitTest")
                .AddSingletonFeature()
                .Build(_scope);

            IFeature childFeature = childContainer.Get<IFeature>();

            // Assert
            Assert.NotNull(parentFeature);
            Assert.NotNull(childFeature);
            Assert.AreNotEqual(parentFeature, childFeature);
        });
    }
}
#nullable disable