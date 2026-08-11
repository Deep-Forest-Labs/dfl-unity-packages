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

        public UniTask EnsureAnonymousAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _ = PlayerId;
            NullPlatformLog.Once(nameof(NullAccountService) + "." + nameof(EnsureAnonymousAsync), "device-local id ready");
            return UniTask.CompletedTask;
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
