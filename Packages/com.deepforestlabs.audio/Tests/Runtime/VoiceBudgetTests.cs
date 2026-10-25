#nullable enable
using NUnit.Framework;
using UnityEngine;

namespace DeepForestLabs.Audio.Tests
{
    [TestFixture]
    public sealed class VoiceBudgetTests
    {
        private static readonly SoundGroupId Weapons = new("Weapons");
        private static readonly SoundGroupId Swarm = new("Swarm");
        private static readonly SoundGroupId Ui = new("UI");

        private GameObject _root = null!;
        private AudioClip _clip = null!;
        private AudioSourcePool _pool = null!;
        private AudioMixerConfig _config = null!;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestVoiceBudgetRoot");
            _clip = AudioClip.Create("test", 44100, 1, 44100, false);
            _pool = new AudioSourcePool(_root.transform, initialCapacity: 0, maxCapacity: 4);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_clip);
            if (_config != null) Object.DestroyImmediate(_config);
        }

        [Test]
        public void GroupMapping_SavedBeforePriority_KeepsTodaysBehavior()
        {
            AudioMixerConfig.GroupMapping mapping =
                JsonUtility.FromJson<AudioMixerConfig.GroupMapping>("{\"_groupName\":\"SFX\",\"_maxVoices\":48}");

            Assert.AreEqual(128, mapping.Priority);
            Assert.AreEqual(GroupFullPolicy.RejectNew, mapping.WhenFull);
        }

        [Test]
        public void SoundEntry_SavedBeforePriority_HasNoPriorityOverride()
        {
            SoundEntry entry = JsonUtility.FromJson<SoundEntry>("{\"_key\":\"weapon_fire\"}");

            Assert.IsNull(entry.Priority);
        }

        [Test]
        public void SoundEntry_WithOverride_ReturnsPriority()
        {
            SoundEntry entry = JsonUtility.FromJson<SoundEntry>(
                "{\"_key\":\"round_sting\",\"_overridePriority\":true,\"_priority\":10}");

            Assert.AreEqual(10, entry.Priority);
        }

        [Test]
        public void PoolCapacity_DefaultsTo64_RegardlessOfRealVoices()
        {
            _config = CreateConfig();

            Assert.IsFalse(_config.SizePoolToRealVoices);
            Assert.AreEqual(64, _config.ResolvePoolCapacity(32));
            Assert.AreEqual(64, _config.ResolvePoolCapacity(128));
        }

        [Test]
        public void PoolCapacity_SizedToRealVoices_WhenEnabled()
        {
            _config = CreateConfig(sizePoolToRealVoices: true);

            Assert.AreEqual(32, _config.ResolvePoolCapacity(32));
            Assert.AreEqual(128, _config.ResolvePoolCapacity(128));
            Assert.AreEqual(1, _config.ResolvePoolCapacity(0));
        }

        [Test]
        public void ResolvePriority_UnmappedGroup_IsUnityDefault()
        {
            VoiceBudget budget = CreateBudget();

            Assert.AreEqual(128, budget.ResolvePriority(Weapons, null));
        }

        [Test]
        public void ResolvePriority_UsesGroupPriority()
        {
            VoiceBudget budget = CreateBudget(Mapping("Weapons", priority: 20));

            Assert.AreEqual(20, budget.ResolvePriority(Weapons, null));
        }

        [Test]
        public void ResolvePriority_SoundPriorityOverridesGroup_AndIsClamped()
        {
            VoiceBudget budget = CreateBudget(Mapping("Weapons", priority: 20));

            Assert.AreEqual(5, budget.ResolvePriority(Weapons, 5));
            Assert.AreEqual(256, budget.ResolvePriority(Weapons, 300));
            Assert.AreEqual(0, budget.ResolvePriority(Weapons, -4));
        }

        [Test]
        public void IsGroupFull_IgnoresUncappedGroups()
        {
            VoiceBudget budget = CreateBudget(Mapping("Weapons"));
            Play(Weapons, 128);
            Play(Weapons, 128);

            Assert.IsFalse(budget.IsGroupFull(Weapons, out _));
        }

        [Test]
        public void IsGroupFull_AtMaxVoices_ReportsPolicy()
        {
            VoiceBudget budget = CreateBudget(Mapping("Weapons", maxVoices: 2, whenFull: GroupFullPolicy.StealOldest));
            Play(Weapons, 128);

            Assert.IsFalse(budget.IsGroupFull(Weapons, out _));

            Play(Weapons, 128);

            Assert.IsTrue(budget.IsGroupFull(Weapons, out GroupFullPolicy whenFull));
            Assert.AreEqual(GroupFullPolicy.StealOldest, whenFull);
        }

        [Test]
        public void HasFreeVoice_HoldsBackOtherGroupsReservations()
        {
            VoiceBudget budget = CreateBudget(Mapping("UI", reservedVoices: 2));
            Play(Swarm, 128);
            Play(Swarm, 128);

            Assert.IsFalse(budget.HasFreeVoice(Swarm));
            Assert.IsTrue(budget.HasFreeVoice(Ui));
        }

        [Test]
        public void FindPriorityStealCandidate_DefaultPriorities_StealsNothing()
        {
            VoiceBudget budget = CreateBudget(Mapping("Weapons"), Mapping("Swarm"));
            FillPool(Swarm, 128);

            Assert.IsNull(budget.FindPriorityStealCandidate(Weapons, budget.ResolvePriority(Weapons, null)));
        }

        [Test]
        public void FindPriorityStealCandidate_PicksLeastImportant()
        {
            VoiceBudget budget = CreateBudget();
            Play(Weapons, 20);
            PooledAudioSource swarm = Play(Swarm, 200);
            Play(Swarm, 150);
            Play(Weapons, 20);

            Assert.AreSame(swarm, budget.FindPriorityStealCandidate(Weapons, 20));
        }

        [Test]
        public void FindPriorityStealCandidate_PicksOldestOnTie()
        {
            VoiceBudget budget = CreateBudget();
            PooledAudioSource oldest = Play(Swarm, 200);
            Play(Swarm, 200);
            Play(Swarm, 200);

            Assert.AreSame(oldest, budget.FindPriorityStealCandidate(Weapons, 20));
        }

        [Test]
        public void FindPriorityStealCandidate_NeverTakesEqualOrMoreImportant()
        {
            VoiceBudget budget = CreateBudget();
            Play(Weapons, 20);
            Play(Swarm, 100);

            Assert.IsNull(budget.FindPriorityStealCandidate(Swarm, 100));
        }

        [Test]
        public void FindPriorityStealCandidate_SkipsOtherGroupsReservedVoices()
        {
            VoiceBudget budget = CreateBudget(Mapping("Swarm", reservedVoices: 1));
            Play(Swarm, 200);

            Assert.IsNull(budget.FindPriorityStealCandidate(Weapons, 20));

            PooledAudioSource second = Play(Swarm, 200);

            Assert.IsNotNull(budget.FindPriorityStealCandidate(Weapons, 20));
            Assert.AreNotSame(second, budget.FindPriorityStealCandidate(Weapons, 20));
        }

        [Test]
        public void FindPriorityStealCandidate_TakesOwnGroupsReservedVoice()
        {
            VoiceBudget budget = CreateBudget(Mapping("Swarm", reservedVoices: 1));
            PooledAudioSource quiet = Play(Swarm, 220);

            Assert.AreSame(quiet, budget.FindPriorityStealCandidate(Swarm, 200));
        }

        [Test]
        public void MergeEntryParams_PriorityPrecedence()
        {
            SoundEntry plain = JsonUtility.FromJson<SoundEntry>("{\"_key\":\"a\"}");
            SoundEntry overridden = JsonUtility.FromJson<SoundEntry>(
                "{\"_key\":\"b\",\"_overridePriority\":true,\"_priority\":40}");

            Assert.IsNull(AudioService.MergeEntryParams(plain, null, false).Priority);
            Assert.AreEqual(40, AudioService.MergeEntryParams(overridden, null, false).Priority);
            Assert.AreEqual(40, AudioService.MergeEntryParams(overridden, new SoundParams { Volume = 0.5f }, false).Priority);
            Assert.AreEqual(7, AudioService.MergeEntryParams(overridden, new SoundParams { Priority = 7 }, false).Priority);
        }

        [Test]
        public void MergeEntryParams_PitchDefaultsToOne()
        {
            SoundEntry plain = JsonUtility.FromJson<SoundEntry>("{\"_key\":\"a\"}");

            Assert.AreEqual(1f, AudioService.MergeEntryParams(plain, null, false).Pitch);
            Assert.AreEqual(1f, AudioService.MergeEntryParams(plain, new SoundParams { Volume = 0.5f }, false).Pitch);
            Assert.AreEqual(1.1f, AudioService.MergeEntryParams(plain, new SoundParams { Pitch = 1.1f }, false).Pitch, 1e-5f);
        }

        private VoiceBudget CreateBudget(params string[] mappings)
        {
            _config = CreateConfig(mappings: mappings);
            return new VoiceBudget(_pool, _config);
        }

        private static AudioMixerConfig CreateConfig(bool sizePoolToRealVoices = false, params string[] mappings)
        {
            AudioMixerConfig config = ScriptableObject.CreateInstance<AudioMixerConfig>();
            string size = sizePoolToRealVoices ? "true" : "false";
            JsonUtility.FromJsonOverwrite(
                $"{{\"_sizePoolToRealVoices\":{size},\"_groupMappings\":[{string.Join(",", mappings)}]}}", config);
            config.OnAfterDeserialize();
            return config;
        }

        private static string Mapping(string group, int maxVoices = 0, int reservedVoices = 0, int priority = 128,
            GroupFullPolicy whenFull = GroupFullPolicy.RejectNew)
        {
            return $"{{\"_groupName\":\"{group}\",\"_maxVoices\":{maxVoices},\"_reservedVoices\":{reservedVoices}," +
                   $"\"_priority\":{priority},\"_whenFull\":{(int)whenFull}}}";
        }

        private PooledAudioSource Play(SoundGroupId group, int priority)
        {
            PooledAudioSource? source = _pool.Rent(null, _clip);
            Assert.IsNotNull(source);
            source!.Configure(_clip, null, 1f, 0f, false);
            source.Group = group;
            source.SetPriority(priority);
            return source;
        }

        private void FillPool(SoundGroupId group, int priority)
        {
            while (_pool.ActiveCount < _pool.MaxCapacity)
            {
                Play(group, priority);
            }
        }
    }
}
#nullable disable
