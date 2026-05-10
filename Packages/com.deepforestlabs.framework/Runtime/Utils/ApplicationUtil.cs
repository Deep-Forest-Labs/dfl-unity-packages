using UnityEngine;

namespace DeepForestLabs.Utils
{
	public static class ApplicationUtil
	{
		public static RuntimePlatform Platform
		{
			get
			{
#if UNITY_ANDROID
				return RuntimePlatform.Android;
#elif UNITY_IOS
				return RuntimePlatform.IPhonePlayer;
#elif UNITY_STANDALONE_OSX
				return RuntimePlatform.OSXPlayer;
#elif UNITY_STANDALONE_WIN
				return RuntimePlatform.WindowsPlayer;
#elif UNITY_STANDALONE_LINUX
				return RuntimePlatform.LinuxPlayer;
#elif UNITY_WEBGL
				return RuntimePlatform.WebGLPlayer;
#else
				return Application.platform;
#endif
			}
		}

		public static string PlatformStr
		{
			get
			{
#if UNITY_ANDROID
				return "Android";
#elif UNITY_IOS
				return "iOS";
#elif UNITY_STANDALONE_OSX
				return "OSX";
#elif UNITY_STANDALONE_WIN
				return "Windows";
#elif UNITY_STANDALONE_LINUX
				return "Linux";
#elif UNITY_WEBGL
				return "WebGL";
#else
				return "Unknown";
#endif
			}
		}
	}
}
