#nullable enable

namespace UnityEngine
{
    [ExecuteInEditMode]
    public sealed class AnimatorEditMode : MonoBehaviour
    {
#if UNITY_EDITOR
        private Animator? _animator;

        private void OnEnable()
        {
            _animator = GetComponent<Animator>();

            if (_animator != null)
            {
                _animator.Rebind();
                _animator.Update(0f);
            }
        }

        private void Update()
        {
            if (!Application.isPlaying && _animator != null)
            {
                _animator.Update(Time.unscaledDeltaTime);
                UnityEditor.SceneView.RepaintAll();
            }
        }
#endif
    }
}
#nullable disable