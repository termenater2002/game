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
    /// EditorContextReport
    /// </summary>
    [DataContract(Name = "EditorContextReport")]
    internal partial class EditorContextReport
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorContextReport" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected EditorContextReport() { }
        public EditorContextReport(List<ContextItem> attachedContext, int characterLimit, List<ContextItem> extractedContext)
        {
            AttachedContext = attachedContext;
            CharacterLimit = characterLimit;
            ExtractedContext = extractedContext;
        }

        /// <summary>
        /// Gets or Sets AttachedContext
        /// </summary>
        [DataMember(Name = "attached_context", IsRequired = true, EmitDefaultValue = true)]
        public List<ContextItem> AttachedContext { get; set; }

        /// <summary>
        /// Gets or Sets CharacterLimit
        /// </summary>
        [DataMember(Name = "character_limit", IsRequired = true, EmitDefaultValue = true)]
        public int CharacterLimit { get; set; }

        /// <summary>
        /// Gets or Sets ExtractedContext
        /// </summary>
        [DataMember(Name = "extracted_context", IsRequired = true, EmitDefaultValue = true)]
        public List<ContextItem> ExtractedContext { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class EditorContextReport {\n");
            sb.Append("  AttachedContext: ").Append(AttachedContext).Append("\n");
            sb.Append("  CharacterLimit: ").Append(CharacterLimit).Append("\n");
            sb.Append("  ExtractedContext: ").Append(ExtractedContext).Append("\n");
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
