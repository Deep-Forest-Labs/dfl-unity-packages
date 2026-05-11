#nullable enable
using NUnit.Framework;

namespace DeepForestLabs.Audio.Tests
{
    [TestFixture]
    public sealed class SoundGroupIdTests
    {
        [Test]
        public void BuiltInConstants_HaveExpectedNames()
        {
            Assert.AreEqual("BGM", SoundGroupId.Bgm.Name);
            Assert.AreEqual("SFX", SoundGroupId.Sfx.Name);
            Assert.AreEqual("UI", SoundGroupId.Ui.Name);
        }

        [Test]
        public void Equality_SameName_AreEqual()
        {
            SoundGroupId a = new("TestGroup");
            SoundGroupId b = new("TestGroup");
            Assert.AreEqual(a, b);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [Test]
        public void Equality_DifferentName_AreNotEqual()
        {
            SoundGroupId a = new("GroupA");
            SoundGroupId b = new("GroupB");
            Assert.AreNotEqual(a, b);
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [Test]
        public void GetHashCode_SameName_SameHash()
        {
            SoundGroupId a = new("Test");
            SoundGroupId b = new("Test");
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ToString_ReturnsName()
        {
            SoundGroupId id = new("MyGroup");
            Assert.AreEqual("MyGroup", id.ToString());
        }
    }
}
#nullable disable
