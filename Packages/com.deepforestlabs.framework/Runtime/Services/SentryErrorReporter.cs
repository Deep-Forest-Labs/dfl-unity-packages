#nullable enable
using System;
using System.Threading;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using Sentry;
using Sentry.Unity;

namespace DeepForestLabs.Services
{
    public sealed class SentryErrorReporter : IErrorReporter
    {
        public const string EXCEPTIONS_DATA_TAG = "SentryTags";

        public bool IsEnabled => SentrySdk.IsEnabled;

        public UniTask Initialize(CancellationToken token)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("product", BuildSettings.Instance.AppName);
                scope.SetTag("platform", "client");
            });

            Log.Debug($"[Sentry] SentryErrorReporter.Initialize - Sentry.IsEnabled : {SentrySdk.IsEnabled}");
            return UniTask.CompletedTask;
        }

        public void Dispose() { }

        public void CaptureException(Exception exception, Action<IErrorReporterScope>? configureScope = null)
        {
            if (configureScope != null)
            {
                SentrySdk.CaptureException(exception, scope =>
                {
                    var wrapper = new SentryErrorReporterScope(scope);
                    configureScope(wrapper);
                });
            }
            else
            {
                SentrySdk.CaptureException(exception);
            }
        }

        public void CaptureMessage(string message, Action<IErrorReporterScope>? configureScope = null)
        {
            if (configureScope != null)
            {
                SentrySdk.CaptureMessage(message, scope =>
                {
                    var wrapper = new SentryErrorReporterScope(scope);
                    configureScope(wrapper);
                }, SentryLevel.Warning);
            }
            else
            {
                SentrySdk.CaptureMessage(message);
            }
        }

        public void AddBreadcrumb(string message, string? category = null, ErrorLevel level = ErrorLevel.Info)
        {
            SentrySdk.AddBreadcrumb(message, category: category, level: ToSentryLevel(level));
        }

        public void StartSession() => SentrySdk.StartSession();
        public void EndSession() => SentrySdk.EndSession();

        public void ConfigureScope(Action<IErrorReporterScope> configureScope)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                var wrapper = new SentryErrorReporterScope(scope);
                configureScope(wrapper);
            });
        }

        private static BreadcrumbLevel ToSentryLevel(ErrorLevel level) => level switch
        {
            ErrorLevel.Debug => BreadcrumbLevel.Debug,
            ErrorLevel.Info => BreadcrumbLevel.Info,
            ErrorLevel.Warning => BreadcrumbLevel.Warning,
            ErrorLevel.Error => BreadcrumbLevel.Error,
            ErrorLevel.Fatal => BreadcrumbLevel.Error,
            _ => BreadcrumbLevel.Info
        };

        private sealed class SentryErrorReporterScope : IErrorReporterScope
        {
            private readonly Scope _scope;
            public SentryErrorReporterScope(Scope scope) => _scope = scope;

            public void SetTag(string key, string value) => _scope.SetTag(key, value);

            public ErrorLevel? Level
            {
                set
                {
                    if (value.HasValue)
                    {
                        _scope.Level = value.Value switch
                        {
                            ErrorLevel.Debug => SentryLevel.Debug,
                            ErrorLevel.Info => SentryLevel.Info,
                            ErrorLevel.Warning => SentryLevel.Warning,
                            ErrorLevel.Error => SentryLevel.Error,
                            ErrorLevel.Fatal => SentryLevel.Fatal,
                            _ => SentryLevel.Info
                        };
                    }
                }
            }
        }
    }
}
#nullable disable
