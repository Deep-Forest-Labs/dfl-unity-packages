#nullable enable
using System.Runtime.CompilerServices;

namespace DeepForestLabs
{
    public static partial class AnalyticsStringValuesHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Fallback(this AnalyticsStringValues values, string str2) =>
            FallbackValues(values, str2, null, null, null, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Fallback(this AnalyticsStringValues values, string str2, string str3) =>
            FallbackValues(values, str2, str3, null, null, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Fallback(this AnalyticsStringValues values, string str2, string str3,
            string str4) =>
            FallbackValues(values, str2, str3, str4, null, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Fallback(this AnalyticsStringValues values, string str2, string str3,
            string str4, string str5) =>
            FallbackValues(values, str2, str3, str4, str5, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Fallback(this AnalyticsStringValues values, string str2, string str3,
            string str4, string str5, double amount) =>
            FallbackValues(values, str2, str3, str4, str5, amount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues FallbackValues(this AnalyticsStringValues values, string? str2,
            string? str3, string? str4, string? str5, double? amount)
        {
            string? value2 = values.str2;
            string? value3 = values.str3;
            string? value4 = values.str4;
            string? value5 = values.str5;
            double? valueAmount = values.amount;

            if (!string.IsNullOrEmpty(str2) && string.IsNullOrEmpty(values.str2))
            {
                value2 = str2;
            }

            if (!string.IsNullOrEmpty(str3) && string.IsNullOrEmpty(values.str3))
            {
                value3 = str3;
            }

            if (!string.IsNullOrEmpty(str4) && string.IsNullOrEmpty(values.str4))
            {
                value4 = str4;
            }

            if (!string.IsNullOrEmpty(str5) && string.IsNullOrEmpty(values.str5))
            {
                value5 = str5;
            }

            if (amount != null && values.amount == null)
            {
                valueAmount = amount;
            }

            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, value2, value3, value4, value5,
                valueAmount, values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues FallbackStr2(this AnalyticsStringValues values, string str2)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1,
                string.IsNullOrEmpty(values.str2) ? values.str2 : str2,
                values.str3, values.str4, values.str5, values.amount, values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues FallbackStr3(this AnalyticsStringValues values, string str3)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, values.str2,
                string.IsNullOrEmpty(values.str3) ? values.str3 : str3, values.str4, values.str5, values.amount,
                values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues FallbackStr4(this AnalyticsStringValues values, string str4)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, values.str2, values.str3,
                string.IsNullOrEmpty(values.str4) ? values.str4 : str4, values.str5, values.amount,
                values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues FallbackStr5(this AnalyticsStringValues values, string str5)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, values.str2, values.str3,
                values.str4,
                string.IsNullOrEmpty(values.str5) ? values.str5 : str5,
                values.amount, values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues FallbackAmount(this AnalyticsStringValues values, double amount)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, values.str2, values.str3,
                values.str4, values.str5,
                values.amount ?? amount,
                values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues FallbackExtraData(this AnalyticsStringValues values, string key,
            object value)
        {
            if (values._cachedExtraData == null || values.extra_data.ContainsKey(key))
            {
                return values;
            }

            values.extra_data.Add(key, value);
            return values;
        }
    }
}
#nullable disable