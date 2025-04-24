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
    /// Model training opt-in/opt-out request.
    /// </summary>
    [DataContract(Name = "OptRequest")]
    internal partial class OptRequest
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="OptRequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected OptRequest() { }
        public OptRequest(OptDecision decision, OptType type)
        {
            Decision = decision;
            Type = type;
        }

        /// <summary>
        /// Gets or Sets Decision
        /// </summary>
        [DataMember(Name = "decision", IsRequired = true, EmitDefaultValue = true)]
        public OptDecision Decision { get; set; }

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", IsRequired = true, EmitDefaultValue = true)]
        public OptType Type { get; set; }

        /// <summary>
        /// UTC milliseconds timestamp of opt-in/opt-out decision.
        /// </summary>
        /// <value>UTC milliseconds timestamp of opt-in/opt-out decision.</value>
        [DataMember(Name = "timestamp", EmitDefaultValue = false)]
        public long Timestamp { get; set; }

        /// <summary>
        /// Gets or Sets UserId
        /// </summary>
        [DataMember(Name = "user_id", EmitDefaultValue = true)]
        public string UserId { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class OptRequest {\n");
            sb.Append("  Decision: ").Append(Decision).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Timestamp: ").Append(Timestamp).Append("\n");
            sb.Append("  UserId: ").Append(UserId).Append("\n");
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
