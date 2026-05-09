#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Analytics;
using UnityEngine.UnityConsent;

namespace DeepForestLabs.Services
{
	public sealed class AnalyticsServiceWrapper : IInitializable
	{
		[Dependency] private readonly UnityServicesWrapper _unityServicesWrapper = null!;

		private static bool _isInitialized;

		private readonly bool _analyticsEnabled;

		public AnalyticsServiceWrapper()
		{
			//TODO [2.5.+] - low priority - add to BuildSettings, currently always off
			_analyticsEnabled = false;
		}

		public async UniTask Initialize(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			if (_isInitialized)
			{
				return;
			}

			await UniTask.WaitUntil(() => _unityServicesWrapper.IsInitialized, cancellationToken: token);

			if (_analyticsEnabled)
			{
				EndUserConsent.SetConsentState(new ConsentState());
			}

			_isInitialized = true;
		}
	}
}
#nullable disable
