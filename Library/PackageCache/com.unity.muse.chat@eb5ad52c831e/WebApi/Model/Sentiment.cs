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
    /// Defines Sentiment
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum Sentiment
    {
        /// <summary>
        /// Enum Positive for value: positive
        /// </summary>
        [EnumMember(Value = "positive")]
        Positive = 1,

        /// <summary>
        /// Enum Negative for value: negative
        /// </summary>
        [EnumMember(Value = "negative")]
        Negative = 2
    }

}
