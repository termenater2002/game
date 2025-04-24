using System;
using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    static class InspectorsPanelUtils
    {
        public enum PageType
        {
            ApplicationSettings,
            TimelineBakingSettings,
            SelectedEntity,
            SelectedHandsSettings,
            SelectedSequencerItem
        }
        
        static readonly Dictionary<PageType, (string title, string icon, string tooltip)> k_PageInfos = new()
        {
            { PageType.ApplicationSettings, ("Application", "settings", "Open the application settings") },
            { PageType.TimelineBakingSettings, ("Timeline Baking", "deep-pose-timeline-baking", "Open the timeline baking settings") },
            { PageType.SelectedEntity, ("Entity", "deep-pose-actor-selected", "Open the selected entity settings") },
            { PageType.SelectedHandsSettings, ("Hands", "deep-pose-hands", "Open the hands settings") },
            { PageType.SelectedSequencerItem, ("Sequencer", "deep-pose-timeline-selected", "Open the sequencer key settings") }
        };

        public static string GetTitle(this PageType type) => k_PageInfos[type].title;
        public static string GetIcon(this PageType type) => k_PageInfos[type].icon;
        public static string GetTooltip(this PageType type) => k_PageInfos[type].tooltip;
    }
}
