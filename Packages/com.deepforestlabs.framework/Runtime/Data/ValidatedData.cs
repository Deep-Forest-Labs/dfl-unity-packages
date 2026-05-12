#nullable enable
using UnityEngine;

namespace DeepForestLabs.Data
{
    public abstract class ValidatedData : ScriptableObject
    {
#if UNITY_EDITOR
        protected internal virtual void OnValidate()
        {
        }
#endif
    }
}