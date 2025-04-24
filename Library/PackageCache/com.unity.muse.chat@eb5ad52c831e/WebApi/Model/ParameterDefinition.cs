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
    /// ParameterDefinition
    /// </summary>
    [DataContract(Name = "ParameterDefinition")]
    internal partial class ParameterDefinition
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDefinition" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ParameterDefinition() { }
        public ParameterDefinition(string description, string name, string type)
        {
            Description = description;
            Name = name;
            Type = type;
        }

        /// <summary>
        /// A description of the parameter used by the LLM
        /// </summary>
        /// <value>A description of the parameter used by the LLM</value>
        [DataMember(Name = "description", IsRequired = true, EmitDefaultValue = true)]
        public string Description { get; set; }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        /// <value>The name of the parameter</value>
        [DataMember(Name = "name", IsRequired = true, EmitDefaultValue = true)]
        public string Name { get; set; }

        /// <summary>
        /// The parameters type, in the form of the origin language. I.E. functions originating from Unity should be C# types.
        /// </summary>
        /// <value>The parameters type, in the form of the origin language. I.E. functions originating from Unity should be C# types.</value>
        [DataMember(Name = "type", IsRequired = true, EmitDefaultValue = true)]
        public string Type { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ParameterDefinition {\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
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
