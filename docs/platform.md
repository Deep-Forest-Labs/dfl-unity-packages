# Platform

`com.deepforestlabs.platform` provides **callable seams** for mobile platform features. Games depend on interfaces; real SDK adapters land in later epics. Ghostgarden is the proof consumer; the web design lab does not need IAP/ads/push parity.

## Opt in

```csharp
using DeepForestLabs.Platform;

public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
{
    return base.AddToBuilder(builder)
        .AddSingleton(BuildSettings.Instance)
        .AddPlatformServices(PlatformServiceOptions.Null)
        // game-specific registrations...
        ;
}
```

`AddPlatformServices` **always registers every seam**. Use required `[Dependency]` fields. Availability is expressed via `IsAvailable` / result enums — not missing DI registrations.

`PlatformServiceOptions.Null` (E0) registers null/no-op implementations plus UGS bootstrap wrappers (analytics collection still disabled).

## Seams

| Interface | Null behavior | Later epic |
|-----------|---------------|------------|
| `IAnalyticsService` | Drops events | E2 — funnels / UGS collection |
| `IRemoteConfigService` | No keys | E3 — remote overlay |
| `IAdService` | `Unavailable` (never grants reward) | E4 — mediation |
| `IIapService` | `Unavailable` (never grants entitlement) | E5 — store + validation |
| `ICloudSaveService` | `Unavailable` blob/key API | E6 — cloud mirror |
| `IPushNotificationService` | `Unavailable` | E7 — push |
| `IAccountService` | Device-local anonymous id | E6 — link / recovery |
| `IConsentService` | `NotRequired` | E2/E4 — ATT + UGS consent |

## Analytics: framework vs platform

**Stays in `com.deepforestlabs.framework`** (Controller-coupled):

- `AnalyticsStringValues` + helpers
- `IAnalyticsUIEventHelper` / `IAnalyticsErrorHelper`
- Controller click analytics wiring

**Lives in platform:**

- `IAnalyticsService` (product / funnel events)
- `UnityServicesWrapper` / `AnalyticsServiceWrapper` (UGS bootstrap)
- `NullAnalyticsUiHelpers` implementing the framework helper interfaces

E0 does **not** auto-forward UI clicks into `IAnalyticsService`. E2 may add an adapter.

## Cloud save vs game save

`ICloudSaveService` is a **generic string blob/key store**. Game schemas (e.g. ghostgarden `ISaveService` / `GardenSaveState`) stay in game code and may mirror through cloud save later — platform never owns garden schema.

## Null semantics

- Player builds: silent / safe defaults
- Editor / `NOT_RELEASE_BUILD`: throttled `Log.Debug` (once per call site)
- Async APIs return `*Unavailable` results — **never throw**, **never grant** rewards or entitlements

## Store / CI

Store provisioning and CI stay in `com.deepforestlabs.buildsystem` (E1). ATT plist / SKAN post-steps remain there; runtime consent queries go through `IConsentService`.

## Related

- [Architecture](architecture.md)
- [Dependency Injection](dependency-injection.md)
- [Getting Started](getting-started.md)
