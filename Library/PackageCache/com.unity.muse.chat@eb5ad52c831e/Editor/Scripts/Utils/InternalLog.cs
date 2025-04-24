using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Unity.Muse.Chat
{
    /// <summary>
    /// Define MUSE_INTERNAL to see the logs.
    /// </summary>
    internal static class InternalLog
    {
        [Conditional("MUSE_INTERNAL")]
        public static void Log(object message)
        {
            Debug.Log(message);
        }

        [Conditional("MUSE_INTERNAL")]
        public static void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        [Conditional("MUSE_INTERNAL")]
        public static void LogError(object message)
        {
            Debug.LogError(message);
        }

        [Conditional("MUSE_INTERNAL")]
        public static void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
