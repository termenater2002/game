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
    /// Editor context from user input.
    /// </summary>
    [JsonConverter(typeof(EditorContextJsonConverter))]
    [DataContract(Name = "Editor_Context")]
    internal partial class EditorContext : AbstractOpenAPISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorContext" /> class.
        /// </summary>
        public EditorContext()
        {
            this.IsNullable = true;
            this.SchemaType= "anyOf";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorContext" /> class
        /// with the <see cref="string" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of string.</param>
        public EditorContext(string actualInstance)
        {
            this.IsNullable = true;
            this.SchemaType= "anyOf";
            this.ActualInstance = actualInstance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorContext" /> class
        /// with the <see cref="EditorContextReport" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of EditorContextReport.</param>
        public EditorContext(EditorContextReport actualInstance)
        {
            this.IsNullable = true;
            this.SchemaType= "anyOf";
            this.ActualInstance = actualInstance;
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
                if (value.GetType() == typeof(EditorContextReport))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(string))
                {
                    this._actualInstance = value;
                }
                else
                {
                    throw new ArgumentException("Invalid instance found. Must be the following types: EditorContextReport, string");
                }
            }
        }

        /// <summary>
        /// Get the actual instance of `string`. If the actual instance is not `string`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of string</returns>
        public string GetString()
        {
            return (string)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `EditorContextReport`. If the actual instance is not `EditorContextReport`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of EditorContextReport</returns>
        public EditorContextReport GetEditorContextReport()
        {
            return (EditorContextReport)this.ActualInstance;
        }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class EditorContext {\n");
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
            return JsonConvert.SerializeObject(this.ActualInstance, EditorContext.SerializerSettings);
        }

        /// <summary>
        /// Converts the JSON string into an instance of EditorContext
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <returns>An instance of EditorContext</returns>
        public static EditorContext FromJson(string jsonString)
        {
            EditorContext newEditorContext = null;

            if (string.IsNullOrEmpty(jsonString))
            {
                return newEditorContext;
            }

            try
            {
                newEditorContext = new EditorContext(JsonConvert.DeserializeObject<EditorContextReport>(jsonString, EditorContext.SerializerSettings));
                // deserialization is considered successful at this point if no exception has been thrown.
                return newEditorContext;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into EditorContextReport: {1}", jsonString, exception.ToString()));
            }

            try
            {
                newEditorContext = new EditorContext(JsonConvert.DeserializeObject<string>(jsonString, EditorContext.SerializerSettings));
                // deserialization is considered successful at this point if no exception has been thrown.
                return newEditorContext;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into string: {1}", jsonString, exception.ToString()));
            }

            // no match found, throw an exception
            throw new InvalidDataException("The JSON string `" + jsonString + "` cannot be deserialized into any schema defined.");
        }

    }

    /// <summary>
    /// Custom JSON converter for EditorContext
    /// </summary>
    internal class EditorContextJsonConverter : JsonConverter
    {
        /// <summary>
        /// To write the JSON string
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="value">Object to be converted into a JSON string</param>
        /// <param name="serializer">JSON Serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue((string)(typeof(EditorContext).GetMethod("ToJson").Invoke(value, null)));
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
                case JsonToken.String:
                    return new EditorContext(Convert.ToString(reader.Value));
                case JsonToken.StartObject:
                    return EditorContext.FromJson(JObject.Load(reader).ToString(Formatting.None));
                case JsonToken.StartArray:
                    return EditorContext.FromJson(JArray.Load(reader).ToString(Formatting.None));
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
