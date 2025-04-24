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
    /// Model of Muse Chat conversation.
    /// </summary>
    [DataContract(Name = "Conversation")]
    internal partial class Conversation
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Conversation" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected Conversation() { }
        public Conversation(List<string> owners, string title)
        {
            Owners = owners;
            Title = title;
        }

        /// <summary>
        /// User IDs of owners of the conversation.
        /// </summary>
        /// <value>User IDs of owners of the conversation.</value>
        [DataMember(Name = "owners", IsRequired = true, EmitDefaultValue = true)]
        public List<string> Owners { get; set; }

        /// <summary>
        /// Title of conversation.
        /// </summary>
        /// <value>Title of conversation.</value>
        [DataMember(Name = "title", IsRequired = true, EmitDefaultValue = true)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets Context
        /// </summary>
        [DataMember(Name = "context", EmitDefaultValue = true)]
        public string Context { get; set; }

        /// <summary>
        /// Gets or Sets FunctionCatalog
        /// </summary>
        [DataMember(Name = "function_catalog", EmitDefaultValue = true)]
        public List<FunctionDefinition> FunctionCatalog { get; set; }

        /// <summary>
        /// Conversation history.
        /// </summary>
        /// <value>Conversation history.</value>
        [DataMember(Name = "history", EmitDefaultValue = false)]
        public List<ConversationFragment> History { get; set; }

        /// <summary>
        /// Uniform conversation ID.
        /// </summary>
        /// <value>Uniform conversation ID.</value>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets IsFavorite
        /// </summary>
        [DataMember(Name = "is_favorite", EmitDefaultValue = true)]
        public bool? IsFavorite { get; set; }

        /// <summary>
        /// Gets or Sets Tags
        /// </summary>
        [DataMember(Name = "tags", EmitDefaultValue = true)]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Conversation {\n");
            sb.Append("  Owners: ").Append(Owners).Append("\n");
            sb.Append("  Title: ").Append(Title).Append("\n");
            sb.Append("  Context: ").Append(Context).Append("\n");
            sb.Append("  FunctionCatalog: ").Append(FunctionCatalog).Append("\n");
            sb.Append("  History: ").Append(History).Append("\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  IsFavorite: ").Append(IsFavorite).Append("\n");
            sb.Append("  Tags: ").Append(Tags).Append("\n");
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
