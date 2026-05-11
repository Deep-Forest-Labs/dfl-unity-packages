#nullable enable
using NUnit.Framework;
using UnityEngine;

namespace DeepForestLabs.Audio.Tests
{
    [TestFixture]
    public sealed class DuckingControllerTests
    {
        [Test]
        public void DuckingProfile_HasExpectedDefaults()
        {
            DuckingProfile profile = ScriptableObject.CreateInstance<DuckingProfile>();

            Assert.AreEqual(SoundGroupId.Bgm, profile.TargetGroup);
            Assert.IsTrue(profile.VolumeReductionDb < 0f);
            Assert.IsTrue(profile.AttackTime >= 0f);
            Assert.IsTrue(profile.ReleaseTime >= 0f);

            Object.DestroyImmediate(profile);
        }
    }
}
#nullable disable
