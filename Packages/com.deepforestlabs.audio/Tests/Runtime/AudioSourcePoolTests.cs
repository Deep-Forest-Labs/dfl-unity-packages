#nullable enable
using NUnit.Framework;
using UnityEngine;

namespace DeepForestLabs.Audio.Tests
{
    [TestFixture]
    public sealed class AudioSourcePoolTests
    {
        private GameObject _root = null!;
        private AudioSourcePool _pool = null!;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestPoolRoot");
            _pool = new AudioSourcePool(_root.transform, initialCapacity: 4, maxCapacity: 8);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void InitialState_HasCorrectCounts()
        {
            Assert.AreEqual(0, _pool.ActiveCount);
            Assert.AreEqual(4, _pool.AvailableCount);
            Assert.AreEqual(4, _pool.TotalCount);
        }

        [Test]
        public void Rent_DecreasesAvailable_IncreasesActive()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);
            PooledAudioSource? source = _pool.Rent(null, clip);

            Assert.IsNotNull(source);
            Assert.AreEqual(1, _pool.ActiveCount);
            Assert.AreEqual(3, _pool.AvailableCount);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Return_IncreasesAvailable_DecreasesActive()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);
            PooledAudioSource? source = _pool.Rent(null, clip);
            _pool.Return(source!);

            Assert.AreEqual(0, _pool.ActiveCount);
            Assert.AreEqual(4, _pool.AvailableCount);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Rent_BeyondInitialCapacity_GrowsPool()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);

            for (int i = 0; i < 6; i++)
            {
                PooledAudioSource? source = _pool.Rent(null, clip);
                Assert.IsNotNull(source);
            }

            Assert.AreEqual(6, _pool.ActiveCount);
            Assert.AreEqual(6, _pool.TotalCount);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Rent_AtMaxCapacity_ReturnsNull()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);

            for (int i = 0; i < 8; i++)
            {
                _pool.Rent(null, clip);
            }

            PooledAudioSource? overflow = _pool.Rent(null, clip);
            Assert.IsNull(overflow);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void CountActiveInstances_CountsSourcesPlayingClip()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);

            PooledAudioSource? a = _pool.Rent(null, clip);
            PooledAudioSource? b = _pool.Rent(null, clip);
            a!.Configure(clip, null, 1f, 0f, false);
            b!.Configure(clip, null, 1f, 0f, false);

            Assert.AreEqual(2, _pool.CountActiveInstances(clip));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void FindStealCandidate_ReturnsOldestActiveSourceForClip()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);

            PooledAudioSource? oldest = _pool.Rent(null, clip);
            oldest!.Configure(clip, null, 1f, 0f, false);
            PooledAudioSource? newer = _pool.Rent(null, clip);
            newer!.Configure(clip, null, 1f, 0f, false);

            Assert.AreSame(oldest, _pool.FindStealCandidate(clip));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void FindStealCandidate_ReturnsNull_WhenClipNotActive()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);

            Assert.IsNull(_pool.FindStealCandidate(clip));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void ReturnAll_ReturnsEverything()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);

            _pool.Rent(null, clip);
            _pool.Rent(null, clip);
            _pool.Rent(null, clip);

            _pool.ReturnAll();

            Assert.AreEqual(0, _pool.ActiveCount);
            Assert.AreEqual(4, _pool.AvailableCount);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Prewarm_AddsToAvailable()
        {
            _pool.Prewarm(3);
            Assert.AreEqual(7, _pool.AvailableCount);
        }

        [Test]
        public void Prewarm_DoesNotExceedMaxCapacity()
        {
            _pool.Prewarm(10);
            Assert.AreEqual(8, _pool.TotalCount);
        }
    }
}
#nullable disable
