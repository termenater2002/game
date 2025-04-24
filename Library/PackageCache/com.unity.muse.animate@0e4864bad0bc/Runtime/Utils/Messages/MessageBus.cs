using System;
using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    class MessageBus
    {
        readonly Dictionary<Type, List<Delegate>> m_Subscribers = new();
        
        public void Subscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (!m_Subscribers.ContainsKey(type))
            {
                m_Subscribers[type] = new List<Delegate>();
            }

            m_Subscribers[type].Add(callback);
        }
        
        public void Unsubscribe<T>(Action<T> callback)
        {
            if (m_Subscribers.TryGetValue(typeof(T), out var subscribers))
            {
                subscribers.Remove(callback);
            }
        }
        
        public void Publish<T>(T message)
        {
            if (m_Subscribers.TryGetValue(typeof(T), out var subscribers))
            {
                foreach (var callback in subscribers)
                {
                    ((Action<T>)callback)(message);
                }
            }
        }
    }
}
