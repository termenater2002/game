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
    /// Conversation patch request, to update conversation.
    /// </summary>
    [DataContract(Name = "ConversationPatchRequest")]
    internal partial class ConversationPatchRequest
    {

        public ConversationPatchRequest()
        {
        }

        /// <summary>
        /// Gets or Sets Context
        /// </summary>
        [DataMember(Name = "context", EmitDefaultValue = true)]
        public string Context { get; set; }

        /// <summary>
        /// Gets or Sets IsFavorite
        /// </summary>
        [DataMember(Name = "is_favorite", EmitDefaultValue = true)]
        public bool? IsFavorite { get; set; }

        /// <summary>
        /// Gets or Sets Title
        /// </summary>
        [DataMember(Name = "title", EmitDefaultValue = true)]
        public string Title { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ConversationPatchRequest {\n");
            sb.Append("  Context: ").Append(Context).Append("\n");
            sb.Append("  IsFavorite: ").Append(IsFavorite).Append("\n");
            sb.Append("  Title: ").Append(Title).Append("\n");
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
