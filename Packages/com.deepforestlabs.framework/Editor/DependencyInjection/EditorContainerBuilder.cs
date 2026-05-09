#nullable enable
using System.Threading;
using UnityEngine;

namespace DeepForestLabs.DependencyInjection
{
    public static class EditorContainerBuilder
    {
        public static IContainerBuilder CreateMain(string name, CancellationToken token)
        {
            IContainerBuilder builder = Container.CreateMain(name, token);
            GameObject root = builder.Get<GameObject>();
            root.hideFlags = HideFlags.HideAndDontSave;

            return builder;
        }
    }
}
#nullable disable