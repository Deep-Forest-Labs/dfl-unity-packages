#nullable enable
using System;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs.States.UnobservedExceptions
{
	public sealed class UnobservedExceptionState : IRunnable
	{
		[Dependency] private readonly ILogFilter _filter = null!;

		private UniTaskCompletionSource? _runTaskCompletionSource;

		public void Trigger(Exception exception)
		{
			_runTaskCompletionSource?.TrySetException(exception);
		}

		public async UniTask Run(CancellationToken token)
		{
			UniTaskScheduler.UnobservedTaskException += UnobservedTaskException;
			_runTaskCompletionSource = new UniTaskCompletionSource();
			try
			{
				await _runTaskCompletionSource.Task
					.AttachExternalCancellation(token);
			}
			finally
			{
				_runTaskCompletionSource = null;	
				UniTaskScheduler.UnobservedTaskException -= UnobservedTaskException;
			}
		}

		private void UnobservedTaskException(Exception? exception)
		{
			if (_filter.IsIgnoredException(exception))
			{
				if (exception != null) Log.DevException(exception, "Filtered Exception");
				return;
			}

			_runTaskCompletionSource?.TrySetException(exception);
			UniTaskScheduler.UnobservedTaskException -= UnobservedTaskException;
		}
	}
}
#nullable disable
