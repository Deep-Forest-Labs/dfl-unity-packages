namespace DeepForestLabs.BuildSystems.PreBuildSteps
{
    public enum PreBuildOrder
    {
        ClientPreBuilder        = int.MinValue,
        SpriteAtlasV2           = int.MinValue + 1,
        SetBuildNumber          = int.MinValue + 2,
        SetBuildSubTarget       = int.MinValue + 3,
        TeakExtensionVersion    = int.MinValue + 4,
        SetScriptingDefines     = int.MinValue + 5,
        SetKeystoreInfo         = int.MinValue + 6,
    }
}