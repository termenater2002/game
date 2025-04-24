using Newtonsoft.Json;
using Unity.Muse.Chat.WebSocketApi.Models;

namespace Unity.Muse.Chat.WebSocket.Model
{
    #pragma warning disable // Disable all warnings

    /// <summary>
    /// A function call response sent from client -&gt; server.
    /// <br/>
    /// <br/>The function call can be directed to the correct workflow and agent, via
    /// <br/>the call_id.  That should match the call_id of the function call request.
    /// <br/>
    /// </summary>
    partial class FunctionCallResponseV1 : IModel
    {
        [JsonProperty("$type")]
        public const string Type = "FUNCTION_CALL_RESPONSE_V1";
        public string GetModelType() => Type;

        /// <summary>
        /// An id to allow us to join function-calls-requests and function-call-responses
        /// </summary>
        [JsonProperty("call_id", Required = Required.Always)]
        public System.Guid CallId { get; set; }

        /// <summary>
        /// The output from a function call
        /// </summary>
        [JsonProperty("function_result", Required = Required.Always)]
        public string FunctionResult { get; set; }
    }
}
