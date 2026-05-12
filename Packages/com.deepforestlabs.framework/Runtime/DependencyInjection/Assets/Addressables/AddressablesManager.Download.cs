#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ZLinq;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;
using AddressablesImpl = UnityEngine.AddressableAssets.Addressables;

namespace DeepForestLabs.Assets.Addressables
{
    internal sealed partial class AddressablesManager
    {
        public UniTask DownloadScene(string guid, CancellationToken token)
        {
            AssetReference assetReference = GetSceneAssetReference(guid);

            return GetSceneDownloadHandle(guid, assetReference)
                .WaitUntilCached(token);
        }

        public UniTask DownloadAudioClip(string guid, CancellationToken token)
        {
            AssetReferenceT<AudioClip> assetReference = GetAudioClipAssetReference(guid);

            return GetAudioClipDownloadHandle(guid, assetReference)
                .WaitUntilCached(token);
        }

        public UniTask DownloadMesh(string guid, CancellationToken token)
        {
            AssetReferenceT<Mesh> assetReference = GetMeshAssetReference(guid);

            return GetMeshDownloadHandle(guid, assetReference)
                .WaitUntilCached(token);
        }

        public UniTask DownloadSprite(string guid, CancellationToken token)
        {
            AssetReferenceSprite assetReference = GetSpriteAssetReference(guid);

            return GetSpriteDownloadHandle(guid, assetReference)
                .WaitUntilCached(token);
        }

        public UniTask DownloadAtlasedSprite(string guid, string spriteName, CancellationToken token)
        {
            return DownloadSpriteAtlas(guid, token);
        }
        
        public UniTask DownloadSpriteAtlas(string guid, CancellationToken token)
        {
            AssetReferenceT<SpriteAtlas> assetReference = GetSpriteAtlasAssetReference(guid);

            return GetSpriteAtlasDownloadHandle(guid, assetReference)
                .WaitUntilCached(token);
        }

        public UniTask DownloadTexture2D(string guid, CancellationToken token)
        {
            AssetReferenceTexture2D assetReference = GetTexture2DAssetReference(guid);

            return GetTexture2DDownloadHandle(guid, assetReference)
                .WaitUntilCached(token);
        }

        public UniTask DownloadScriptableObject(string guid, CancellationToken token)
        {
            AssetReferenceT<ScriptableObject> assetReference = GetScriptableObjectAssetReference(guid);

            return GetScriptableObjectDownloadHandle(guid, assetReference)
                .WaitUntilCached(token);
        }
        
        public UniTask DownloadGameObject(string guid, CancellationToken token)
        {
            AssetReferenceGameObject assetReference = GetGameObjectAssetReference(guid);

            return GetGameObjectDownloadHandle(guid, assetReference)
                .WaitUntilCached(token);
        }
        
        private SceneDownloadHandle GetSceneDownloadHandle(string guid, AssetReference assetReference)
        {
            if (!_scenesDownloads.TryGetValue(assetReference, out SceneDownloadHandle download))
            {
                download = new SceneDownloadHandle(guid, assetReference);
                _scenesDownloads.Add(assetReference, download);
                _downloadQueue.Add(download);
            }

            return download;
        }
        
        private AudioClipDownloadHandle GetAudioClipDownloadHandle(string guid, AssetReferenceT<AudioClip> assetReference)
        {
            if (!_audioClipDownloads.TryGetValue(assetReference, out AudioClipDownloadHandle download))
            {
                download = new AudioClipDownloadHandle(guid, assetReference);
                _audioClipDownloads.Add(assetReference, download);
                _downloadQueue.Add(download);
            }

            return download;
        }
        
        private MeshDownloadHandle GetMeshDownloadHandle(string guid, AssetReferenceT<Mesh> assetReference)
        {
            if (!_meshDownloads.TryGetValue(assetReference, out MeshDownloadHandle download))
            {
                download = new MeshDownloadHandle(guid, assetReference);
                _meshDownloads.Add(assetReference, download);
                _downloadQueue.Add(download);
            }

            return download;
        }
        
        private SpriteDownloadHandle GetSpriteDownloadHandle(string guid, AssetReferenceSprite assetReference)
        {
            if (!_spriteDownloads.TryGetValue(assetReference, out SpriteDownloadHandle download))
            {
                download = new SpriteDownloadHandle(guid, assetReference);
                _spriteDownloads.Add(assetReference, download);
                _downloadQueue.Add(download);
            }

            return download;
        }
        
        private SpriteAtlasDownloadHandle GetSpriteAtlasDownloadHandle(string guid, AssetReferenceT<SpriteAtlas> assetReference)
        {
            if (!_spriteAtlasDownloads.TryGetValue(assetReference, out SpriteAtlasDownloadHandle download))
            {
                download = new SpriteAtlasDownloadHandle(guid, assetReference);
                _spriteAtlasDownloads.Add(assetReference, download);
                _downloadQueue.Add(download);
            }

            return download;
        }
        
