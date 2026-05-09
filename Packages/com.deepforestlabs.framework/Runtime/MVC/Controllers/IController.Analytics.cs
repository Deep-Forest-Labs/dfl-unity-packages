#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace DeepForestLabs.MVC.Controllers
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial interface IController
    {
        AnalyticsStringValues Analytics { get; }
        
        UniTask OnClick(Button? component, CancellationToken token);
        UniTask OnClick(Button? component, string buttonName, CancellationToken token);
        UniTask OnSelectedChanged(ToggleGroup? component, CancellationToken token);
        UniTask<bool> OnToggled(Toggle? component, CancellationToken token);
        UniTask<bool> OnToggled(Toggle? component, string buttonName, CancellationToken token);
        UniTask OnToggledOn(Toggle? component, string buttonName, CancellationToken token);
        UniTask OnToggledOn(Toggle? component, CancellationToken token);
        UniTask OnCloseClick(Button? component, CancellationToken token);
        UniTask OnCloseClick(Button? component, string buttonName, CancellationToken token);
        UniTask<string> OnEndEdit(InputField? component, CancellationToken token);
        UniTask<bool> OnValueChanged(Toggle? component, CancellationToken token);
        UniTask<float> OnValueChanged(Slider? component, CancellationToken token);
    }
}
#nullable disable