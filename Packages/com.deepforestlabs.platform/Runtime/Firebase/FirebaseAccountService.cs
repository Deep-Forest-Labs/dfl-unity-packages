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
using UnityEngine;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Firebase Auth adapter via reflection. Games install <c>com.google.firebase.auth</c>.
    /// Missing SDK falls back to a device-local id; mutations return <see cref="AccountActionResult.Unavailable"/>.
    /// </summary>
    [Preserve]
    public sealed class FirebaseAccountService : IAccountService, IInitializable
    {
        private const string FallbackPrefsKey = "dfl.platform.anonymousPlayerId";

        public const string EventCreate = "account_create";
        public const string EventSignIn = "account_sign_in";
        public const string EventSignOut = "account_sign_out";
        public const string EventPasswordReset = "account_password_reset";

        [Dependency] private readonly IAnalyticsService _analytics = default!;

        private bool _sdkReady;
        private string _playerId = string.Empty;
        private string? _email;
        private bool _isLinked;
        private static bool _missingSdkLogged;

        private static Type? s_authType;
        private static Type? s_userType;
        private static Type? s_emailProviderType;
        private static Type? s_authErrorType;
        private static object? s_auth;
        private static MethodInfo? s_signInAnonymously;
        private static MethodInfo? s_createEmail;
        private static MethodInfo? s_signInEmail;
        private static MethodInfo? s_signOut;
        private static MethodInfo? s_sendReset;
        private static MethodInfo? s_getCredential;
        private static MethodInfo? s_linkCredential;
        private static PropertyInfo? s_currentUser;
        private static PropertyInfo? s_userId;
        private static PropertyInfo? s_userEmail;
        private static PropertyInfo? s_isAnonymous;
        private static bool s_resolved;

        public string PlayerId => string.IsNullOrEmpty(_playerId) ? FallbackId() : _playerId;

        public bool IsLinked => _isLinked;

        public string? Email => _email;

        public async UniTask Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ResolveFirebaseApi();

            if (s_authType == null)
            {
                _sdkReady = false;
                EnsureFallbackId();
                if (!_missingSdkLogged)
                {
                    _missingSdkLogged = true;
                    Log.Warning(
                        "PlatformServiceOptions.Firebase selected but Firebase Auth assembly was not found; using device-local id.");
                }

                return;
            }

            try
            {
                // Must finish CheckAndFixDependencies before DefaultInstance —
                // Firebase throws if Auth/Firestore/RC are touched while it runs.
                if (!await FirebaseReflection.CheckAndFixDependencies(token))
                {
                    _sdkReady = false;
                    EnsureFallbackId();
                    return;
                }

                s_auth ??= FirebaseReflection.GetStaticPropertyValue(s_authType, "DefaultInstance");
                if (s_auth == null)
                {
                    _sdkReady = false;
                    EnsureFallbackId();
                    Log.Warning("FirebaseAuth.DefaultInstance is null after dependency check.");
                    return;
                }

                _sdkReady = true;
                await EnsureAnonymousAsync(token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _sdkReady = false;
                EnsureFallbackId();
                Log.Exception(e, "Firebase Auth initialization failed.");
            }
        }

        public async UniTask EnsureAnonymousAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!_sdkReady || s_auth == null)
            {
                EnsureFallbackId();
                return;
            }

            try
            {
                if (TryReadCurrentUser())
                {
                    return;
                }

                if (s_signInAnonymously?.Invoke(s_auth, null) is not Task task)
                {
                    Log.Warning("FirebaseAuth.SignInAnonymouslyAsync not found.");
                    EnsureFallbackId();
                    return;
                }

                await FirebaseReflection.AwaitTask(task, token);
                CacheUser(FirebaseReflection.UnwrapUser(FirebaseReflection.TaskResult(task)));
                if (string.IsNullOrEmpty(_playerId))
                {
                    TryReadCurrentUser();
                }

                if (string.IsNullOrEmpty(_playerId))
                {
                    EnsureFallbackId();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Exception(e, "Firebase anonymous sign-in failed.");
                EnsureFallbackId();
            }
        }

        public async UniTask<AccountActionResult> CreateAccount(
            string email,
            string password,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!TryBeginMutation(email, password, requirePassword: true, out AccountActionResult early))
            {
                Track(EventCreate, early);
                return early;
            }

            try
            {
                await EnsureAnonymousAsync(token);
                if (_isLinked)
                {
                    AccountActionResult already = string.Equals(_email, email.Trim(), StringComparison.OrdinalIgnoreCase)
                        ? AccountActionResult.Succeeded
                        : AccountActionResult.Failed;
                    Track(EventCreate, already);
                    return already;
                }

                object? user = CurrentUser();
                object? credential = s_getCredential?.Invoke(null, new object[] { email.Trim(), password });
                if (user != null && credential != null && s_linkCredential != null)
                {
                    if (s_linkCredential.Invoke(user, new[] { credential }) is not Task linkTask)
                    {
                        Track(EventCreate, AccountActionResult.Failed);
                        return AccountActionResult.Failed;
                    }

                    await FirebaseReflection.AwaitTask(linkTask, token);
                    CacheUser(FirebaseReflection.UnwrapUser(FirebaseReflection.TaskResult(linkTask)));
                    if (!_isLinked)
                    {
                        TryReadCurrentUser();
                    }

                    AccountActionResult linked = _isLinked
                        ? AccountActionResult.Succeeded
                        : AccountActionResult.Failed;
                    Track(EventCreate, linked);
                    return linked;
                }

                if (s_createEmail?.Invoke(s_auth, new object[] { email.Trim(), password }) is not Task createTask)
                {
                    Track(EventCreate, AccountActionResult.Failed);
                    return AccountActionResult.Failed;
                }

                await FirebaseReflection.AwaitTask(createTask, token);
                CacheUser(FirebaseReflection.UnwrapUser(FirebaseReflection.TaskResult(createTask)));
                if (!_isLinked)
                {
                    TryReadCurrentUser();
                }

                AccountActionResult created = _isLinked
                    ? AccountActionResult.Succeeded
                    : AccountActionResult.Failed;
                Track(EventCreate, created);
                return created;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                AccountActionResult mapped = MapException(e);
                Log.Exception(e, "Firebase CreateAccount failed.");
                Track(EventCreate, mapped);
                return mapped;
            }
        }

        public async UniTask<AccountActionResult> SignIn(string email, string password, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!TryBeginMutation(email, password, requirePassword: true, out AccountActionResult early))
            {
                Track(EventSignIn, early);
                return early;
            }

            try
            {
                if (s_signInEmail?.Invoke(s_auth, new object[] { email.Trim(), password }) is not Task task)
                {
                    Track(EventSignIn, AccountActionResult.Failed);
                    return AccountActionResult.Failed;
                }

                await FirebaseReflection.AwaitTask(task, token);
                CacheUser(FirebaseReflection.UnwrapUser(FirebaseReflection.TaskResult(task)));
                if (!_isLinked)
                {
                    TryReadCurrentUser();
                }

                AccountActionResult result = _isLinked
                    ? AccountActionResult.Succeeded
                    : AccountActionResult.Failed;
                Track(EventSignIn, result);
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                AccountActionResult mapped = MapException(e);
                Log.Exception(e, "Firebase SignIn failed.");
                Track(EventSignIn, mapped);
                return mapped;
            }
        }

        public async UniTask<AccountActionResult> SignOut(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!_sdkReady || s_auth == null || s_signOut == null)
            {
                Track(EventSignOut, AccountActionResult.Unavailable);
                return AccountActionResult.Unavailable;
            }

            try
            {
                s_signOut.Invoke(s_auth, null);
                _playerId = string.Empty;
                _email = null;
                _isLinked = false;
                await EnsureAnonymousAsync(token);
                Track(EventSignOut, AccountActionResult.Succeeded);
                return AccountActionResult.Succeeded;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Exception(e, "Firebase SignOut failed.");
                Track(EventSignOut, AccountActionResult.Failed);
                return AccountActionResult.Failed;
            }
        }

        public async UniTask<AccountActionResult> SendPasswordReset(string email, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!TryBeginMutation(email, password: "x", requirePassword: false, out AccountActionResult early))
            {
                Track(EventPasswordReset, early);
                return early;
            }

            try
            {
                if (s_sendReset?.Invoke(s_auth, new object[] { email.Trim() }) is not Task task)
                {
                    Track(EventPasswordReset, AccountActionResult.Failed);
                    return AccountActionResult.Failed;
                }

                await FirebaseReflection.AwaitTask(task, token);
                Track(EventPasswordReset, AccountActionResult.Succeeded);
                return AccountActionResult.Succeeded;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                AccountActionResult mapped = MapException(e);
                Log.Exception(e, "Firebase SendPasswordReset failed.");
                Track(EventPasswordReset, mapped);
                return mapped;
            }
        }

        private bool TryBeginMutation(
            string email,
            string password,
            bool requirePassword,
            out AccountActionResult result)
        {
            if (!_sdkReady || s_auth == null)
            {
                result = AccountActionResult.Unavailable;
                return false;
            }

            if (string.IsNullOrWhiteSpace(email) || (requirePassword && string.IsNullOrEmpty(password)))
            {
                result = AccountActionResult.Failed;
                return false;
            }

            result = AccountActionResult.Succeeded;
            return true;
        }

        private void Track(string eventName, AccountActionResult result)
        {
            _analytics.Track(
                eventName,
                new Dictionary<string, object?>
                {
                    ["result"] = result.ToString().ToLowerInvariant()
                });
        }

        private bool TryReadCurrentUser()
        {
            object? user = CurrentUser();
            if (user == null)
            {
                return false;
            }

            CacheUser(user);
            return !string.IsNullOrEmpty(_playerId);
        }

        private object? CurrentUser()
        {
            return s_currentUser?.GetValue(s_auth);
        }

        private void CacheUser(object? user)
        {
            if (user == null)
            {
                return;
            }

            _playerId = s_userId?.GetValue(user) as string ?? string.Empty;
            string? email = s_userEmail?.GetValue(user) as string;
            _email = string.IsNullOrEmpty(email) ? null : email;
            bool anonymous = s_isAnonymous != null && s_isAnonymous.GetValue(user) is bool flag && flag;
            _isLinked = !anonymous && !string.IsNullOrEmpty(_email);
        }

        private void EnsureFallbackId()
        {
            if (string.IsNullOrEmpty(_playerId))
            {
                _playerId = FallbackId();
            }

            _email = null;
            _isLinked = false;
        }

        private static string FallbackId()
        {
            string existing = PlayerPrefs.GetString(FallbackPrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(existing))
            {
                return existing;
            }

            string created = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(FallbackPrefsKey, created);
            PlayerPrefs.Save();
            return created;
        }

        private static AccountActionResult MapException(Exception e)
        {
            Exception inner = e is TargetInvocationException tie && tie.InnerException != null
                ? tie.InnerException
                : e;
            string name = ReadAuthErrorName(inner);
            string haystack = (name + " " + inner.GetType().Name + " " + inner.Message).ToLowerInvariant();
            if (haystack.Contains("emailalreadyinuse")
                || haystack.Contains("credentialalreadyinuse")
                || haystack.Contains("accountexists")
                || haystack.Contains("provideralreadylinked"))
            {
                return AccountActionResult.EmailInUse;
            }

            if (haystack.Contains("wrongpassword")
                || haystack.Contains("usernotfound")
                || haystack.Contains("invalidcredential")
                || haystack.Contains("invalidemail")
                || haystack.Contains("weakpassword"))
            {
                return AccountActionResult.InvalidCredential;
            }

            return AccountActionResult.Failed;
        }

        private static string ReadAuthErrorName(Exception exception)
        {
            PropertyInfo? codeProp = exception.GetType().GetProperty("ErrorCode");
            object? raw = codeProp?.GetValue(exception);
            if (raw == null)
            {
                return string.Empty;
            }

            if (s_authErrorType != null)
            {
                try
                {
                    return Enum.GetName(s_authErrorType, raw) ?? raw.ToString() ?? string.Empty;
                }
                catch
                {
                    return raw.ToString() ?? string.Empty;
                }
            }

            return raw.ToString() ?? string.Empty;
        }

        private static void ResolveFirebaseApi()
        {
            if (s_resolved)
            {
                return;
            }

            s_resolved = true;
            FirebaseReflection.TryLoadAssembly("Firebase.App");
            FirebaseReflection.TryLoadAssembly("Firebase.Auth");

            s_authType = FirebaseReflection.FindType("Firebase.Auth.FirebaseAuth", "Firebase.Auth");
            if (s_authType == null)
            {
                return;
            }

            // Defer DefaultInstance until after CheckAndFixDependencies (see Initialize).
            s_currentUser = s_authType.GetProperty("CurrentUser", BindingFlags.Public | BindingFlags.Instance);
            s_signInAnonymously = s_authType.GetMethod(
                "SignInAnonymouslyAsync",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);
            s_createEmail = s_authType.GetMethod(
                "CreateUserWithEmailAndPasswordAsync",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(string), typeof(string) },
                modifiers: null);
            s_signInEmail = s_authType.GetMethod(
                "SignInWithEmailAndPasswordAsync",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(string), typeof(string) },
                modifiers: null);
            s_signOut = s_authType.GetMethod(
                "SignOut",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);
            s_sendReset = s_authType.GetMethod(
                "SendPasswordResetEmailAsync",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(string) },
                modifiers: null);

            s_emailProviderType = FirebaseReflection.FindType("Firebase.Auth.EmailAuthProvider", "Firebase.Auth");
            s_getCredential = s_emailProviderType?.GetMethod(
                "GetCredential",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(string), typeof(string) },
                modifiers: null);

            s_userType = FirebaseReflection.FindType("Firebase.Auth.FirebaseUser", "Firebase.Auth");
            if (s_userType != null)
            {
                s_userId = s_userType.GetProperty("UserId");
                s_userEmail = s_userType.GetProperty("Email");
                s_isAnonymous = s_userType.GetProperty("IsAnonymous");
                Type? credentialType = FirebaseReflection.FindType("Firebase.Auth.Credential", "Firebase.Auth");
                if (credentialType != null)
                {
                    s_linkCredential = s_userType.GetMethod(
                        "LinkWithCredentialAsync",
                        BindingFlags.Public | BindingFlags.Instance,
                        binder: null,
                        types: new[] { credentialType },
                        modifiers: null);
                }
            }

            s_authErrorType = FirebaseReflection.FindType("Firebase.Auth.AuthError", "Firebase.Auth");
        }
    }
}
#nullable disable
