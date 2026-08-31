#nullable enable
using System;
using System.Collections;
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
    /// Firestore blob store via reflection. Document <c>saves/{uid}</c>, fields are keys.
    /// Games install <c>com.google.firebase.firestore</c>.
    /// </summary>
    [Preserve]
    public sealed class FirebaseCloudSaveService : ICloudSaveService, IInitializable
    {
        public const string EventCloudSave = "cloud_save";
        public const string CollectionId = "saves";

        [Dependency] private readonly IAccountService _account = default!;
        [Dependency] private readonly IAnalyticsService _analytics = default!;

        private bool _sdkReady;
        private static bool _missingSdkLogged;

        private static Type? s_firestoreType;
        private static object? s_firestore;
        private static MethodInfo? s_collection;
        private static MethodInfo? s_document;
        private static MethodInfo? s_getSnapshot;
        private static MethodInfo? s_setAsync;
        private static MethodInfo? s_setAsyncMerge;
        private static MethodInfo? s_updateAsync;
        private static MethodInfo? s_toDictionary;
        private static PropertyInfo? s_exists;
        private static object? s_mergeAll;
        private static object? s_fieldDelete;
        private static bool s_resolved;

        public bool IsAvailable => _sdkReady;

        public async UniTask Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ResolveFirebaseApi();

            if (s_firestoreType == null || s_firestore == null)
            {
                _sdkReady = false;
                if (!_missingSdkLogged)
                {
                    _missingSdkLogged = true;
                    Log.Warning(
                        "PlatformServiceOptions.Firebase selected but Firebase Firestore assembly was not found; cloud save unavailable.");
                }

                return;
            }

            try
            {
                _sdkReady = await FirebaseReflection.CheckAndFixDependencies(token);
                if (!_sdkReady)
                {
                    Log.Warning("Firebase Firestore disabled; dependencies unavailable.");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _sdkReady = false;
                Log.Exception(e, "Firebase Firestore initialization failed.");
            }
        }

        public async UniTask<CloudSaveLoadResult> Load(string key, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!TryBegin(key, out CloudSaveStatus early))
            {
                return new CloudSaveLoadResult(early, null);
            }

            try
            {
                object? doc = UserDocument();
                if (doc == null || s_getSnapshot == null)
                {
                    TrackFailure();
                    return new CloudSaveLoadResult(CloudSaveStatus.Failed, null);
                }

                if (s_getSnapshot.Invoke(doc, null) is not Task task)
                {
                    TrackFailure();
                    return new CloudSaveLoadResult(CloudSaveStatus.Failed, null);
                }

                await FirebaseReflection.AwaitTask(task, token);
                object? snapshot = FirebaseReflection.TaskResult(task);
                if (snapshot == null || s_exists?.GetValue(snapshot) is not true)
                {
                    return new CloudSaveLoadResult(CloudSaveStatus.NotFound, null);
                }

                if (s_toDictionary?.Invoke(snapshot, null) is not IDictionary fields
                    || !fields.Contains(key)
                    || fields[key] is not string data
                    || string.IsNullOrEmpty(data))
                {
                    return new CloudSaveLoadResult(CloudSaveStatus.NotFound, null);
                }

                return new CloudSaveLoadResult(CloudSaveStatus.Succeeded, data);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Exception(e, "Firestore Load('{0}') failed.", key);
                TrackFailure();
                return new CloudSaveLoadResult(CloudSaveStatus.Failed, null);
            }
        }

        public async UniTask<CloudSaveWriteResult> Save(string key, string data, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!TryBegin(key, out CloudSaveStatus early))
            {
                return new CloudSaveWriteResult(early);
            }

            if (data == null)
            {
                TrackFailure();
                return new CloudSaveWriteResult(CloudSaveStatus.Failed);
            }

            try
            {
                object? doc = UserDocument();
                if (doc == null)
                {
                    TrackFailure();
                    return new CloudSaveWriteResult(CloudSaveStatus.Failed);
                }

                var payload = new Dictionary<string, object> { [key] = data };
                Task? task = null;
                if (s_setAsyncMerge != null && s_mergeAll != null)
                {
                    task = s_setAsyncMerge.Invoke(doc, new[] { payload, s_mergeAll }) as Task;
                }
                else if (s_setAsync != null)
                {
                    task = s_setAsync.Invoke(doc, new object[] { payload }) as Task;
                }

                if (task == null)
                {
                    TrackFailure();
                    return new CloudSaveWriteResult(CloudSaveStatus.Failed);
                }

                await FirebaseReflection.AwaitTask(task, token);
                return new CloudSaveWriteResult(CloudSaveStatus.Succeeded);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Exception(e, "Firestore Save('{0}') failed.", key);
                TrackFailure();
                return new CloudSaveWriteResult(CloudSaveStatus.Failed);
            }
        }

        public async UniTask<CloudSaveWriteResult> Delete(string key, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!TryBegin(key, out CloudSaveStatus early))
            {
                return new CloudSaveWriteResult(early);
            }

            try
            {
                CloudSaveLoadResult existing = await Load(key, token);
                if (existing.Status == CloudSaveStatus.NotFound)
                {
                    return new CloudSaveWriteResult(CloudSaveStatus.Succeeded);
                }

                if (existing.Status != CloudSaveStatus.Succeeded)
                {
                    return new CloudSaveWriteResult(existing.Status);
                }

                object? doc = UserDocument();
                if (doc == null || s_updateAsync == null || s_fieldDelete == null)
                {
                    TrackFailure();
                    return new CloudSaveWriteResult(CloudSaveStatus.Failed);
                }

                var updates = new Dictionary<string, object> { [key] = s_fieldDelete };
                if (s_updateAsync.Invoke(doc, new object[] { updates }) is not Task task)
                {
                    TrackFailure();
                    return new CloudSaveWriteResult(CloudSaveStatus.Failed);
                }

                await FirebaseReflection.AwaitTask(task, token);
                return new CloudSaveWriteResult(CloudSaveStatus.Succeeded);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Exception(e, "Firestore Delete('{0}') failed.", key);
                TrackFailure();
                return new CloudSaveWriteResult(CloudSaveStatus.Failed);
            }
        }

        private bool TryBegin(string key, out CloudSaveStatus status)
        {
            if (!_sdkReady || s_firestore == null)
            {
                status = CloudSaveStatus.Unavailable;
                return false;
            }

            if (string.IsNullOrWhiteSpace(_account.PlayerId)
                || string.IsNullOrWhiteSpace(key)
                || key.IndexOf('/') >= 0)
            {
                status = CloudSaveStatus.Failed;
                TrackFailure();
                return false;
            }

            status = CloudSaveStatus.Succeeded;
            return true;
        }

        private object? UserDocument()
        {
            if (s_collection == null || s_document == null || s_firestore == null)
            {
                return null;
            }

            object? collection = s_collection.Invoke(s_firestore, new object[] { CollectionId });
            if (collection == null)
            {
                return null;
            }

            return s_document.Invoke(collection, new object[] { _account.PlayerId.Trim() });
        }

        private void TrackFailure()
        {
            _analytics.Track(
                EventCloudSave,
                new Dictionary<string, object?>
                {
                    ["result"] = CloudSaveStatus.Failed.ToString().ToLowerInvariant()
                });
        }

        private static void ResolveFirebaseApi()
        {
            if (s_resolved)
            {
                return;
            }

            s_resolved = true;
            FirebaseReflection.TryLoadAssembly("Firebase.App");
            FirebaseReflection.TryLoadAssembly("Firebase.Firestore");

            s_firestoreType = FirebaseReflection.FindType("Firebase.Firestore.FirebaseFirestore", "Firebase.Firestore");
            if (s_firestoreType == null)
            {
                return;
            }

            PropertyInfo? defaultInstance = s_firestoreType.GetProperty(
                "DefaultInstance",
                BindingFlags.Public | BindingFlags.Static);
            s_firestore = defaultInstance?.GetValue(null);
            s_collection = s_firestoreType.GetMethod(
                "Collection",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(string) },
                modifiers: null);

            Type? collectionType = FirebaseReflection.FindType(
                "Firebase.Firestore.CollectionReference",
                "Firebase.Firestore");
            s_document = collectionType?.GetMethod(
                "Document",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(string) },
                modifiers: null);

            Type? docType = FirebaseReflection.FindType(
                "Firebase.Firestore.DocumentReference",
                "Firebase.Firestore");
            if (docType != null)
            {
                s_getSnapshot = docType.GetMethod(
                    "GetSnapshotAsync",
                    BindingFlags.Public | BindingFlags.Instance,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);
                s_setAsync = docType.GetMethod(
                    "SetAsync",
                    BindingFlags.Public | BindingFlags.Instance,
                    binder: null,
                    types: new[] { typeof(object) },
                    modifiers: null);

                Type? setOptions = FirebaseReflection.FindType(
                    "Firebase.Firestore.SetOptions",
                    "Firebase.Firestore");
                if (setOptions != null)
                {
                    s_mergeAll = setOptions.GetProperty("MergeAll", BindingFlags.Public | BindingFlags.Static)
                        ?.GetValue(null);
                    s_setAsyncMerge = docType.GetMethod(
                        "SetAsync",
                        BindingFlags.Public | BindingFlags.Instance,
                        binder: null,
                        types: new[] { typeof(object), setOptions },
                        modifiers: null);
                }

                s_updateAsync = docType.GetMethod(
                    "UpdateAsync",
                    BindingFlags.Public | BindingFlags.Instance,
                    binder: null,
                    types: new[] { typeof(IDictionary<string, object>) },
                    modifiers: null);
            }

            Type? snapshotType = FirebaseReflection.FindType(
                "Firebase.Firestore.DocumentSnapshot",
                "Firebase.Firestore");
            if (snapshotType != null)
            {
                s_exists = snapshotType.GetProperty("Exists");
                s_toDictionary = snapshotType.GetMethod(
                    "ToDictionary",
                    BindingFlags.Public | BindingFlags.Instance,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);
            }

            Type? fieldValue = FirebaseReflection.FindType(
                "Firebase.Firestore.FieldValue",
                "Firebase.Firestore");
            s_fieldDelete = fieldValue?.GetProperty("Delete", BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null);
        }
    }
}
#nullable disable
