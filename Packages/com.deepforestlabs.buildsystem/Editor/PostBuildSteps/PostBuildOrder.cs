namespace DeepForestLabs.BuildSystems.PostBuildSteps
{
    public enum PostBuildOrder
    {
        ClientPostBuilder           = int.MaxValue - 0,
        PodFilePostProcess          = int.MaxValue - 1,
        SpriteAtlasV2               = int.MaxValue - 2,
        RenameSplitAPKFiles         = int.MaxValue - 3,
        SaveAndroidManifest         = int.MaxValue - 4,
        AppFlyerSkan                = int.MaxValue - 5,
        AppTrackingTransparency     = int.MaxValue - 6,
        CreateExportOptions         = int.MaxValue - 7,
        DisableBitCode              = int.MaxValue - 8,
        ExemptFromEncryption        = int.MaxValue - 9,
        LaunchStoryBoard            = int.MaxValue - 10,
        FakeUploadToken             = int.MaxValue - 11,
        BootConfigModifier          = int.MaxValue - 12,
    }
}