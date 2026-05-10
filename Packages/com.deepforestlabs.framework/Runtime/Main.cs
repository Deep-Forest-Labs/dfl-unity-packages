#nullable enable
using System;
using System.Threading;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using DeepForestLabs.States.Main;
using DeepForestLabs.Utils;
using DeepForestLabs.Services;
using DeepForestLabs.Settings;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeepForestLabs
{
    [UsedImplicitly]
    public sealed class Main
    {
        public const string MAIN_ARGS_RESOURCES_PATH = "MainArgs";
        public const string MAIN_CONTAINER_NAME = "Main";
        private const string APPLICATION_SHUTTING_DOWN = "Application Shutting Down.";

        public static DateTime StartTime { get; internal set; }
        public static uint UnhandledExceptionsSinceLaunch { get; internal set; }

        public static event Action? OnExit;
        public static event Action? OnRestart;
        public static event Action? OnResetLoginData;
        public static Func<IDiCollection, object, string>? ToStringOverride;

        private static CancellationTokenSource? _runScope = null;
        private static IContainer? _container = null;
        private static MainState? _mainState = null;
        private static IErrorReporter? _errorReporter = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void BeforeSplashScreen()
        {
#if UNITY_EDITOR
            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                return;
            }
            GL.Clear(true, true, Color.black);
            Application.runInBackground = true;
            UnityEditor.EditorApplication.playModeStateChanged += change =>
            {
                if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    EditorExit();
                }
            };
#endif
            StartTime = DateTime.Now;

            // Log some build info
            BuildSettings build = BuildSettings.Instance;
            Log.Info("Client {0} on {1} built with Unity {2}", build.FullVersionNumber, ApplicationUtil.PlatformStr,
                Application.unityVersion);
            Log.Debug("Screen: {0}x{1} - UniqueId: {2} - Environment: {3} - Build: {4}",
                Screen.height, Screen.width, build.Addressables.UniqueId, build.Environment.Name,
                build.BuildNumber);
            Log.Debug("[{0}]\n{1}", nameof(BuildSettings), JsonUtility.ToJson(build, true));

            // Setup unity engine statics.
            Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;
            Application.lowMemory += () => Log.Warning("OnLowMemory");
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            Screen.orientation = build.Orientation;
            Screen.autorotateToPortrait = build.Orientation == ScreenOrientation.AutoRotation;
            Screen.autorotateToPortraitUpsideDown = build.Orientation == ScreenOrientation.AutoRotation;
            SettingsBundle.AppVersion = build.ShortVersion;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = build.TargetFps;
            QualitySettings.vSyncCount = build.VSyncCount;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad()
        {
#if UNITY_EDITOR
            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                return;
            }
#endif
            
            if (Application.isPlaying)
            {
                MainArgs? args = Resources.Load(MAIN_ARGS_RESOURCES_PATH) as MainArgs;
                Log.Assert(args != null, "args != null, Args not found at '{0}'", MAIN_ARGS_RESOURCES_PATH);

                _runScope?.Cancel();
                _runScope?.Dispose();
                _runScope = CancellationTokenSource.CreateLinkedTokenSource(Application.exitCancellationToken);
                Run(args, _runScope.Token).Forget();
            }
        }

        private static async UniTaskVoid Run(MainArgs args, CancellationToken token)
        {
            Log.Assert(_container == null, "_container ==null");
            Log.Assert(_mainState == null, "_mainState ==null");

            try
            {
                await using (_container = await Container.CreateMain(MAIN_CONTAINER_NAME, token)
                                 .AddSingleton(args)
                                 .AddFromBuilder(args)
                                 .AddScoped<MainState>()
                                 .Build(token))
                {
                    _errorReporter = _container.Get<IErrorReporter>();
                    _mainState = _container.Get<MainState>();
                    await _mainState.Run(token);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception unhandled)
            {
                _errorReporter?.CaptureException(unhandled);
                throw;
            }
            finally
            {
                _runScope?.Cancel();
                _runScope?.Dispose();
                _runScope = null;
                
                _mainState = null;
                if (_container != null)
                {
                    await _container.DisposeAsync();
                }
                _container = null;
                _errorReporter = null;
                
                OnExit?.Invoke();
                Log.Info(APPLICATION_SHUTTING_DOWN);

#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    UnityEditor.EditorApplication.ExitPlaymode();
                }
                GC.Collect();
#endif
            }
        }

        /// <summary>
        /// Trigger app reset flow
        /// </summary>
        /// <param name="clearLoginData">Cleans up all data (should be true in most cases) regarding Auth and Login</param>
        /// <param name="message">Optional message for logging</param>
        public static void Reset(bool clearLoginData = false, string? message = null)
        {
            _mainState?.Reset(clearLoginData, message);
        }

        internal static void TriggerOnRestartEvent(bool clearLoginData)
        {
            if (clearLoginData)
            {
                OnResetLoginData?.Invoke();
            }
            OnRestart?.Invoke();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        internal static void EditorExit()
        {
            // One frame delayed needed for AddressablesManager to get it callback to OnEditorPlayModeStateChanged 
            UniTask.NextFrame(PlayerLoopTiming.Initialization)
                .ContinueWith(() =>
                {
                    _runScope?.Cancel();
                    _runScope?.Dispose();
                    _runScope = null;
                });
        }
    }
}
#nullable disable