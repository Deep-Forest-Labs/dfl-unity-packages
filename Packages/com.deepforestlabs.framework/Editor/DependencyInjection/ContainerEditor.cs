// #nullable enable
// using System;
// using System.Threading;
// using Cysharp.Threading.Tasks;
//
// namespace DeepForestLabs.DependencyInjection
// {
// 	//TODO - Board Tool - pretty sure this shouldn't be used now.
// 	public class ContainerEditor : UnityEditor.Editor
// 	{
// 		[Dependency] protected readonly IContainer _container = null!;
// 		[Dependency] protected readonly CancellationToken _scope = default;
//
// 		private CancellationTokenSource? _enabledScoped = null;
// 		private UniTaskCompletionSource _isLoaded = new();
//
// 		private void OnEnable()
// 		{
// 			_enabledScoped?.Cancel();
// 			_enabledScoped?.Dispose();
// 			_enabledScoped = null;
// 			
// 			BuildContainer(_enabledScoped.Token);
// 		}
//
// 		private void OnDisable()
// 		{
// 			_enabledScoped?.Cancel();
// 			_enabledScoped?.Dispose();
// 			_enabledScoped = null;
// 			
// 			_container.Dispose();
// 		}
//
// 		private async UniTask BuildContainer(CancellationToken token)
// 		{
// 			if (target == null)
// 			{
// 				return;
// 			}
// 			
// 			IContainerBuilder builder = Container.CreateEditorRoot(target.name + " Container", CancellationToken.None);
// 			AddToContainer(builder);
// 			builder.AddSingleton(this)
// 				.Build(token)
// 				.ContinueWith(BuildComplete)
// 				.Forget();
// 		}
//
// 		private void BuildComplete(IContainer _)
// 		{
// 			OnInitialize(_scope)
// 				.ContinueWith(() => _isLoaded.TrySetResult())
// 				.Forget();
// 		}
//
// 		protected virtual IContainerBuilder AddToContainer(IContainerBuilder builder)
// 		{
// 			return builder;
// 		}
//
// 		protected virtual UniTask OnInitialize(CancellationToken token)
// 		{
// 			return UniTask.CompletedTask;
// 		}
//
// 		protected virtual void Reload()
// 		{
// 			BuildContainer(true);
// 		}
//
// 		public sealed override void OnInspectorGUI()
// 		{
// 			if (_isLoaded.Task.Status != UniTaskStatus.Succeeded)
// 			{
// 				base.OnInspectorGUI();
// 				return;
// 			}
//
// 			OnContainerGUI();
// 		}
//
// 		protected virtual void OnContainerGUI()
// 		{
// 			base.OnInspectorGUI();
// 		}
//
// 		protected virtual void OnDestroy()
// 		{
// 			// Exception to rule here since the "Editor" exists and calls OnEnable before its injected.
// 			// this means we could get disposed before _container is injected.
// 			// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
// 			_container?.Dispose();
// 		}
//
// 		// public virtual UniTask DisposeAsync()
// 		// {
// 		// 	return UniTask.CompletedTask;
// 		// }
// 	}
// }
// #nullable disable