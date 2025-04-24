using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class InspectorsPanelViewModel : TogglePanelUIModel
    {
        public InspectorsPanelViewModel()
        {
            IsVisible = true;
        }
        
        public void AddPanel(InspectorsPanelUtils.PageType page, VisualElement panel)
        {
            base.AddPanel((int)page, panel);
        }

        public void RemovePanel(InspectorsPanelUtils.PageType page, VisualElement panel)
        {
            base.RemovePanel((int)page, panel);
        }

    }
}
