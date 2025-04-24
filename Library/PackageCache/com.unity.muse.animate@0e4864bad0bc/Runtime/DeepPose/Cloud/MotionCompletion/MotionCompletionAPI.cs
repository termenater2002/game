using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

using Unity.Muse.Common.Account;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Unity.DeepPose.Cloud
{
    static partial class MotionCompletionAPI
    {
        public static string ApiName => $"/api/v2/animate/organizations/{AccountInfo.Instance.Organization.Id}/motion_completion";

        [Serializable]
        public class LoopKey
        {
            public int TargetFrame
            {
                get => m_TargetFrame;
                set => m_TargetFrame = value;
            }

            public int NumLoopbacks
            {
                get => m_NumLoopbacks;
                set => m_NumLoopbacks = value;
            }

            public Vector3 Translation
            {
                get => m_Translation;
                set => m_Translation = value;
            }

            public Quaternion Rotation
            {
                get => m_Rotation;
                set => m_Rotation = value;
            }

            [SerializeField]
            Quaternion m_Rotation;

            [SerializeField]
            Vector3 m_Translation;

            [SerializeField]
            int m_NumLoopbacks;

            [SerializeField]
            int m_TargetFrame;

            public LoopKey()
            {
                m_Translation = Vector3.zero;
                m_Rotation = Quaternion.identity;
                m_NumLoopbacks = 1;
                m_TargetFrame = 0;
            }

            internal SerializedLoopKey Serialize()
            {
                return new SerializedLoopKey
                {
                    TargetFrame = m_TargetFrame,
                    NumLoopbacks = m_NumLoopbacks,
                    TranslationBase64 = m_Translation.ToBase64(),
                    RotationBase64 = m_Rotation.ToBase64()
                };
            }
        }

        [Serializable]
        public class Key
        {
            public enum KeyType : int
            {
                Empty = 0,
                FullPose = 1,
                Loop = 2
            }

            public int Index
            {
                get => m_Index;
                set => m_Index = value;
            }

            public KeyType Type
            {
                get => m_Type;
                set => m_Type = value;
            }

            public Vector3[] Positions => m_Positions;
            public Quaternion[] Rotations => m_Rotations;

            public LoopKey Loop => m_LoopKey;

            [SerializeField]
            Quaternion[] m_Rotations;

            [SerializeField]
            Vector3[] m_Positions;

            [SerializeField]
            KeyType m_Type;

            [SerializeField]
            int m_Index;

            [SerializeField]
            LoopKey m_LoopKey;

            public Key(int numPosition, int numRotations)
            {
                m_Positions = new Vector3[numPosition];
                m_Rotations = new Quaternion[numRotations];
                m_Index = -1;
                m_Type = KeyType.Empty;
                m_LoopKey = new LoopKey();
            }

            internal SerializedKey Serialize()
            {
                return new SerializedKey
                {
                    Index = m_Index,
                    Type = (int)m_Type,
                    PositionsBase64 = m_Type == KeyType.FullPose ? m_Positions.ToBase64() : "",
                    RotationsBase64 = m_Type == KeyType.FullPose ? m_Rotations.ToBase64() : "",
                    Loop = m_Type == KeyType.Loop ? m_LoopKey.Serialize() : null
                };
            }
        }

        [Serializable]
        public class Frame : IDisposable
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
        public class Request : ISerializable
        {
            public string CharacterID
            {
                get => m_CharacterID;
                set => m_CharacterID = value;
            }

            public List<Key> Keys => m_Keys;

            [SerializeField]
            List<Key> m_Keys;

            [SerializeField]
            string m_CharacterID;

            public Request()
            {
                m_Keys = new List<Key>();
                m_CharacterID = "";
            }

            SerializedRequest SerializeImpl()
            {
                var serializedKeys = new SerializedKey[m_Keys.Count];
                for (var i = 0; i < serializedKeys.Length; i++)
                {
                    var key = m_Keys[i];
                    var serializedKey = key.Serialize();

                    serializedKeys[i] = serializedKey;
                }

                var serializedRequest = new SerializedRequest
                {
                    Keys = serializedKeys,
                    CharacterID = m_CharacterID
                };

                return serializedRequest;
            }

            public object Serialize() => SerializeImpl();
        }

        [Serializable]
        public class Response : IDisposable, IDeserializable<JObject>
        {
            public List<Frame> Frames => m_Frames;

            [SerializeField]
            List<Frame> m_Frames = new();

            public void Dispose()
            {
                foreach (var frame in m_Frames)
                {
                    frame.Dispose();
                }
                m_Frames.Clear();
            }

            public void Deserialize(JObject data)
            {
                Dispose();

                var serializedResponse = data.ToObject<SerializedResponse>();
                
                m_Frames = new List<Frame>(serializedResponse.Frames.Length);
                for (var i = 0; i < serializedResponse.Frames.Length; i++)
                {
                    var serializedFrame = serializedResponse.Frames[i];
                    var frame = new Frame(serializedFrame);

                    m_Frames.Add(frame);
                }
            }
        }
    }
}