        private Texture2DDownloadHandle GetTexture2DDownloadHandle(string guid, AssetReferenceTexture2D assetReference)
        {
            if (!_texture2DDownloads.TryGetValue(assetReference, out Texture2DDownloadHandle download))
            {
                download = new Texture2DDownloadHandle(guid, assetReference);
                _texture2DDownloads.Add(assetReference, download);
                _downloadQueue.Add(download);
            }

            return download;
        }
        
        private ScriptableObjectDownloadHandle GetScriptableObjectDownloadHandle(string guid, AssetReferenceT<ScriptableObject> assetReference)
        {
            if (!_scriptableObjectDownloads.TryGetValue(assetReference, out ScriptableObjectDownloadHandle download))
            {
                download = new ScriptableObjectDownloadHandle(guid, assetReference);
                _scriptableObjectDownloads.Add(assetReference, download);
                _downloadQueue.Add(download);
            }

            return download;
        }

        private GameObjectDownloadHandle GetGameObjectDownloadHandle(string guid, AssetReferenceGameObject assetReference)
        {
            if (!_gameObjectDownloads.TryGetValue(assetReference, out GameObjectDownloadHandle download))
            {
                download = new GameObjectDownloadHandle(guid, assetReference);
                _gameObjectDownloads.Add(assetReference, download);
                _downloadQueue.Add(download);
            }

            return download;
        }
        
        private async UniTaskVoid DownloadInBackground(CancellationToken token)
        {
            await UniTask.Yield(PlayerLoopTiming.LastUpdate, token);
            
            while (!token.IsCancellationRequested)
            {
                if (_downloadQueue.Count > 0)
                {
                    // Cache list of processing downloads
                    List<IDownloadHandle> handles = new List<IDownloadHandle>(_downloadQueue);
                    _downloadQueue.Clear();
                    
                    await DownloadInBackground(handles, token);
                }
                else
                {
                    await UniTask.NextFrame(PlayerLoopTiming.LastUpdate, token)
                        .SuppressCancellationThrow();   
                }
            }
        }

        private async UniTask DownloadInBackground(IList<IDownloadHandle> handles, CancellationToken token) 
        {
            // 1) Build key → locations map and prune handles
            Dictionary<object, List<IResourceLocation>> keyToLocations = new Dictionary<object, List<IResourceLocation>>(handles.Count);
            foreach (IDownloadHandle? h in handles.ToList())
            {
                List<IResourceLocation>? locations = null;
                foreach (IResourceLocator? loc in _locators)
                {
                    if (loc.Locate(h.AssetReference.RuntimeKey, h.AssetType, out IList<IResourceLocation>? found) && found != null && found.Count > 0)
                    {
                        locations = found as List<IResourceLocation> ?? found.ToList();
                        break;
                    }
                }

                if (locations == null)
                {
                    handles.Remove(h);
                    h.OnDownloadComplete(ResultV<IReadOnlyList<IResourceLocation>>.FromError("{0} has no addressable location.", h));
                    continue;
                }
                keyToLocations[h.AssetReference.RuntimeKey] = locations;
            }

            if (handles.Count == 0)
            {
                return;
            }

            // 2) Union of all locations for a single download op
            List<IResourceLocation> unionLocations = keyToLocations.Values
                .SelectMany(x => x)
                .Distinct(ResourceLocationComparer.Instance)
                .ToList();
            
            // 3) If nothing to download, early out.
            if (unionLocations.Count == 0)
            {
                foreach (IDownloadHandle? h in handles)
                {
                    h.OnDownloadComplete(ResultV<IReadOnlyList<IResourceLocation>>.FromError("No resource locations found for requested assets."));
                }

                return;
            }

            // 4) Per-handle sizes (parallel) with explicit handle management
            // List<AsyncOperationHandle<long>> sizeHandles = new List<AsyncOperationHandle<long>>(handles.Count);
            // List<UniTask<long>> sizeTasks = new List<UniTask<long>>(handles.Count);
            //
            // foreach (IDownloadHandle h in handles)
            // {
            //     AsyncOperationHandle<long> sh = AddressablesImpl.GetDownloadSizeAsync(keyToLocations[h.AssetReference.RuntimeKey]);
            //     sizeHandles.Add(sh);
            //     sizeTasks.Add(sh.ToUniTask(cancellationToken: token));
            // }
            //
            // long[] sizeResults = Array.Empty<long>();
            // try
            // {
            //     sizeResults = await UniTask.WhenAll(sizeTasks);
            // }
            // finally
            // {
            //     // Always release size handles to avoid leaks regardless of cancellation/success
            //     foreach (AsyncOperationHandle<long> sh in sizeHandles)
            //     {
            //         SafeReleaseSize(sh);
            //     }
            // }
            //
            // for (int i = 0; i < handles.Count && i < sizeResults.Length; i++)
            // {
            //     handles[i].Size = sizeResults[i];
            // }

            // 5) Download dependencies in batches to avoid mega-unions
            for (int i = 0; i < unionLocations.Count; i += kMaxLocationsPerBatch)
            {
                List<IResourceLocation> batch = unionLocations
                    .Skip(i)
                    .Take(kMaxLocationsPerBatch)
                    .ToList();
                await DownloadBatch(batch, token);
            }

            // Let the cache settle once after all batches
            await UniTask.DelayFrame(2, PlayerLoopTiming.LastPostLateUpdate, token);

            // 6) Complete each handle (choose first loc as representative)
            foreach (IDownloadHandle? h in handles)
            {
                IReadOnlyList<IResourceLocation> locations = keyToLocations[h.AssetReference.RuntimeKey];
                h.OnDownloadComplete(ResultV<IReadOnlyList<IResourceLocation>>.FromResult(locations));
            }
        }
        
