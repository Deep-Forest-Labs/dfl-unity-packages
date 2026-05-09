#nullable enable
using DeepForestLabs.MVC.Views;

namespace DeepForestLabs.MVC.Controllers
{
    public interface IController<TModel, TView, TResult> : IViewController<TView>, IController<TModel, TResult>
        where TView : IView
    {
    }
}
#nullable disable