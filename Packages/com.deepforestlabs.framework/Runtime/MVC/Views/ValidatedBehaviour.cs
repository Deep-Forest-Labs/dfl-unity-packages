#nullable enable
using UnityEngine;

namespace DeepForestLabs.MVC.Views
{
    public abstract class ValidatedBehaviour : MonoBehaviour
    {
#if UNITY_EDITOR
        protected internal virtual void OnValidate()
        {
        }
#endif
    }
}