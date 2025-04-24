using System;
using System.Collections.Generic;
using Unity.Muse.Animate.Prop;
using UnityEngine;

namespace Unity.Muse.Animate
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "PropRegistry", menuName = "Muse Animate Dev/Prop Registry")]
#endif
    class PropRegistry : ScriptableObject
    {
        public List<PropDefinition> VisibleEntries
        {
            get
            {
                UpdateRegistryIfNeeded();
                return m_VisibleEntries;
            }
        }

        [SerializeField]
        List<PropDefinition> m_Definitions = new ();

        [NonSerialized]
        Dictionary<string, PropDefinition> m_Registry = new();

        [NonSerialized]
        List<PropDefinition> m_VisibleEntries = new();

        [NonSerialized]
        bool m_RegistryUpToDate;

        public bool TryGetPropInfo(string prefabId, out PropDefinition propDefinition)
        {
            UpdateRegistryIfNeeded();
            return m_Registry.TryGetValue(prefabId, out propDefinition);
        }

        void UpdateRegistryIfNeeded()
        {
            if (m_RegistryUpToDate)
                return;

            m_Registry.Clear();
            m_VisibleEntries.Clear();

            foreach (var propDefinition in m_Definitions)
            {
                if (m_Registry.ContainsKey(propDefinition.ID))
                    AssertUtils.Fail($"Prefab ID is already registered: {propDefinition.ID}");

                m_Registry[propDefinition.ID] = propDefinition;

                if (!propDefinition.IsHidden)
                    m_VisibleEntries.Add(propDefinition);
            }

            m_RegistryUpToDate = true;
        }
    }
}
