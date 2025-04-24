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
    /// SmartContextRequest
    /// </summary>
    [DataContract(Name = "SmartContextRequest")]
    internal partial class SmartContextRequest
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartContextRequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected SmartContextRequest() { }
        public SmartContextRequest(string organizationId, string prompt)
        {
            OrganizationId = organizationId;
            Prompt = prompt;
        }

        /// <summary>
        /// The ID of the Unity organization.
        /// </summary>
        /// <value>The ID of the Unity organization.</value>
        [DataMember(Name = "organization_id", IsRequired = true, EmitDefaultValue = true)]
        public string OrganizationId { get; set; }

        /// <summary>
        /// User message to Smart Context.
        /// </summary>
        /// <value>User message to Smart Context.</value>
        [DataMember(Name = "prompt", IsRequired = true, EmitDefaultValue = true)]
        public string Prompt { get; set; }

        /// <summary>
        /// Gets or Sets ConversationId
        /// </summary>
        [DataMember(Name = "conversation_id", EmitDefaultValue = true)]
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or Sets EditorContext
        /// </summary>
        [DataMember(Name = "editor_context", EmitDefaultValue = true)]
        public EditorContext EditorContext { get; set; }

        /// <summary>
        /// Gets or Sets JsonCatalog
        /// </summary>
        [DataMember(Name = "json_catalog", EmitDefaultValue = true)]
        public List<FunctionDefinition> JsonCatalog { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class SmartContextRequest {\n");
            sb.Append("  OrganizationId: ").Append(OrganizationId).Append("\n");
            sb.Append("  Prompt: ").Append(Prompt).Append("\n");
            sb.Append("  ConversationId: ").Append(ConversationId).Append("\n");
            sb.Append("  EditorContext: ").Append(EditorContext).Append("\n");
            sb.Append("  JsonCatalog: ").Append(JsonCatalog).Append("\n");
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
