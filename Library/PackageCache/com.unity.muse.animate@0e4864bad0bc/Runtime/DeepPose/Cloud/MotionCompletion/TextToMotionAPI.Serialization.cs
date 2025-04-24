using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.DeepPose.Cloud
{
    partial class TextToMotionAPI
    {
        [Serializable]
        internal struct SerializedRequest
        {
            [JsonProperty("character_id")]
            public string CharacterID;

            [JsonProperty("prompt")]
            public string Prompt;
            
            [JsonProperty("seed", Required = Required.AllowNull)]
            public int? Seed;
            
            [JsonProperty("temperature", Required = Required.AllowNull)]
            public float? Temperature;
        }

        [Serializable]
        internal struct SerializedResponse
        {
            [JsonProperty("fps")]
            public float FPS;

            [JsonProperty("frames", Required = Required.Always)]
            public SerializedFrame[] Frames;
            
            [JsonProperty("seed")]
            public int Seed;
            
            [JsonProperty("temperature")]
            public float Temperature;
            
            [JsonProperty("joint_names")]
            public string[] JointNames;
        }

    }
}
