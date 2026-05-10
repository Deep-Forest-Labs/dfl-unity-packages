#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.States.UnobservedExceptions;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Services
{
    public sealed class LoggingService : ILogHandler, ILoggingService, IInitializable, IDisposable
    {
        [Dependency] private readonly ILogFilter _logFilter = null!;
        [Dependency] private readonly IErrorReporter _errorReporter = null!;
        [Dependency] private readonly UnobservedExceptionState _unobservedExceptionState = null!;
        [Dependency] private readonly IAnalyticsErrorHelper _analyticsErrorHelper = null!;

        private bool _captureEvents;
        private ILogHandler? _unityLogHandler;

        public UniTask Initialize(CancellationToken token)
        {
            _captureEvents = _errorReporter.IsEnabled;
            
            _unityLogHandler = Debug.unityLogger.logHandler;
            Debug.unityLogger.logHandler = this;
            
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            if (_unityLogHandler != null)
            {
                Debug.unityLogger.logHandler = _unityLogHandler;
                _unityLogHandler = null;
            }
        }
        
        [HideInCallstack]
        public void LogFormat(LogType logType, Object context, string format, params object[]? args)
        {
            string formatted;
            if (args != null && args.Length > 0)
            {
                if (args.Length == 1 && args[0] is string arg0)
                {
                    formatted = arg0;
                }
                else
                {
                    formatted = Format(format, args) ?? format;    
                }
            }
            else
            {
                formatted = format;
            }
			
            switch (logType)
            {
                case LogType.Log:
                    CaptureLoggedInfo(logType, context, formatted, format, args);
                    break;

                case LogType.Warning:
                    CaptureLoggedWarning(logType, context,formatted, format, args);
                    break;

                case LogType.Assert:
                    CaptureLoggedAssert(logType, context, formatted, format, args);
                    break;

                case LogType.Error:
                    CaptureLoggedError(logType, context, formatted, format, args);
                    break;
                
                case LogType.Exception:
                    CaptureLoggedException(logType, formatted, context);
                    break;
            }
        }
        
        /// <summary>
        /// This method is called by unity anytime a Unity pumped trigger throws and exception.
        ///
        /// For example a MonoBehaviour that threw an exception while updating.
        /// </summary>
        [HideInCallstack]
        public void LogException(Exception? exception, Object context)
        {
            if (exception is ResetException re)
            {
                _unobservedExceptionState.Trigger(re);
                return;
            }
            CaptureLoggedUnhandledException(exception, context);
        }
        
        [HideInCallstack]
        public void CaptureUnhandledException(Exception? e, Object? context = null)
        {
            if (e == null)
            {
                return;
            }

            if (e is GameException && e.InnerException != null)
            {
                _unityLogHandler?.LogException(e.InnerException, context);
            }
            _unityLogHandler?.LogException(e, context);

            if (_captureEvents)
            {
                if (!_logFilter.IsIgnoredException(e))
                {
                    _errorReporter.CaptureException(e, scope =>
                    {
                        SetTags(scope, e, "Log.Exception", false);
                        scope.Level = ErrorLevel.Warning;
                    });
                }

                _analyticsErrorHelper.Log(e.Message, e.StackTrace, LogType.Exception);
            }
            else
            {
                if (_logFilter.IsIgnoredException(e))
                {
                    _errorReporter.AddBreadcrumb(ZString.Format("{0}\n{1}", e.Message, e.StackTrace), category: "Log.Exception", level: ErrorLevel.Error);
                }

                _analyticsErrorHelper.Log(e.Message, e.StackTrace, LogType.Exception);
            }
        }

        [HideInCallstack]
        private void CaptureLoggedInfo(LogType logType, Object context, string formatted, string format, object[]? args)
        {
            if (_logFilter.IsIgnoredInfo(formatted))
            {
                return;
            }

            // Forward to Unity's ILogHandler
            _unityLogHandler?.LogFormat(logType, context, format, args);

            _errorReporter.AddBreadcrumb(formatted, category: "Log.Info", level: ErrorLevel.Info);

            // Add to analytics, but too noisy and web request intensive.
            //_analyticsLogHandler.AnalyticsLog(condition, null, LogType.Log);
        }
        
        [HideInCallstack]
        private void CaptureLoggedWarning(LogType logType, Object? context, string formatted, string format, object[]? args)
        {
            if (_logFilter.IsIgnoredWarning(formatted))
            {
                return;
            }

            // Forward to Unity's ILogHandler
            _unityLogHandler?.LogFormat(logType, context, format, args);

            _errorReporter.AddBreadcrumb(formatted, category: "Log.Warning", level: ErrorLevel.Warning);

            // Add to analytics, but too noisy and web request intensive.
            //_analyticsLogHandler.AnalyticsLog(condition, null, LogType.Warning);
        }
        
        [HideInCallstack]
        private void CaptureLoggedAssert(LogType logType, Object context, string formatted, string format, object[]? args)
        {
            if (_logFilter.IsIgnoredAssert(formatted))
            {
                return;
            }

            // Forward to Unity's ILogHandler
            _unityLogHandler?.LogFormat(logType, context, format, args);

            _errorReporter.AddBreadcrumb(formatted, category: "Log.Assert", level: ErrorLevel.Error);

            // Add to analytics
            _analyticsErrorHelper.Log(formatted, null, LogType.Assert);
        }
        
        [HideInCallstack]
        private void CaptureLoggedError(LogType logType, Object? context, string formatted, string format, object[]? args)
        {
            if (_logFilter.IsIgnoredError(formatted))
            {
                return;
            }
            else if (_logFilter.IsWarningError(formatted))
            {
                CaptureLoggedWarning(LogType.Warning, context, formatted, format, args);
                return;
            }
            
            _unityLogHandler?.LogFormat(LogType.Error, context, format, args);

            if (_captureEvents)
            {
                _errorReporter.CaptureMessage(formatted, scope =>
                {
                    SetTags(scope, null, logType.ToString(), true);
                    scope.Level = ErrorLevel.Warning;
                });

                _analyticsErrorHelper.Log(formatted, string.Empty, LogType.Error);
            }
            else
            {
                _errorReporter.AddBreadcrumb(formatted, category: logType.ToString(), level: ErrorLevel.Error);
                _analyticsErrorHelper.Log(formatted, string.Empty, LogType.Error);
            }
        }

        [HideInCallstack]
        private void CaptureLoggedException(LogType logType, string message, Object? context)
        {
            if (_logFilter.IsIgnoredError(message))
            {
                return;
            }
            else if (_logFilter.IsWarningError(message))
            {
                CaptureLoggedWarning(LogType.Warning, context, message, "{0}", new object[] { message });
                return;
            }

            _unityLogHandler?.LogFormat(LogType.Error, context, message);

            if (_captureEvents)
            {
                _errorReporter.CaptureMessage(message, scope =>
                {
                    SetTags(scope, null, logType.ToString(), true);
                    scope.Level = ErrorLevel.Warning;
                });

                _analyticsErrorHelper.Log(message, string.Empty, LogType.Error);
            }
            else
            {
                _errorReporter.AddBreadcrumb(message, category: logType.ToString(), level: ErrorLevel.Error);
                _analyticsErrorHelper.Log(message, string.Empty, LogType.Error);
            }
        }

        [HideInCallstack]
        private void CaptureLoggedUnhandledException(Exception? e, Object context)
        {
            if (e == null || _logFilter.IsIgnoredException(e))
            {
                return;
            }
            
            _unityLogHandler?.LogException(e, context);

            if (_captureEvents)
            {
                _errorReporter.CaptureException(e, scope =>
                {
                    SetTags(scope, e, "Log.Exception", false);
                    scope.Level = ErrorLevel.Warning;
                });

                _analyticsErrorHelper.Log(e.Message, e.StackTrace, LogType.Error);
            }
            else
            {
                _errorReporter.AddBreadcrumb(ZString.Format("{0}\n{1}", e.Message, e.StackTrace), category: "Log.Exception", level: ErrorLevel.Error);
                _analyticsErrorHelper.Log(e.Message, e.StackTrace, LogType.Error);
            }
        }

        [HideInCallstack]
        private static void SetTags(IErrorReporterScope scope, Exception? e, string mechanism,
            bool handled)
        {
            if (e == null)
            {
                return;
            }
            
            IDictionary<string, string> tags;
            if (!e.Data.Contains(SentryErrorReporter.EXCEPTIONS_DATA_TAG))
            {
                tags = new Dictionary<string, string>();
                e.Data[SentryErrorReporter.EXCEPTIONS_DATA_TAG] = tags;
            }
            else
            {
                tags = (e.Data[SentryErrorReporter.EXCEPTIONS_DATA_TAG] as IDictionary<string, string>)!;
            }
            
            tags["handled"] = handled ? "yes" : "no";
            tags["mechanism"] = mechanism;
            
            foreach (KeyValuePair<string, string> pair in tags)
            {
                scope.SetTag(pair.Key, pair.Value);
            }
        }
        
        [StringFormatMethod("format")]
        private static string? Format(string format, object?[] args)
        {
            return args.Length switch
            {
                1 => ZString.Format(format, args[0]),
                2 => ZString.Format(format, args[0], args[1]),
                3 => ZString.Format(format, args[0], args[1], args[2]),
                4 => ZString.Format(format, args[0], args[1], args[2], args[3]),
                5 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4]),
                6 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5]),
                7 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6]),
                8 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]),
                9 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]),
                10 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]),
                11 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]),
                12 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]),
                13 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]),
                14 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]),
                15 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]),
                16 => ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15]),
                _ => null,
            };
        }
    }
}