using System;
using Unity.Muse.Common;
using UnityEngine;

#pragma warning disable 0067

namespace Unity.Muse.Sprite.Data
{
    [Serializable]
    internal class SessionData : IModelData
    {
        public event Action OnModified;
        public event Action OnSaveRequested;

        [SerializeField]
        string m_SessionId = String.Empty;

        string m_ActiveSession = string.Empty;
        public string sessionLoaded = String.Empty;

        public string activeSession
        {
            get
            {
                if (!string.IsNullOrEmpty(m_SessionId))
                    m_ActiveSession = m_SessionId;
                else if(string.IsNullOrEmpty(m_ActiveSession))
                    m_SessionId = m_ActiveSession = Guid.NewGuid().ToString();
                return m_ActiveSession;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    m_ActiveSession = value;
                    OnModified?.Invoke();
                }
            }
        }
    }
}