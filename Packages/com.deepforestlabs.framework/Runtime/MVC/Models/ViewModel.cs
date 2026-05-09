#nullable enable
using System;
using Cysharp.Text;
using UnityEngine;

namespace DeepForestLabs.MVC.Models
{
    [Serializable]
    public struct ViewModel<TView> where TView : MonoBehaviour 
    {
        [SerializeField] private GameObjectAssetRefT<TView> _prefab;
        [SerializeField] private GameObjectManagerOptions _options;
        [SerializeField] private bool _worldPositionStays;
        
        //[SerializeField] private ViewDownloadOptions _download;
        //[SerializeField] private ViewLoadOptions _load;
        //[SerializeField] private InstantiateOptions _instantiate;

        public GameObjectAssetRefT<TView> Prefab => _prefab;

        public GameObjectManagerOptions Options => _options;

        public bool WorldPositionStays
        {
            get { return _worldPositionStays; }
            set { _worldPositionStays = value; }
        }

        //public ViewDownloadOptions Download => _download;

        //public ViewLoadOptions Load => _load;

        //public InstantiateOptions Instantiate => _instantiate;

        public object ComponentId()
        {
            return ZString.Concat(nameof(ViewModel<TView>), Prefab);
        }

        public override int GetHashCode()
        {
            return _prefab.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is not ViewModel<TView> other)
            {
                return false;
            }
            
            return other.Prefab.Equals(Prefab);
        }
    }
}
#nullable disable