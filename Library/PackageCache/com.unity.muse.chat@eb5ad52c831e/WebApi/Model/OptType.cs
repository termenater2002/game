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
    /// Defines OptType
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum OptType
    {
        /// <summary>
        /// Enum Muse for value: all_muse
        /// </summary>
        [EnumMember(Value = "all_muse")]
        Muse = 1,

        /// <summary>
        /// Enum Prerelease for value: all_prerelease
        /// </summary>
        [EnumMember(Value = "all_prerelease")]
        Prerelease = 2
    }

}
