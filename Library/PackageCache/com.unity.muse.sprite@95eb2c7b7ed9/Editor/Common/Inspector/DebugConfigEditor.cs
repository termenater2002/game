using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Sprite.Common.Editor.Inspector
{
    [CustomEditor(typeof(DebugConfig.DebugConfig))]
    class DebugConfigEditor : UnityEditor.Editor
    {
        DebugConfig.DebugConfig targetConfig => (DebugConfig.DebugConfig)target;
        bool m_Foldout;
        public override void OnInspectorGUI()
        {
            m_Foldout = EditorGUILayout.Foldout(m_Foldout, "Sprite Muse Debug Config");
            if (m_Foldout)
            {
                base.OnInspectorGUI();
                if (GUILayout.Button("Reveal Log File"))
                {
                    targetConfig.OpenLogFile();
                }
                if (GUILayout.Button("Clear Log File"))
                {
                    targetConfig.ClearLogFile();
                }
            }
        }

        public void CreateInspectorUI(VisualElement element)
        {
            VisualElement imguiContainer;
            imguiContainer = new IMGUIContainer(OnInspectorGUI);
            element.Add(imguiContainer);
        }
    }
}