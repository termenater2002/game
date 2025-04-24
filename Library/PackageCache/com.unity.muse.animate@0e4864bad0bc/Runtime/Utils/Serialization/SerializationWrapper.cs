using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class SerializationWrapper<T> : ISerializationCallbackReceiver
        where T : new()
    {
        static JsonSerializerSettings s_Settings = new()
        {
            MaxDepth = 50,
            ContractResolver = new UnityContractResolver()
        };

        public T Value
        {
            get => m_Value;
            set => m_Value = value;
        }

        [NonSerialized]
        T m_Value;

        [SerializeField]
        string m_SerializedValue;

        public SerializationWrapper(T value = default)
        {
            m_Value = value;
        }

        public void OnBeforeSerialize()
        {
            m_SerializedValue = m_Value != null ? JsonConvert.SerializeObject(m_Value, s_Settings) : null;
        }

        public void OnAfterDeserialize()
        {
            try
            {
                m_Value = string.IsNullOrEmpty(m_SerializedValue) ? new T() : JsonConvert.DeserializeObject<T>(m_SerializedValue, s_Settings);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                m_Value = new T();
            }
        }
    }
}
