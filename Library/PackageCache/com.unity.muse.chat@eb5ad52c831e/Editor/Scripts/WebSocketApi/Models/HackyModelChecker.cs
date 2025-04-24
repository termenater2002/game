namespace Unity.Muse.Chat.WebSocketApi.Models
{
    // TODO: This is a quick hack
    class HackyModelChecker
    {
        [Newtonsoft.Json.JsonProperty("$type")]
        public string Type { get; set; }

        private System.Collections.Generic.IDictionary<string, object> _additionalProperties;

        [Newtonsoft.Json.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }
    }
}
