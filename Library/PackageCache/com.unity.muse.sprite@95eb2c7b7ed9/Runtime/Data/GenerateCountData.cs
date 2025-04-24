using System;
using Unity.Muse.Common;
using UnityEngine;

#pragma warning disable 0067

namespace Unity.Muse.Sprite.Data
{
    [Serializable]
    internal class GenerateCountData : IModelData
    {
        [SerializeField]
        int m_Counter = 0;
        public event Action OnModified;
        public event Action OnSaveRequested;

        public void ResetCounter()
        {
            m_Counter = 0;

            OnModified?.Invoke();
        }

        public int GetAndIncrementCount()
        {
            var c = m_Counter;
            ++m_Counter;

            OnModified?.Invoke();
            return c;
        }
    }
}