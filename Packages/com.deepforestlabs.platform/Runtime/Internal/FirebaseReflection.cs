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

        public static async UniTask<bool> CheckAndFixDependencies(CancellationToken token)
        {
            Type? appType = FindType("Firebase.FirebaseApp", "Firebase.App");
            if (appType == null)
            {
                Log.Warning("Firebase.App assembly not found.");
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

            await AwaitTask(task, token);
            object? status = TaskResult(task);
            bool available = status != null && status.ToString() == "Available";
            if (!available)
            {
                Log.Warning("Firebase dependencies unavailable: {0}", status);
            }

            return available;
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
