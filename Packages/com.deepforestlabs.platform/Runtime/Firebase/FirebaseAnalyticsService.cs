#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeepForestLabs;
using DeepForestLabs.Logger;
using DeepForestLabs.Platform.Internal;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Firebase Analytics adapter via reflection so this package compiles without Firebase UPM packages.
    /// Games install <c>com.google.firebase.analytics</c>; when present, events are forwarded.
    /// </summary>
    [Preserve]
    public sealed class FirebaseAnalyticsService : IAnalyticsService, IInitializable
    {
        [Dependency] private readonly IConsentService _consent = default!;

        private bool _sdkReady;
        private static bool _missingSdkLogged;

        private static Type? s_analyticsType;
        private static Type? s_parameterType;
        private static MethodInfo? s_logEventName;
        private static MethodInfo? s_logEventParams;
        private static ConstructorInfo? s_paramString;
        private static ConstructorInfo? s_paramLong;
        private static ConstructorInfo? s_paramDouble;
        private static bool s_resolved;

        public bool IsEnabled => _consent.AllowsAnalytics && _sdkReady;

        public async UniTask Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ResolveFirebaseApi();

            if (s_analyticsType == null)
            {
                _sdkReady = false;
                if (!_missingSdkLogged)
                {
                    _missingSdkLogged = true;
                    Log.Warning(
                        "PlatformServiceOptions.Firebase selected but Firebase Analytics assembly was not found; events dropped.");
                }

                return;
            }

            try
            {
                Type? appType = FindType("Firebase.FirebaseApp", "Firebase.App");
                if (appType == null)
                {
                    _sdkReady = false;
                    Log.Warning("Firebase.App assembly not found; Analytics disabled.");
                    return;
                }

                MethodInfo? check = appType.GetMethod(
                    "CheckAndFixDependenciesAsync",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);
                if (check == null || check.Invoke(null, null) is not Task task)
                {
                    _sdkReady = false;
                    Log.Warning("FirebaseApp.CheckAndFixDependenciesAsync not found.");
                    return;
                }

                await task.AsUniTask().AttachExternalCancellation(token);

                PropertyInfo? resultProp = task.GetType().GetProperty("Result");
                object? status = resultProp?.GetValue(task);
                _sdkReady = status != null && status.ToString() == "Available";
                if (!_sdkReady)
                {
                    Log.Warning("Firebase dependencies unavailable: {0}", status);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _sdkReady = false;
                Log.Exception(e, "Firebase Analytics initialization failed.");
            }
        }

        public void Track(string eventName, IReadOnlyDictionary<string, object?>? parameters = null)
        {
            if (string.IsNullOrEmpty(eventName) || !IsEnabled || s_analyticsType == null)
            {
                if (!IsEnabled && s_analyticsType == null)
                {
                    NullPlatformLog.Once(
                        nameof(FirebaseAnalyticsService) + "." + nameof(Track),
                        "dropped event '{0}' (Firebase SDK missing)",
                        eventName);
                }

                return;
            }

            try
            {
                if (parameters == null || parameters.Count == 0 || s_logEventParams == null || s_parameterType == null)
                {
                    s_logEventName?.Invoke(null, new object[] { eventName });
                    return;
                }

                var list = new List<object>();
                foreach (KeyValuePair<string, object?> pair in parameters)
                {
                    if (string.IsNullOrEmpty(pair.Key) || pair.Value == null)
                    {
                        continue;
                    }

                    object? param = CreateParameter(pair.Key, pair.Value);
                    if (param != null)
                    {
                        list.Add(param);
                    }
                }

                if (list.Count == 0)
                {
                    s_logEventName?.Invoke(null, new object[] { eventName });
                    return;
                }

                Array typed = Array.CreateInstance(s_parameterType, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    typed.SetValue(list[i], i);
                }

                s_logEventParams!.Invoke(null, new object[] { eventName, typed });
            }
            catch (Exception e)
            {
                Log.Exception(e, "FirebaseAnalytics.LogEvent failed for '{0}'.", eventName);
            }
        }

        public UniTask Flush(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return UniTask.CompletedTask;
        }

        private static void ResolveFirebaseApi()
        {
            if (s_resolved)
            {
                return;
            }

            s_resolved = true;
            TryLoadAssembly("Firebase.App");
            TryLoadAssembly("Firebase.Analytics");
            s_analyticsType = FindType("Firebase.Analytics.FirebaseAnalytics", "Firebase.Analytics");
            if (s_analyticsType == null)
            {
                return;
            }

            s_parameterType = FindType("Firebase.Analytics.Parameter", "Firebase.Analytics");
            s_logEventName = s_analyticsType.GetMethod(
                "LogEvent",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(string) },
                modifiers: null);

            if (s_parameterType != null)
            {
                s_logEventParams = s_analyticsType.GetMethod(
                    "LogEvent",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(string), s_parameterType.MakeArrayType() },
                    modifiers: null);

                s_paramString = s_parameterType.GetConstructor(new[] { typeof(string), typeof(string) });
                s_paramLong = s_parameterType.GetConstructor(new[] { typeof(string), typeof(long) });
                s_paramDouble = s_parameterType.GetConstructor(new[] { typeof(string), typeof(double) });
            }
        }

        private static object? CreateParameter(string key, object value)
        {
            if (value is string s)
            {
                return s_paramString?.Invoke(new object[] { key, s });
            }

            if (value is bool b)
            {
                return s_paramLong?.Invoke(new object[] { key, b ? 1L : 0L });
            }

            if (value is byte or sbyte or short or ushort or int or uint or long)
            {
                return s_paramLong?.Invoke(new object[] { key, Convert.ToInt64(value) });
            }

            if (value is float or double or decimal)
            {
                return s_paramDouble?.Invoke(new object[] { key, Convert.ToDouble(value) });
            }

            return s_paramString?.Invoke(new object[] { key, value.ToString() });
        }

        private static void TryLoadAssembly(string assemblyName)
        {
            try
            {
                Assembly.Load(assemblyName);
            }
            catch
            {
                // SDK not installed in this project.
            }
        }

        private static Type? FindType(string fullName, string assemblyName)
        {
            Type? direct = Type.GetType($"{fullName}, {assemblyName}", throwOnError: false);
            if (direct != null)
            {
                return direct;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string? name = assembly.GetName().Name;
                if (name == null || !name.StartsWith("Firebase", StringComparison.Ordinal))
                {
                    continue;
                }

                Type? type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
#nullable disable
