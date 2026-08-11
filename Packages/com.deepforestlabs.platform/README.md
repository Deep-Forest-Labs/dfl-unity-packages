# Deep Forest Labs Platform

Reusable mobile platform seams for portrait games (analytics, ads, IAP, remote config, cloud save, push, account, consent).

## Quick start

```csharp
using DeepForestLabs.Platform;

public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
{
    return base.AddToBuilder(builder)
        .AddPlatformServices(PlatformServiceOptions.Null);
}
```

See [docs/platform.md](../../docs/platform.md) for the full seam list and framework vs platform analytics split.
