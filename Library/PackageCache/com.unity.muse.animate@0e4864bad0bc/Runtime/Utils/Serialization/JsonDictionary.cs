using System.Collections.Generic;
using Newtonsoft.Json;

namespace Unity.Muse.Animate
{
    [JsonArray]
    class JsonDictionary<TKey, TValue> : Dictionary<TKey, TValue> { }
}
