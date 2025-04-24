using System;
using Unity.Muse.Sprite.Common.DebugConfig;
using Unity.Muse.StyleTrainer.Debug;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer.Editor
{
    [CustomEditor(typeof(StyleTrainerConfig))]
    class StyleTrainerConfigEditor : UnityEditor.Editor, IDisposable
    {
        SerializedProperty m_DebugLog;
        SerializedProperty m_LogToFile;
        bool m_Foldout;

        void OnEnable()
        {
            m_DebugLog = serializedObject.FindProperty("debugLog");
            m_LogToFile = serializedObject.FindProperty("logToFile");
        }

        void DeveloperUI()
        {
            m_Foldout = EditorGUILayout.Foldout(m_Foldout, "Style Trainer Config");
            if (m_Foldout)
            {
                base.OnInspectorGUI();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reveal Log File")) StyleTrainerDebug.OpenLogFile();

                if (GUILayout.Button("Clear Log File")) StyleTrainerDebug.ClearLogFile();

                GUILayout.EndHorizontal();
            }
        }

        void ReleaseUI()
        {
            serializedObject.Update();
            m_Foldout = EditorGUILayout.Foldout(m_Foldout, "Style Trainer Config");
            if (m_Foldout)
            {
                EditorGUILayout.PropertyField(m_DebugLog);
                EditorGUILayout.PropertyField(m_LogToFile);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reveal Log File")) StyleTrainerDebug.OpenLogFile();

                if (GUILayout.Button("Clear Log File")) StyleTrainerDebug.ClearLogFile();

                GUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void CreateInspectorUI(VisualElement element)
        {
            VisualElement imguiContainer;
            if (DebugConfig.developerMode)
                imguiContainer = new IMGUIContainer(DeveloperUI);
            else
                imguiContainer = new IMGUIContainer(ReleaseUI);
            element.Add(imguiContainer);
        }

        public void Dispose()
        {
            // not used for now.
        }
    }
}