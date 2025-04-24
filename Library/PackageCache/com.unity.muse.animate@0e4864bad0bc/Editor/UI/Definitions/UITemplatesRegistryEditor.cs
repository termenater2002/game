using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Animate.Editor
{
    [CustomEditor(typeof(UITemplatesRegistry))]
    class UITemplatesRegistryEditor : UnityEditor.Editor
    {
        UITemplatesRegistry Registry => target as UITemplatesRegistry;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("List all available"))
            {
                Registry.ListAllAssets();
                serializedObject.Update();
                EditorUtility.SetDirty(Registry);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
