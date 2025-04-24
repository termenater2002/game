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
    /// Enum for the type of script being repaired.
    /// </summary>
    /// <value>Enum for the type of script being repaired.</value>
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum ScriptType
    {
        /// <summary>
        /// Enum AgentAction for value: agent_action
        /// </summary>
        [EnumMember(Value = "agent_action")]
        AgentAction = 1,

        /// <summary>
        /// Enum Monobehaviour for value: monobehaviour
        /// </summary>
        [EnumMember(Value = "monobehaviour")]
        Monobehaviour = 2,

        /// <summary>
        /// Enum CodeGen for value: code_gen
        /// </summary>
        [EnumMember(Value = "code_gen")]
        CodeGen = 3
    }

}
