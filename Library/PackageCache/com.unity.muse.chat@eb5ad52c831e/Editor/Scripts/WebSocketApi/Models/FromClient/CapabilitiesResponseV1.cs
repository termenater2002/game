using Newtonsoft.Json;
using Unity.Muse.Chat.WebSocketApi.Models;

namespace Unity.Muse.Chat.WebSocket.Model
{
    #pragma warning disable // Disable all warnings

    /// <summary>
    /// A documented list of the functions that are available to be called on the editor.
    /// <br/>
    /// <br/>The descriptions of the function and parameters are important because backend LLMs
    /// <br/>will decide to call (or not call) the functions using the info provided here.
    /// <br/>
    /// </summary>
    partial class CapabilitiesResponseV1 : IModel
    {
        [JsonProperty("$type")]
        public const string Type = "CAPABILITIES_RESPONSE_V1";

        public string GetModelType() => Type;

        /// <summary>
        /// The output formats that the client supports.
        /// <br/>
        /// <br/>Examples:
        /// <br/>  markdown,
        /// <br/>  code,
        /// <br/>  action,
        /// <br/>  plugins (Animate, Texture, Sprite),
        /// <br/>  match3
        /// <br/>
        /// </summary>
        [JsonProperty("outputs", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<OutputsObject> Outputs { get; set; }

        [JsonProperty("functions", Required = Required.Always)]
        public System.Collections.Generic.ICollection<FunctionsObject> Functions { get; set; } = new System.Collections.ObjectModel.Collection<FunctionsObject>();



    public partial class OutputsObject
    {
        [JsonProperty("output_name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string OutputName { get; set; }
    }

    public partial class FunctionsObject
    {
        /// <summary>
        /// Groups functions together.
        /// <br/>
        /// <br/>Example: ContextExtraction
        /// <br/>
        /// </summary>
        [JsonProperty("function_tag", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<string> FunctionTag { get; set; }

        /// <summary>
        /// This value MAY be overridden by the backend.
        /// <br/>
        /// <br/>We are allowing the client to specify them, so that we can dynamically add functions.
        /// <br/>
        /// </summary>
        [JsonProperty("function_description", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string FunctionDescription { get; set; }

        /// <summary>
        /// Example - Unity.Muse.Chat.Context.SmartContext.ContextRetrievalTools:ProjectStructureExtractor
        /// <br/>Warning - Function namespace + Function names must be unique.
        /// <br/>
        /// </summary>
        [JsonProperty("function_namespace", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string FunctionNamespace { get; set; }

        /// <summary>
        /// Example - ProjectStructureExtractor
        /// <br/>Warning - Function namespace + Function names must be unique.
        /// <br/>
        /// </summary>
        [JsonProperty("function_name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string FunctionName { get; set; }

        [JsonProperty("function_return", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public FunctionReturnObject FunctionReturn { get; set; }

        /// <summary>
        /// The parameters that are required to call the function - order is important,
        /// <br/>name is maybe not used for function calling.
        /// <br/>
        /// </summary>
        [JsonProperty("function_parameters", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<FunctionParametersObject> FunctionParameters { get; set; }


    public partial class FunctionReturnObject
    {
        /// <summary>
        /// The parameters type, in the form of the origin language. I.E.
        /// <br/>functions originating from Unity should be C# types.
        /// <br/>
        /// </summary>
        [JsonProperty("type", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        /// <summary>
        /// A description of the parameter used by the LLM
        /// <br/>
        /// </summary>
        [JsonProperty("description", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }

    public partial class FunctionParametersObject
    {
        /// <summary>
        /// The name of the parameter
        /// <br/>
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// The parameters type, in the form of the origin language. I.E.
        /// <br/>functions originating from Unity should be C# types.
        /// <br/>
        /// </summary>
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        /// <summary>
        /// A description of the parameter used by the LLM
        /// <br/>
        /// </summary>
        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }
    }}}
}
