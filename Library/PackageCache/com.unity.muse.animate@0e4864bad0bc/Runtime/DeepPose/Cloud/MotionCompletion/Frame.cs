using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.DeepPose.Cloud
{
    [Serializable]
    class Frame : IDisposable
    {
        public Vector3[] Positions => m_Positions.Array;
        public Quaternion[] Rotations => m_Rotations.Array;
        public float[] Contacts => m_Contacts.Array;

        [SerializeField]
        PooledArray<Quaternion> m_Rotations;

        [SerializeField]
        PooledArray<Vector3> m_Positions;

        [SerializeField]
        PooledArray<float> m_Contacts;

        internal Frame(SerializedFrame serializedFrame)
        {
            m_Positions = serializedFrame.PositionsBase64.Base64ToVector3Array();
            m_Rotations = serializedFrame.RotationsBase64.Base64ToQuaternionArray();
            m_Contacts = serializedFrame.ContactsBase64.Base64ToFloatArray();
        }

        public void Dispose()
        {
            m_Positions.Dispose();
            m_Rotations.Dispose();
            m_Contacts.Dispose();
        }
    }

    [Serializable]
    internal struct SerializedFrame
    {
        [JsonProperty("positions")]
        public string PositionsBase64; // float base64

        [JsonProperty("rotations", Required = Required.Always)]
        public string RotationsBase64; // float base64

        [JsonProperty("contacts")]
        public string ContactsBase64; // float base64
    }
}
