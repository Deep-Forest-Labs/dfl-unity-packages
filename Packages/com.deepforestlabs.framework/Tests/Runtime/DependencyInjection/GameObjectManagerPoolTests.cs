#nullable enable
using NUnit.Framework;
using UnityEngine;

namespace DeepForestLabs.DependencyInjection
{
    [TestFixture]
    public sealed class GameObjectManagerPoolTests
    {
        [Test]
        public void ResolvePrewarmCount_PrewarmTrue_CountZero_ReturnsOne()
        {
            var options = new GameObjectManagerOptions(
                GameObjectManagerDownloadOptions.Required,
                GameObjectManagerLoadOptions.Required,
                prewarm: true, useCreate: false, autoUnload: false);

            Assert.AreEqual(1, options.ResolvePrewarmCount());
        }

        [Test]
        public void ResolvePrewarmCount_PrewarmFalse_CountZero_ReturnsZero()
        {
            var options = new GameObjectManagerOptions(
                GameObjectManagerDownloadOptions.Required,
                GameObjectManagerLoadOptions.Required,
                prewarm: false, useCreate: false, autoUnload: false);

            Assert.AreEqual(0, options.ResolvePrewarmCount());
        }

        [Test]
        public void ResolvePrewarmCount_ExplicitCount_OverridesPrewarmBool()
        {
            var options = new GameObjectManagerOptions(
                GameObjectManagerDownloadOptions.Required,
                GameObjectManagerLoadOptions.Required,
                prewarm: false, useCreate: false, autoUnload: false,
                prewarmCount: 5);

            Assert.AreEqual(5, options.ResolvePrewarmCount());
        }

        [Test]
        public void ResolvePrewarmCount_ExplicitCount_IgnoresPrewarmTrue()
        {
            var options = new GameObjectManagerOptions(
                GameObjectManagerDownloadOptions.Required,
                GameObjectManagerLoadOptions.Required,
                prewarm: true, useCreate: false, autoUnload: false,
                prewarmCount: 10);

            Assert.AreEqual(10, options.ResolvePrewarmCount());
        }

        [Test]
        public void MaxPoolSize_DefaultsToZero()
        {
            var options = new GameObjectManagerOptions(
                GameObjectManagerDownloadOptions.Required,
                GameObjectManagerLoadOptions.Required,
                prewarm: true, useCreate: false, autoUnload: false);

            Assert.AreEqual(0, options.MaxPoolSize);
        }

        [Test]
        public void MaxPoolSize_StoresValue()
        {
            var options = new GameObjectManagerOptions(
                GameObjectManagerDownloadOptions.Required,
                GameObjectManagerLoadOptions.Required,
                prewarm: false, useCreate: false, autoUnload: false,
                prewarmCount: 3, maxPoolSize: 8);

            Assert.AreEqual(8, options.MaxPoolSize);
        }

        [Test]
        public void RequiredPreset_BackwardCompat_PrewarmsOne()
        {
            Assert.AreEqual(1, GameObjectManagerOptions.Required.ResolvePrewarmCount());
            Assert.AreEqual(0, GameObjectManagerOptions.Required.MaxPoolSize);
        }

        [Test]
        public void BackgroundPreset_BackwardCompat_PrewarmsOne()
        {
            Assert.AreEqual(1, GameObjectManagerOptions.Background.ResolvePrewarmCount());
            Assert.AreEqual(0, GameObjectManagerOptions.Background.MaxPoolSize);
        }

        [Test]
        public void OnDemandPreset_BackwardCompat_NoPrewarm()
        {
            Assert.AreEqual(0, GameObjectManagerOptions.OnDemand.ResolvePrewarmCount());
        }

        [Test]
        public void LegacyInstancePool_BackwardCompat_NoPrewarm()
        {
            Assert.AreEqual(0, GameObjectManagerOptions.LegacyInstancePool.ResolvePrewarmCount());
        }

        [Test]
        public void RequiredWithPool_SetsPrewarmAndMaxPool()
        {
            var options = GameObjectManagerOptions.RequiredWithPool(15, 20);

            Assert.AreEqual(15, options.ResolvePrewarmCount());
            Assert.AreEqual(20, options.MaxPoolSize);
            Assert.IsFalse(options.UseCreate);
            Assert.IsFalse(options.AutoUnload);
        }

        [Test]
        public void RequiredWithPool_DefaultMaxPoolIsUnlimited()
        {
            var options = GameObjectManagerOptions.RequiredWithPool(5);

            Assert.AreEqual(5, options.ResolvePrewarmCount());
            Assert.AreEqual(0, options.MaxPoolSize);
        }
    }
}
#nullable disable
