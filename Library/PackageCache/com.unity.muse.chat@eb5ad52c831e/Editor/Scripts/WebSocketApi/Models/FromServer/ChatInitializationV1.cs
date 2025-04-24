using Newtonsoft.Json;
using Unity.Muse.Chat.WebSocketApi.Models;

namespace Unity.Muse.Chat.WebSocket.Model
{
    #pragma warning disable // Disable all warnings

    partial class ChatInitializationV1 : IModel
    {
        [JsonProperty("$type")]
        public const string Type = "CHAT_INITIALIZATION_V1";
        public string GetModelType() => Type;

        [JsonProperty("user_prompt_conversation_fragment_id", Required = Required.Always)]
        public string UserPromptConversationFragmentId { get; set; }

        [JsonProperty("agent_response_conversation_fragment_id", Required = Required.Always)]
        public string AgentResponseConversationFragmentId { get; set; }

        [JsonProperty("message_author", Required = Required.Always)]
        public string MessageAuthor { get; set; }

    }
}
