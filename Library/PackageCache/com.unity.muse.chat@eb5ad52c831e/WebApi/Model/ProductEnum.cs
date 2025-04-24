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
    /// Defines ProductEnum
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum ProductEnum
    {
        /// <summary>
        /// Enum AiAssistant for value: ai-assistant
        /// </summary>
        [EnumMember(Value = "ai-assistant")]
        AiAssistant = 1,

        /// <summary>
        /// Enum MuseBehavior for value: muse-behavior
        /// </summary>
        [EnumMember(Value = "muse-behavior")]
        MuseBehavior = 2,

        /// <summary>
        /// Enum WorldAi for value: world-ai
        /// </summary>
        [EnumMember(Value = "world-ai")]
        WorldAi = 3,

        /// <summary>
        /// Enum CodeGen for value: code_gen
        /// </summary>
        [EnumMember(Value = "code_gen")]
        CodeGen = 4,

        /// <summary>
        /// Enum Match3 for value: match3
        /// </summary>
        [EnumMember(Value = "match3")]
        Match3 = 5
    }

}
