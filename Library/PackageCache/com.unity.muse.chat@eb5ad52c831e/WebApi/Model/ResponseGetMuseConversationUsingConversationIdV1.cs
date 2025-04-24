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
using System.Reflection;

namespace Unity.Muse.Chat.BackendApi.Model
{
    /// <summary>
    /// ResponseGetMuseConversationUsingConversationIdV1
    /// </summary>
    [JsonConverter(typeof(ResponseGetMuseConversationUsingConversationIdV1JsonConverter))]
    [DataContract(Name = "Response_Get_Muse_Conversation_Using_Conversation_Id_V1")]
    internal partial class ResponseGetMuseConversationUsingConversationIdV1 : AbstractOpenAPISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseGetMuseConversationUsingConversationIdV1" /> class
        /// with the <see cref="ClientConversation" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of ClientConversation.</param>
        public ResponseGetMuseConversationUsingConversationIdV1(ClientConversation actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "anyOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseGetMuseConversationUsingConversationIdV1" /> class
        /// with the <see cref="ErrorResponse" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of ErrorResponse.</param>
        public ResponseGetMuseConversationUsingConversationIdV1(ErrorResponse actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "anyOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }


        private Object _actualInstance;

        /// <summary>
        /// Gets or Sets ActualInstance
        /// </summary>
        public override Object ActualInstance
        {
            get
            {
                return _actualInstance;
            }
            set
            {
                if (value.GetType() == typeof(ClientConversation))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(ErrorResponse))
                {
                    this._actualInstance = value;
                }
                else
                {
                    throw new ArgumentException("Invalid instance found. Must be the following types: ClientConversation, ErrorResponse");
                }
            }
        }

        /// <summary>
        /// Get the actual instance of `ClientConversation`. If the actual instance is not `ClientConversation`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of ClientConversation</returns>
        public ClientConversation GetClientConversation()
        {
            return (ClientConversation)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `ErrorResponse`. If the actual instance is not `ErrorResponse`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of ErrorResponse</returns>
        public ErrorResponse GetErrorResponse()
        {
            return (ErrorResponse)this.ActualInstance;
        }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ResponseGetMuseConversationUsingConversationIdV1 {\n");
            sb.Append("  ActualInstance: ").Append(this.ActualInstance).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public override string ToJson()
        {
            return JsonConvert.SerializeObject(this.ActualInstance, ResponseGetMuseConversationUsingConversationIdV1.SerializerSettings);
        }

        /// <summary>
        /// Converts the JSON string into an instance of ResponseGetMuseConversationUsingConversationIdV1
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <returns>An instance of ResponseGetMuseConversationUsingConversationIdV1</returns>
        public static ResponseGetMuseConversationUsingConversationIdV1 FromJson(string jsonString)
        {
            ResponseGetMuseConversationUsingConversationIdV1 newResponseGetMuseConversationUsingConversationIdV1 = null;

            if (string.IsNullOrEmpty(jsonString))
            {
                return newResponseGetMuseConversationUsingConversationIdV1;
            }

            try
            {
                newResponseGetMuseConversationUsingConversationIdV1 = new ResponseGetMuseConversationUsingConversationIdV1(JsonConvert.DeserializeObject<ClientConversation>(jsonString, ResponseGetMuseConversationUsingConversationIdV1.SerializerSettings));
                // deserialization is considered successful at this point if no exception has been thrown.
                return newResponseGetMuseConversationUsingConversationIdV1;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into ClientConversation: {1}", jsonString, exception.ToString()));
            }

            try
            {
                newResponseGetMuseConversationUsingConversationIdV1 = new ResponseGetMuseConversationUsingConversationIdV1(JsonConvert.DeserializeObject<ErrorResponse>(jsonString, ResponseGetMuseConversationUsingConversationIdV1.SerializerSettings));
                // deserialization is considered successful at this point if no exception has been thrown.
                return newResponseGetMuseConversationUsingConversationIdV1;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into ErrorResponse: {1}", jsonString, exception.ToString()));
            }

            // no match found, throw an exception
            throw new InvalidDataException("The JSON string `" + jsonString + "` cannot be deserialized into any schema defined.");
        }

    }

    /// <summary>
    /// Custom JSON converter for ResponseGetMuseConversationUsingConversationIdV1
    /// </summary>
    internal class ResponseGetMuseConversationUsingConversationIdV1JsonConverter : JsonConverter
    {
        /// <summary>
        /// To write the JSON string
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="value">Object to be converted into a JSON string</param>
        /// <param name="serializer">JSON Serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue((string)(typeof(ResponseGetMuseConversationUsingConversationIdV1).GetMethod("ToJson").Invoke(value, null)));
        }

        /// <summary>
        /// To convert a JSON string into an object
        /// </summary>
        /// <param name="reader">JSON reader</param>
        /// <param name="objectType">Object type</param>
        /// <param name="existingValue">Existing value</param>
        /// <param name="serializer">JSON Serializer</param>
        /// <returns>The object converted from the JSON string</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch(reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ResponseGetMuseConversationUsingConversationIdV1.FromJson(JObject.Load(reader).ToString(Formatting.None));
                case JsonToken.StartArray:
                    return ResponseGetMuseConversationUsingConversationIdV1.FromJson(JArray.Load(reader).ToString(Formatting.None));
                default:
                    return null;
            }
        }

        /// <summary>
        /// Check if the object can be converted
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <returns>True if the object can be converted</returns>
        public override bool CanConvert(Type objectType)
        {
            return false;
        }
    }

}
