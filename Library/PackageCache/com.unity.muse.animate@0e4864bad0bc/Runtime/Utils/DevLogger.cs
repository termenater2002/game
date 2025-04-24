using UnityEngine;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Custom logger class than can be activated by defining UNITY_MUSE_DEV.
    /// </summary>
    /// <remarks>
    /// Note the naming scheme of the class and logging methods. This is Unity convention for logging classes.
    /// If the class name ends with "Logger", and the methods start with "Log", double-clicking on a log message
    /// in the console will take to you where the log method was called, and not inside this class.
    /// </remarks>
    static class DevLogger
    {
        public static TraceLevel TraceLevel => ApplicationConstants.TraceLevel;
        
        [Conditional("UNITY_MUSE_DEV")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogSeverity(TraceLevel severity, string message)
        {
            if (TraceLevel < severity)
                return;
            
            switch (severity)
            {
                case > TraceLevel.Warning:
                    Debug.Log(message);
                    break;
                case TraceLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case TraceLevel.Error:
                    Debug.LogError(message);
                    if (UnityEngine.Application.isPlaying)
                    {
                        Debug.Break();
                    }
                    break;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogInfo(string message)
        {
            LogSeverity(TraceLevel.Info, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(string message)
        {
            LogSeverity(TraceLevel.Error, message);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning(string message)
        {
            LogSeverity(TraceLevel.Warning, message);
        }
    }
}
