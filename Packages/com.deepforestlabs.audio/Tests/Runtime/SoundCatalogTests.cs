#nullable enable
using NUnit.Framework;
using UnityEngine;

namespace DeepForestLabs.Audio.Tests
{
    [TestFixture]
    public sealed class SoundCatalogTests
    {
        [Test]
        public void EmptyCatalog_TryGetEntry_ReturnsFalse()
        {
            SoundCatalog catalog = ScriptableObject.CreateInstance<SoundCatalog>();
            catalog.OnAfterDeserialize();

            bool found = catalog.TryGetEntry("nonexistent", out _);
            Assert.IsFalse(found);

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void Entries_ReturnsReadOnlyList()
        {
            SoundCatalog catalog = ScriptableObject.CreateInstance<SoundCatalog>();
            Assert.IsNotNull(catalog.Entries);
            Assert.AreEqual(0, catalog.Entries.Count);

            Object.DestroyImmediate(catalog);
        }
    }
}
#nullable disable
