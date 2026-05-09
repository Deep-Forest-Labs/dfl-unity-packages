#nullable enable
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DeepForestLabs
{
    public static partial class AnalyticsStringValuesHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Override(this AnalyticsStringValues values, string str2) =>
            OverrideValues(values, str2, null, null, null, null, null);
		
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Override(this AnalyticsStringValues values, string str2, string str3) =>
            OverrideValues(values, str2, str3, null, null, null, null);
		
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Override(this AnalyticsStringValues values, string str2, string str3, string str4) =>
            OverrideValues(values, str2, str3, str4, null, null, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Override(this AnalyticsStringValues values, string str2, string str3, string str4, string str5) =>
            OverrideValues(values, str2, str3, str4, str5, null, null);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Override(this AnalyticsStringValues values, string str2, string str3, string str4, string str5, double amount) =>
            OverrideValues(values, str2, str3, str4, str5, amount, null);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues Override(this AnalyticsStringValues values, string str2, string str3, string str4, string str5, double amount, IReadOnlyDictionary<string, object?> extraData) =>
            OverrideValues(values, str2, str3, str4, str5, amount, extraData);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues OverrideValues(this AnalyticsStringValues values, string? str2 = null, 
            string? str3 = null, string? str4 = null, string? str5 = null, double? amount = null, 
            IReadOnlyDictionary<string, object?>? extraData = null)
        {
            string? value2 = values.str2;
            string? value3 = values.str3;
            string? value4 = values.str4;
            string? value5 = values.str5;
            double? valueAmount = values.amount;
            Dictionary<string, object?>? cachedExtraData = values._cachedExtraData;

            if (!string.IsNullOrEmpty(str2))
            {
                value2 = str2;
            }
			
            if (!string.IsNullOrEmpty(str3))
            {
                value3 = str3;
            }
			
            if (!string.IsNullOrEmpty(str4))
            {
                value4 = str4;
            }
			
            if (!string.IsNullOrEmpty(str5))
            {
                value5 = str5;
            }
			
            if (amount != null)
            {
                valueAmount = amount;
            }

            if (extraData != null)
            {
                cachedExtraData = values.extra_data;
                foreach (KeyValuePair<string, object?> pair in extraData)
                {
                    cachedExtraData[pair.Key] = pair.Value;
                }
            }
			
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, value2, value3, value4, value5,   
                valueAmount, cachedExtraData);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[Obsolete("//TODO [2.5.+] - Create JIRA - delete usage, remove access")]
        public static AnalyticsStringValues OverrideStr1(this AnalyticsStringValues values, string str1)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, str1, values.str2, values.str3, values.str4,
                values.str5, values.amount, values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues OverrideStr2(this AnalyticsStringValues values, string str2)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, str2, values.str3, values.str4,
                values.str5, values.amount, values._cachedExtraData);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues SetInputValue(this AnalyticsStringValues values, string input)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, input, values.str3, values.str4,
                values.str5, values.amount, values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues OverrideStr3(this AnalyticsStringValues values, string str3)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, values.str2, str3, values.str4,
                values.str5, values.amount, values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues OverrideStr4(this AnalyticsStringValues values, string str4)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, values.str2, values.str3, str4,
                values.str5, values.amount, values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues OverrideStr5(this AnalyticsStringValues values, string str5)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, values.str2, values.str3,
                values.str4, str5, values.amount, values._cachedExtraData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues OverrideAmount(this AnalyticsStringValues values, double amount)
        {
            return new AnalyticsStringValues(values.SuppressAnalyticsCalls, values.str1, values.str2, values.str3,
                values.str4, values.str5, amount, values._cachedExtraData);
        }
		
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues OverrideExtraData(this AnalyticsStringValues values, string key, object? value)
        {
            values.extra_data[key] = value;
            return values;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues OverrideExtraData(this AnalyticsStringValues values, KeyValuePair<string, object?> pair)
        {
            values.extra_data[pair.Key] = pair.Value;
            return values;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalyticsStringValues OverrideExtraData(this AnalyticsStringValues values, IReadOnlyDictionary<string, object?> pairs)
        {
            foreach (KeyValuePair<string, object?> pair in pairs)
            {
                values.extra_data[pair.Key] = pair.Value;
            }
            return values;
        }
    }
}
#nullable disable