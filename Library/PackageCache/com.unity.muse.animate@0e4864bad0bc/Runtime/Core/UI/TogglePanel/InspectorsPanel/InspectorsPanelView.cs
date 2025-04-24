using System;
using UnityEngine;
using AppUI = Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class InspectorsPanelView : TogglePanel, IUITemplate
    {
        const string k_UssClassName = "deeppose-inspectors-panel";

        public InspectorsPanelView() : base(k_UssClassName) { }

        void IUITemplate.InitComponents()
        {
            base.InitComponents();
            
            var pageTypes = Enum.GetValues(typeof(InspectorsPanelUtils.PageType));
            
            for (var i = 0; i < pageTypes.Length; i++)
            {
                var pageType = (InspectorsPanelUtils.PageType)i;
                AddPage(pageType.GetTitle(), pageType.GetIcon());
                AddButton(pageType.GetIcon(), pageType.GetTooltip());
            }
        }

        public void SetModel(InspectorsPanelViewModel model)
        {
            base.SetModel(model);
            
            if (model == null)
                return;
            
            model.DefaultWidth = 200;
            Update();
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<InspectorsPanelView, UxmlTraits> { }
#endif
    }
}
