using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    [CustomEditor(typeof(Model))]
    class EditorModelAssetEditor : UnityEditor.Editor
    {
        Model Target => target as Model;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Open asset in Muse Window", MessageType.Info);
            if (GUILayout.Button("Open in Muse Window"))
            {
                OpenEditorTo(Target);
            }
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var doubleClickedAsset = EditorUtility.InstanceIDToObject(instanceID) as Model;
            if (doubleClickedAsset != null)
            {
                OpenEditorTo(doubleClickedAsset);
                return true;
            }

            return false;
        }

        internal static MuseEditor OpenEditorTo(Model asset)
        {
            MuseEditor window = null;
            var windows = GetAllInstances<MuseEditor>();
            foreach (var genWindow in windows)
            {
                if (genWindow.CurrentModel != asset) continue;

                window = genWindow;
                break;
            }

            if (!window)
            {
                window = CreateInstance<MuseEditor>();
                window.SetContext(asset);
            }

            window.Show();
            window.Focus();

            return window;
        }

        public static MuseEditor OpenWindowForMode(string mode)
        {
            var modeIndex = ModesFactory.GetModeIndexFromKey(mode);
            if (modeIndex == -1)
                return null;

            var model = CreateInstance<Model>();
            model.Initialize();
            model.ModeChanged(modeIndex);

            var wins = GetAllInstances<MuseEditor>();
            var window = wins.FirstOrDefault(w => w.CurrentModel == model);
            if (window != null)
            {
                window.Focus();
                return window;
            }

            return OpenEditorTo(model);
        }

        public static T[] GetAllInstances<T>() where T : EditorWindow
        {
            return Resources.FindObjectsOfTypeAll<T>().Where(window => window.GetType() == typeof(T)).ToArray();
        }

        public static string GetSavePath(Model currentModel, bool showDialog)
        {
            var defaultName = TextContent.DefaultAssetName(ModesFactory.GetModeData(currentModel.CurrentMode)?.title ?? "Muse Generator");
            var promptOperator = currentModel.CurrentOperators?.GetOperator<PromptOperator>();
            var fileName = promptOperator != null && !string.IsNullOrWhiteSpace(promptOperator.GetPrompt())
                ? promptOperator.GetPrompt()
                : defaultName;

            var invalidCharsPattern = $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]";
            var sanitizedFileName = Regex.Replace(fileName, invalidCharsPattern, "_");

            var directory = GlobalPreferences.GetMuseAssetGeneratedFolderPathFromMode(currentModel.CurrentMode);
            var path = ExporterHelpers.GetUniquePath(directory, sanitizedFileName, "asset");

            return showDialog ? EditorUtility.SaveFilePanelInProject(TextContent.savePanelTitle, Path.GetFileNameWithoutExtension(path), "asset", TextContent.savePanelMessage) : path;
        }
    }
}
