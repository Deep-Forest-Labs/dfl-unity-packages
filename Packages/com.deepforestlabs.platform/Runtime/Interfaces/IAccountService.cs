#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    [Preserve]
    [RequireImplementors]
    public interface IAccountService
    {
        string PlayerId { get; }

        bool IsLinked { get; }

        string? Email { get; }

        UniTask EnsureAnonymousAsync(CancellationToken token);

        UniTask<AccountActionResult> CreateAccount(string email, string password, CancellationToken token);

        UniTask<AccountActionResult> SignIn(string email, string password, CancellationToken token);

        UniTask<AccountActionResult> SignOut(CancellationToken token);

        UniTask<AccountActionResult> SendPasswordReset(string email, CancellationToken token);

        /// <summary>Firebase ID token for Auth-gated HTTPS. Null when Auth is unavailable.</summary>
        UniTask<string?> GetIdToken(CancellationToken token);
    }
}
#nullable disable
