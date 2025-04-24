using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    [Serializable]
    class PreferenceDataWrapper<T>
    {
        // Serializing as a reference here allows serializing it or any sub-field to null.
        [SerializeReference]
        public T value;
    }
}
