#nullable enable
using System;
using UnityEngine;

namespace DeepForestLabs.MVC.Models
{
	[Serializable]
	public struct ControlModel<TResult>
	{
		[SerializeField] private bool _autoPromoteController;
		//[SerializeField] private FactoryOptions _factoryOptions;
		[SerializeField] private ResultV<TResult> _defaultResult;
		[SerializeField] private SkippableControlStates _skippable;

		public bool AutoPromoteController => _autoPromoteController;

		//public FactoryOptions FactoryOptions => _factoryOptions;
		public ResultV<TResult> DefaultResult => _defaultResult;
		public SkippableControlStates Skippable => _skippable;
	}
}

#nullable disable