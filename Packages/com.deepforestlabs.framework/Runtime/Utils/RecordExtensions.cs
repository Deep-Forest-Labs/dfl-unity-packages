#nullable enable
using System;
using UnityEngine.Scripting;

namespace DeepForestLabs.Utils
{
    [Preserve]
    public static class RecordExtensions
    {
        public static bool IsRecord(this Type type) => type.GetMethod("<Clone>$") != null;
    }
}
#nullable disable