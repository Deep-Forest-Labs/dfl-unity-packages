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

        [Test]
        public void MaxCapacity_ReturnsConfiguredValue()
        {
            Assert.AreEqual(8, _pool.MaxCapacity);
        }

        [Test]
        public void CountActiveForGroup_ReturnsZero_WhenEmpty()
        {
            Assert.AreEqual(0, _pool.CountActiveForGroup(new SoundGroupId("SFX")));
        }

        [Test]
        public void CountActiveForGroup_CountsOnlyMatchingGroup()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);
            SoundGroupId sfx = new SoundGroupId("SFX");
            SoundGroupId bgm = new SoundGroupId("BGM");

            PooledAudioSource? a = _pool.Rent(null, clip);
            a!.Group = sfx;
            PooledAudioSource? b = _pool.Rent(null, clip);
            b!.Group = sfx;
            PooledAudioSource? c = _pool.Rent(null, clip);
            c!.Group = bgm;

            Assert.AreEqual(2, _pool.CountActiveForGroup(sfx));
            Assert.AreEqual(1, _pool.CountActiveForGroup(bgm));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void CountActiveForGroup_DecreasesAfterReturn()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);
            SoundGroupId sfx = new SoundGroupId("SFX");

            PooledAudioSource? a = _pool.Rent(null, clip);
            a!.Group = sfx;
            PooledAudioSource? b = _pool.Rent(null, clip);
            b!.Group = sfx;

            _pool.Return(a);

            Assert.AreEqual(1, _pool.CountActiveForGroup(sfx));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void InitialCapacity_IsClampedToMaxCapacity()
        {
            AudioSourcePool small = new AudioSourcePool(_root.transform, initialCapacity: 8, maxCapacity: 3);

            Assert.AreEqual(3, small.TotalCount);
        }

        [Test]
        public void FindOldestInGroup_ReturnsOldestMatchingSource()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);
            SoundGroupId sfx = new SoundGroupId("SFX");
            SoundGroupId bgm = new SoundGroupId("BGM");

            PooledAudioSource? music = _pool.Rent(null, clip);
            music!.Group = bgm;
            PooledAudioSource? oldest = _pool.Rent(null, clip);
            oldest!.Group = sfx;
            PooledAudioSource? newer = _pool.Rent(null, clip);
            newer!.Group = sfx;

            Assert.AreSame(oldest, _pool.FindOldestInGroup(sfx));
            Assert.IsNull(_pool.FindOldestInGroup(new SoundGroupId("UI")));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void NewSource_HasUnityDefaultPriority()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);
            PooledAudioSource? source = _pool.Rent(null, clip);

            Assert.AreEqual(128, source!.Priority);
            Assert.AreEqual(128, source.Source.priority);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void SetPriority_SetsAudioSourcePriority_Clamped()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);
            PooledAudioSource? source = _pool.Rent(null, clip);

            source!.SetPriority(12);
            Assert.AreEqual(12, source.Source.priority);

            source.SetPriority(999);
            Assert.AreEqual(256, source.Source.priority);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void SetPitch_ZeroPlaysAtOne_AndReturnRestoresOne()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);
            PooledAudioSource? source = _pool.Rent(null, clip);

            source!.SetPitch(0f);
            Assert.AreEqual(1f, source.Source.pitch);
            source.SetPitch(1.25f);
            Assert.AreEqual(1.25f, source.Source.pitch, 1e-5f);

            _pool.Return(source);

            Assert.AreEqual(1f, source.Source.pitch);
            Assert.AreEqual(4f, PooledAudioSource.PlayedSeconds(2f, 0.5f), 1e-5f, "a lower pitch plays longer");
            Assert.AreEqual(2f, PooledAudioSource.PlayedSeconds(2f, 0f), 1e-5f);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Return_RestoresDefaultPriority()
        {
            AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);
            PooledAudioSource? source = _pool.Rent(null, clip);
            source!.SetPriority(12);

            _pool.Return(source);

            Assert.AreEqual(128, source.Source.priority);

            Object.DestroyImmediate(clip);
        }
    }
}
#nullable disable
