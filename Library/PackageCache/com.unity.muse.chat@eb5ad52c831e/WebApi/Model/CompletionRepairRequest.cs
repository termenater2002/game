using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OpenAPIDateConverter = Unity.Muse.Chat.BackendApi.Client.OpenAPIDateConverter;

namespace Unity.Muse.Chat.BackendApi.Model
{
    /// <summary>
    /// Action code repair request, to interact with Muse Agent.
    /// </summary>
    [DataContract(Name = "CompletionRepairRequest")]
    internal partial class CompletionRepairRequest
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletionRepairRequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected CompletionRepairRequest() { }
        public CompletionRepairRequest(string errorToRepair, string itemToRepair, int messageIndex, string organizationId, bool streamResponse)
        {
            ErrorToRepair = errorToRepair;
            ItemToRepair = itemToRepair;
            MessageIndex = messageIndex;
            OrganizationId = organizationId;
            StreamResponse = streamResponse;
        }

        /// <summary>
        /// Gets or Sets Product
        /// </summary>
        [DataMember(Name = "product", EmitDefaultValue = true)]
        public ProductEnum? Product { get; set; }

        /// <summary>
        /// Gets or Sets ScriptType
        /// </summary>
        [DataMember(Name = "script_type", EmitDefaultValue = false)]
        public ScriptType? ScriptType { get; set; }

        /// <summary>
        /// The generated item with an error.
        /// </summary>
        /// <value>The generated item with an error.</value>
        [DataMember(Name = "error_to_repair", IsRequired = true, EmitDefaultValue = true)]
        public string ErrorToRepair { get; set; }

        /// <summary>
        /// Generated item to repair.
        /// </summary>
        /// <value>Generated item to repair.</value>
        [DataMember(Name = "item_to_repair", IsRequired = true, EmitDefaultValue = true)]
        public string ItemToRepair { get; set; }

        /// <summary>
        /// Message index.
        /// </summary>
        /// <value>Message index.</value>
        [DataMember(Name = "message_index", IsRequired = true, EmitDefaultValue = true)]
        public int MessageIndex { get; set; }

        /// <summary>
        /// The ID of the Unity organization.
        /// </summary>
        /// <value>The ID of the Unity organization.</value>
        [DataMember(Name = "organization_id", IsRequired = true, EmitDefaultValue = true)]
        public string OrganizationId { get; set; }

        /// <summary>
        /// Whether to stream Muse Chat response.
        /// </summary>
        /// <value>Whether to stream Muse Chat response.</value>
        [DataMember(Name = "stream_response", IsRequired = true, EmitDefaultValue = true)]
        public bool StreamResponse { get; set; }

        /// <summary>
        /// Gets or Sets ConversationId
        /// </summary>
        [DataMember(Name = "conversation_id", EmitDefaultValue = true)]
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or Sets Debug
        /// </summary>
        [DataMember(Name = "debug", EmitDefaultValue = true)]
        public bool? Debug { get; set; }

        /// <summary>
        /// Extra body for repair request.
        /// </summary>
        /// <value>Extra body for repair request.</value>
        [DataMember(Name = "extra_body", EmitDefaultValue = false)]
        public Object ExtraBody { get; set; }

        /// <summary>
        /// List of tags associated with chat request
        /// </summary>
        /// <value>List of tags associated with chat request</value>
        [DataMember(Name = "tags", EmitDefaultValue = false)]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or Sets UnityVersion
        /// </summary>
        [DataMember(Name = "unity_version", EmitDefaultValue = true)]
        public string UnityVersion { get; set; }

        /// <summary>
        /// Gets or Sets UserPrompt
        /// </summary>
        [DataMember(Name = "user_prompt", EmitDefaultValue = true)]
        public string UserPrompt { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class CompletionRepairRequest {\n");
            sb.Append("  ErrorToRepair: ").Append(ErrorToRepair).Append("\n");
            sb.Append("  ItemToRepair: ").Append(ItemToRepair).Append("\n");
            sb.Append("  MessageIndex: ").Append(MessageIndex).Append("\n");
            sb.Append("  OrganizationId: ").Append(OrganizationId).Append("\n");
            sb.Append("  StreamResponse: ").Append(StreamResponse).Append("\n");
            sb.Append("  ConversationId: ").Append(ConversationId).Append("\n");
            sb.Append("  Debug: ").Append(Debug).Append("\n");
            sb.Append("  ExtraBody: ").Append(ExtraBody).Append("\n");
            sb.Append("  Product: ").Append(Product).Append("\n");
            sb.Append("  ScriptType: ").Append(ScriptType).Append("\n");
            sb.Append("  Tags: ").Append(Tags).Append("\n");
            sb.Append("  UnityVersion: ").Append(UnityVersion).Append("\n");
            sb.Append("  UserPrompt: ").Append(UserPrompt).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }

}
