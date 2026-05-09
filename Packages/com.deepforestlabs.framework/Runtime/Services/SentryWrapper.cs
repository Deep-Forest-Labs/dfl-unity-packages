#nullable enable
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using Sentry.Unity;

namespace DeepForestLabs.Services
{
    public sealed class SentryWrapper : IInitializable
    {
        public const string EXCEPTIONS_DATA_TAG = "SentryTags";

        public UniTask Initialize(CancellationToken token)
        {
            // Runtime scope context belongs here (not options):
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("product", DeepForestLabs.BuildSystems.BuildSettings.Instance.AppName);
                scope.SetTag("platform", "client");
            });

            Log.Debug($"[Sentry] SentryWrapper.Initialize - Sentry.IsEnabled : {SentrySdk.IsEnabled}");

            return UniTask.CompletedTask;
        }
    }
}
#nullable disable
