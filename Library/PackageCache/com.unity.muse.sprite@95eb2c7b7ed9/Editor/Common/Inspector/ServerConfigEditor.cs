using Unity.Muse.Sprite.Common.Backend;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Sprite.Common.Editor.Inspector
{
    [CustomEditor(typeof(ServerConfig))]
    class ServerConfigEditor : UnityEditor.Editor
    {
        // SerializedProperty m_ModelGuid;
        SerializedProperty m_DebugMode;

        static string[] s_Models = new[]{
            "spritegen_lostcryptmodel"
        };
        int m_ModelIndex;
        void OnEnable()
        {
            // m_ModelGuid = serializedObject.FindProperty("model_guid");
            m_DebugMode = serializedObject.FindProperty("m_DebugMode");
            // m_ModelIndex = System.Array.IndexOf(s_Models, m_ModelGuid.stringValue);
            // if (m_ModelIndex <= 0)
            //     m_ModelIndex = 0;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if(m_DebugMode.intValue > 0 && !Unsupported.IsDeveloperMode())
                EditorGUILayout.HelpBox("Debug mode is enabled but you are not in developer mode.", MessageType.Warning);
            var hasModifiedProperties = EditorGUI.EndChangeCheck();            
            serializedObject.ApplyModifiedProperties();
            if (hasModifiedProperties)
            {
                Startup.VersionCheck();
            }
        }
    }
}
