#nullable enable
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Logger;

namespace DeepForestLabs.Platform.Internal
{
    internal static class FirebaseReflection
    {
        private static readonly object CheckGate = new();
        private static UniTaskCompletionSource<bool>? s_checkTcs;

        public static void TryLoadAssembly(string assemblyName)
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

        public static Type? FindType(string fullName, string assemblyName)
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

        /// <summary>
        /// Single-flight Firebase dependency check. Callers must not touch
        /// DefaultInstance (Auth/Firestore/RemoteConfig) until this returns true —
        /// concurrent GetValue on those properties throws while the check runs.
        /// </summary>
        public static UniTask<bool> CheckAndFixDependencies(CancellationToken token)
        {
            lock (CheckGate)
            {
                if (s_checkTcs == null)
                {
                    s_checkTcs = new UniTaskCompletionSource<bool>();
                    RunCheckAndFixDependencies(s_checkTcs).Forget();
                }
            }

            return AwaitSharedCheck(token);
        }

        private static async UniTask<bool> AwaitSharedCheck(CancellationToken token)
        {
            UniTaskCompletionSource<bool> tcs = s_checkTcs
                ?? throw new InvalidOperationException("Firebase check TCS missing.");
            return await tcs.Task.AttachExternalCancellation(token);
        }

        private static async UniTask<bool> RunCheckAndFixDependencies(UniTaskCompletionSource<bool> tcs)
        {
            bool available = false;
            try
            {
                Type? appType = FindType("Firebase.FirebaseApp", "Firebase.App");
                if (appType == null)
                {
                    Log.Warning("Firebase.App assembly not found.");
                    tcs.TrySetResult(false);
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
                    tcs.TrySetResult(false);
                    return false;
                }

                await AwaitTask(task, CancellationToken.None);
                object? status = TaskResult(task);
                available = status != null && status.ToString() == "Available";
                if (!available)
                {
                    Log.Warning("Firebase dependencies unavailable: {0}", status);
                }

                tcs.TrySetResult(available);
                return available;
            }
            catch (Exception e)
            {
                Log.Exception(e, "Firebase CheckAndFixDependencies failed.");
                tcs.TrySetResult(false);
                return false;
            }
        }

        public static object? GetStaticPropertyValue(Type type, string propertyName)
        {
            return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null);
        }

        public static async UniTask AwaitTask(Task task, CancellationToken token)
        {
            await task.AsUniTask().AttachExternalCancellation(token);
        }

        public static object? TaskResult(Task task)
        {
            return task.GetType().GetProperty("Result")?.GetValue(task);
        }

        public static object? UnwrapUser(object? authResultOrUser)
        {
            if (authResultOrUser == null)
            {
                return null;
            }

            PropertyInfo? userProp = authResultOrUser.GetType().GetProperty("User");
            if (userProp != null)
            {
                return userProp.GetValue(authResultOrUser);
            }

            return authResultOrUser;
        }
    }
}
#nullable disable
