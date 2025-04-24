using System.CodeDom.Compiler;
using Newtonsoft.Json;
using Unity.Muse.Chat.WebSocketApi.Models;

namespace Unity.Muse.Chat.WebSocket.Model
{
    #pragma warning disable // Disable all warnings

    /// <summary>
    /// Sent once per websocket.
    /// <br/>
    /// <br/>When a websocket is setup, it can be opened for either:
    /// <br/>
    /// <br/>* An existing conversation - a conversation_id is sent during websocket setup
    /// <br/>* A new conversation - no conversation_id is sent during websocket setup
    /// <br/>
    /// <br/>Why not have a rest API to create conversations:
    /// <br/>
    /// <br/>* This way we can avoid having empty conversations
    /// <br/>* All writes are through the websocket
    /// <br/>
    /// </summary>
    [GeneratedCode("NJsonSchema", "11.1.0.0 (Newtonsoft.Json v13.0.0.0)")]
    partial class DiscussionInitializationV1 : IModel
    {
        [JsonProperty("$type")]
        public const string Type = "DISCUSSION_INITIALIZATION_V1";
        public string GetModelType() => Type;

        [JsonProperty("conversation_id", Required = Required.Always)]
        public string ConversationId { get; set; }

    }
}
