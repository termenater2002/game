using System;
using System.Collections.Generic;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.Sprite.Operators;
using UnityEngine.Events;

namespace Unity.Muse.Sprite.Data
{
    internal class OperatorOverridePublishers: IModelData
    {
        public event Action OnModified = () => { };
        public event Action OnSaveRequested = () => { };

        readonly List<IOperatorOverridePublisher> m_Publishers = new List<IOperatorOverridePublisher>();

        public void RegisterPublisher(IList<IOperatorOverridePublisher> publishers)
        {
            for (int i = 0; i < publishers.Count; ++i)
            {
                int j;
                for (j = 0; j < m_Publishers.Count; ++j)
                {
                    if(publishers[i] == m_Publishers[j])
                        break;
                }
                if(j >= m_Publishers.Count)
                    m_Publishers.Add(publishers[i]);
            }
            OnModified?.Invoke();
        }

        public void UnregisterPublisher(IList<IOperatorOverridePublisher> publishers)
        {
            for (int i = 0; i < publishers.Count; ++i)
            {
                m_Publishers.Remove(publishers[i]);
            }
            OnModified?.Invoke();
        }

        public void RegisterToPublisher<T>(UnityAction<T> callback) where T : class, IEvent, new()
        {
            for (int i = 0; i < m_Publishers.Count; ++i)
            {
                if(m_Publishers[i].SupportOverrideType<T>())
                    m_Publishers[i].RegisterOperatorOverrideEvent(callback);
            }
        }

        public void UnregisterFromPublisher<T>(UnityAction<T> callback) where T : class, IEvent, new()
        {
            for (int i = 0; i < m_Publishers.Count; ++i)
            {
                m_Publishers[i].UnRegisterOperatorOverrideEvent(callback);
            }
        }

        public T RequestCurrentPublisherData<T>() where T : class, IEvent, new()
        {
            for (int i = 0; i < m_Publishers.Count; ++i)
            {
                if (m_Publishers[i].SupportOverrideType<T>())
                    return m_Publishers[i].RequestCurrentOperatorOverride<T>();
            }
            return null;
        }

        public string GetPublisherName<T>() where T : class, IEvent, new()
        {
            int j;
            for (j = 0; j < m_Publishers.Count; ++j)
            {
                if (m_Publishers[j].SupportOverrideType<T>())
                    return m_Publishers[j].name;
            }
            return null;
        }

        public bool HavePublisher<T>() where T : class, IEvent, new()
        {
            int j;
            for (j = 0; j < m_Publishers.Count; ++j)
            {
                if (m_Publishers[j].SupportOverrideType<T>())
                    return true;
            }

            return false;
        }
    }
}