#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeepForestLabs
{
    internal sealed partial class ContainerBuilder
    {
        public IContainerBuilder AddScopedComponent<TComponent>()
            where TComponent : Component
        {
            UniTask<TComponent> Factory(IDiCollection collection, CancellationToken token)
            {
                TComponent component = _container._gameObject.AddComponent<TComponent>();
                token.Register(() =>
                {
#if UNITY_EDITOR
                    GameObject.DestroyImmediate(component);
#else
                    GameObject.Destroy(component);
#endif
                });
                return UniTask.FromResult(component);
            }

            return AddScoped(Factory);
        }
    }
}
#nullable disable