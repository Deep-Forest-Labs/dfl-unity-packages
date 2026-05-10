using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs.Services
{
	/// <summary>
	/// Analytics data collection is disabled by default in SDK 6.0+ (dormant until
	/// StartDataCollection is explicitly called). This class is retained as a
	/// placeholder for future privacy/consent logic.
	/// </summary>
	public sealed class DataPrivacy : IInitializable
	{
		public UniTask Initialize(CancellationToken token)
		{
			return UniTask.CompletedTask;
		}
	}
}
