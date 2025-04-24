using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    abstract class InspectorView : UITemplateContainer, IUITemplate
    {
        InspectorViewModel m_Model;
        bool m_IsAttachedToPage;
        
        const string k_USSClassName = "deeppose-inspector";
        
        protected InspectorViewModel Model => m_Model;

        protected InspectorView()
            : base(k_USSClassName)
        {
            pickingMode = PickingMode.Ignore;
        }
        
        protected void SetModel(InspectorViewModel inspectorViewModel)
        {
            UnregisterModel();
            m_Model = inspectorViewModel;
            RegisterModel();
            Update();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged += OnChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnChanged;
            m_Model = null;
        }

        void OnChanged()
        {
            UpdateVisibility();
            UpdateUI();
        }

        public void Update()
        {
        }

        protected abstract void UpdateUI();
        void UpdateVisibility()
        {
            if (m_Model == null)
                return;

            switch (m_IsAttachedToPage)
            {
                case false when m_Model.IsVisible:
                    AddToInspectorsMenu();
                    break;
                case true when !m_Model.IsVisible:
                    RemoveFromInspectorsMenu();
                    break;
            }
        }

        void AddToInspectorsMenu()
        {
            m_IsAttachedToPage = true;
            style.display = DisplayStyle.Flex;
            
            if (m_Model.InspectorsPanel != null)
            {
                m_Model.InspectorsPanel.AddPanel(m_Model.PageType, this);
            }
        }

        void RemoveFromInspectorsMenu()
        {
            m_IsAttachedToPage = false;
            style.display = DisplayStyle.None;
            
            if (m_Model.InspectorsPanel != null)
            {
                m_Model.InspectorsPanel.RemovePanel(m_Model.PageType, this);
            }
        }
    }
}
