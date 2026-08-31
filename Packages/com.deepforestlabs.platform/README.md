# Deep Forest Labs Platform

Reusable mobile platform seams for portrait games (analytics, ads, IAP, remote config, cloud save, push, account, consent).

## Quick start

```csharp
using DeepForestLabs.Platform;

public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
{
    return base.AddToBuilder(builder)
#if UNITY_EDITOR
        .AddPlatformServices(PlatformServiceOptions.Null)
#else
        .AddPlatformServices(PlatformServiceOptions.Firebase)
#endif
        ;
}
```

`Firebase` requires the game to install Firebase Unity App + Analytics + Remote Config (+ Auth and Firestore for E6) packages and commit `google-services.json` / `GoogleService-Info.plist`. See [docs/platform.md](../../docs/platform.md) and ghostgarden `ci/firebase.md`.

Error reporting stays on **Sentry** (`IAnalyticsErrorHelper` remains a no-op). Force-update / boot gate: `IRemoteConfigService` + `IBootConfigClient` + `AppVersionGate` (game wires App-scope orchestration + Update Required UI).

Ads: `AddPlatformServices` stays on `NullAdService`. After ATT, a game App scope can register `MaxAdService` + `IAdPlacementConfig` + an `IMaxSdkClient` (MAX plugin + `DFL_MAX_SDK` live in the game). Without the plugin the client reports `IsPresent == false` and rewarded stays `Unavailable`.
