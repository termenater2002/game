using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.DeepPose.Cloud
{
    partial class JsonToFbxAPI
    {
        [Serializable]
        public struct SerializedFrame
        {
            [JsonProperty("positions")]
            public string PositionsBase64; // float base64

            [JsonProperty("rotations")]
            public string RotationsBase64; // float base64

            [JsonProperty("contacts")]
            public string ContactsBase64; // float base64
        }
        
        [Serializable]
        internal struct SerializedRequest
        {
            [JsonProperty("character_id")] 
            public string CharacterId;
            
            [JsonProperty("frames")]
            public SerializedFrame[] Frames;

            [JsonProperty("joint_names")] 
            public string[] JointNames;
        }
    }
}
