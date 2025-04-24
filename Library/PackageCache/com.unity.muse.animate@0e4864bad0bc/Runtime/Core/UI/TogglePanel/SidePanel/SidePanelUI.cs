using System;
using UnityEngine;
using AppUI = Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class SidePanelUI : TogglePanel, IUITemplate
    {
        const string k_UssClassName = "deeppose-inspectors-panel";

        SidePanelUIModel m_Model;
        
        public SidePanelUI()
            : base(k_UssClassName) { }

        void IUITemplate.InitComponents()
        {
            base.InitComponents();
            
            var pageTypes = Enum.GetValues(typeof(SidePanelUtils.PageType));
            var nbPages = pageTypes.Length;
            
            for (var i = 0; i < nbPages; i++)
            {
                var pageType = (SidePanelUtils.PageType)i;
                AddPage(pageType.GetTitle(), pageType.GetIcon());
                AddButton(pageType.GetIcon(), pageType.GetTooltip());
            }
        }

        public void SetModel(SidePanelUIModel model)
        {
            m_Model = model;
            base.SetModel(model);
            Update();
        }
        
        public override void ClickedLeft()
        {
            SelectPage(-1);
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<SidePanelUI, UxmlTraits> { }
#endif
    }
}
