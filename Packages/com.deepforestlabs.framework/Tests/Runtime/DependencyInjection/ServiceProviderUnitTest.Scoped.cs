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
        public IEnumerator AddScoped_Extension_GetNotNull() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            _unit.AddScopedFeature();
            IContainer container = await _unit.Build(_scope);
            IFeature feature = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature);
        });

        [UnityTest]
        public IEnumerator AddScoped_TFeature_GetNotNull() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            _unit.AddScoped<Feature>();
            IContainer container = await _unit.Build(_scope);
            Feature feature = container.Get<Feature>();

            // Assert
            Assert.NotNull(feature);
        });

        [UnityTest]
        public IEnumerator AddScoped_IFeatureTFeature_GetNotNull() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            _unit.AddScoped<IFeature, Feature>();
            IContainer container = await _unit.Build(_scope);
            IFeature feature = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature);
        });

        [UnityTest]
        public IEnumerator AddScoped_TLambda_GetNotNull() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            _unit.AddScoped<IFeature>(_ => new Feature());
            IContainer container = await _unit.Build(_scope);
            IFeature feature = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature);
        });

        [UnityTest]
        public IEnumerator AddScopedAsync_TLambda_GetNotNull() => UniTask.ToCoroutine(async () =>
        {
            // Arrange

            // Act
            _unit.AddScoped((_, _) => UniTask.FromResult<IFeature>(new Feature()));
            IContainer container = await _unit.Build(_scope);
            IFeature feature = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature);
        });
        

        [UnityTest]
        public IEnumerator AddScopedPlatformFeature_TNewFactory_GetNotNull() => UniTask.ToCoroutine(async () =>
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
        public void AddScoped_DuplicateScoped_ThrowsException()
        {
            // Arrange

            // Act
            _unit.AddScopedFeature();

            // Assert
            Assert.Throws<DiException>(() => _unit.AddScopedFeature());
        }
        
        [Test]
        public void AddScoped_DuplicateSingleton_ThrowsException()
        {
            // Arrange

            // Act
            _unit.AddSingletonFeature();

            // Assert
            Assert.Throws<DiException>(() => _unit.AddScopedFeature());
        }
        
        [Test]
        public void AddScoped_DuplicateTransient_ThrowsException()
        {
            // Arrange

            // Act
            _unit.AddTransientFeature();

            // Assert
            Assert.Throws<DiException>(() => _unit.AddScopedFeature());
        }
        
        [UnityTest]
        public IEnumerator AddScoped_GetServiceFromParent_IsEqualToParent() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            _unit.AddScopedFeature();
            IContainer parentContainer = await _unit.Build(_scope);
            IFeature parentFeature = parentContainer.Get<IFeature>();
            
            // Act
            await using IContainer childContainer = await parentContainer.CreateChild("UnitTest")
                .Build(_scope);

            IFeature childFeature = childContainer.Get<IFeature>();

            // Assert
            Assert.NotNull(parentFeature);
            Assert.NotNull(childFeature);
            Assert.AreEqual(parentFeature, childFeature);
        });
        
        [UnityTest]
        public IEnumerator AddScoped_OverrideParent_NotEqualToParent() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            _unit.AddScopedFeature();
            IContainer parentContainer = await _unit.Build(_scope);
            IFeature parentFeature = parentContainer.Get<IFeature>();
            
            // Act
            await using IContainer childContainer = await parentContainer.CreateChild("UnitTest")
                .AddScopedFeature()
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