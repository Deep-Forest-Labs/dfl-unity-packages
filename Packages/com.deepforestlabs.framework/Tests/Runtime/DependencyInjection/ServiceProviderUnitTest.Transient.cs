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
        public IEnumerator AddTransient_Extension_GetNotNullOrEqual() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            await using IContainer container = await _unit
                .AddTransientFeature()
                .Build(_scope);
            
            // Act
            IFeature feature1 = container.Get<IFeature>();
            IFeature feature2 = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature1);
            Assert.NotNull(feature2);
            Assert.AreNotEqual(feature1, feature2);
        });
        
        [UnityTest]
        public IEnumerator AddTransient_TFeature_GetNotNullOrEqual() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            await using IContainer container = await _unit
                .AddTransient<Feature>()
                .Build(_scope);

            // Act
            Feature feature = container.Get<Feature>();

            // Assert
            Assert.NotNull(feature);
        });

        [UnityTest]
        public IEnumerator AddTransient_IFeatureTFeature_GetNotNullOrEqual() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            await using IContainer container = await _unit
                .AddTransient<IFeature, Feature>()
                .Build(_scope);

            // Act
            IFeature feature1 = container.Get<IFeature>();
            IFeature feature2 = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature1);
            Assert.NotNull(feature2);
            Assert.AreNotEqual(feature1, feature2);
        });

        [UnityTest]
        public IEnumerator AddTransient_TLambda_GetNotNullOrEqual() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            await using IContainer container = await _unit
                .AddTransient<IFeature>(_ => new Feature())
                .Build(_scope);

            // Act
            IFeature feature1 = container.Get<IFeature>();
            IFeature feature2 = container.Get<IFeature>();

            // Assert
            Assert.NotNull(feature1);
            Assert.NotNull(feature2);
            Assert.AreNotEqual(feature1, feature2);
        });

        [Test]
        public void AddTransient_DuplicateScoped_ThrowsException()
        {
            // Arrange

            // Act
            _unit.AddScopedFeature();

            // Assert
            Assert.Throws<DiException>(() => _unit.AddTransientFeature());
        }
        
        [Test]
        public void AddTransient_DuplicateSingleton_ThrowsException()
        {
            // Arrange

            // Act
            _unit.AddSingletonFeature();

            // Assert
            Assert.Throws<DiException>(() => _unit.AddTransientFeature());
        }
        
        [Test]
        public void AddTransient_DuplicateTransient_ThrowsException()
        {
            // Arrange

            // Act
            _unit.AddTransientFeature();

            // Assert
            Assert.Throws<DiException>(() => _unit.AddTransientFeature());
        }
        
        [UnityTest]
        public IEnumerator AddTransient_GetServiceFromParent_IsEqualToParentType() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            await using IContainer parentContainer = await _unit
                .AddScopedFeature()
                .Build(_scope);
            
            IFeature parentFeature = parentContainer.Get<IFeature>();
            
            // Act
            await using IContainer childContainer = await parentContainer.CreateChild("UnitTest")
                .Build(_scope);

            IFeature childFeature = childContainer.Get<IFeature>();

            // Assert
            Assert.NotNull(parentFeature);
            Assert.NotNull(childFeature);
            Assert.AreEqual(parentFeature.GetType(), childFeature.GetType());
        });
        
        [UnityTest]
        public IEnumerator AddTransient_OverrideParent_NotEqualToParentType() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            await using IContainer parentContainer = await _unit
                .AddSingletonFeature()
                .Build(_scope);
                    
            IFeature parentFeature = parentContainer.Get<IFeature>();
            
            // Act
            await using IContainer childContainer = await parentContainer.CreateChild("UnitTest")
                .AddSingletonFeatureAndroid()
                .Build(_scope);

            IFeature childFeature = childContainer.Get<IFeature>();

            // Assert
            Assert.NotNull(parentFeature);
            Assert.NotNull(childFeature);
            Assert.AreNotEqual(parentFeature.GetType(), childFeature.GetType());
        });
    }
}
#nullable disable