using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Muse.Chat.WebSocketApi.Models;

namespace Unity.Muse.Chat.WebSocket.Model
{
    #pragma warning disable // Disable all warnings

    [GeneratedCode("NJsonSchema", "11.1.0.0 (Newtonsoft.Json v13.0.0.0)")]
    partial class FunctionCallRequestV1 : IModel
    {
        [JsonProperty("$type")]
        public const string Type = "FUNCTION_CALL_REQUEST_V1";
        public string GetModelType() => Type;

        /// <summary>
        /// An id to allow us to join function-calls-requests and function-call-responses
        /// </summary>
        [JsonProperty("call_id", Required = Required.Always)]
        public Guid CallId { get; set; }

        /// <summary>
        /// The name of the function to execute on the client
        /// </summary>
        [JsonProperty("function_name", Required = Required.Always)]
        public string FunctionName { get; set; }

        [JsonProperty("function_parameters", Required = Required.Always)]
        public List<string> FunctionParameters { get; set; } = new List<string>();
    }
}
