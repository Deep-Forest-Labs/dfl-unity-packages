#nullable enable
using System;
using System.Diagnostics;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.MVC.Models;
using DeepForestLabs.MVC.Views;
using DeepForestLabs.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace DeepForestLabs.MVC.Controllers
{
	public abstract partial class Controller<TModel, TView, TResult>
	{
		public AnalyticsStringValues Analytics
		{
			get => _analytics.Value;
			protected set => _analytics.Value = value;
		}

		public virtual string str1
		{
			get => Analytics.str1;
		}

		private UniTask InitializeAnalytics(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			return UniTask.CompletedTask;
		}

		public async UniTask OnClick(Button? component, CancellationToken token)
		{
			if (component != null)
			{
				await component.OnClickAsync(token);
			}
			else
			{
				Controller.LogMissingUIElement(this, component);
				await UniTask.Never(token);
			}

			_analyticsUIEventHelper.ClickedEvent(Analytics.str1, Analytics.str2, Analytics.str3, Analytics.str4,
				Analytics.str5, Analytics.amount, Analytics.extra_data);
		}
		
		public async UniTask OnClick(Button? component, string buttonName, CancellationToken token)
		{
			if (component != null)
			{
				await component.OnClickAsync(token);
			}
			else
			{
				Controller.LogMissingUIElement(this, component);
				await UniTask.Never(token);
			}

			if (Analytics.SuppressAnalyticsCalls)
			{
				return;
			}

			AnalyticsStringValues analytics = Analytics.SetInputValue(buttonName);
			_analyticsUIEventHelper.ClickedEvent(analytics.str1, analytics.str2, analytics.str3, analytics.str4,
				analytics.str5, analytics.amount, analytics.extra_data);
		}

		/// <summary>
		/// UNTESTED
		/// </summary>
		/// <param name="component"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async UniTask OnSelectedChanged(ToggleGroup? component, CancellationToken token)
		{
			if (component != null)
			{
				await component.GetFirstActiveToggle().OnValueChangedAsync(token);
			}
			else
			{
				Controller.LogMissingUIElement(this, component);
				await UniTask.Never(token);
			}

            _analyticsUIEventHelper.ClickedEvent(Analytics.str1, Analytics.str2, Analytics.str3, Analytics.str4,
			    Analytics.str5, Analytics.amount, Analytics.extra_data);
        }
		
		public async UniTask<bool> OnToggled(Toggle? component, CancellationToken token)
        {
            if (component != null)
            {
                await component.OnValueChangedAsync(token);
            }
            else
            {
                Controller.LogMissingUIElement(this, component);
                await UniTask.Never(token);
            }

            _analyticsUIEventHelper.ClickedEvent(Analytics.str1, Analytics.str2, Analytics.str3, Analytics.str4,
			    Analytics.str5, Analytics.amount, Analytics.extra_data);

            return component != null && component.isOn;
        }

        public async UniTask<bool> OnToggled(Toggle? component, string buttonName, CancellationToken token)
        {
            if (component != null)
            {
                await component.OnValueChangedAsync(token);
            }
            else
            {
                Controller.LogMissingUIElement(this, component);
                await UniTask.Never(token);
            }

            AnalyticsStringValues analytics = Analytics.SetInputValue(buttonName);
            _analyticsUIEventHelper.ClickedEvent(analytics.str1, analytics.str2, analytics.str3, analytics.str4,
                analytics.str5, analytics.amount, analytics.extra_data);

            return component != null && component.isOn;
        }

        public async UniTask OnToggledOn(Toggle? component, string buttonName, CancellationToken token)
        {
            if (component != null)
            {
                while (!await component.OnValueChangedAsync(token)) { }
            }
            else
            {
                Controller.LogMissingUIElement(this, component);
                await UniTask.Never(token);
            }
            AnalyticsStringValues analytics = Analytics.SetInputValue(buttonName);
            _analyticsUIEventHelper.ClickedEvent(analytics.str1, analytics.str2, analytics.str3, analytics.str4,
                analytics.str5, analytics.amount, analytics.extra_data);
        }

        public async UniTask OnToggledOn(Toggle? component, CancellationToken token)
        {
            if (component != null)
            {
                while (!await component.OnValueChangedAsync(token)) { }
            }
            else
            {
                Controller.LogMissingUIElement(this, component);
                await UniTask.Never(token);
            }

            _analyticsUIEventHelper.ClickedEvent(Analytics.str1, Analytics.str2, Analytics.str3, Analytics.str4,
                Analytics.str5, Analytics.amount, Analytics.extra_data);
        }

        public async UniTask OnCloseClick(Button? component, CancellationToken token)
		{
			if (component != null)
			{
				await component.OnClickAsync(token);
			}
			else
			{
				Controller.LogMissingUIElement(this, component);
				await UniTask.Never(token);
			}

			_analyticsUIEventHelper.ClickedCloseEvent(Analytics.str1, Analytics.str2, Analytics.str3, Analytics.str4,
				Analytics.str5, Analytics.amount, Analytics.extra_data);
		}
		
		public async UniTask OnCloseClick(Button? component, string buttonName, CancellationToken token)
		{
			if (component != null)
			{
				await component.OnClickAsync(token);
			}
			else
			{
				Controller.LogMissingUIElement(this, component);
				await UniTask.Never(token);
			}

			AnalyticsStringValues analytics = Analytics.SetInputValue(buttonName);
			_analyticsUIEventHelper.ClickedEvent(analytics.str1, analytics.str2, analytics.str3, analytics.str4,
				analytics.str5, analytics.amount, analytics.extra_data);
		}
		
		public async UniTask<string> OnEndEdit(InputField? component, CancellationToken token)
		{
			if (component != null)
			{
				string? result = await component.OnEndEditAsync(token);

				return result;
			}
			else
			{
				Controller.LogMissingUIElement(this, component);
				await UniTask.Never(token);
				throw new OperationCanceledException(token);
			}
		}
		
		public async UniTask<bool> OnValueChanged( Toggle? component, CancellationToken token)
		{
			if (component != null)
			{
				bool result = await component.OnValueChangedAsync(token);

				return result;
			}
			else
			{
				Controller.LogMissingUIElement(this, component);
				await UniTask.Never(token);
				throw new OperationCanceledException(token);
			}
		}

		public async UniTask<float> OnValueChanged(Slider? component, CancellationToken token)
		{
			if (component != null)
			{
				float result = await component.OnValueChangedAsync(token);

				return result;
			}
			else
			{
				Controller.LogMissingUIElement(this, component);
				await UniTask.Never(token);
				throw new OperationCanceledException(token);
			}
		}
	}
	
#if  UNITY_EDITOR
	[UnityEditor.InitializeOnLoad]
#endif
	public static class Controller
	{
		public const string kLoggingMissingComponentsPlayerPref = "DisableUIHelperWarnings";
		public const string kMissingUIElement = "Missing UI Element";
		
#if UNITY_EDITOR
		private const string MenuPath = "Deep Forest Labs/Tools/Disable Log Asserts for UI Helpers";
		static Controller()
		{
			UnityEditor.Menu.SetChecked(MenuPath, LoggingMissingComponents);
		}
		
		[UnityEditor.MenuItem(MenuPath, false)]
		public static void ToggleLoggingMissingComponents()
		{
			LoggingMissingComponents = !LoggingMissingComponents;
			UnityEditor.Menu.SetChecked(MenuPath, LoggingMissingComponents);
		}

#endif
		
		public static bool LoggingMissingComponents
		{
			get => PrefUtils.GetInt(kLoggingMissingComponentsPlayerPref, 0) > 0;
#if UNITY_EDITOR
			set => PrefUtils.SetInt(kLoggingMissingComponentsPlayerPref, value ? 1 : 0);
#endif
		}
		
		[Conditional("NOT_RELEASE_BUILD")]
		public static void LogMissingUIElement<T>(object owner, T? component)
		{
			Log.Assert(component != null, "{0} - {1} {2} ", kMissingUIElement, typeof(T).Name, GetMissingUIElementLocation(owner, component));
		}
		
		private static string GetMissingUIElementLocation(object owner, object? component)
		{
			if (component == null)
			{
				return ZString.Format("@{0} <{1}>", "NULL", owner.GetType().Name);
			}
			else if (component is not Component casted)
			{
				return ZString.Format("@{0} <{1}>", component.GetType().Name, owner.GetType().Name);
			}
			else
			{
				return ZString.Format("@{0} <{1}>", casted.name, owner.GetType().Name);
			}
		}
	}
}
#nullable disable
