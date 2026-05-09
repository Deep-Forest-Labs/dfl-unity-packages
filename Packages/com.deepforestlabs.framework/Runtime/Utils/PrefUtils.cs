#nullable enable
using System;
using System.Globalization;
using UnityEngine;

namespace DeepForestLabs.Utils
{
	public static class PrefUtils
	{
		//Adds to the functionality of player prefs

		public static bool HasKey(string key)
		{
			return PlayerPrefs.HasKey(key);
		}

		public static void SetBool(string key, bool value)
		{
			PlayerPrefs.SetInt(key, value ? 1 : 0);
			PlayerPrefs.Save();
		}

		public static bool GetBool(string key, bool defaultValue = false)
		{
			return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
		}

		public static void SetString(string key, string value)
		{
			PlayerPrefs.SetString(key, value);
			PlayerPrefs.Save();
		}

		public static string GetString(string key, string? defaultValue = null)
		{
			return PlayerPrefs.GetString(key, defaultValue);
		}

		public static void SetInt(string key, int value)
		{
			PlayerPrefs.SetInt(key, value);
			PlayerPrefs.Save();
		}

		public static int GetInt(string key, int defaultValue = 0)
		{
			return PlayerPrefs.GetInt(key, defaultValue);
		}

		public static void SetFloat(string key, float value)
		{
			PlayerPrefs.SetFloat(key, value);
			PlayerPrefs.Save();
		}

		public static float GetFloat(string key, float defaultValue = 0)
		{
			return PlayerPrefs.GetFloat(key, defaultValue);
		}

		public static void SetDateTime(string key, DateTime value)
		{
			PlayerPrefs.SetString(key, value.ToString(CultureInfo.InvariantCulture));
			PlayerPrefs.Save();
		}

		public static DateTime GetDateTime(string key, string? defaultValue = null)
		{
			string dateTimeInput = PlayerPrefs.GetString(key, defaultValue);
			return DateTime.TryParse(dateTimeInput, out DateTime dateTime) ? dateTime : DateTime.MinValue;
		}

		public static void DeleteKey(string key)
		{
			PlayerPrefs.DeleteKey(key);
		}

		public static void DeleteAll()
		{
			PlayerPrefs.DeleteAll();
		}
	}
}
#nullable disable