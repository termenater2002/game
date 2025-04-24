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
    /// Action request, to interact with Muse Agent.
    /// </summary>
    [DataContract(Name = "ActionRequest")]
    internal partial class ActionRequest
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionRequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ActionRequest() { }
        public ActionRequest(string organizationId, string prompt, bool streamResponse)
        {
            OrganizationId = organizationId;
            Prompt = prompt;
            StreamResponse = streamResponse;
        }

        /// <summary>
        /// Gets or Sets Product
        /// </summary>
        [DataMember(Name = "product", EmitDefaultValue = true)]
        public ProductEnum? Product { get; set; }

        /// <summary>
        /// The ID of the Unity organization.
        /// </summary>
        /// <value>The ID of the Unity organization.</value>
        [DataMember(Name = "organization_id", IsRequired = true, EmitDefaultValue = true)]
        public string OrganizationId { get; set; }

        /// <summary>
        /// User message to Muse Chat.
        /// </summary>
        /// <value>User message to Muse Chat.</value>
        [DataMember(Name = "prompt", IsRequired = true, EmitDefaultValue = true)]
        public string Prompt { get; set; }

        /// <summary>
        /// Whether to stream Muse Chat response.
        /// </summary>
        /// <value>Whether to stream Muse Chat response.</value>
        [DataMember(Name = "stream_response", IsRequired = true, EmitDefaultValue = true)]
        public bool StreamResponse { get; set; }

        /// <summary>
        /// Gets or Sets Context
        /// </summary>
        [DataMember(Name = "context", EmitDefaultValue = true)]
        public Context Context { get; set; }

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
        /// Gets or Sets DependencyInformation
        /// </summary>
        [DataMember(Name = "dependency_information", EmitDefaultValue = true)]
        public Dictionary<string, string> DependencyInformation { get; set; }

        /// <summary>
        /// Extra body for chat request.
        /// </summary>
        /// <value>Extra body for chat request.</value>
        [DataMember(Name = "extra_body", EmitDefaultValue = false)]
        public Object ExtraBody { get; set; }

        /// <summary>
        /// Gets or Sets ProjectSummary
        /// </summary>
        [DataMember(Name = "project_summary", EmitDefaultValue = true)]
        public Dictionary<string, string> ProjectSummary { get; set; }

        /// <summary>
        /// Gets or Sets SelectedContextMetadata
        /// </summary>
        [DataMember(Name = "selected_context_metadata", EmitDefaultValue = true)]
        public List<SelectedContextMetadataItems> SelectedContextMetadata { get; set; }

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
        /// Gets or Sets UnityVersions
        /// </summary>
        [DataMember(Name = "unity_versions", EmitDefaultValue = true)]
        public List<string> UnityVersions { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ActionRequest {\n");
            sb.Append("  OrganizationId: ").Append(OrganizationId).Append("\n");
            sb.Append("  Prompt: ").Append(Prompt).Append("\n");
            sb.Append("  StreamResponse: ").Append(StreamResponse).Append("\n");
            sb.Append("  Context: ").Append(Context).Append("\n");
            sb.Append("  ConversationId: ").Append(ConversationId).Append("\n");
            sb.Append("  Debug: ").Append(Debug).Append("\n");
            sb.Append("  DependencyInformation: ").Append(DependencyInformation).Append("\n");
            sb.Append("  ExtraBody: ").Append(ExtraBody).Append("\n");
            sb.Append("  Product: ").Append(Product).Append("\n");
            sb.Append("  ProjectSummary: ").Append(ProjectSummary).Append("\n");
            sb.Append("  SelectedContextMetadata: ").Append(SelectedContextMetadata).Append("\n");
            sb.Append("  Tags: ").Append(Tags).Append("\n");
            sb.Append("  UnityVersion: ").Append(UnityVersion).Append("\n");
            sb.Append("  UnityVersions: ").Append(UnityVersions).Append("\n");
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
