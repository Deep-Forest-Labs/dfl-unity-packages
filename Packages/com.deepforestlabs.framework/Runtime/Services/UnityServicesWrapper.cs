#nullable enable
using System;
using System.Threading;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

namespace DeepForestLabs.Services
{
	public sealed class UnityServicesWrapper : IInitializable
	{
		[Dependency] private readonly BuildSettings _buildSettings = null!;
		public bool IsInitialized { get; private set; }
		
		public async UniTask Initialize(CancellationToken token)
		{
			try
			{
				InitializationOptions options = new InitializationOptions().SetEnvironmentName(_buildSettings.Environment.Name);
				await UnityServices.InitializeAsync(options).AsUniTask()
					.AttachExternalCancellation(token);
				IsInitialized = true;
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception e)
			{
				// An error occurred during initialization.
				Log.Exception(e, "Failed to initialize UnityServicesWrapper.");
			}
		}
	}
}
#nullable disable
