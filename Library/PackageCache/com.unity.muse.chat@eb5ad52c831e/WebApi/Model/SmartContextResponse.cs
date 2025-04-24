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
    /// Smart context response.
    /// </summary>
    [DataContract(Name = "SmartContextResponse")]
    internal partial class SmartContextResponse
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartContextResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected SmartContextResponse() { }
        public SmartContextResponse(List<FunctionCall> functionCalls)
        {
            FunctionCalls = functionCalls;
        }

        /// <summary>
        /// Gets or Sets FunctionCalls
        /// </summary>
        [DataMember(Name = "function_calls", IsRequired = true, EmitDefaultValue = true)]
        public List<FunctionCall> FunctionCalls { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class SmartContextResponse {\n");
            sb.Append("  FunctionCalls: ").Append(FunctionCalls).Append("\n");
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
