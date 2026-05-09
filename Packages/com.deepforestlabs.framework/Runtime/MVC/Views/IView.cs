#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs.MVC.Views
{
    public interface IView
    {
        UniTask OpenAnimation(CancellationToken token);
        void OpenAnimationFinished();
        UniTask CloseAnimation(CancellationToken token);
        void CloseAnimationFinished();
    }
}
#nullable disable