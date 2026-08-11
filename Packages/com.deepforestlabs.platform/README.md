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

`Firebase` requires the game to install Firebase Unity Analytics packages and commit `google-services.json` / `GoogleService-Info.plist`. See [docs/platform.md](../../docs/platform.md) and ghostgarden `ci/firebase.md`.

Error reporting stays on **Sentry** (`IAnalyticsErrorHelper` remains a no-op).
