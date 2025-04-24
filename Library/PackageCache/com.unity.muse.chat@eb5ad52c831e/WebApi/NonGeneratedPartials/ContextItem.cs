using Newtonsoft.Json;

namespace Unity.Muse.Chat.BackendApi.Model
{
    partial class ContextItem
    {
        [JsonIgnore] public int Priority;
        [JsonIgnore] public object Context;
        [JsonIgnore] public int? DeduplicationID;
    }
}