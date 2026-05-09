#nullable enable
using DeepForestLabs.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace DeepForestLabs.PostProcessing
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/RenderSettingsProfile", order = 2)]
    public sealed class RenderSettingsProfile : ValidatedData
    {
        [SerializeField]
        private float _transitionDuration = 0f;
        public float TransitionDuration
        {
            get { return _transitionDuration; }
            private set { _transitionDuration = value; }
        }

        [Space(10)]

        #region Render Settings
        [SerializeField]
        [HideInInspector]
        private AmbientMode _ambientMode = AmbientMode.Flat;
        public AmbientMode AmbientMode
        {
            get { return _ambientMode; }
            private set { _ambientMode = value; }
        }

        [SerializeField]
        [HideInInspector]
        private Color _ambientColor = Color.black;
        public Color AmbientColor
        {
            get { return _ambientColor; }
            private set { _ambientColor = value; }
        }

        [SerializeField]
        [HideInInspector]
        private Color _ambientSkyColor = Color.black;
        public Color AmbientSkyColor
        {
            get { return _ambientSkyColor; }
            private set { _ambientSkyColor = value; }
        }

        [SerializeField]
        [HideInInspector]
        private Color _ambientEquatorColor = Color.black;
        public Color AmbientEquatorColor
        {
            get { return _ambientEquatorColor; }
            private set { _ambientEquatorColor = value; }
        }

        [SerializeField]
        [HideInInspector]
        private Color _ambientGroundColor = Color.black;
        public Color AmbientGroundColor
        {
            get { return _ambientGroundColor; }
            private set { _ambientGroundColor = value; }
        }


        [SerializeField]
        private Color _fogColor = Color.black;
        public Color FogColor
        {
            get { return _fogColor; }
            private set { _fogColor = value; }
        }

        [SerializeField]
        private FogMode _fogMode = FogMode.Linear;
        public FogMode FogMode
        {
            get { return _fogMode; }
            private set { _fogMode = value; }
        }

        [SerializeField]
        private float _fogDensity = 0.01f;
        public float FogDensity
        {
            get { return _fogDensity; }
            private set { _fogDensity = value; }
        }

        [SerializeField]
        private float _fogStartDistance = 10f;
        public float FogStartDistance
        {
            get { return _fogStartDistance; }
            private set { _fogStartDistance = value; }
        }

        [SerializeField]
        private float _fogEndDistance = 100f;
        public float FogEndDistance
        {
            get { return _fogEndDistance; }
            private set { _fogEndDistance = value; }
        }
        #endregion

        public void Init()
        {
            AmbientMode = RenderSettings.ambientMode;
            AmbientColor = RenderSettings.ambientLight;
            AmbientSkyColor = RenderSettings.ambientSkyColor;
            AmbientEquatorColor = RenderSettings.ambientEquatorColor;
            AmbientGroundColor = RenderSettings.ambientGroundColor;
            FogColor = RenderSettings.fogColor;
            FogMode = RenderSettings.fogMode;
            FogStartDistance = RenderSettings.fogStartDistance;
            FogEndDistance = RenderSettings.fogEndDistance;
            FogDensity = RenderSettings.fogDensity;
        }
    }
}
#nullable disable