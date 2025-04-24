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
    static partial class MotionDiffusionAPI
    {
        public static string ApiName => $"/api/v2/animate/organizations/{AccountInfo.Instance.Organization.Id}/motion_diffusion";

        // TODO: Do we still need this class? It mirrors SerializedRequest exactly.
        public class Request : ISerializable
        {
            public string CharacterID { get; set; }

            public string Prompt { get; set; }

            public int? Seed { get; set; }
            
            public int Length { get; set; }

            public Request()
            {
                Prompt = "";
                CharacterID = "";
            }

            SerializedRequest SerializeImpl()
            {
                var serializedRequest = new SerializedRequest
                {
                    Prompt = Prompt,
                    CharacterID = CharacterID,
                    Seed = Seed,
                    Length = Length
                };

                return serializedRequest;
            }
            
            public object Serialize() => SerializeImpl();
        }

        [Serializable]
        public class Response : IDisposable, IDeserializable<JObject>
        {
            public List<Frame> Frames => m_Frames;
            public float FramesPerSecond => m_FramesPerSecond;
            public int Seed => m_Seed;
            public List<string> JointNames => m_JointNames;

            [SerializeField]
            List<Frame> m_Frames;
            
            [SerializeField]
            int m_Seed;

            [SerializeField]
            float m_FramesPerSecond;
            
            [SerializeField]
            List<string> m_JointNames;

            public Response()
            {
                m_Frames = new List<Frame>();
                m_JointNames = new List<string>();
            }

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
                
                m_Seed = serializedResponse.Seed;
                if (serializedResponse.JointNames != null)
                {
                    m_JointNames = new List<string>(serializedResponse.JointNames);
                }
                m_FramesPerSecond = serializedResponse.FPS;
            }
        }
    }
}
