# Testing

Guide to writing tests for code built on the DFL framework. Tests use NUnit via Unity's Test Framework and run as editor-only assemblies.

## Test Project Structure

Each package has a test assembly under `Tests/Runtime/`:

```
Packages/com.deepforestlabs.audio/
  Tests/
    Runtime/
      DeepForestLabs.Audio.Tests.asmdef    # Editor-only test assembly
      AudioSourcePoolTests.cs
      VolumeConversionTests.cs
      SoundGroupIdTests.cs
      SoundCatalogTests.cs
      DuckingControllerTests.cs
```

### Assembly Definition

Test asmdefs should:
- Set `includePlatforms` to `["Editor"]` (tests run only in the editor)
- Reference the package's runtime asmdef (via GUID)
- Reference `UnityEngine.TestRunner` and `UnityEditor.TestRunner`
- Reference UniTask if using async tests

```json
{
    "name": "MyPackage.Tests",
    "references": [
        "GUID:<runtime-asmdef-guid>",
        "GUID:27619889b8ba8c24980f49ee34dbb44a",
        "GUID:0acc523941302664db1f4e527237feb3"
    ],
    "includePlatforms": ["Editor"]
}
```

### InternalsVisibleTo

If your tests need access to `internal` types, add `InternalsVisibleTo` in the runtime assembly:

```csharp
// Runtime/AssemblyInfo.cs
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MyPackage.Tests")]
```

## NUnit Patterns

### Basic Test Fixture

```csharp
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public sealed class SoundGroupIdTests
{
    [Test]
    public void BuiltInConstants_HaveExpectedNames()
    {
        Assert.AreEqual("BGM", SoundGroupId.Bgm.Name);
        Assert.AreEqual("SFX", SoundGroupId.Sfx.Name);
    }

    [Test]
    public void Equality_SameName_AreEqual()
    {
        var a = new SoundGroupId("Test");
        var b = new SoundGroupId("Test");
        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
    }
}
```

### Setup and Teardown

For tests that create Unity objects, use `[SetUp]` / `[TearDown]` with `DestroyImmediate`:

```csharp
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
    public void Rent_DecreasesAvailable()
    {
        AudioClip clip = AudioClip.Create("test", 44100, 1, 44100, false);
        var source = _pool.Rent(null, clip, 0);
        Assert.IsNotNull(source);
        Assert.AreEqual(1, _pool.ActiveCount);
        Object.DestroyImmediate(clip);
    }
}
```

### Testing ScriptableObjects

Use `ScriptableObject.CreateInstance<T>()` in tests and destroy in teardown:

```csharp
[Test]
public void EmptyCatalog_TryGetEntry_ReturnsFalse()
{
    SoundCatalog catalog = ScriptableObject.CreateInstance<SoundCatalog>();
    catalog.OnAfterDeserialize();

    bool found = catalog.TryGetEntry("nonexistent", out _);
    Assert.IsFalse(found);

    Object.DestroyImmediate(catalog);
}
```

### Pure Logic Tests

For testing math, data structures, or logic without Unity objects:

```csharp
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
    public void RoundTrip_PreservesValue()
    {
        float[] values = { 0f, 0.1f, 0.25f, 0.5f, 0.75f, 1f };
        foreach (float original in values)
        {
            float db = AudioMixerUtils.LinearToDecibels(original);
            float result = AudioMixerUtils.DecibelsToLinear(db);
            Assert.AreEqual(original, result, 0.01f);
        }
    }
}
```

## Async Tests (UniTask)

For tests that need async behavior, use `[UnityTest]` with `UniTask.ToCoroutine`:

```csharp
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

[UnityTest]
public IEnumerator Initialize_LoadsPreloadEntries() => UniTask.ToCoroutine(async () =>
{
    // Arrange
    var service = CreateTestService();

    // Act
    await service.Initialize(CancellationToken.None);

    // Assert
    Assert.IsTrue(service.IsReady);
});
```

For async teardown, use `[UnityTearDown]`:

```csharp
[UnityTearDown]
public IEnumerator TearDown() => UniTask.ToCoroutine(async () =>
{
    if (_container != null)
    {
        await _container.DisposeAsync();
    }
});
```

## Testing DI-Dependent Code

### Manual Injection

For unit tests that don't need a full container, set `[Dependency]` fields directly via reflection or by making them internal:

```csharp
// If the field is internal and you have InternalsVisibleTo
var service = new MyService();
service._dependency = mockDependency;
```

### Mock Registration Extensions

The framework provides Moq-based helpers in `MockServiceProviderExtensions`:

```csharp
using Moq;

// Register a mock on the builder
builder.AddMockScoped<IMyService>();      // Mock<IMyService> registered as scoped
builder.AddMockSingleton<IMyService>();   // Mock<IMyService> as singleton
builder.AddMockTransient<IMyService>();   // Mock<IMyService> as transient
```

These register both `Mock<T>` (for setup/verification) and `T` (the mock's `.Object`) in the container.

### Creating a Test Container

For integration tests that need the DI container:

```csharp
[UnityTest]
public IEnumerator ServiceResolvesCorrectly() => UniTask.ToCoroutine(async () =>
{
    await using var container = await Container.CreateMain("Test", CancellationToken.None)
        .AddScoped<IMyService, MyService>()
        .AddMockSingleton<IDependency>()
        .Build(CancellationToken.None);

    var service = container.Get<IMyService>();
    Assert.IsNotNull(service);
});
```

## What to Test

Focus tests on logic that doesn't require actual Unity runtime (audio playback, rendering, etc.):

- **Data structures**: pool rent/return, ref counting, cache behavior
- **Math**: volume conversions, interpolation, clamping
- **Catalog lookups**: deserialization, key resolution, duplicate detection
- **State machines**: state transitions, equality, enum behavior
- **Configuration**: ScriptableObject field defaults, serialization callbacks

For integration tests that exercise DI wiring, use `Container.CreateMain` with mock dependencies as shown above.

## Running Tests

Open Unity's Test Runner window (Window > General > Test Runner) and run All or specific test assemblies. Tests run in the editor only (`includePlatforms: ["Editor"]`).
