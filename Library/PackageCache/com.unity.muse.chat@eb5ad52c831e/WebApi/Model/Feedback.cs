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
    /// Feedback request, with feedback.
    /// </summary>
    [DataContract(Name = "Feedback")]
    internal partial class Feedback
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Feedback" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected Feedback() { }
        public Feedback(Category category, string conversationFragmentId, string conversationId, string details, string organizationId, Sentiment sentiment)
        {
            Category = category;
            ConversationFragmentId = conversationFragmentId;
            ConversationId = conversationId;
            Details = details;
            OrganizationId = organizationId;
            Sentiment = sentiment;
        }

        /// <summary>
        /// Gets or Sets Category
        /// </summary>
        [DataMember(Name = "category", IsRequired = true, EmitDefaultValue = true)]
        public Category Category { get; set; }

        /// <summary>
        /// Gets or Sets Sentiment
        /// </summary>
        [DataMember(Name = "sentiment", IsRequired = true, EmitDefaultValue = true)]
        public Sentiment Sentiment { get; set; }

        /// <summary>
        /// Conversation fragment ID.
        /// </summary>
        /// <value>Conversation fragment ID.</value>
        [DataMember(Name = "conversation_fragment_id", IsRequired = true, EmitDefaultValue = true)]
        public string ConversationFragmentId { get; set; }

        /// <summary>
        /// Uniform conversation ID.
        /// </summary>
        /// <value>Uniform conversation ID.</value>
        [DataMember(Name = "conversation_id", IsRequired = true, EmitDefaultValue = true)]
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or Sets Details
        /// </summary>
        [DataMember(Name = "details", IsRequired = true, EmitDefaultValue = true)]
        public string Details { get; set; }

        /// <summary>
        /// The ID of the Unity organization.
        /// </summary>
        /// <value>The ID of the Unity organization.</value>
        [DataMember(Name = "organization_id", IsRequired = true, EmitDefaultValue = true)]
        public string OrganizationId { get; set; }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "_id", EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets CreationDateUtc
        /// </summary>
        [DataMember(Name = "creation_date_utc", EmitDefaultValue = false)]
        public DateTime CreationDateUtc { get; set; }

        /// <summary>
        /// Gets or Sets HeapSessionUrl
        /// </summary>
        [DataMember(Name = "heap_session_url", EmitDefaultValue = true)]
        public string HeapSessionUrl { get; set; }

        /// <summary>
        /// Gets or Sets IsUnityEmployee
        /// </summary>
        [DataMember(Name = "is_unity_employee", EmitDefaultValue = true)]
        public bool? IsUnityEmployee { get; set; }

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
            sb.Append("class Feedback {\n");
            sb.Append("  Category: ").Append(Category).Append("\n");
            sb.Append("  ConversationFragmentId: ").Append(ConversationFragmentId).Append("\n");
            sb.Append("  ConversationId: ").Append(ConversationId).Append("\n");
            sb.Append("  Details: ").Append(Details).Append("\n");
            sb.Append("  OrganizationId: ").Append(OrganizationId).Append("\n");
            sb.Append("  Sentiment: ").Append(Sentiment).Append("\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  CreationDateUtc: ").Append(CreationDateUtc).Append("\n");
            sb.Append("  HeapSessionUrl: ").Append(HeapSessionUrl).Append("\n");
            sb.Append("  IsUnityEmployee: ").Append(IsUnityEmployee).Append("\n");
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
