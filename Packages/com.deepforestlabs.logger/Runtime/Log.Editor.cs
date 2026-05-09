#nullable enable
using System;
using System.Diagnostics;
using Cysharp.Text;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Logger
{
    public static partial class Log
    {
#if UNITY_EDITOR
        private const string EDITOR_LOG_PREFIX = "<color=\"white\">EDITOR</color>: ";
        private const string EDITOR_WARNING_PREFIX = "<color=\"yellow\">EDITOR WARNING</color>: ";
        private const string EDITOR_ERROR_PREFIX = "<color=\"red\">EDITOR ERROR</color>: ";
#else
        private const string EDITOR_LOG_PREFIX = "";
        private const string EDITOR_WARNING_PREFIX = "";
        private const string EDITOR_ERROR_PREFIX = "";
#endif
        
        [Conditional("UNITY_EDITOR")]
        [HideInCallstack]
        public static void EditorException(Exception exception)
        {
            Exception(exception);
        }
        
        [Conditional("UNITY_EDITOR")]
        [HideInCallstack]
        public static void Editor(string message)
        {
            Editor((Object?)null, message);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Editor<T1>(string format, T1 arg1)
        {
            Editor((Object?)null, format, arg1);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Editor<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Editor((Object?)null, format, arg1, arg2);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Editor<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Editor((Object?)null, format, arg1, arg2, arg3);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Editor<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Editor((Object?)null, format, arg1, arg2, arg3, arg4);
        }
        
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Editor<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Editor(null, format, arg1, arg2, arg3, arg4, arg5);
        }
        
        [Conditional("UNITY_EDITOR")]
        [HideInCallstack]
        public static void Editor(Object? context, string message)
        {
            Info(context, ZString.Concat(EDITOR_LOG_PREFIX, message));
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Editor<T1>(Object? context, string format, T1 arg1)
        {
            Info(context, ZString.Concat(EDITOR_LOG_PREFIX, format), arg1);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Editor<T1, T2>(Object? context, string format, T1 arg1, T2 arg2)
        {
            Info(context, ZString.Concat(EDITOR_LOG_PREFIX, format), arg1, arg2);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Editor<T1, T2, T3>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Info(context, ZString.Concat(EDITOR_LOG_PREFIX, format), arg1, arg2, arg3);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Editor<T1, T2, T3, T4>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Info(context, ZString.Concat(EDITOR_LOG_PREFIX, format), arg1, arg2, arg3, arg4);
        }
        
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Editor<T1, T2, T3, T4, T5>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Info(context, ZString.Concat(EDITOR_LOG_PREFIX, format), arg1, arg2, arg3, arg4, arg5);
        }
        
        [Conditional("UNITY_EDITOR")]
        [HideInCallstack]
        public static void EditorWarning(string message)
        {
            EditorWarning((Object?)null, message);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorWarning<T1>(string format, T1 arg1)
        {
            EditorWarning((Object?)null, format, arg1);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorWarning<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            EditorWarning((Object?)null, format, arg1, arg2);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorWarning<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            EditorWarning((Object?)null, format, arg1, arg2, arg3);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorWarning<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            EditorWarning((Object?)null, format, arg1, arg2, arg3, arg4);
        }
        
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorWarning<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            EditorWarning(null, format, arg1, arg2, arg3, arg4, arg5);
        }
        
        [Conditional("UNITY_EDITOR")]
        [HideInCallstack]
        public static void EditorWarning(Object? context, string message)
        {
            Warning(context, ZString.Concat(EDITOR_WARNING_PREFIX, message));
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorWarning<T1>(Object? context, string format, T1 arg1)
        {
            Warning(context, ZString.Concat(EDITOR_WARNING_PREFIX, format), arg1);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorWarning<T1, T2>(Object? context, string format, T1 arg1, T2 arg2)
        {
            Warning(context, ZString.Concat(EDITOR_WARNING_PREFIX, format), arg1, arg2);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorWarning<T1, T2, T3>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Warning(context, ZString.Concat(EDITOR_WARNING_PREFIX, format), arg1, arg2, arg3);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorWarning<T1, T2, T3, T4>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Warning(context, ZString.Concat(EDITOR_WARNING_PREFIX, format), arg1, arg2, arg3, arg4);
        }
        
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorWarning<T1, T2, T3, T4, T5>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Warning(context, ZString.Concat(EDITOR_WARNING_PREFIX, format), arg1, arg2, arg3, arg4, arg5);
        }
        
        [Conditional("UNITY_EDITOR")]
        [HideInCallstack]
        public static void EditorError(string message)
        {
            EditorError((Object?)null, message);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorError<T1>(string format, T1 arg1)
        {
            EditorError((Object?)null, format, arg1);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorError<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            EditorError((Object?)null, format, arg1, arg2);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorError<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            EditorError((Object?)null, format, arg1, arg2, arg3);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorError<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            EditorError((Object?)null, format, arg1, arg2, arg3, arg4);
        }
        
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorError<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            EditorError(null, format, arg1, arg2, arg3, arg4, arg5);
        }
        
        [Conditional("UNITY_EDITOR")]
        [HideInCallstack]
        public static void EditorError(Object? context, string message)
        {
            Error(context, ZString.Concat(EDITOR_ERROR_PREFIX, message));
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorError<T1>(Object? context, string format, T1 arg1)
        {
            Error(context, ZString.Concat(EDITOR_ERROR_PREFIX, format), arg1);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorError<T1, T2>(Object? context, string format, T1 arg1, T2 arg2)
        {
            Error(context, ZString.Concat(EDITOR_ERROR_PREFIX, format), arg1, arg2);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorError<T1, T2, T3>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Error(context, ZString.Concat(EDITOR_ERROR_PREFIX, format), arg1, arg2, arg3);
        }
		
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorError<T1, T2, T3, T4>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Error(context, ZString.Concat(EDITOR_ERROR_PREFIX, format), arg1, arg2, arg3, arg4);
        }
        
        [Conditional("UNITY_EDITOR")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void EditorError<T1, T2, T3, T4, T5>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Error(context, ZString.Concat(EDITOR_ERROR_PREFIX, format), arg1, arg2, arg3, arg4, arg5);
        }
    }
}