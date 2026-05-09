#nullable enable
using System;
using UnityEngine;

namespace DeepForestLabs.PostProcessing
{
    [Serializable]
    public class PostProcessingStateListener : StateMachineBehaviour
    {
        internal static RenderController? RenderController { get; set; }
        
        [SerializeField]
        [HideInInspector]
        private bool _onEnter = false;
        [SerializeField]
        [HideInInspector]
        private TransitionStyle _enterStyle = TransitionStyle.Instant;
        [SerializeField]
        [HideInInspector]
        private PostProcessingProfile? _enterProfile = null;

        [SerializeField]
        [HideInInspector]
        private bool _onExit = false;
        [SerializeField]
        [HideInInspector]
        private TransitionStyle _exitStyle = TransitionStyle.Instant;
        [SerializeField]
        [HideInInspector]
        private PostProcessingProfile? _exitProfile = null;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_onEnter)
            {
                return;
            }

            if (_enterProfile == null || RenderController == null)
            {
                return;
            }

            if (_enterStyle == TransitionStyle.Blend)
            {
                RenderController.TransitionPostProcessing(_enterProfile);
            }
            else
            {
                RenderController.LoadPostProcessingProfile(_enterProfile);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_onExit)
            {
                return;
            }

            if (_exitProfile == null || RenderController == null)
            {
                return;
            }

            if (_exitStyle == TransitionStyle.Blend)
            {
                RenderController.TransitionPostProcessing(_exitProfile);
            }
            else
            {
                RenderController.LoadPostProcessingProfile(_exitProfile);
            }
        }
    }
}
#nullable disable