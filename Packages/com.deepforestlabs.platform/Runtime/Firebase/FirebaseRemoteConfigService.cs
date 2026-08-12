#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Logger;
using DeepForestLabs.Platform.Internal;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Firebase Remote Config via reflection (compiles without Firebase UPM).
    /// Boot refresh forces a fresh network fetch; cache-only activate is not success.
    /// </summary>
    [Preserve]
    public sealed class FirebaseRemoteConfigService : IRemoteConfigService
    {
        private const string DefaultMinRequiredVersion = "1.0.0";

        private bool _sdkReady;
        private static bool _missingSdkLogged;

        private static Type? s_rcType;
        private static object? s_defaultInstance;
        private static MethodInfo? s_setDefaultsAsync;
        private static MethodInfo? s_setConfigSettingsAsync;
        private static MethodInfo? s_fetchAsyncTimeSpan;
        private static MethodInfo? s_activateAsync;
        private static MethodInfo? s_getValue;
        private static PropertyInfo? s_stringValue;
        private static PropertyInfo? s_longValue;
        private static PropertyInfo? s_doubleValue;
        private static PropertyInfo? s_booleanValue;
        private static Type? s_configSettingsType;
        private static PropertyInfo? s_minFetchIntervalMs;
        private static bool s_resolved;

        public async UniTask<RemoteConfigRefreshStatus> Refresh(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ResolveFirebaseApi();

            if (s_rcType == null || s_defaultInstance == null)
            {
                if (!_missingSdkLogged)
                {
                    _missingSdkLogged = true;
                    Log.Warning(
                        "PlatformServiceOptions.Firebase selected but Firebase Remote Config assembly was not found.");
                }

                return RemoteConfigRefreshStatus.Failed;
            }

            try
            {
                if (!await EnsureFirebaseAppAvailable(token))
                {
                    _sdkReady = false;
                    return RemoteConfigRefreshStatus.Failed;
                }

                await ApplyBootFetchSettings(token);
                await SetBootDefaults(token);

                if (s_fetchAsyncTimeSpan?.Invoke(s_defaultInstance, new object[] { TimeSpan.Zero }) is not Task fetchTask)
                {
                    Log.Warning("FirebaseRemoteConfig.FetchAsync(TimeSpan) not found.");
                    return RemoteConfigRefreshStatus.Failed;
                }

                await fetchTask.AsUniTask().AttachExternalCancellation(token);

                if (s_activateAsync?.Invoke(s_defaultInstance, null) is not Task activateTask)
                {
                    Log.Warning("FirebaseRemoteConfig.ActivateAsync not found.");
                    return RemoteConfigRefreshStatus.Failed;
                }

                await activateTask.AsUniTask().AttachExternalCancellation(token);

                // ActivateAsync returns Task<bool> on some SDK versions — true means activated.
                PropertyInfo? resultProp = activateTask.GetType().GetProperty("Result");
                if (resultProp != null)
                {
                    object? activated = resultProp.GetValue(activateTask);
                    if (activated is bool ok && !ok)
                    {
                        // Still treat as success if fetch completed; values may already be active.
                        NullPlatformLog.Once(
                            nameof(FirebaseRemoteConfigService) + ".Activate",
                            "ActivateAsync returned false (values may already be current)");
                    }
                }

                _sdkReady = true;
                return RemoteConfigRefreshStatus.Succeeded;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _sdkReady = false;
                Log.Exception(e, "Firebase Remote Config refresh failed.");
                return RemoteConfigRefreshStatus.Failed;
            }
        }

        public bool TryGetString(string key, out string value)
        {
            value = string.Empty;
            if (!_sdkReady || s_defaultInstance == null || s_getValue == null || s_stringValue == null)
            {
                return false;
            }

            try
            {
                object? configValue = s_getValue.Invoke(s_defaultInstance, new object[] { key });
                if (configValue == null)
                {
                    return false;
                }

                value = s_stringValue.GetValue(configValue) as string ?? string.Empty;
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception e)
            {
                Log.Exception(e, "RemoteConfig TryGetString('{0}') failed.", key);
                return false;
            }
        }

        public bool TryGetInt(string key, out int value)
        {
            value = 0;
            if (!_sdkReady || s_defaultInstance == null || s_getValue == null || s_longValue == null)
            {
                return false;
            }

            try
            {
                object? configValue = s_getValue.Invoke(s_defaultInstance, new object[] { key });
                if (configValue == null)
                {
                    return false;
                }

                object? raw = s_longValue.GetValue(configValue);
                value = Convert.ToInt32(raw);
                return true;
            }
            catch (Exception e)
            {
                Log.Exception(e, "RemoteConfig TryGetInt('{0}') failed.", key);
                return false;
            }
        }

        public bool TryGetFloat(string key, out float value)
        {
            value = 0f;
            if (!_sdkReady || s_defaultInstance == null || s_getValue == null || s_doubleValue == null)
            {
                return false;
            }

            try
            {
                object? configValue = s_getValue.Invoke(s_defaultInstance, new object[] { key });
                if (configValue == null)
                {
                    return false;
                }

                object? raw = s_doubleValue.GetValue(configValue);
                value = Convert.ToSingle(raw);
                return true;
            }
            catch (Exception e)
            {
                Log.Exception(e, "RemoteConfig TryGetFloat('{0}') failed.", key);
                return false;
            }
        }

        public bool TryGetBool(string key, out bool value)
        {
            value = false;
            if (!_sdkReady || s_defaultInstance == null || s_getValue == null || s_booleanValue == null)
            {
                return false;
            }

            try
            {
                object? configValue = s_getValue.Invoke(s_defaultInstance, new object[] { key });
                if (configValue == null)
                {
                    return false;
                }

                object? raw = s_booleanValue.GetValue(configValue);
                value = Convert.ToBoolean(raw);
                return true;
            }
            catch (Exception e)
            {
                Log.Exception(e, "RemoteConfig TryGetBool('{0}') failed.", key);
                return false;
            }
        }

        private static async UniTask ApplyBootFetchSettings(CancellationToken token)
        {
            if (s_configSettingsType == null || s_setConfigSettingsAsync == null || s_minFetchIntervalMs == null)
            {
                return;
            }

            object settings = Activator.CreateInstance(s_configSettingsType)!;
            s_minFetchIntervalMs.SetValue(settings, 0L);

            if (s_setConfigSettingsAsync.Invoke(s_defaultInstance, new[] { settings }) is Task settingsTask)
            {
                await settingsTask.AsUniTask().AttachExternalCancellation(token);
            }
        }

        private static async UniTask SetBootDefaults(CancellationToken token)
        {
            if (s_setDefaultsAsync == null)
            {
                return;
            }

            var defaults = new Dictionary<string, object>
            {
                { RemoteConfigKeys.MinRequiredVersion, DefaultMinRequiredVersion }
            };

            if (s_setDefaultsAsync.Invoke(s_defaultInstance, new object[] { defaults }) is Task defaultsTask)
            {
                await defaultsTask.AsUniTask().AttachExternalCancellation(token);
            }
        }

        private static async UniTask<bool> EnsureFirebaseAppAvailable(CancellationToken token)
        {
            Type? appType = FindType("Firebase.FirebaseApp", "Firebase.App");
            if (appType == null)
            {
                Log.Warning("Firebase.App assembly not found; Remote Config disabled.");
                return false;
            }

            MethodInfo? check = appType.GetMethod(
                "CheckAndFixDependenciesAsync",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);
            if (check == null || check.Invoke(null, null) is not Task task)
            {
                Log.Warning("FirebaseApp.CheckAndFixDependenciesAsync not found.");
                return false;
            }

            await task.AsUniTask().AttachExternalCancellation(token);

            PropertyInfo? resultProp = task.GetType().GetProperty("Result");
            object? status = resultProp?.GetValue(task);
            bool available = status != null && status.ToString() == "Available";
            if (!available)
            {
                Log.Warning("Firebase dependencies unavailable: {0}", status);
            }

            return available;
        }

        private static void ResolveFirebaseApi()
        {
            if (s_resolved)
            {
                return;
            }

            s_resolved = true;
            TryLoadAssembly("Firebase.App");
            TryLoadAssembly("Firebase.RemoteConfig");

            s_rcType = FindType("Firebase.RemoteConfig.FirebaseRemoteConfig", "Firebase.RemoteConfig");
            if (s_rcType == null)
            {
                return;
            }

            PropertyInfo? defaultInstance = s_rcType.GetProperty(
                "DefaultInstance",
                BindingFlags.Public | BindingFlags.Static);
            s_defaultInstance = defaultInstance?.GetValue(null);

            s_setDefaultsAsync = s_rcType.GetMethod(
                "SetDefaultsAsync",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(IDictionary<string, object>) },
                modifiers: null)
                ?? s_rcType.GetMethod(
                    "SetDefaultsAsync",
                    BindingFlags.Public | BindingFlags.Instance,
                    binder: null,
                    types: new[] { typeof(Dictionary<string, object>) },
                    modifiers: null);

            s_fetchAsyncTimeSpan = s_rcType.GetMethod(
                "FetchAsync",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(TimeSpan) },
                modifiers: null);

            s_activateAsync = s_rcType.GetMethod(
                "ActivateAsync",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);

            s_getValue = s_rcType.GetMethod(
                "GetValue",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(string) },
                modifiers: null);

            Type? configValueType = FindType("Firebase.RemoteConfig.ConfigValue", "Firebase.RemoteConfig");
            if (configValueType != null)
            {
                s_stringValue = configValueType.GetProperty("StringValue");
                s_longValue = configValueType.GetProperty("LongValue");
                s_doubleValue = configValueType.GetProperty("DoubleValue");
                s_booleanValue = configValueType.GetProperty("BooleanValue");
            }

            s_configSettingsType = FindType("Firebase.RemoteConfig.ConfigSettings", "Firebase.RemoteConfig");
            if (s_configSettingsType != null)
            {
                s_minFetchIntervalMs = s_configSettingsType.GetProperty("MinimumFetchIntervalInMilliseconds")
                    ?? s_configSettingsType.GetProperty("MinimumFetchInternalInMilliseconds");
                s_setConfigSettingsAsync = s_rcType.GetMethod(
                    "SetConfigSettingsAsync",
                    BindingFlags.Public | BindingFlags.Instance,
                    binder: null,
                    types: new[] { s_configSettingsType },
                    modifiers: null);
            }
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
