namespace Unity.AppUI.Redux
{
    /// <summary>
    /// The status of the thunk.
    /// </summary>
    internal enum ThunkStatus
    {
        /// <summary>
        /// The thunk is pending.
        /// </summary>
        Pending,

        /// <summary>
        /// The thunk has been fulfilled.
        /// </summary>
        Fulfilled,

        /// <summary>
        /// The thunk has been rejected.
        /// </summary>
        Rejected
    }
}
