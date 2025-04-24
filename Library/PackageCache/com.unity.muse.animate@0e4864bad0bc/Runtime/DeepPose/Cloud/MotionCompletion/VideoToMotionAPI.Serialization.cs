using System;
using Newtonsoft.Json;

namespace Unity.DeepPose.Cloud
{
    partial class VideoToMotionAPI
    {
        [Serializable]
        internal struct SerializedResponse
        {
            [JsonProperty("fps")]
            public float FPS;

            [JsonProperty("frames", Required = Required.Always)]
            public SerializedFrame[] Frames;
        }
    }
}
