#nullable enable
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Game-owned MAX SDK key + placement → ad unit map. Platform never hardcodes keys.
    /// </summary>
    [Preserve]
    [RequireImplementors]
    public interface IAdPlacementConfig
    {
        string SdkKey { get; }

        /// <summary>Placement ids to preload on init (e.g. <c>debug_rewarded</c>).</summary>
        IReadOnlyList<string> PlacementIds { get; }

        bool TryGetAdUnitId(string placementId, out string adUnitId);
    }
}
#nullable disable
