using Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
    class TogglePanelMenuButton : IconButton
    {
        const string k_ButtonUssClassName = "deeppose-toggle-panel-menu-button";
        
        public int Index { get; }
        
        public TogglePanelMenuButton(int index, string icon, string tooltip) : base(icon)
        {
            Index = index;
            AddToClassList(k_ButtonUssClassName);
            this.tooltip = tooltip;
            this.SetPreferredTooltipPlacement(PopoverPlacement.Right);
        }
    }
}
