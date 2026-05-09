#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DeepForestLabs.Logger;
using Cysharp.Text;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs
{
	[Serializable]
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public struct AnalyticsStringValues : ISerializationCallbackReceiver
	{
		[SerializeField] private bool _suppressAnalyticsCalls;
		[SerializeField] private string _str1;
		[SerializeField] private string? _str2;
		[SerializeField] private string? _str3;
		[SerializeField] private string? _str4;
		[SerializeField] private string? _str5;
		[SerializeField] private bool _hasAmount;
		[SerializeField] private float _amount;
		[SerializeField] private KeyValuePair[]? _extraData;

		internal Dictionary<string, object?>? _cachedExtraData;

		public bool SuppressAnalyticsCalls => _suppressAnalyticsCalls;

		public string str1
		{
			get
			{
				//Log.Assert(_str1 != null, "Missing required value for {0} in {1}.{2}", nameof(str1),new StackTrace().GetFrame(1).GetMethod().GetType().Name, nameof(AnalyticsStringValues));
				return _str1;
			}
			set => _str1 = value;
		}

		public string? str2
		{
			get => string.IsNullOrEmpty(_str2) ? null : _str2;
			set => _str2 = value;
		}

		public string? str3
		{
			get => string.IsNullOrEmpty(_str3) ? null : _str3;
			set => _str3 = value;
		}

		public string? str4
		{
			get => string.IsNullOrEmpty(_str4) ? null : _str4;
			set => _str4 = value;
		}

		public string? str5
		{
			get => string.IsNullOrEmpty(_str5) ? null : _str5;
			set => _str5 = value;
		}

		public double? amount
		{
			get => !_hasAmount ? null : _amount;
			set => _amount = value != null ? (int)value.Value : 0.0f;
		}

		public Dictionary<string, object?> extra_data
		{
			get => _cachedExtraData ??= CreateExtraData();
			set => _cachedExtraData = value;
		}

		private Dictionary<string, object?> CreateExtraData()
		{
			_cachedExtraData ??= new Dictionary<string, object?>();
			if (_extraData == null || _extraData.Length == 0)
			{
				return _cachedExtraData;
			}

			Dictionary<string, object?> extraData = new();
			foreach (KeyValuePair pair in _extraData)
			{
				//Log.Assert(extraData.ContainsKey(pair.Key), "extraData.ContainsKey({0}) in {1} named {2}", pair.Key, nameof(AnalyticsStringValues), _str1);

				extraData.TryAdd(pair.Key, pair.Value);
			}

			return extraData;
		}

		public AnalyticsStringValues(bool suppressAnalyticsCalls, string str1, string? str2, string? str3, string? str4,
			string? str5,
			double? amount, Dictionary<string, object?>? cachedExtraData)
		{
			_suppressAnalyticsCalls = suppressAnalyticsCalls;
			_str1 = str1;
			_str2 = str2;
			_str3 = str3;
			_str4 = str4;
			_str5 = str5;
			_hasAmount = amount.HasValue;
			_amount = (float)(amount != null ? amount.Value : 0.0f);
			_cachedExtraData = cachedExtraData;
			_extraData = default;
		}

		private enum SerializedPropertyType
		{
			Integer = 0,
			Boolean = 1,
			Float = 2,
			String = 3,
		}

		[Serializable]
		private struct KeyValuePair
		{
			[SerializeField] private string _key;
			[SerializeField] private SerializedPropertyType _valueType;
			[SerializeField] private bool _boolean;
			[SerializeField] private int _integer;
			[SerializeField] private float _float;
			[SerializeField] private string? _string;

			public KeyValuePair(KeyValuePair<string, object?> kvp)
			{
				_key = kvp.Key;
				_boolean = default;
				_integer = default;
				_float = default;
				_float = default;
				_string = null;

				object? value = kvp.Value;
				if (value is bool b)
				{
					_boolean = b;
					_valueType = SerializedPropertyType.Integer;
				}
				else if (value is int i)
				{
					_integer = i;
					_valueType = SerializedPropertyType.Boolean;
				}
				else if (value is float f)
				{
					_float = f;
					_valueType = SerializedPropertyType.Float;
				}
				else if (value is double d)
				{
					_float = (float)d;
					_valueType = SerializedPropertyType.String;
				}
				else if (value is string s)
				{
					_string = s;
					_valueType = SerializedPropertyType.String;
				}
				else
				{
					throw new NotSupportedException(ZString.Format("{0}.{1} does not support {2}.{3}.",
						nameof(AnalyticsStringValues), nameof(Key), nameof(SerializedPropertyType), value?.GetType()));
				}
			}

			public string Key => _key;

			public object? Value => GetValue();

			private object? GetValue()
			{
				switch (_valueType)
				{
					case SerializedPropertyType.Boolean:
						return _boolean;
					case SerializedPropertyType.Integer:
						return _integer;
					case SerializedPropertyType.Float:
						return _float;
					case SerializedPropertyType.String:
						return _string;

					// Can support if needed. Would add more serialized field(s) if supported.
					//case SerializedPropertyType.Color:
					//case SerializedPropertyType.LayerMask:
					//case SerializedPropertyType.Vector2:
					//case SerializedPropertyType.Vector3:
					//case SerializedPropertyType.Vector4:
					//case SerializedPropertyType.Rect:
					//case SerializedPropertyType.Bounds:
					//case SerializedPropertyType.Quaternion:
					//case SerializedPropertyType.Vector2Int:
					//case SerializedPropertyType.Vector3Int:
					//case SerializedPropertyType.RectInt:
					//case SerializedPropertyType.BoundsInt:
					//case SerializedPropertyType.Hash128:

					// Cannot Support
					//case SerializedPropertyType.Generic:
					//case SerializedPropertyType.ObjectReference:
					//case SerializedPropertyType.ArraySize:
					//case SerializedPropertyType.Character:
					//case SerializedPropertyType.AnimationCurve:
					//case SerializedPropertyType.ExposedReference:
					//case SerializedPropertyType.FixedBufferSize:
					//case SerializedPropertyType.ManagedReference:
					//case SerializedPropertyType.Gradient:
					//case SerializedPropertyType.Enum:
					//default:

				}

				throw new NotSupportedException(ZString.Format("{0}.{1} does not support {2}.{3}.",
					nameof(AnalyticsStringValues), nameof(Key), nameof(SerializedPropertyType), _valueType));
			}

			private bool SetValue(object? value)
			{
				switch (_valueType)
				{
					case SerializedPropertyType.Boolean:
						_boolean = (bool)value!;
						break;
					case SerializedPropertyType.Integer:
						_integer = (int)value!;
						break;
					case SerializedPropertyType.Float:
						_float = (float)value!;
						break;
					case SerializedPropertyType.String:
						_string = (string?)value ?? string.Empty;
						break;

					// Can support if needed. Would add more serialized field(s) if supported.
					//case SerializedPropertyType.Color:
					//case SerializedPropertyType.LayerMask:
					//case SerializedPropertyType.Vector2:
					//case SerializedPropertyType.Vector3:
					//case SerializedPropertyType.Vector4:
					//case SerializedPropertyType.Rect:
					//case SerializedPropertyType.Bounds:
					//case SerializedPropertyType.Quaternion:
					//case SerializedPropertyType.Vector2Int:
					//case SerializedPropertyType.Vector3Int:
					//case SerializedPropertyType.RectInt:
					//case SerializedPropertyType.BoundsInt:
					//case SerializedPropertyType.Hash128:

					// Cannot Support
					//case SerializedPropertyType.Generic:
					//case SerializedPropertyType.ObjectReference:
					//case SerializedPropertyType.ArraySize:
					//case SerializedPropertyType.Character:
					//case SerializedPropertyType.AnimationCurve:
					//case SerializedPropertyType.ExposedReference:
					//case SerializedPropertyType.FixedBufferSize:
					//case SerializedPropertyType.ManagedReference:
					//case SerializedPropertyType.Gradient:
					//case SerializedPropertyType.Enum:
					//default:
				}

				throw new NotSupportedException(ZString.Format("{0}.{1} does not support {2}.{3}.",
					nameof(AnalyticsStringValues), nameof(Key), nameof(SerializedPropertyType), _valueType));
			}
		}

		public void OnBeforeSerialize()
		{
			if (_cachedExtraData == null || _cachedExtraData.Count == 0)
			{
				_extraData = Array.Empty<KeyValuePair>();
				return;
			}

			List<KeyValuePair> extraData = new List<KeyValuePair>();
			_cachedExtraData.Clear();
			foreach (KeyValuePair<string, object?> kvp in _cachedExtraData)
			{
				//Log.Assert(_cachedExtraData.ContainsKey(kvp.Key), "extraData.ContainsKey({0}) in {1} named {2}", kvp.Key, nameof(AnalyticsStringValues), _str1);
				extraData.Add(new KeyValuePair(kvp));
			}

			_extraData = extraData.ToArray();
		}

		public void OnAfterDeserialize()
		{
			if (_extraData == null || _extraData.Length == 0)
			{
				return;
			}

			_cachedExtraData ??= new Dictionary<string, object?>();
			_cachedExtraData.Clear();
			foreach (KeyValuePair pair in _extraData)
			{
				// Log.Assert(!_cachedExtraData.ContainsKey(pair.Key), "extraData.ContainsKey({0}) in {1} named {2}",
				// 	pair.Key, nameof(AnalyticsStringValues), _str1);

				_cachedExtraData.TryAdd(pair.Key, pair.Value);
			}
		}
	}
}
#nullable disable