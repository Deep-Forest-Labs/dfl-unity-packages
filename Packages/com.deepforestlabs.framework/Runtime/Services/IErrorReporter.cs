#nullable enable
using System;

namespace DeepForestLabs.Services
{
    public enum ErrorLevel { Debug, Info, Warning, Error, Fatal }

    public interface IErrorReporterScope
    {
        void SetTag(string key, string value);
        ErrorLevel? Level { set; }
    }

    public interface IErrorReporter : IInitializable, IDisposable
    {
        bool IsEnabled { get; }
        void CaptureException(Exception exception, Action<IErrorReporterScope>? configureScope = null);
        void CaptureMessage(string message, Action<IErrorReporterScope>? configureScope = null);
        void AddBreadcrumb(string message, string? category = null, ErrorLevel level = ErrorLevel.Info);
        void StartSession();
        void EndSession();
        void ConfigureScope(Action<IErrorReporterScope> configureScope);
    }
}
#nullable disable
