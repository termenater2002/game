using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.DeepPose.Cloud
{
    partial class MotionCompletionAPI
    {
        [Serializable]
        internal struct SerializedLoopKey
        {
            [JsonProperty("num_loopbacks")]
            public int NumLoopbacks;

            [JsonProperty("target_frame")]
            public int TargetFrame;

            [JsonProperty("translation")]
            public string TranslationBase64; // float base64

            [JsonProperty("rotation")]
            public string RotationBase64; // float base64
        }

        [Serializable]
        internal struct SerializedKey
        {
            [JsonProperty("index")]
            public int Index;

            [JsonProperty("type")]
            public int Type;

            [JsonProperty("positions")]
            public string PositionsBase64; // float base64

            [JsonProperty("rotations")]
            public string RotationsBase64; // float base64

            [JsonProperty("loop")]
            public SerializedLoopKey? Loop;
        }

        [Serializable]
        internal struct SerializedFrame
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
            public string CharacterID;

            [JsonProperty("keys")]
            public SerializedKey[] Keys;
        }

        [Serializable]
        internal struct SerializedResponse
        {
            [JsonProperty("frames")]
            public SerializedFrame[] Frames;
        }

    }
}
