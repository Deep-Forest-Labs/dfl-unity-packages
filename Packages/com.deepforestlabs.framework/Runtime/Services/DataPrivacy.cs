using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Analytics;

namespace DeepForestLabs.Services
{
	public sealed class DataPrivacy : IInitializable
	{
		public UniTask Initialize(CancellationToken token)
		{
			Analytics.initializeOnStartup = false;
			Analytics.limitUserTracking = true;

			Analytics.enabled = false;
			Analytics.deviceStatsEnabled = false;
			PerformanceReporting.enabled = false;

			return UniTask.CompletedTask;
		}
	}
}