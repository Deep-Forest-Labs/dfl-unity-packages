#nullable enable
namespace DeepForestLabs.Platform
{
    public enum RewardedAdResult
    {
        Unavailable = 0,
        Completed = 1,
        Skipped = 2,
        Failed = 3,
        Cancelled = 4
    }

    public enum InterstitialAdResult
    {
        Unavailable = 0,
        Shown = 1,
        Failed = 2,
        Cancelled = 3
    }

    public enum PurchaseResultStatus
    {
        Unavailable = 0,
        Succeeded = 1,
        Cancelled = 2,
        Failed = 3,
        Pending = 4
    }

    public readonly record struct PurchaseResult(PurchaseResultStatus Status, string ProductId);

    public enum RestorePurchasesResult
    {
        Unavailable = 0,
        Succeeded = 1,
        Failed = 2
    }

    public enum CloudSaveStatus
    {
        Unavailable = 0,
        Succeeded = 1,
        NotFound = 2,
        Failed = 3
    }

    public readonly record struct CloudSaveLoadResult(CloudSaveStatus Status, string? Data);

    public readonly record struct CloudSaveWriteResult(CloudSaveStatus Status);

    public enum PushPermissionResult
    {
        Unavailable = 0,
        Granted = 1,
        Denied = 2,
        NotDetermined = 3
    }

    public enum ConsentStatus
    {
        Unknown = 0,
        NotRequired = 1,
        Authorized = 2,
        Denied = 3,
        Restricted = 4
    }

    public enum PlatformServiceOptions
    {
        Null = 0
    }
}
#nullable disable
