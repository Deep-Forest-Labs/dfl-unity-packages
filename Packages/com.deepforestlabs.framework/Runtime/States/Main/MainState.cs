#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Factories;
using DeepForestLabs.Services;
using DeepForestLabs.States.Error;
using DeepForestLabs.States.Error.Controllers;
using DeepForestLabs.States.UnobservedExceptions;
using UnityEngine;

namespace DeepForestLabs.States.Main
{
	internal sealed class MainState : IRunnable
	{
		private const string RESET_ANALYTICS_MESSAGE = "Game reset triggered.";

	    [Dependency] private readonly IContainer _container = null!;
	    [Dependency] private readonly IMain _main = null!;
	    [Dependency] private readonly BuildSettings _buildSettings = default!;
	    [Dependency] private readonly IAnalyticsErrorHelper _analyticsErrorHelper = null!;
	    [Dependency] private readonly ILoggingService _loggingService = null!;
	    [Dependency] private readonly IErrorReporter _errorReporter = null!;
	    [Dependency] private readonly UnobservedExceptionState _unobservedExceptionState = null!;
	    [Dependency] private readonly IErrorStateController _errorStateController = null!;

	    public async UniTask Run(CancellationToken token)
	    {
		    _errorReporter.StartSession();
		    _main.Start();
		    
		    while (true)
	        {
		        Exception? unhandled = null;
		        await using (IContainer app = await _container.CreateChild("App")
			                     .AddFromBuilder(_container.Get<ContainerBuilderFactory>())
			                     .Build(token))
		        {
			        try
			        {
				        await UniTask.WhenAny(
					        app.Run<IRunnable>(token),
					        _unobservedExceptionState.Run(token)
				        );
			        }
			        catch (ResetException re)
			        {
				        // We fully ignore these as we want to reset without error popup when this is thrown
				        Log.Info(RESET_ANALYTICS_MESSAGE);
				        _analyticsErrorHelper.Log(RESET_ANALYTICS_MESSAGE, re.StackTrace, LogType.Log);
				        DeepForestLabs.Main.TriggerOnRestartEvent(re.ClearLoginData);
				        _main.PreRestart();
			        }
					catch (RefreshExpiredException)
					{
						await _container.Run<IRefreshExpiredState>(token);
						DeepForestLabs.Main.TriggerOnRestartEvent(false);
						_main.PreRestart();
					}
			        catch (OperationCanceledException)
			        {
				        throw;
			        }
			        catch (Exception e)
			        {
				        CaptureSentryTags(e);
				        unhandled = e;
				        
				        _loggingService.CaptureUnhandledException(unhandled);
				        DeepForestLabs.Main.UnhandledExceptionsSinceLaunch++;
				        DeepForestLabs.Main.TriggerOnRestartEvent(false);
				        _main.PreRestart();
			        }
		        }

		        try
		        {
			        if (unhandled != null)
			        {
				        _main.ShowingErrorPopup(unhandled);
				        await _errorStateController.Run(unhandled, token)
					        .AttachExternalCancellation(token); //Required to ensure that an OperationCanceledException is thrown if editor is stopped.  
				        _main.DismissingErrorPopup(unhandled);
			        }

                    _errorReporter.EndSession();
                    _errorReporter.StartSession();

                    _main.PostRestart();
		        }
		        catch (OperationCanceledException)
		        {
			        throw;
		        }
		        catch (Exception critical)
		        {
			        Log.Exception(critical, "Critical MainState Exception");
			        
			        CaptureSentryTags(critical);
			        _errorReporter.CaptureException(critical);
			        return;
		        }
	        }
	    }

	    internal void Reset(bool clearLoginData, string? message)
	    {
		    _unobservedExceptionState.Trigger(new ResetException(clearLoginData, message));
	    }

	    private void CaptureSentryTags(Exception unhandled)
        {
	        IDictionary<string, string> tags;
	        if (!unhandled.Data.Contains(SentryErrorReporter.EXCEPTIONS_DATA_TAG))
	        {
		        tags = new Dictionary<string, string>();
		        unhandled.Data[SentryErrorReporter.EXCEPTIONS_DATA_TAG] = tags;
	        }
	        else
	        {
		        tags = (unhandled.Data[SentryErrorReporter.EXCEPTIONS_DATA_TAG] as IDictionary<string, string>)!;
	        }
	        
	        tags["_unhandledSinceLaunch"] = DeepForestLabs.Main.UnhandledExceptionsSinceLaunch.ToString();
	        tags["unique_id"] = _buildSettings.Addressables.UniqueId;
	        tags["asset_id"] = _buildSettings.Addressables.AssetId;
        }
    }
}
#nullable disable