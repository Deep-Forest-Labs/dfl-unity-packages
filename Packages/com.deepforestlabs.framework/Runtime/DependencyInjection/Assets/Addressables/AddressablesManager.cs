#nullable enable

using System;
using System.Collections;
using ZLinq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Services.Connectivity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using AddressablesImpl = UnityEngine.AddressableAssets.Addressables;

namespace DeepForestLabs.Assets.Addressables
{
    internal sealed partial class AddressablesManager : IInitializable, IAsyncDisposable
    {
        private static readonly string DEBUG_LOG_PREFIX = ZString.Format(Application.isEditor ? "<color='#64FFCD'>[{0}]</color>" : "[{0}]", nameof(AddressablesManager));
        private const int kMaxLocationsPerBatch = 128;

        private static Texture2D _defaultTexture = new(1, 1);
        private static Sprite _defaultSprite = default!;

        private readonly Container _container;
        private readonly IConnectivityService _connectivityService;
        private readonly CancellationToken _scope;

        private readonly List<IResourceLocator> _locators = new();

        private readonly Dictionary<string, AssetReference> _assetReferences = new();
        private readonly List<IDownloadHandle> _downloadQueue = new();

        private readonly Dictionary<AssetReference, SceneDownloadHandle> _scenesDownloads =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<IResourceLocation, SceneLoadHandle> _scenesLoadHandles =
            new(ResourceLocationRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReference, SceneAssetHandle> _sceneAssetHandles =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceT<AudioClip>, AudioClipDownloadHandle> _audioClipDownloads =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<IResourceLocation, AudioClipLoadHandle> _audioClipLoadHandles =
            new(ResourceLocationRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceT<AudioClip>, AudioClipAssetHandle> _audioClipAssetHandles =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceT<Mesh>, MeshDownloadHandle> _meshDownloads =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<IResourceLocation, MeshLoadHandle> _meshLoadHandles =
            new(ResourceLocationRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceT<Mesh>, MeshAssetHandle> _meshAssetHandles =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceSprite, SpriteDownloadHandle> _spriteDownloads =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<IResourceLocation, SpriteLoadHandle> _spriteLoadHandles =
            new(ResourceLocationRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceSprite, SpriteAssetHandle> _spriteAssetHandles =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceT<SpriteAtlas>, SpriteAtlasDownloadHandle> _spriteAtlasDownloads =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<IResourceLocation, SpriteAtlasLoadHandle> _spriteAtlasLoadHandles =
            new(ResourceLocationRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceT<SpriteAtlas>, SpriteAtlasAssetHandle> _spriteAtlasAssetHandles =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceTexture2D, Texture2DDownloadHandle> _texture2DDownloads =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<IResourceLocation, Texture2DLoadHandle> _texture2DLoadHandles =
            new(ResourceLocationRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceTexture2D, Texture2DAssetHandle> _texture2DAssetHandles =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceT<ScriptableObject>, ScriptableObjectDownloadHandle>
            _scriptableObjectDownloads = new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<IResourceLocation, ScriptableObjectLoadHandle> _scriptableObjectLoadHandles =
            new(ResourceLocationRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceT<ScriptableObject>, ScriptableObjectAssetHandle>
            _scriptableObjectAssetHandles = new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceGameObject, GameObjectDownloadHandle> _gameObjectDownloads =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<IResourceLocation, GameObjectLoadHandle> _gameObjectLoadHandles =
            new(ResourceLocationRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceGameObject, GameObjectAssetHandle> _gameObjectAssetHandles =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceGameObject, GameObjectManager> _gameObjectManagers =
            new(AssetReferenceRuntimeKeyComparer.Instance);

        private readonly Dictionary<AssetReferenceGameObject, Dictionary<Type, UniTaskCompletionSource<object>>>
            _typedGameObjectManagers = new();

#if UNITY_EDITOR
        internal static event Action<Scene>? onLoadedScene;
        internal static event Action<GameObject>? onLoadedPrefab;
        internal static Action<GameObject>? onInstantiated;
#endif

        public static Texture2D DefaultTexture => _defaultTexture;

        public static Sprite DefaultSprite => _defaultSprite;

        public CancellationToken Scope => _scope;

        public AddressablesManager(Container container, CancellationToken scope)
        {
            _scope = scope;
            _container = container;
            _connectivityService = container.Get<IConnectivityService>();
        }

        public async UniTask Initialize(CancellationToken token)
        {
            ResourceManager.ExceptionHandler += HandleResourceManagerException;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnEditorPlayModeStateChanged;
#endif
#if !RELEASE_BUILD
            AddressablesImpl.WebRequestOverride -= WebRequestOverride;
            AddressablesImpl.WebRequestOverride += WebRequestOverride;
#endif

            if (_defaultTexture == null)
            {
                _defaultTexture = new Texture2D(1, 1);
                _defaultTexture.SetPixel(0, 0, Color.clear);
                _defaultTexture.Apply();
                _defaultSprite = Sprite.Create(_defaultTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            }
            
            string catalogExt = BuildSettings.Instance.Addressables.EnableJsonCatalog ? "json" : "bin";
            
            #if UNITY_EDITOR
            if (BuildSettings.Instance.Addressables.ActivePlayModeIndex == BuilderIndex.RemoteAssetBundlePlayMode)
            #endif
            {
                AddressablesImpl.ResourceManager.InternalIdTransformFunc = RemoteAssetBundlePlayModeInternalIdTransformFunc;
            }

            #if UNITY_EDITOR
            // Editor: if using Asset Database (fastest), skip external catalog loads
            if (BuildSettings.Instance.Addressables.ActivePlayModeIndex == BuilderIndex.AssetDatabasePlayMode)
            {
                if (AddressablesImpl.ResourceLocators.AsValueEnumerable().Any())
                {
                    _locators.Clear();
                    _locators.AddRange(AddressablesImpl.ResourceLocators);
                }
                else
                {
                    AsyncOperationHandle<IResourceLocator> initOperation = AddressablesImpl.InitializeAsync();
                    try
                    {
                        await initOperation.ToUniTask(cancellationToken: token);
                        _locators.Clear();
                        _locators.AddRange(AddressablesImpl.ResourceLocators);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        throw new GameException("[AddressablesManager] Failed to initialize.", e);
                    }
                    finally
                    {
                        SafeReleaseInitialize(initOperation);
                    }
                }
            }
            else
            #endif
                
            // If initial release, use local catalog
            if (Variables.AssetId == AddressablesBuildSettings.RELEASE_ASSET_ID)
            {
                string localCatalogLoadPath =
                    ZString.Format("{0}/catalog.{1}", Variables.LocalLoadPath, catalogExt);
                
                Log.Debug("[AddressablesManager] Loading release catalog '{0}'.", localCatalogLoadPath);
                AsyncOperationHandle<IResourceLocator> catalogContentHandle = AddressablesImpl
                    .LoadContentCatalogAsync(localCatalogLoadPath, false);
                try
                {
                    await catalogContentHandle.ToUniTask(cancellationToken: token);
                    if (catalogContentHandle.IsValid() &&
                        catalogContentHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Log.Debug("[AddressablesManager] Loaded release catalog '{0}'.", localCatalogLoadPath);
                    }
                    else
                    {
                        throw GameException.FromFormat(
                            "[AddressablesManager] Loading release catalog '{0}' returned status '{1}'.",
                            localCatalogLoadPath, catalogContentHandle.Status);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw GameException.FromFormat(e, "[AddressablesManager] Failed to load release catalog '{0}'.",
                        localCatalogLoadPath);
                }
                finally
                {
                    SafeReleaseCheckForCatalogUpdates(catalogContentHandle);
                }
            }
            // Otherwise, load from remote content update catalog 
            else
            {
                string remoteCatalogLoadPath =
                    ZString.Format("{0}/catalog_{1}.{2}", Variables.RemoteLoadPath, Variables.AssetId, catalogExt);
                
                while (true)
                {
                    Log.Debug("[AddressablesManager] Loading content update catalog '{0}'.", remoteCatalogLoadPath);
                    AsyncOperationHandle<IResourceLocator> catalogContentHandle = AddressablesImpl
                        .LoadContentCatalogAsync(remoteCatalogLoadPath, false);
                    try
                    {
                        await catalogContentHandle.ToUniTask(cancellationToken: token);
                        if (catalogContentHandle.IsValid() &&
                            catalogContentHandle.Status == AsyncOperationStatus.Succeeded)
                        {
                            Log.Debug("[AddressablesManager] Loaded content update catalog '{0}'.", remoteCatalogLoadPath);
                            break;
                        }
                        else
                        {
                            Log.DebugWarning(
                                "[AddressablesManager] Loading content update catalog '{0}' returned status '{1}'.  Checking connection.",
                                remoteCatalogLoadPath, catalogContentHandle.Status);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        throw GameException.FromFormat(e, "[AddressablesManager] Failed to load content update catalog '{0}'.",
                            remoteCatalogLoadPath);
                    }
                    finally
                    {
                        SafeReleaseCheckForCatalogUpdates(catalogContentHandle);
                    }

                    await _connectivityService.WaitForConnection(token);
                    await UniTask.NextFrame(token);
                }
            }

            _locators.Clear();
            foreach (var loc in AddressablesImpl.ResourceLocators.AsValueEnumerable().ToList())
            {
                if (loc.GetType().Name.Contains("SubObjectResourceLocator"))
                    AddressablesImpl.RemoveResourceLocator(loc);
            }
            _locators.AddRange(AddressablesImpl.ResourceLocators);

            DownloadInBackground(token).Forget();
        }

        public async UniTask DisposeAsync()
        {
            AddressablesImpl.ResourceManager.InternalIdTransformFunc = null;
            ResourceManager.ExceptionHandler -= HandleResourceManagerException;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnEditorPlayModeStateChanged;
#endif

            if (Application.isEditor)
            {
                GameObject.DestroyImmediate(_defaultSprite);
                GameObject.DestroyImmediate(_defaultTexture);
            }
            else
            {
                GameObject.Destroy(_defaultSprite);
                GameObject.Destroy(_defaultTexture);
            }

            _downloadQueue.Clear();
            _scenesDownloads.Clear();
            _audioClipDownloads.Clear();
            _meshDownloads.Clear();
            _spriteDownloads.Clear();
            _spriteAtlasDownloads.Clear();
            _texture2DDownloads.Clear();
            _scriptableObjectDownloads.Clear();
            _gameObjectDownloads.Clear();

            List<UniTask> tasks = new List<UniTask>();
            foreach (SceneAssetHandle? handle in _sceneAssetHandles.Values)
            {
                tasks.Add(SafeReleaseScene(handle.OperationHandle));
            }

            _sceneAssetHandles.Clear();
            _scenesLoadHandles.Clear();
            await UniTask.WhenAll(tasks);

            foreach (AudioClipAssetHandle handle in _audioClipAssetHandles.Values)
            {
                SafeReleaseAudioClip(handle.OperationHandle);
            }

            _audioClipAssetHandles.Clear();
            _audioClipLoadHandles.Clear();

            foreach (MeshAssetHandle handle in _meshAssetHandles.Values)
            {
                SafeReleaseMesh(handle.OperationHandle);
            }

            _meshAssetHandles.Clear();
            _meshLoadHandles.Clear();

            foreach (SpriteAssetHandle handle in _spriteAssetHandles.Values)
            {
                SafeReleaseSprite(handle.OperationHandle);
            }

            _spriteAssetHandles.Clear();
            _spriteLoadHandles.Clear();

            foreach (Texture2DAssetHandle handle in _texture2DAssetHandles.Values)
            {
                SafeReleaseTexture2D(handle.OperationHandle);
            }

            _texture2DAssetHandles.Clear();
            _texture2DLoadHandles.Clear();

            foreach (ScriptableObjectAssetHandle handle in _scriptableObjectAssetHandles.Values)
            {
                SafeReleaseScriptableObject(handle.OperationHandle);
            }

            _scriptableObjectAssetHandles.Clear();
            _scriptableObjectLoadHandles.Clear();

            foreach (GameObjectAssetHandle handle in _gameObjectAssetHandles.Values)
            {
                SafeReleaseGameObject(handle.OperationHandle);
            }

            _gameObjectAssetHandles.Clear();
            _gameObjectLoadHandles.Clear();

            foreach (GameObjectManager? manager in _gameObjectManagers.Values)
            {
                manager.Dispose();
            }

            _gameObjectManagers.Clear();
            _typedGameObjectManagers.Clear();

#if UNITY_EDITOR
            bool isAssetDatabaseMode = BuildSettings.Instance.Addressables.ActivePlayModeIndex == BuilderIndex.AssetDatabasePlayMode;
            if (!isAssetDatabaseMode)
#endif
            {
                ForceReleaseAllAsyncOperationHandles();

                foreach (IResourceLocator? locator in _locators)
                {
                    AddressablesImpl.RemoveResourceLocator(locator);
                }
            }
            _locators.Clear();
        }

        private AssetReference GetSceneAssetReference(string address)
        {
            if (!_assetReferences.TryGetValue(address, out AssetReference assetReference))
            {
                assetReference = new AssetReference(address);
                _assetReferences.Add(address, assetReference);
            }

            return assetReference;
        }

        private AssetReferenceT<AudioClip> GetAudioClipAssetReference(string address)
        {
            AssetReferenceT<AudioClip> assetReference;
            if (!_assetReferences.TryGetValue(address, out AssetReference? uncasted))
            {
                assetReference = new AssetReferenceT<AudioClip>(address);
                _assetReferences.Add(address, assetReference);
            }
            else
            {
                assetReference = (uncasted as AssetReferenceT<AudioClip>)!;
            }

            return assetReference;
        }

        private AssetReferenceT<Mesh> GetMeshAssetReference(string address)
        {
            AssetReferenceT<Mesh> assetReference;
            if (!_assetReferences.TryGetValue(address, out AssetReference? uncasted))
            {
                assetReference = new AssetReferenceT<Mesh>(address);
                _assetReferences.Add(address, assetReference);
            }
            else
            {
                assetReference = (uncasted as AssetReferenceT<Mesh>)!;
            }

            return assetReference;
        }

        private AssetReferenceSprite GetSpriteAssetReference(string address)
        {
            AssetReferenceSprite assetReference;
            if (!_assetReferences.TryGetValue(address, out AssetReference? uncasted))
            {
                assetReference = new AssetReferenceSprite(address);
                _assetReferences.Add(address, assetReference);
            }
            else
            {
                assetReference = (uncasted as AssetReferenceSprite)!;
            }

            return assetReference;
        }

        private AssetReferenceT<SpriteAtlas> GetSpriteAtlasAssetReference(string address)
        {
            AssetReferenceT<SpriteAtlas> assetReference;
            if (!_assetReferences.TryGetValue(address, out AssetReference? uncasted))
            {
                assetReference = new AssetReferenceT<SpriteAtlas>(address);
                _assetReferences.Add(address, assetReference);
            }
            else
            {
                assetReference = (uncasted as AssetReferenceT<SpriteAtlas>)!;
            }

            return assetReference;
        }

        private AssetReferenceTexture2D GetTexture2DAssetReference(string address)
        {
            AssetReferenceTexture2D assetReference;
            if (!_assetReferences.TryGetValue(address, out AssetReference? uncasted))
            {
                assetReference = new AssetReferenceTexture2D(address);
                _assetReferences.Add(address, assetReference);
            }
            else
            {
                assetReference = (uncasted as AssetReferenceTexture2D)!;
            }

            return assetReference;
        }

        private AssetReferenceT<ScriptableObject> GetScriptableObjectAssetReference(string address)
        {
            AssetReferenceT<ScriptableObject> assetReference;
            if (!_assetReferences.TryGetValue(address, out AssetReference? uncasted))
            {
                assetReference = new AssetReferenceT<ScriptableObject>(address);
                _assetReferences.Add(address, assetReference);
            }
            else
            {
                assetReference = (uncasted as AssetReferenceT<ScriptableObject>)!;
            }

            return assetReference;
        }

        private AssetReferenceGameObject GetGameObjectAssetReference(string address)
        {
            AssetReferenceGameObject assetReference;
            if (!_assetReferences.TryGetValue(address, out AssetReference? uncasted))
            {
                assetReference = new AssetReferenceGameObject(address);
                _assetReferences.Add(address, assetReference);
            }
            else
            {
                assetReference = (uncasted as AssetReferenceGameObject)!;
            }

            return assetReference;
        }

        private void HandleResourceManagerException(AsyncOperationHandle op, Exception e)
        {
            Log.DevException(e);
        }
        
#if UNITY_EDITOR
        private void OnEditorPlayModeStateChanged(UnityEditor.PlayModeStateChange change)
        {
            // This prevents NRE on scene loads during dispose due to AddressablesImpl re-init on editor mode changes. 
            // See AddressablesImpl.m_Addressables
            if (UnityEditor.EditorSettings.enterPlayModeOptionsEnabled &&
                (change == UnityEditor.PlayModeStateChange.EnteredEditMode ||
                 change == UnityEditor.PlayModeStateChange.ExitingPlayMode))
            {
                _scenesDownloads.Clear();
                _audioClipDownloads.Clear();
                _meshDownloads.Clear();
                _spriteDownloads.Clear();
                _spriteAtlasDownloads.Clear();
                _texture2DDownloads.Clear();
                _scriptableObjectDownloads.Clear();
                _gameObjectDownloads.Clear();
                _sceneAssetHandles.Clear();
                _scenesLoadHandles.Clear();
                _audioClipAssetHandles.Clear();
                _audioClipLoadHandles.Clear();
                _meshAssetHandles.Clear();
                _meshLoadHandles.Clear();
                _spriteAssetHandles.Clear();
                _spriteLoadHandles.Clear();
                _texture2DAssetHandles.Clear();
                _texture2DLoadHandles.Clear();
                _scriptableObjectAssetHandles.Clear();
                _scriptableObjectLoadHandles.Clear();
                _gameObjectAssetHandles.Clear();
                _gameObjectLoadHandles.Clear();
                _gameObjectManagers.Clear();
                _typedGameObjectManagers.Clear();
            }
        }
#endif
        
#if !RELEASE_BUILD
        private void WebRequestOverride(UnityWebRequest request)
        {
            if (BuildSettings.Instance.ContainerLogFlag.HasFlag(ContainerLogFlag.Addressables))
            {
                if ((request.url.StartsWith("http", StringComparison.OrdinalIgnoreCase) || request.url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    && request.url.IndexOf(".bundle", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    MonitorBundleDownload(request, _scope).Forget();
                }
            }
            
#if ADDRESSABLES_CHAOS_MONKEY_CORRUPTION
            if (request.url.Contains(".bundle") && UnityEngine.Random.value < 0.1f)
            {
                string projectRoot = System.IO.Directory.GetParent(Application.dataPath)!.FullName;
                string full = System.IO.Path.GetFullPath(System.IO.Path.Combine(projectRoot, "Packages/com.deepforestlabs.framework/Runtime/DependencyInjection/Assets/Addressables/SimulatedCorruptAssetBundle.bundle"));
                request.url = new Uri(full).AbsoluteUri;
            }
#endif
        }

        private async UniTaskVoid MonitorBundleDownload(UnityWebRequest request, CancellationToken token)
        {
            // Correlate by URL (stable for Addressables bundle fetches); include a short name for readability.
            string url = request.url;
            try
            {
                int lastPct = -1;
                double lastEmit = 0; // seconds
                Stopwatch sw;

                while (true)
                {
                    if (request.downloadedBytes > 0)
                    {
                        sw = Stopwatch.StartNew();
                        Log.Debug("{0} - Download Started '{1}'", DEBUG_LOG_PREFIX, url);
                        break;
                    }
                    if (request.isDone && request.downloadedBytes == 0)
                    {
                        // Cached
                        Log.Debug("{0} - Is Cached '{1}'", DEBUG_LOG_PREFIX, url);
                        return;
                    }
                    
                    await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, token);
                }

                while (!request.isDone)
                {
                    // Throttle to either 10% increments or >= 0.5s since last emit
                    int pct = (int)(Mathf.Clamp01(request.downloadProgress) * 100); // downloadProgress is 0..1
                    double now = sw.Elapsed.TotalSeconds;
                    if ((pct >= 0 && pct != lastPct && (pct % 10 == 0)) || now - lastEmit >= 0.5)
                    {
                        lastPct = pct;
                        lastEmit = now;
                        Log.Debug("{0} - Downloading {1,3}% '{2}'", DEBUG_LOG_PREFIX, pct, url);
                    }

                    await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, token);
                }

                sw.Stop();

                // Pull final stats
                long downloaded = (long)request.downloadedBytes;
                // Content-Length is available only after headers
                string contentLen = request.GetResponseHeader("Content-Length");
                long.TryParse(contentLen, out long expectedBytes);

                bool ok = request.result == UnityWebRequest.Result.Success;
                bool httpError = request.result == UnityWebRequest.Result.ProtocolError;
                long code = request.responseCode;

                if (ok)
                {
                    double seconds = Math.Max(sw.Elapsed.TotalSeconds, 0.0001);
                    double mb = downloaded / (1024.0 * 1024.0);
                    double mbps = mb / seconds;
                    if (expectedBytes > 0 && downloaded == 0) downloaded = expectedBytes; // fallback

                    Log.Debug("{0} - Download Complete '{1}' size={2:0.00} MB dur={3:0.00}s avg={4:0.00} MB/s", 
                              DEBUG_LOG_PREFIX, url, downloaded / (1024.0 * 1024.0), seconds, mbps);
                }
                else
                {
                    string err = request.error;
                    Log.DebugError("{0} - Download Failed '{1}' code={2} httpError={3} error='{4}'", 
                                   DEBUG_LOG_PREFIX, url, code, httpError, err);
                }
            }
            catch (OperationCanceledException)
            {
                // Silent: scope shutdown or caller canceled
            }
            catch (Exception)
            {
                // ignore
            }
        }
#endif
        
        // Force any 'local' AA bundle internal IDs to resolve to our RemoteLoadPath (CDN).
        // Addressables 2.x tightened how InternalId is generated for BuiltIn/Local groups during Editor play.
        // When using our RemoteAssetBundlePlayMode, those still point to Library/com.unity.addressables/... by default. (It evaluates Variables.RemoteLoadPath to be the jenkins Volume where they were built)
        // This transform rewrites any paths rooted at build time evaluated Variables.RemoteLoadPath to runtime evaluated Variables.RemoteLoadPath
        private static string RemoteAssetBundlePlayModeInternalIdTransformFunc(IResourceLocation loc)
        {
            try
            {
                string id = loc.InternalId;
                if (string.IsNullOrEmpty(id)) return id;
                if (!id.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase) &&
                    !id.EndsWith("settings.json", StringComparison.OrdinalIgnoreCase) &&
                    !id.EndsWith(".hash", StringComparison.OrdinalIgnoreCase) &&
                    !id.StartsWith("catalog", StringComparison.OrdinalIgnoreCase))
                {
                    return id;
                }

                // Do not touch properly napped entries
                if (id.StartsWith(Variables.LocalLoadPath, StringComparison.OrdinalIgnoreCase) || id.StartsWith(Variables.RemoteLoadPath, StringComparison.OrdinalIgnoreCase))
                {
                    return id;
                }

                // Normalize separators to compare prefixes reliably
                string normId = id.Replace('\\', '/');
                string buildRoot = AddressablesImpl.BuildPath.Replace('\\', '/');
                string runtimeRoot = AddressablesImpl.RuntimePath.Replace('\\', '/');

                if (!string.IsNullOrEmpty(buildRoot) && normId.StartsWith(buildRoot, StringComparison.OrdinalIgnoreCase))
                {
                    string tail = normId.Substring(buildRoot.Length).TrimStart('/');
                    return ZString.Concat(Variables.RemoteLoadPath.TrimEnd('/'), "/", tail);
                }

                if (!string.IsNullOrEmpty(runtimeRoot) && normId.StartsWith(runtimeRoot, StringComparison.OrdinalIgnoreCase))
                {
                    string tail = normId.Substring(runtimeRoot.Length).TrimStart('/');
                    return ZString.Concat(Variables.RemoteLoadPath.TrimEnd('/'), "/", tail);
                }

                // Fallback: Jenkins-built catalogs may serialize absolute machine paths (e.g., /Volumes/... or C:\...).
                // If we see an absolute path or file:// URI, redirect to CDN using just the filename (bundles are flat under RemoteLoadPath).
                string abs = normId;
                if (abs.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    abs = abs.Substring("file://".Length);
                    abs = abs.Replace('\\', '/');
                }

                bool looksUnixAbs = abs.StartsWith("/");
                bool looksWinAbs = abs.Length > 2 && char.IsLetter(abs[0]) && abs[1] == ':' && abs[2] == '/';
                if (looksUnixAbs || looksWinAbs)
                {
                    int slash = abs.LastIndexOf('/');
                    string file = slash >= 0 ? abs.Substring(slash + 1) : abs;

                    return ZString.Concat(Variables.RemoteLoadPath.TrimEnd('/'), "/", file);
                }

                return id;
            }
            catch
            {
                return loc.InternalId;
            }
        }

        private static void ForceReleaseAllAsyncOperationHandles()
        {
            // Workaround for problems:
            // https://forum.unity.com/threads/how-to-unload-everything-currently-loaded-by-addressables.1121998/
            List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();

            Type resourceManagerType = AddressablesImpl.ResourceManager.GetType();
            FieldInfo? dictionaryMember = resourceManagerType.GetField("m_AssetOperationCache",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Log.Assert(dictionaryMember != null, nameof(dictionaryMember) + " != null");
            IDictionary? dictionary = dictionaryMember.GetValue(AddressablesImpl.ResourceManager) as IDictionary;
            Log.Assert(dictionary != null, nameof(dictionary) + " != null");

            foreach (object? asyncOperationInterface in dictionary.Values)
            {
                if (asyncOperationInterface == null)
                {
                    continue;
                }

                object? handle = typeof(AsyncOperationHandle).InvokeMember(nameof(AsyncOperationHandle),
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance,
                    null, null, new[] { asyncOperationInterface });

                handles.Add((AsyncOperationHandle)handle);
            }

            foreach (AsyncOperationHandle handle in handles)
            {
                if (!handle.IsDone)
                {
                    Log.DevError("AsyncOperationHandle '{0}' not completed yet. Releasing anyway!", handle.DebugName);
                }

                while (handle.IsValid())
                {
                    try
                    {
                        if (!handle.IsValid())
                        {
                            return;
                        }

                        AddressablesImpl.ReleaseInstance(handle);
                    }
                    catch (Exception e)
                    {
                        Log.DebugException(e, "Failed to release {0}.", handle.DebugName);
                    }
                }
            }
        }
    }
}