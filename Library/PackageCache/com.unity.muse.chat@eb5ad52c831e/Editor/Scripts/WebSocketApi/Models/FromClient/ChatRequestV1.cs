using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Muse.Chat.WebSocketApi.Models;

namespace Unity.Muse.Chat.WebSocket.Model
{
    /// <summary>
    /// A chat request, aka a prompt or message from the user
    /// <br/>
    /// </summary>
    partial class ChatRequestV1 : IModel
    {
        [JsonProperty("$type", Required = Required.Always)]
        public const string Type = "CHAT_REQUEST_V1";
        public string GetModelType() => Type;

        /// <summary>
        /// The prompt written by the user.
        /// <br/>
        /// <br/>In cases where the user is using the ask/code/action buttons,
        /// <br/>this will be prefixed by "/ask", "/code", "/action" or another
        /// <br/>action.
        /// <br/>
        /// <br/>TENTITIVE PLAN: Remove buttons before GA.
        /// <br/>
        /// <br/>---
        /// <br/>
        /// <br/>Example:
        /// <br/>* "/ask What version of Unity should I use?"
        /// <br/>* "What version of Unity should I use?"
        /// <br/>* "/code Generate me a script to bla bla bla."
        /// <br/>* "/ask What version of Unity am I using and /code write me a script to print the version"
        /// <br/>
        /// </summary>
        [JsonProperty("prompt", Required = Required.Always)]
        public string Prompt { get; set; }

        [JsonProperty("selected_context", Required = Required.Always)]
        public List<SelectedContextModel> SelectedContext { get; set; } = new();

        public partial class SelectedContextModel
        {
            [JsonProperty("metadata", Required = Required.DisallowNull,
                NullValueHandling = NullValueHandling.Ignore)]
            public MetadataModel Metadata { get; set; }

            [JsonProperty("body", Required = Required.DisallowNull,
                NullValueHandling = NullValueHandling.Ignore)]
            public BodyModel Body { get; set; }

            IDictionary<string, object> _additionalProperties;
            [JsonExtensionData]
            public IDictionary<string, object> AdditionalProperties
            {
                get => _additionalProperties ??= new Dictionary<string, object>();
                set => _additionalProperties = value;
            }

            public class MetadataModel
            {
                /// <summary>
                /// The name / title of the context object displayed when showing context information
                /// </summary>
                [JsonProperty("display_value", Required = Required.DisallowNull,
                    NullValueHandling = NullValueHandling.Ignore)]
                public string DisplayValue { get; set; }

                /// <summary>
                /// Raw context value used to reconnect and display targeted context information
                /// </summary>
                [JsonProperty("value", Required = Required.DisallowNull,
                    NullValueHandling = NullValueHandling.Ignore)]
                public string Value { get; set; }

                /// <summary>
                /// Underlying type of the context object
                /// </summary>
                [JsonProperty("value_type", Required = Required.DisallowNull,
                    NullValueHandling = NullValueHandling.Ignore)]
                public string ValueType { get; set; }

                /// <summary>
                /// Index within the underlying type or the hierarchy of the object
                /// </summary>
                [JsonProperty("value_index", Required = Required.DisallowNull,
                    NullValueHandling = NullValueHandling.Ignore)]
                public int ValueIndex { get; set; }

                /// <summary>
                /// The type of the context object entry
                /// </summary>
                [JsonProperty("entry_type", Required = Required.DisallowNull,
                    NullValueHandling = NullValueHandling.Ignore)]
                public int EntryType { get; set; }
            }

            public class BodyModel
            {
                [JsonProperty("type", Required = Required.Always)]
                public string Type { get; set; }

                [JsonProperty("payload", Required = Required.Always)]
                public string Payload { get; set; }

                [JsonProperty("truncated", Required = Required.Always)]
                public bool Truncated { get; set; }
            }
        }
    }
}
