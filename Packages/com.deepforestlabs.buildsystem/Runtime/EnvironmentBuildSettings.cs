#nullable enable
using System;
using UnityEngine;

namespace DeepForestLabs.BuildSystems
{
	[Serializable]
	public struct EnvironmentBuildSettings
	{
		[SerializeField] internal string name;
		[SerializeField] internal string serverUrl;
		[SerializeField] internal string analyticsUrl;
		[SerializeField] internal string apiUrl;
		[SerializeField] internal string serverCommsKey;
		[SerializeField] internal string userServiceKey;
		
		public string Name => name;
		public string ServerUrl => serverUrl;
		public string AnalyticsUrl => analyticsUrl;
		public string ApiUrl => apiUrl;
		public string ServerCommsKey => serverCommsKey;
		public string UserServiceKey => userServiceKey;
		
		public EnvironmentBuildSettings(string name) : this()
        {
        	this.name = name;
			serverUrl = string.Empty;
			analyticsUrl = string.Empty;
			apiUrl = string.Empty;
			serverCommsKey = string.Empty;
			userServiceKey = string.Empty;
        }
	}
}
#nullable disable
