using System.CodeDom.Compiler;
using Newtonsoft.Json;
using Unity.Muse.Chat.WebSocketApi.Models;

namespace Unity.Muse.Chat.WebSocket.Model
{
    #pragma warning disable // Disable all warnings

    [GeneratedCode("NJsonSchema", "11.1.0.0 (Newtonsoft.Json v13.0.0.0)")]
    partial class ChatResponseFragmentV1 : IModel
    {
        [JsonProperty("$type")]
        public const string Type = "CHAT_RESPONSE_FRAGMENT_V1";
        public string GetModelType() => Type;

        /// <summary>
        /// whether streaming has finished
        /// <br/>
        /// </summary>
        [JsonProperty("last_message", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool LastMessage { get; set; }

        /// <summary>
        /// Markdown (bold/italic/hyperlinks/etc)
        /// <br/>
        /// <br/>Also uses some special blocks that are parsed by the
        /// <br/>editor.  Including:
        /// <br/>
        /// <br/>* Code
        /// <br/>* Action
        /// <br/>* Plugins
        /// <br/>* Match3
        /// <br/>* ...?
        /// <br/>
        /// <br/>---
        /// <br/>
        /// <br/>Code blocks are wrapped with:
        /// <br/>
        /// <br/>  ```code
        /// <br/>  C# code here
        /// <br/>  ```
        /// <br/>
        /// <br/>Action blocks are wrapped with:
        /// <br/>
        /// <br/>  ```action
        /// <br/>  C# code here
        /// <br/>  ```
        /// <br/>
        /// </summary>
        [JsonProperty("markdown", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Markdown { get; set; }



    }
}
