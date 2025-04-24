using System;
using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    static class SidePanelUtils
    {
        public enum PageType
        {
            TakesLibrary,
            TimelinesLibrary,
            PosesLibrary,
            ActorsLibrary,
            PropsLibrary,
            StagesLibrary,
            ConvertToFrames,
        }

        static readonly Dictionary<PageType, (string title, string icon, string tooltip)> k_PageInfos = new()
        {
            { PageType.TakesLibrary, ("Generations", "caret-double-right", "Open the Generations panel") },
            { PageType.TimelinesLibrary, ("Timelines", "film-roll", "Open the Timelines panel") },
            { PageType.PosesLibrary, ("Poses", "deep-pose-poses-library", "Open the Poses Library panel") },
            { PageType.ActorsLibrary, ("Actors", "deep-pose-actors-library", "Open the Actors Library panel") },
            { PageType.PropsLibrary, ("Props", "deep-pose-props-library", "Open the Props Library panel") },
            { PageType.StagesLibrary, ("Stages", "deep-pose-stages-library", "Open the Stages Library panel") },
            { PageType.ConvertToFrames, ("Convert to Frames", "deep-pose-convert-to-frames", "Open the Convert to Frames panel") },
        };

        public static string GetTitle(this PageType type) => k_PageInfos[type].title;
        public static string GetIcon(this PageType type) => k_PageInfos[type].icon;
        public static string GetTooltip(this PageType type) => k_PageInfos[type].tooltip;
    }
}
