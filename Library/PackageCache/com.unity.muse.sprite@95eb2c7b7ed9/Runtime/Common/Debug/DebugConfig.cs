using System;
using System.IO;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Backend;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Muse.Sprite.Common.DebugConfig
{
    //[CreateAssetMenu(fileName = "DebugConfig.asset", menuName = "Muse/Sprite/DebugConfig")]
    internal class DebugConfig : ScriptableObject
    {
        public bool requestDebugLog = false;
        public bool responseDebugLog = false;
        public bool refineImageDebug = false;
        public bool generateImageDebug = false;
        const string k_DebugDirectory = "Assets/SpriteMuseDebug";
        const string k_LogFilePath = "Logs/MuseSpriteWeb.log";
        const string k_LogStringFormat = "[{0}] {1}\n";
        static public DebugConfig instance => 
            ResourceManager.Load<DebugConfig>(PackageResources.spriteGeneratorDebugConfig);

        static void CheckDirectory()
        {
#if UNITY_EDITOR
            if (!Directory.Exists(k_DebugDirectory))
                Directory.CreateDirectory(k_DebugDirectory);
#endif
        }

        public static void DebugGenerateImage(byte[] raw)
        {
            var i = instance;
            if (i == null || i.generateImageDebug == false || raw?.Length == 0)
                return;
            CheckDirectory();
            BackendUtilities.SaveBytesToFile(Path.Combine(k_DebugDirectory, "generateDebug.png"), raw);
        }

        public static void DebugRefineSrcImage(byte[] raw)
        {
            var i = instance;
            if (i == null || i.refineImageDebug == false || raw?.Length == 0)
                return;
            CheckDirectory();
            BackendUtilities.SaveBytesToFile(Path.Combine(k_DebugDirectory, "refineSrcDebug.png"), raw);
        }

        public static void DebugRefineDoodleImage(byte[] raw)
        {
            var i = instance;
            if (i == null || i.refineImageDebug == false || raw?.Length == 0)
                return;
            CheckDirectory();
            BackendUtilities.SaveBytesToFile(Path.Combine(k_DebugDirectory, "refineDoodleDebug.png"), raw);
        }

        public static void LogRequest(Func<string> getLog)
        {
            var i = instance;
            if (i == null || i?.requestDebugLog == false)
                return;
            BackendUtilities.LogFile(k_LogFilePath, string.Format(k_LogStringFormat, System.DateTime.Now, getLog()));
        }

        public static void LogResonse(Func<string> getLog)
        {
            var i = instance;
            if (i == null || i.responseDebugLog == false)
                return;
            BackendUtilities.LogFile(k_LogFilePath, string.Format(k_LogStringFormat, System.DateTime.Now, getLog()));
        }

        public void ClearLogFile()
        {
#if UNITY_EDITOR
            File.Delete(k_LogFilePath);
#endif
        }

        public void OpenLogFile()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(k_LogFilePath);
#endif
        }

        public static bool developerMode
        {
            get =>
#if UNITY_EDITOR
                Unsupported.IsDeveloperMode();
#else
                false;
#endif
        }
    }
}