#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Platform.Internal;
using UnityEngine;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Device-local anonymous player id persisted in PlayerPrefs. No account linking.
    /// </summary>
    public sealed class NullAccountService : IAccountService
    {
        private const string PrefsKey = "dfl.platform.anonymousPlayerId";

        private string? _playerId;

        public string PlayerId => _playerId ??= LoadOrCreateId();

        public bool IsLinked => false;

        public string? Email => null;

        public UniTask EnsureAnonymousAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _ = PlayerId;
            NullPlatformLog.Once(nameof(NullAccountService) + "." + nameof(EnsureAnonymousAsync), "device-local id ready");
            return UniTask.CompletedTask;
        }

        public UniTask<AccountActionResult> CreateAccount(string email, string password, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullAccountService) + "." + nameof(CreateAccount), "unavailable");
            return UniTask.FromResult(AccountActionResult.Unavailable);
        }

        public UniTask<AccountActionResult> SignIn(string email, string password, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullAccountService) + "." + nameof(SignIn), "unavailable");
            return UniTask.FromResult(AccountActionResult.Unavailable);
        }

        public UniTask<AccountActionResult> SignOut(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return UniTask.FromResult(AccountActionResult.Succeeded);
        }

        public UniTask<AccountActionResult> SendPasswordReset(string email, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullAccountService) + "." + nameof(SendPasswordReset), "unavailable");
            return UniTask.FromResult(AccountActionResult.Unavailable);
        }

        private static string LoadOrCreateId()
        {
            string existing = PlayerPrefs.GetString(PrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(existing))
            {
                return existing;
            }

            string created = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(PrefsKey, created);
            PlayerPrefs.Save();
            return created;
        }
    }
}
#nullable disable
