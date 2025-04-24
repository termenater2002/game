using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Unity.DeepPose.Cloud
{
    static partial class JsonToFbxAPI
    {
        const string k_ApiName = "api/v0/json_to_fbx";
        public static string ApiName => k_ApiName;
        
        [Serializable]
        public class Frame : IDisposable
        {
            public Vector3[] Positions => m_Positions.Array;
            public Quaternion[] Rotations => m_Rotations.Array;
            public float[] Contacts => m_Contacts.Array;

            PooledArray<Quaternion> m_Rotations;
            PooledArray<Vector3> m_Positions;
            PooledArray<float> m_Contacts;
            
            public Frame(Vector3 rootPosition, NativeArray<Quaternion> rotations)
            {
                m_Contacts = new PooledArray<float>(0);

                m_Positions = new PooledArray<Vector3>(1);
                m_Positions.Array[0] = rootPosition;

                m_Rotations = new PooledArray<Quaternion>(rotations.Length);
                for (int i = 0; i < rotations.Length; i++)
                {
                    m_Rotations.Array[i] = rotations[i];
                }
            }

            public SerializedFrame Serialize()
            {
                SerializedFrame frame = new()
                {
                    ContactsBase64 = m_Contacts.Array.ToBase64(),
                    PositionsBase64 = m_Positions.Array.ToBase64(),
                    RotationsBase64 = m_Rotations.Array.ToBase64()
                };

                return frame;
            }

            public void Dispose()
            {
                m_Positions.Dispose();
                m_Rotations.Dispose();
                m_Contacts.Dispose();
            }
        }
        
        [Serializable]
        public class Request : ISerializable, IDisposable
        {
            public List<Frame> Frames => m_Frames;

            [SerializeField] 
            string m_CharacterId;
            
            [SerializeField]
            List<Frame> m_Frames;

            [SerializeField] 
            List<string> m_JointNames;

            public Request(string characterId, List<Frame> frames, List<string> jointNames = null)
            {
                m_CharacterId = characterId;
                m_Frames = frames;
                m_JointNames = jointNames;
            }

            SerializedRequest SerializeImpl()
            {
                return new SerializedRequest()
                {
                    CharacterId = m_CharacterId,
                    Frames = m_Frames.Select(f => f.Serialize()).ToArray(),
                    JointNames = m_JointNames?.ToArray()
                };
            }
            
            public object Serialize() => SerializeImpl();

            public void Dispose()
            {
                foreach (var frame in m_Frames)
                {
                    frame.Dispose();
                }

                m_Frames.Clear();
            }
        }
    }
}