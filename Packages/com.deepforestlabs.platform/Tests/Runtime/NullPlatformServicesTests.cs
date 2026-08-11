#nullable enable
using System.Threading;
using NUnit.Framework;

namespace DeepForestLabs.Platform.Tests
{
    [TestFixture]
    public sealed class NullPlatformServicesTests
    {
        [Test]
        public void NullAdService_ShowRewarded_ReturnsUnavailable()
        {
            var ads = new NullAdService();
            Assert.IsFalse(ads.IsAvailable);
            Assert.IsFalse(ads.IsRewardedReady("double_harvest"));

            RewardedAdResult result = ads.ShowRewarded("double_harvest", CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(RewardedAdResult.Unavailable, result);
        }

        [Test]
        public void NullIapService_Purchase_ReturnsUnavailable_AndGrantsNothing()
        {
            var iap = new NullIapService();
            Assert.IsFalse(iap.IsAvailable);
            Assert.IsFalse(iap.HasEntitlement("remove_ads"));

            PurchaseResult result = iap.Purchase("remove_ads", CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(PurchaseResultStatus.Unavailable, result.Status);
            Assert.AreEqual("remove_ads", result.ProductId);
            Assert.IsFalse(iap.HasEntitlement("remove_ads"));
        }

        [Test]
        public void NullCloudSaveService_Load_ReturnsUnavailable()
        {
            var cloud = new NullCloudSaveService();
            CloudSaveLoadResult result = cloud.Load("gw-save", CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(CloudSaveStatus.Unavailable, result.Status);
            Assert.IsNull(result.Data);
        }

        [Test]
        public void NullRemoteConfigService_TryGet_ReturnsFalse()
        {
            var remote = new NullRemoteConfigService();
            Assert.IsFalse(remote.TryGetString("grow_time", out string value));
            Assert.AreEqual(string.Empty, value);
            Assert.IsFalse(remote.TryGetInt("max_plots", out _));
            Assert.IsFalse(remote.TryGetFloat("crit_chance", out _));
            Assert.IsFalse(remote.TryGetBool("feature_flag", out _));
        }

        [Test]
        public void NullAnalyticsService_IsDisabled()
        {
            var analytics = new NullAnalyticsService();
            Assert.IsFalse(analytics.IsEnabled);
            Assert.DoesNotThrow(() => analytics.Track("session_start"));
            Assert.DoesNotThrow(() => analytics.Flush(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void NullConsentService_NotRequired()
        {
            var consent = new NullConsentService();
            Assert.AreEqual(ConsentStatus.NotRequired, consent.Status);
            Assert.IsTrue(consent.AllowsAnalytics);
            Assert.IsFalse(consent.AllowsPersonalizedAds);

            ConsentStatus requested = consent.RequestTrackingAuthorization(CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(ConsentStatus.NotRequired, requested);
        }

        [Test]
        public void NullPushNotificationService_Unavailable()
        {
            var push = new NullPushNotificationService();
            Assert.IsFalse(push.IsAvailable);
            Assert.AreEqual(
                PushPermissionResult.Unavailable,
                push.RequestPermission(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void NullAccountService_ProvidesStableDeviceLocalId()
        {
            var account = new NullAccountService();
            Assert.IsFalse(account.IsLinked);
            string first = account.PlayerId;
            Assert.IsFalse(string.IsNullOrEmpty(first));
            Assert.AreEqual(first, account.PlayerId);
            Assert.DoesNotThrow(() => account.EnsureAnonymousAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void NullAnalyticsUiHelpers_DoNotThrow()
        {
            var helpers = new NullAnalyticsUiHelpers();
            Assert.DoesNotThrow(() => helpers.Log("cond", null, UnityEngine.LogType.Log));
            Assert.DoesNotThrow(() => helpers.ClickedEvent("a", null, null, null, null, null, null));
            Assert.DoesNotThrow(() => helpers.ClickedCloseEvent("a", null, null, null, null, null, null));
        }

        [Test]
        public void AnalyticsOnce_TryClaim_OnlyOnce()
        {
            const string key = "test_funnel_once_" + nameof(AnalyticsOnce_TryClaim_OnlyOnce);
            UnityEngine.PlayerPrefs.DeleteKey("dfl.analytics.once." + key);
            Assert.IsTrue(AnalyticsOnce.TryClaim(key));
            Assert.IsFalse(AnalyticsOnce.TryClaim(key));
            Assert.IsTrue(AnalyticsOnce.HasClaimed(key));
            UnityEngine.PlayerPrefs.DeleteKey("dfl.analytics.once." + key);
        }

        [Test]
        public void AttConsentService_Editor_NotRequired()
        {
            var consent = new AttConsentService();
            Assert.AreEqual(ConsentStatus.NotRequired, consent.Status);
            Assert.IsTrue(consent.AllowsAnalytics);
            Assert.IsFalse(consent.AllowsPersonalizedAds);

            ConsentStatus requested = consent.RequestTrackingAuthorization(CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(ConsentStatus.NotRequired, requested);
        }

    }
}
#nullable disable
