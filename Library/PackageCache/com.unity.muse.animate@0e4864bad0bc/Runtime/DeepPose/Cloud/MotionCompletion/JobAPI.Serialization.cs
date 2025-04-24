using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.DeepPose.Cloud
{
    partial class JobAPI
    {
        [Serializable]
        internal struct SerializedRequest {}

        [Serializable]
        internal struct SerializedResponse
        {
            [JsonProperty("guid")]
            public string Guid;

            [JsonProperty("status")]
            public string Status;

            [JsonProperty("success")]
            public bool Success;

            [JsonProperty("error")]
            public string Error;
        }

    }
}
