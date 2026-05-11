#nullable enable
using NUnit.Framework;
using UnityEngine;

namespace DeepForestLabs.Audio.Tests
{
    [TestFixture]
    public sealed class VolumeConversionTests
    {
        [Test]
        public void LinearToDecibels_FullVolume_ReturnsZeroDb()
        {
            float db = AudioMixerUtils.LinearToDecibels(1f);
            Assert.AreEqual(0f, db, 0.001f);
        }

        [Test]
        public void LinearToDecibels_ZeroVolume_ReturnsMinDb()
        {
            float db = AudioMixerUtils.LinearToDecibels(0f);
            Assert.AreEqual(-80f, db);
        }

        [Test]
        public void LinearToDecibels_HalfVolume_ReturnsApproxNeg6Db()
        {
            float db = AudioMixerUtils.LinearToDecibels(0.5f);
            Assert.AreEqual(-6.02f, db, 0.1f);
        }

        [Test]
        public void DecibelsToLinear_ZeroDb_ReturnsOne()
        {
            float linear = AudioMixerUtils.DecibelsToLinear(0f);
            Assert.AreEqual(1f, linear, 0.001f);
        }

        [Test]
        public void DecibelsToLinear_MinDb_ReturnsZero()
        {
            float linear = AudioMixerUtils.DecibelsToLinear(-80f);
            Assert.AreEqual(0f, linear);
        }

        [Test]
        public void RoundTrip_PreservesValue()
        {
            float[] testValues = { 0f, 0.1f, 0.25f, 0.5f, 0.75f, 1f };
            foreach (float original in testValues)
            {
                float db = AudioMixerUtils.LinearToDecibels(original);
                float result = AudioMixerUtils.DecibelsToLinear(db);
                Assert.AreEqual(original, result, 0.01f,
                    $"Round-trip failed for {original}: got {result} via {db}dB");
            }
        }

        [Test]
        public void LinearToDecibels_ClampsAboveOne()
        {
            float db = AudioMixerUtils.LinearToDecibels(2f);
            Assert.AreEqual(0f, db, 0.001f);
        }

        [Test]
        public void LinearToDecibels_ClampsNegative()
        {
            float db = AudioMixerUtils.LinearToDecibels(-1f);
            Assert.AreEqual(-80f, db);
        }
    }
}
#nullable disable
