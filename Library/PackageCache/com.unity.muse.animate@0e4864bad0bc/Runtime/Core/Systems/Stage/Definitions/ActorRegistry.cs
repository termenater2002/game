using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace Unity.Muse.Animate
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "ActorRegistry", menuName = "Muse Animate Dev/Actor Registry")]
#endif
    class ActorRegistry : ScriptableObject
    {
        public List<ActorDefinition> VisibleEntries
        {
            get
            {
                UpdateRegistryIfNeeded();
                return m_VisibleEntries;
            }
        }

        [SerializeField]
        List<ActorDefinition> m_Definitions = new ();

        [NonSerialized]
        Dictionary<string, ActorDefinition> m_Registry = new();

        [NonSerialized]
        List<ActorDefinition> m_VisibleEntries = new();

        [NonSerialized]
        bool m_RegistryUpToDate;

        public bool TryGetActorDefinition(string prefabId, out ActorDefinition actorDefinition)
        {
            UpdateRegistryIfNeeded();
            return m_Registry.TryGetValue(prefabId, out actorDefinition);
        }

        void UpdateRegistryIfNeeded()
        {
            if (m_RegistryUpToDate)
                return;

            m_Registry.Clear();
            m_VisibleEntries.Clear();

            foreach (var actorDefinition in m_Definitions)
            {
                if (m_Registry.ContainsKey(actorDefinition.ID))
                    AssertUtils.Fail($"Prefab ID is already registered: {actorDefinition.ID}");

                m_Registry[actorDefinition.ID] = actorDefinition;

                if (!actorDefinition.IsHidden)
                    m_VisibleEntries.Add(actorDefinition);
            }

            m_RegistryUpToDate = true;
        }
    }
}
