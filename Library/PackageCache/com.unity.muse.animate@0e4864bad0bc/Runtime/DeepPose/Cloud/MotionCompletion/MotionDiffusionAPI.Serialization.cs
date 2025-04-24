using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.DeepPose.Cloud
{
    partial class MotionDiffusionAPI
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
            
            [JsonProperty("length")]
            public int Length;
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
            
            [JsonProperty("joint_names")]
            public string[] JointNames;
        }

    }
}
