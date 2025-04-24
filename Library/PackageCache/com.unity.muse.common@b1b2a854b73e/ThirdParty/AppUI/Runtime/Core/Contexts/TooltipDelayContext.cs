namespace Unity.AppUI.Core
{
    /// <summary>
    /// The TooltipDelay context of the application.
    /// </summary>
    /// <param name="tooltipDelayMs"> The delay in milliseconds before a tooltip is shown. </param>
    internal record TooltipDelayContext(int tooltipDelayMs) : IContext
    {
        /// <summary>
        /// The delay in milliseconds before a tooltip is shown.
        /// </summary>
        public int tooltipDelayMs { get; } = tooltipDelayMs;
    }
}
