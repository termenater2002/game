namespace Unity.Muse.Chat.BackendApi.Model
{
    internal partial class ParameterDefinition
    {
        /// <summary>
        /// Whether this parameter is optional or not. Parameters with the params keyword in C# are considered optional.
        /// </summary>
        public bool Optional { get; set; }
    }
}
