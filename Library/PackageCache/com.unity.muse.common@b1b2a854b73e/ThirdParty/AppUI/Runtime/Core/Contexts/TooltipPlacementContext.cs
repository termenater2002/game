using System;
using Unity.Muse.AppUI.UI;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// The preferred placement of a tooltip in the current context.
    /// </summary>
    /// <param name="placement"> The preferred placement of a tooltip. </param>
    internal record TooltipPlacementContext(PopoverPlacement placement) : IContext
    {
        /// <summary>
        /// The preferred placement of a tooltip.
        /// </summary>
        public PopoverPlacement placement { get; } = placement;
    }
}
