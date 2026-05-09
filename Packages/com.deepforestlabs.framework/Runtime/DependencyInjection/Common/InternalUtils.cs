#nullable enable
using System;
using System.Linq;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using Cysharp.Text;
using UnityEngine.Scripting;

namespace DeepForestLabs.Common
{
    internal static class InternalUtils
    {
        public static string FormatTypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }
            
            Type[] genericArguments = type.GetGenericArguments();
            
            if (genericArguments.Length > 0)
            {
                return ZString.Format("{0}<{1}>", type.Name, string.Join(',', genericArguments.Select(t => t.Name)));
            }
            else
            {
                return type.Name;
            }
        }
        
        [System.Diagnostics.Conditional("DEBUG_CONTAINER")]
        internal static void VerboseLog(string name, string message, ContainerLogFlag flag)
        {
            BuildSettings? buildSettings = BuildSettings.Instance;
            if (buildSettings != null && buildSettings.ContainerLogFlag.HasFlag(flag))
            {
#if UNITY_EDITOR
                Log.Debug("<color='#8983FF'>[{0} {1}]</color> - {2}", name, nameof(Container), message);
#else
                Log.Debug($"[{name} {nameof(Container)}] - {message}");
#endif
            }
        }
        
        [Preserve]
        [System.Diagnostics.Conditional("DEBUG_CONTAINER")]
        internal static void VerboseWarn(string name, string message, ContainerLogFlag flag)
        {
            BuildSettings? buildSettings = BuildSettings.Instance;
            if (buildSettings != null && buildSettings.ContainerLogFlag.HasFlag(flag))
            {
                Log.Warning("[{0} {1}] - {2}", name, nameof(Container), message);
            }
        }
    }
}
#nullable disable