        private async UniTask DownloadBatch(IList<IResourceLocation> batch, CancellationToken token)
        {
            while (true)
            {
                Exception? ex = null;
                AsyncOperationHandle downloadOperation = AddressablesImpl.DownloadDependenciesAsync(batch, false);
                try
                {
                    await downloadOperation.ToUniTask(cancellationToken: token);

                    if (downloadOperation.IsValid() && downloadOperation.Status == AsyncOperationStatus.Succeeded)
                    {
                        return;
                    }

                    Log.DebugWarning("Downloading ['{0}'] batch was not successful. Check network.",
                        string.Join(',', batch.Select(l => l.InternalId).ToArray()));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ex = PreferOperationException(downloadOperation, e);
                }
                finally
                {
                    SafeReleaseDownload(downloadOperation);
                }

                if (ex == null)
                {
                    return;
                }

                Log.DebugWarning("Download batch failure classified. Type={0}, Msg={1}", ex.GetType().Name, ex.Message);

                if (IsConfigError(ex))
                {
                    throw ex;
                }

                if (IsTransient(ex))
                {
                    Log.DebugException(ex, "Downloading ['{0}'] batch was not successful. Check network.",
                        string.Join(',', batch.Select(l => l.InternalId).ToArray()));
                 
                    await _connectivityService.WaitForConnection(token);
                    continue;
                }

                if (LooksLikeCorruption(ex))
                {
                    Log.DebugException(ex, "Downloading ['{0}'] batch was not successful. One or more files were corrupt.",
                        string.Join(',', batch.Select(l => l.InternalId).ToArray()));
                    
                    // Clear only the dependencies of this key
                    AsyncOperationHandle<bool> clearOperation = AddressablesImpl.ClearDependencyCacheAsync(batch, autoReleaseHandle: true);
                    await clearOperation.ToUniTask();
                    await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, token);
                    continue;
                }

                // Unclassified, should not happen
                throw ex;
            }
        }
        
        private static Exception PreferOperationException(AsyncOperationHandle op, Exception fallback)
        {
            try
            {
                if (op.IsValid() && op.OperationException != null)
                    return UnwrapException(op.OperationException);
            }
            catch { /* ignore */ }
            return UnwrapException(fallback);
        }

        private static Exception UnwrapException(Exception ex)
        {
            if (ex is AggregateException ae)
            {
                var first = ae.Flatten().InnerExceptions.FirstOrDefault();
                return first != null ? UnwrapException(first) : ex;
            }
            return ex.InnerException != null ? UnwrapException(ex.InnerException) : ex;
        }

        private static bool ContainsTransientMessage(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            return s.IndexOf("temporar", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   s.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   s.IndexOf("connection", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   s.IndexOf("cannot connect", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   s.IndexOf("aborted", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   s.IndexOf("request error", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   s.IndexOf("tls", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   s.IndexOf("ssl", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsTransient(Exception ex)
        {
            // Unwrap has already been applied by PreferOperationException, but be defensive
            var msg = ex.Message ?? string.Empty;
            if (ContainsTransientMessage(msg)) return true;

            // Unity Addressables often wraps provider failures in OperationException
            if (ex is OperationException && msg.IndexOf("dependent operation failed", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Some UnityWebRequest details only appear in ToString()
            var full = ex.ToString();
            return ContainsTransientMessage(full);
        }

        private static bool LooksLikeCorruption(Exception ex)
        {
            var msg = ex.Message ?? string.Empty;
            if (msg.IndexOf("crc", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("decompress", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("corrupt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("header", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("failed to read", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Fallback: sometimes the indicative text is only present in the full string
            var full = ex.ToString();
            return full.IndexOf("crc", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   full.IndexOf("decompress", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   full.IndexOf("corrupt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   full.IndexOf("header", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   full.IndexOf("failed to read", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsConfigError(Exception ex)
        {
            var msg = ex.Message ?? string.Empty;
            if (msg.IndexOf("InvalidKey", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (msg.IndexOf("Unable to load", StringComparison.OrdinalIgnoreCase) >= 0 && msg.IndexOf("location", StringComparison.OrdinalIgnoreCase) >= 0) ||
                msg.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Some provider/type mismatch messages show only in full string
            var full = ex.ToString();
            return full.IndexOf("InvalidKey", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   (full.IndexOf("Unable to load", StringComparison.OrdinalIgnoreCase) >= 0 && full.IndexOf("location", StringComparison.OrdinalIgnoreCase) >= 0) ||
                   full.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}