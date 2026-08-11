# Platform

`com.deepforestlabs.platform` provides **callable seams** for mobile platform features. Games depend on interfaces; adapters opt in via `PlatformServiceOptions`. Ghostgarden is the proof consumer; the web design lab does not need IAP/ads/push parity.

## Opt in

```csharp
using DeepForestLabs.Platform;

public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
{
    return base.AddToBuilder(builder)
        .AddSingleton(BuildSettings.Instance)
#if UNITY_EDITOR
        .AddPlatformServices(PlatformServiceOptions.Null)
#else
        .AddPlatformServices(PlatformServiceOptions.Firebase)
#endif
        // game-specific registrations...
        ;
}
```

`AddPlatformServices` **always registers every seam**. Use required `[Dependency]` fields. Availability is expressed via `IsAvailable` / result enums — not missing DI registrations.

| Option | Behavior |
|--------|----------|
| `Null` | No-op seams (editor / tests) |
| `Firebase` | Firebase Analytics + ATT consent; other seams still null until later epics |

Firebase Unity packages are **owned by the game** (`com.google.firebase.app` / `com.google.firebase.analytics`). The platform Firebase adapter uses reflection so this package always compiles; without the SDK present at runtime, events are dropped.

## Seams

| Interface | Null behavior | Later epic |
|-----------|---------------|------------|
| `IAnalyticsService` | Drops events | E2 — Firebase funnels |
| `IRemoteConfigService` | No keys | E3 — remote overlay |
| `IAdService` | `Unavailable` (never grants reward) | E4 — mediation |
| `IIapService` | `Unavailable` (never grants entitlement) | E5 — store + validation |
| `ICloudSaveService` | `Unavailable` blob/key API | E6 — cloud mirror |
| `IPushNotificationService` | `Unavailable` | E7 — push |
| `IAccountService` | Device-local anonymous id | E6 — link / recovery |
| `IConsentService` | `NotRequired` | E2 — ATT (`AttConsentService` on Firebase option) |

## Analytics: framework vs platform

**Stays in `com.deepforestlabs.framework`** (Controller-coupled):

- `AnalyticsStringValues` + helpers
- `IAnalyticsUIEventHelper` / `IAnalyticsErrorHelper`
- Controller click analytics wiring

**Lives in platform:**

- `IAnalyticsService` (product / funnel events)
- `FirebaseAnalyticsService` + `FirebaseAnalyticsUiEventHelper` (`ui_click` forwarder)
- `NullAnalyticsUiHelpers` (error helper stays no-op — **Sentry** is crash/error truth)
- `AnalyticsOnce` — lifetime-once funnel flags in PlayerPrefs (not game save)

## Consent policy (ATT)

- iOS: request ATT early (`IConsentService.RequestTrackingAuthorization`)
- Analytics allowed even when ATT is denied
- `AllowsPersonalizedAds` only when status is `Authorized`
- Editor / Android: `NotRequired`

## Cloud save vs game save

`ICloudSaveService` is a **generic string blob/key store**. Game schemas (e.g. ghostgarden `ISaveService` / `GardenSaveState`) stay in game code and may mirror through cloud save later — platform never owns garden schema.

## Null semantics

- Player builds: silent / safe defaults
- Editor / `NOT_RELEASE_BUILD`: throttled `Log.Debug` (once per call site)
- Async APIs return `*Unavailable` results — **never throw**, **never grant** rewards or entitlements

## Store / CI

Store provisioning and CI use `com.deepforestlabs.buildsystem` plus game-repo workflows. Ghostgarden’s `ci/README.md` is the reference (self-hosted Mac GHA, TestFlight + Play internal, LocalBundles, local `ci/envlist.json`). ATT plist / framework / SKAN post-steps remain in buildsystem; runtime consent queries go through `IConsentService`.

## Related

- [Architecture](architecture.md)
- [Dependency Injection](dependency-injection.md)
- [Getting Started](getting-started.md)
