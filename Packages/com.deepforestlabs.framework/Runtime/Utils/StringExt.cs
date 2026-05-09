#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace DeepForestLabs.Utils
{
	public static class StringExt
	{
		public static bool IsNullOrEmpty([NotNullWhen(false)] this string? data)
		{
			return string.IsNullOrEmpty(data);
		}

		public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? data)
		{
			return string.IsNullOrWhiteSpace(data);
		}
	}
}
#nullable restore
