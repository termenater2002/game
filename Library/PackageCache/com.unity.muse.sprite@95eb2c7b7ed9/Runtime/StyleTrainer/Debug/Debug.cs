using System.IO;
using Unity.Muse.Sprite.Common.Backend;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Muse.StyleTrainer.Debug
{
    static class StyleTrainerDebug
    {
        const string k_LogFilePath = "Logs/StyleTrainer.log";
        const string k_LogStringFormat = "[{0}]{2} {1}\n";

        public static void Log(string log)
        {
#if UNITY_EDITOR
            var config = StyleTrainerConfig.config;
            if (config != null)
            {
                if (config.logToFile)
                    BackendUtilities.LogFile(k_LogFilePath, string.Format(k_LogStringFormat, System.DateTime.Now, log, ""));

                if (config.debugLog)UnityEngine.Debug.Log(log);
            }
#endif
        }

        public static void LogWarning(string log)
        {
#if UNITY_EDITOR
            var config = StyleTrainerConfig.config;
            if (config != null && config.logToFile)
                BackendUtilities.LogFile(k_LogFilePath, string.Format(k_LogStringFormat, System.DateTime.Now, log, "[Warning]"));
#endif
            UnityEngine.Debug.LogWarning(log);
        }

        public static void LogError(string log)
        {
#if UNITY_EDITOR
            var config = StyleTrainerConfig.config;
            if (config != null && config.logToFile)
                BackendUtilities.LogFile(k_LogFilePath, string.Format(k_LogStringFormat, System.DateTime.Now, log, "[Error]"));
#endif
            UnityEngine.Debug.LogError(log);
        }

#if UNITY_EDITOR
        public static void OpenLogFile()
        {
            EditorUtility.RevealInFinder(k_LogFilePath);
        }
#endif
        public static void ClearLogFile()
        {
#if UNITY_EDITOR
            File.Delete(k_LogFilePath);
#endif
        }
    }
}