using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{

#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class BakingNoticeView : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-notice";

        const string k_MainLabelName = "notice-main-label";
        const string k_IconName = "notice-icon";

        BakingNoticeViewModel m_Model;
        Icon m_Icon;
        Label m_Label;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<BakingNoticeView, UxmlTraits> { }
#endif

        public BakingNoticeView()
            : base(k_UssClassName) { }

        public void FindComponents()
        {
            m_Icon = this.Q<Icon>(k_IconName);
            m_Label = this.Q<Label>(k_MainLabelName);
        }

        public void SetModel(BakingNoticeViewModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
            Update();
        }

        public void Update()
        {
            if (m_Model == null)
                return;

            if (!IsAttachedToPanel)
                return;
            
            UpdateVisibility();
            m_Icon.iconName = m_Model.Icon;
            m_Label.text = m_Model.MainLabel;
        }
        
        void UpdateVisibility()
        {
            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged += OnModelChanged;
            Update();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnModelChanged;
            m_Model = null;
        }

        void OnModelChanged()
        {
            Update();
        }
    }
}
