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
    /// Defines Category
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum Category
    {
        /// <summary>
        /// Enum ResponseQuality for value: response quality
        /// </summary>
        [EnumMember(Value = "response quality")]
        ResponseQuality = 1,

        /// <summary>
        /// Enum CodeGeneration for value: code generation
        /// </summary>
        [EnumMember(Value = "code generation")]
        CodeGeneration = 2,

        /// <summary>
        /// Enum SpeedToResponse for value: speed to response
        /// </summary>
        [EnumMember(Value = "speed to response")]
        SpeedToResponse = 3,

        /// <summary>
        /// Enum Sources for value: sources
        /// </summary>
        [EnumMember(Value = "sources")]
        Sources = 4,

        /// <summary>
        /// Enum AdditionalResources for value: additional resources
        /// </summary>
        [EnumMember(Value = "additional resources")]
        AdditionalResources = 5
    }

}
