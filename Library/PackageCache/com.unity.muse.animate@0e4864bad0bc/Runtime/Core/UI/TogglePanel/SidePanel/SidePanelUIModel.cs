using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class SidePanelUIModel : TogglePanelUIModel
    {   
        public SidePanelUIModel()
        {
            IsVisible = true;
        }

        public void AddPanel(SidePanelUtils.PageType page, VisualElement panel)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"Adding panel {panel} to page: {page}");

            base.AddPanel((int)page, panel);

        }

        public void RemovePanel(SidePanelUtils.PageType page, VisualElement panel)
        { 
            DevLogger.LogSeverity(TraceLevel.Verbose, $"Removing panel {panel} from page: {page}");
            
            base.RemovePanel((int)page, panel);
        }
    }
}
