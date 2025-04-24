using System;
using System.IO;
using StyleTrainer.Backend;
using Unity.Muse.Sprite.Common.DebugConfig;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer.Editor
{
    class Preference : SettingsProvider, IDisposable
    {
        const string k_SettingsPath = "Project/Style Trainer";

        public static void Open()
        {
            SettingsService.OpenProjectSettings(k_SettingsPath);
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new Preference();
        }

        static StyleTrainerProjectData s_Project;
        StyleTrainerProjectDataEditor m_ProjectDataInspector;
        StyleTrainerConfigEditor m_StyleTrainerConfigEditor;
        IMGUIContainer m_CacheInspector;

        public Preference()
            : base(k_SettingsPath, SettingsScope.Project, new[] { "Style Trainer", "Muse" })
        {
            label = "Style Trainer";

            activateHandler = ActivateHandler;
            deactivateHandler = DeactivateHandler;
        }

        void DeactivateHandler()
        {
            if (m_ProjectDataInspector is not null) m_ProjectDataInspector.Dispose();
            m_ProjectDataInspector = null;

            if (m_StyleTrainerConfigEditor is not null) m_StyleTrainerConfigEditor.Dispose();
            m_StyleTrainerConfigEditor = null;

            if (m_CacheInspector is not null) m_CacheInspector.Dispose();
            m_CacheInspector = null;
        }

        void ActivateHandler(string text, VisualElement element)
        {
            var styleTrainerProjectData = project;
            var styleTrainerConfig = StyleTrainerConfig.config;

            var newScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            element.Add(newScrollView);

            newScrollView.Add(new Label(label)
            {
                style =
                {
                    fontSize = 20
                }
            });

            if (DebugConfig.developerMode)
            {
                newScrollView.Add(new Label("Developer Mode Settings")
                {
                    style =
                    {
                        height = 20,
                        borderTopWidth = 2,
                        borderTopColor = Color.gray,
                        borderBottomWidth = 2,
                        borderBottomColor = Color.gray
                    }
                });

                m_CacheInspector = new IMGUIContainer(CacheInspector);
                newScrollView.Add(m_CacheInspector);
            }

            m_ProjectDataInspector = UnityEditor.Editor.CreateEditor(styleTrainerProjectData) as StyleTrainerProjectDataEditor;
            if (m_ProjectDataInspector != null)
                m_ProjectDataInspector.CreateInspectorUI(newScrollView);

            m_StyleTrainerConfigEditor = UnityEditor.Editor.CreateEditor(styleTrainerConfig) as StyleTrainerConfigEditor;
            if (m_StyleTrainerConfigEditor != null)
                m_StyleTrainerConfigEditor.CreateInspectorUI(newScrollView);
        }

        void CacheInspector()
        {
            var databasePath = StyleTrainerConfig.config.artifactCachePath;
            if (File.Exists(databasePath))
            {
                var fi = new FileInfo(databasePath);
                GUILayout.Label($"Cache Size: {fi?.Length / 1000000f} MB");
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete Cache")) File.Delete(databasePath);
            if (GUILayout.Button("Show Cache File")) EditorUtility.RevealInFinder(databasePath);
            GUILayout.EndHorizontal();
        }

        public static StyleTrainerProjectData project
        {
            get
            {
                if (s_Project is null)
                    s_Project = StyleTrainerProjectData.instance;

                return s_Project;
            }
        }

        public void Dispose()
        {
            //not used for now
        }
    }
}