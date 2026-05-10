#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeepForestLabs.Services
{
    public sealed class NullErrorReporter : IErrorReporter
    {
        public bool IsEnabled => false;

        public UniTask Initialize(CancellationToken token) => UniTask.CompletedTask;
        public void Dispose() { }

        public void CaptureException(Exception exception, Action<IErrorReporterScope>? configureScope = null)
        {
            Debug.LogException(exception);
        }

        public void CaptureMessage(string message, Action<IErrorReporterScope>? configureScope = null)
        {
            Debug.LogWarning(message);
        }

        public void AddBreadcrumb(string message, string? category = null, ErrorLevel level = ErrorLevel.Info) { }
        public void StartSession() { }
        public void EndSession() { }
        public void ConfigureScope(Action<IErrorReporterScope> configureScope) { }
    }
}
#nullable disable
