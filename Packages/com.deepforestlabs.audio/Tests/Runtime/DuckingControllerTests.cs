#nullable enable
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace DeepForestLabs.Audio.Tests
{
    [TestFixture]
    public sealed class DuckingControllerTests
    {
        private readonly List<Object> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (Object created in _created)
                Object.DestroyImmediate(created);
            _created.Clear();
        }

        [Test]
        public void DuckingProfile_HasExpectedDefaults()
        {
            DuckingProfile profile = Create("{}");

            Assert.AreEqual(SoundGroupId.Bgm, profile.TargetGroup);
            CollectionAssert.AreEqual(new[] { SoundGroupId.Bgm }, profile.TargetGroups);
            Assert.IsTrue(profile.VolumeReductionDb < 0f);
            Assert.IsTrue(profile.AttackTime >= 0f);
            Assert.IsTrue(profile.ReleaseTime >= 0f);
        }

        [Test]
        public void TargetGroups_ListTheTargetThenAlsoDucks_WithoutRepeatsOrUnnamedGroups()
        {
            DuckingProfile profile = Create(
                "{\"_targetGroup\":{\"_name\":\"Swarm\"},\"_alsoDucks\":["
                + "{\"_name\":\"Ambience\"},{\"_name\":\"Swarm\"},{\"_name\":\"\"},{\"_name\":\"Music\"}]}");

            CollectionAssert.AreEqual(
                new[] { new SoundGroupId("Swarm"), new SoundGroupId("Ambience"), new SoundGroupId("Music") },
                profile.TargetGroups);
        }

        [Test]
        public void Profile_SavedBeforeAlsoDucks_DucksOnlyItsTarget()
        {
            DuckingProfile profile = Create("{\"_targetGroup\":{\"_name\":\"SFX\"},\"_volumeReductionDb\":-6.0}");

            CollectionAssert.AreEqual(new[] { SoundGroupId.Sfx }, profile.TargetGroups);
        }

        [Test]
        public void Resolve_DeepestProfileSetsTheLevel_OnTheFastestAttack()
        {
            DuckingProfile impact = Profile(-4f, 0.05f, 0.4f);
            DuckingProfile telegraph = Profile(-8f, 0.2f, 0.3f);

            DuckLevel level = DuckLevel.Resolve(new[] { impact, telegraph }, 0f, 0f);

            Assert.AreEqual(-8f, level.OffsetDb);
            Assert.AreEqual(0.05f, level.FadeSeconds);
        }

        [Test]
        public void Resolve_ReleasingTheDeepest_FadesUpToTheNextOnTheRelease()
        {
            DuckingProfile impact = Profile(-4f, 0.05f, 0.4f);

            DuckLevel level = DuckLevel.Resolve(new[] { impact }, -8f, 0.3f);

            Assert.AreEqual(-4f, level.OffsetDb);
            Assert.AreEqual(0.3f, level.FadeSeconds);
        }

        [Test]
        public void Resolve_NothingActive_FadesBackTo0dB_OnTheRelease()
        {
            DuckLevel level = DuckLevel.Resolve(new DuckingProfile[0], -4f, 0.4f);

            Assert.AreEqual(0f, level.OffsetDb);
            Assert.AreEqual(0.4f, level.FadeSeconds);
        }

        [Test]
        public void Resolve_SameLevel_DoesNotFade()
        {
            DuckingProfile impact = Profile(-4f, 0.05f, 0.4f);

            DuckLevel level = DuckLevel.Resolve(new[] { impact, impact }, -4f, 0.4f);

            Assert.AreEqual(-4f, level.OffsetDb);
            Assert.AreEqual(0f, level.FadeSeconds);
        }

        private DuckingProfile Profile(float db, float attack, float release) => Create(
            "{\"_volumeReductionDb\":" + db.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + ",\"_attackTime\":" + attack.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + ",\"_releaseTime\":" + release.ToString(System.Globalization.CultureInfo.InvariantCulture) + "}");

        private DuckingProfile Create(string json)
        {
            DuckingProfile profile = ScriptableObject.CreateInstance<DuckingProfile>();
            JsonUtility.FromJsonOverwrite(json, profile);
            _created.Add(profile);
            return profile;
        }
    }
}
#nullable disable
