namespace Unity.AppUI.Redux
{
    /// <summary>
    /// Options for the async thunk.
    /// </summary>
    internal class AsyncThunkOptions
    {
        /// <summary>
        /// Whether to dispatch a rejection action in the store when the condition is not met.
        /// </summary>
        public bool dispatchConditionRejection { get; set; }
    }
}
