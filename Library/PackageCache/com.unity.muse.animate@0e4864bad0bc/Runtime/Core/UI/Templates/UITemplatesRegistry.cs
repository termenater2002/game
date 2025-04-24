using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Unity.Muse.Animate
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "UIRegistry", menuName = "Muse Animate Dev/UI/UI Registry")]
#endif
    class UITemplatesRegistry : ScriptableObject
    {
        public List<UITemplateDefinition> Templates
        {
            get
            {
                UpdateRegistryIfNeeded();
                return m_Templates;
            }
        }

        [SerializeField]
        List<UITemplateDefinition> m_Templates = new();

        [NonSerialized]
        Dictionary<string, UITemplateDefinition> m_Registry = new();

        [NonSerialized]
        bool m_RegistryUpToDate;

        public bool TryGetUITemplate(string name, out UITemplateDefinition definition)
        {
            UpdateRegistryIfNeeded();
            return m_Registry.TryGetValue(name, out definition);
        }

        public void ListAllAssets()
        {
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets($"t: {nameof(UITemplateDefinition)}");

            m_Templates.Clear();

            for (int i = 0; i < guids.Length; i++)
            {
                var template = AssetDatabase.LoadAssetAtPath<UITemplateDefinition>(AssetDatabase.GUIDToAssetPath(guids[i]));
                m_Templates.Add(template);
            }

            AssetDatabase.SaveAssets();

            m_RegistryUpToDate = false;
            UpdateRegistryIfNeeded();
#endif
        }

        void UpdateRegistryIfNeeded()
        {
            if (m_RegistryUpToDate)
                return;

            m_Registry.Clear();

            foreach (var uiTemplateDefinition in m_Templates)
            {
                if (m_Registry.ContainsKey(uiTemplateDefinition.Name))
                    AssertUtils.Fail($"Template is already registered: {uiTemplateDefinition.Name}");

                m_Registry[uiTemplateDefinition.Name] = uiTemplateDefinition;
            }

            m_RegistryUpToDate = true;
        }
    }
}
