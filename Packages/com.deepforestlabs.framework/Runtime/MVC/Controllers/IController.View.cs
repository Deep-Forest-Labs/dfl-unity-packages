#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.MVC.Models;
using DeepForestLabs.MVC.Views;
using UnityEngine.UI;

namespace DeepForestLabs.MVC.Controllers
{
    public partial interface IController
    {
        ViewState ViewState { get; }
        bool IsVisible();
        bool IsBlocking();
        void Show();
        UniTask Show(CancellationToken token);
        void Refresh();
        void Hide();
        UniTask Hide(CancellationToken token);
        UniTask NextInput(CancellationToken token); //TODO mwood todo / temp - useful in rows / panels, where parent control can TryGetView & wait for input
    }

    public interface IViewController<TView> : IController
        where TView : IView
    {
        IUniTaskAsyncEnumerable<TView> ViewLoaded { get; }
        IUniTaskAsyncEnumerable<TView> ViewReleased { get; }
        
        UniTask<(TView view, CancellationToken scope)> GetView(CancellationToken token);
        bool TryGetView([NotNullWhen(true)] out TView view, out CancellationToken token);
    }
}
#nullable disable