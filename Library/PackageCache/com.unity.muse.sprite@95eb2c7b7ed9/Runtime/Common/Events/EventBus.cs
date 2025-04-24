using System;
using System.Collections.Generic;
using Unity.Muse.Common;
using UnityEngine.Events;
using UnityEngine.UIElements;

#pragma warning disable 0067

namespace Unity.Muse.Sprite.Common.Events
{
    [Serializable]
    class EventBus : IModelData
    {
        Dictionary<Type, UnityEventBase> m_Events = new Dictionary<Type, UnityEventBase>();
        Queue<IEvent> m_QueueEvent = new Queue<IEvent>();
        int m_SendEventCount = 0;
        public void RegisterEvent<T>(UnityAction<T> action) where T : IEvent, new()
        {
            var type = typeof(T);
            m_Events.TryAdd(type, new UnityEvent<T>());
            var evt = m_Events[type];
            ((UnityEvent<T>)evt).AddListener(action);
        }

        public void UnregisterEvent<T>(UnityAction<T> action)
        {
            var type = typeof(T);
            if (m_Events.TryGetValue(type, out var evt))
            {
                ((UnityEvent<T>)evt).RemoveListener(action);
            }
        }

        public void SendEvent<T>(T arg, bool queue = false) where T : IEvent
        {
            if (m_SendEventCount > 0 && queue)
            {
                m_QueueEvent.Enqueue(arg);
                return;
            }
            var type = typeof(T);
            if (m_Events.TryGetValue(type, out var evt))
            {
                m_SendEventCount++;
                ((UnityEvent<T>)evt).Invoke(arg);
                m_SendEventCount--;
            }
            ClearQueue();
        }

        void ClearQueue()
        {
            if (m_SendEventCount == 0 && m_QueueEvent.Count > 0)
            {
                m_QueueEvent.Dequeue().SendEvent(this);
            }
        }


        public VisualElement GetInspector()
        {
            return null;
        }

        public void Clear()
        {
            m_QueueEvent.Clear();
            m_Events.Clear();
        }

        public event Action OnModified = () => { };
        public event Action OnSaveRequested;
    }
}