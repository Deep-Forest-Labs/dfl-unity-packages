#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs;
using DeepForestLabs.Logger;
using DeepForestLabs.Platform.Internal;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// AppLovin MAX rewarded orchestration. Register App-scoped after ATT with a real
    /// <see cref="IMaxSdkClient"/> when <c>DFL_MAX_SDK</c> is defined.
    /// </summary>
    [Preserve]
    public sealed class MaxAdService : IAdService, IInitializable, IDisposable
    {
        public const string DebugRewardedPlacement = "debug_rewarded";

        [Dependency] private readonly IAdPlacementConfig _config = default!;
        [Dependency] private readonly IConsentService _consent = default!;
        [Dependency] private readonly IAnalyticsService _analytics = default!;
        [Dependency] private readonly IMaxSdkClient _max = default!;

        private bool _sdkReady;
        private bool _bound;
        private bool _disposed;

        private readonly Dictionary<string, bool> _readyByAdUnit = new(StringComparer.Ordinal);
        private readonly Dictionary<string, UniTaskCompletionSource<RewardedAdResult>> _pendingByAdUnit =
            new(StringComparer.Ordinal);
        private readonly HashSet<string> _rewardGrantedAdUnits = new(StringComparer.Ordinal);

        public bool IsAvailable => _sdkReady && _max.IsPresent;

        public async UniTask Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (_disposed)
            {
                return;
            }

            if (!_max.IsPresent)
            {
                _sdkReady = false;
                Log.Warning(
                    "MaxAdService: IMaxSdkClient is not present (install AppLovin MAX and define DFL_MAX_SDK).");
                return;
            }

            string sdkKey = _config.SdkKey?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(sdkKey))
            {
                _sdkReady = false;
                Log.Warning("MaxAdService: IAdPlacementConfig.SdkKey is empty; ads unavailable.");
                return;
            }

            try
            {
                bool personalized = _consent.AllowsPersonalizedAds;
                _max.SetHasUserConsent(personalized);
                _max.SetDoNotSell(!personalized);
                _max.SetSdkKey(sdkKey);
                Bind();
                await _max.Initialize(token);
                _sdkReady = true;
                PreloadAll();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _sdkReady = false;
                Log.Exception(e, "MAX SDK initialization failed.");
            }
        }

        public bool IsRewardedReady(string placementId)
        {
            if (!_sdkReady || !TryResolveAdUnit(placementId, out string adUnit))
            {
                return false;
            }

            if (_readyByAdUnit.TryGetValue(adUnit, out bool cached) && cached)
            {
                return true;
            }

            bool ready = _max.IsRewardedReady(adUnit);
            _readyByAdUnit[adUnit] = ready;
            return ready;
        }

        public async UniTask<RewardedAdResult> ShowRewarded(string placementId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!_sdkReady || !TryResolveAdUnit(placementId, out string adUnit))
            {
                TrackResult(placementId, RewardedAdResult.Unavailable);
                return RewardedAdResult.Unavailable;
            }

            if (!IsRewardedReady(placementId))
            {
                _max.LoadRewarded(adUnit);
                TrackResult(placementId, RewardedAdResult.Unavailable);
                return RewardedAdResult.Unavailable;
            }

            if (_pendingByAdUnit.ContainsKey(adUnit))
            {
                TrackResult(placementId, RewardedAdResult.Unavailable);
                return RewardedAdResult.Unavailable;
            }

            var tcs = new UniTaskCompletionSource<RewardedAdResult>();
            _pendingByAdUnit[adUnit] = tcs;
            _rewardGrantedAdUnits.Remove(adUnit);
            _readyByAdUnit[adUnit] = false;

            using (token.Register(() =>
                   {
                       if (_pendingByAdUnit.TryGetValue(adUnit, out UniTaskCompletionSource<RewardedAdResult>? pending)
                           && pending == tcs)
                       {
                           _pendingByAdUnit.Remove(adUnit);
                           tcs.TrySetResult(RewardedAdResult.Cancelled);
                       }
                   }))
            {
                try
                {
                    _max.ShowRewarded(adUnit);
                }
                catch (Exception e)
                {
                    _pendingByAdUnit.Remove(adUnit);
                    Log.Exception(e, "MAX ShowRewarded failed for '{0}'.", placementId);
                    TrackResult(placementId, RewardedAdResult.Failed);
                    _max.LoadRewarded(adUnit);
                    return RewardedAdResult.Failed;
                }

                RewardedAdResult result = await tcs.Task;
                TrackResult(placementId, result);
                _max.LoadRewarded(adUnit);
                return result;
            }
        }

        public bool IsInterstitialReady(string placementId) => false;

        public UniTask<InterstitialAdResult> ShowInterstitial(string placementId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(
                nameof(MaxAdService) + "." + nameof(ShowInterstitial),
                "interstitial unused in E4 ('{0}')",
                placementId);
            return UniTask.FromResult(InterstitialAdResult.Unavailable);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Unbind();
            foreach (UniTaskCompletionSource<RewardedAdResult> pending in _pendingByAdUnit.Values)
            {
                pending.TrySetResult(RewardedAdResult.Cancelled);
            }

            _pendingByAdUnit.Clear();
        }

        private void Bind()
        {
            if (_bound)
            {
                return;
            }

            _bound = true;
            _max.RewardedLoaded += OnLoaded;
            _max.RewardedLoadFailed += OnLoadFailed;
            _max.RewardedDisplayed += OnDisplayed;
            _max.RewardedDisplayFailed += OnDisplayFailed;
            _max.RewardedHidden += OnHidden;
            _max.RewardedReceivedReward += OnReceivedReward;
        }

        private void Unbind()
        {
            if (!_bound)
            {
                return;
            }

            _bound = false;
            _max.RewardedLoaded -= OnLoaded;
            _max.RewardedLoadFailed -= OnLoadFailed;
            _max.RewardedDisplayed -= OnDisplayed;
            _max.RewardedDisplayFailed -= OnDisplayFailed;
            _max.RewardedHidden -= OnHidden;
            _max.RewardedReceivedReward -= OnReceivedReward;
        }

        private void PreloadAll()
        {
            IReadOnlyList<string> placements = _config.PlacementIds;
            for (int i = 0; i < placements.Count; i++)
            {
                if (TryResolveAdUnit(placements[i], out string adUnit))
                {
                    _max.LoadRewarded(adUnit);
                }
            }
        }

        private bool TryResolveAdUnit(string placementId, out string adUnit)
        {
            if (_config.TryGetAdUnitId(placementId, out adUnit!) && !string.IsNullOrWhiteSpace(adUnit))
            {
                adUnit = adUnit.Trim();
                return true;
            }

            adUnit = string.Empty;
            return false;
        }

        private void TrackResult(string placementId, RewardedAdResult result)
        {
            _analytics.Track(
                "ad_rewarded",
                new Dictionary<string, object?>
                {
                    ["placement"] = placementId,
                    ["result"] = result.ToString().ToLowerInvariant()
                });
        }

        private void OnLoaded(string adUnitId) => _readyByAdUnit[adUnitId] = true;

        private void OnLoadFailed(string adUnitId) => _readyByAdUnit[adUnitId] = false;

        private void OnDisplayed(string adUnitId) => _readyByAdUnit[adUnitId] = false;

        private void OnDisplayFailed(string adUnitId) => CompletePending(adUnitId, RewardedAdResult.Failed);

        private void OnHidden(string adUnitId)
        {
            RewardedAdResult result = _rewardGrantedAdUnits.Remove(adUnitId)
                ? RewardedAdResult.Completed
                : RewardedAdResult.Skipped;
            CompletePending(adUnitId, result);
        }

        private void OnReceivedReward(string adUnitId) => _rewardGrantedAdUnits.Add(adUnitId);

        private void CompletePending(string adUnitId, RewardedAdResult result)
        {
            if (_pendingByAdUnit.TryGetValue(adUnitId, out UniTaskCompletionSource<RewardedAdResult>? tcs))
            {
                _pendingByAdUnit.Remove(adUnitId);
                tcs.TrySetResult(result);
            }
        }
    }
}
#nullable disable
