using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    abstract class SequenceItemView<T> : VisualElement
    {
        const string k_BaseUssClassName = "deeppose-sequence-item";
        string m_UssClassNameSelected;
        string m_UssClassNameHighlighted;
        string m_UssClassNameEditing;
        protected SequenceItemViewModel<T> m_Model;

        protected SequenceItemView(string ussClassName = null, string ussClassNameSelected = null,
            string ussClassNameHighlighted = null, string ussClassNameEditing = null)
        {
            m_UssClassNameSelected = ussClassNameSelected;
            m_UssClassNameHighlighted = ussClassNameHighlighted;
            m_UssClassNameEditing = ussClassNameEditing;
            
            if (!string.IsNullOrWhiteSpace(k_BaseUssClassName))
            {
                AddToClassList(k_BaseUssClassName);
            }
            
            if (!string.IsNullOrWhiteSpace(ussClassName))
            {
                AddToClassList(ussClassName);
            }
            
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            
            focusable = true;
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            RegisterCallback<ClickEvent>(OnClicked);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            UnregisterCallback<ClickEvent>(OnClicked);
            UnregisterCallback<PointerDownEvent>(OnPointerDown);
        }
        
        void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 1)
            {
                m_Model?.Clicked(evt.button);
                evt.StopPropagation();
            }
        }

        public void SetModel(SequenceItemViewModel<T> viewModel)
        {
            if (m_Model != null)
                UnregisterModel();

            m_Model = viewModel;

            if (m_Model == null)
                return;
            
            RegisterModel();
            Update();
        }

        protected virtual void Update()
        {
            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;

            if (m_Model.IsEditing && !string.IsNullOrWhiteSpace(m_UssClassNameEditing))
            {
                AddToClassList(m_UssClassNameEditing);
                RemoveFromClassList(m_UssClassNameHighlighted);
                RemoveFromClassList(m_UssClassNameSelected);
            }
            else if (m_Model.IsHighlighted && !string.IsNullOrWhiteSpace(m_UssClassNameHighlighted))
            {
                AddToClassList(m_UssClassNameHighlighted);
                RemoveFromClassList(m_UssClassNameEditing);
                RemoveFromClassList(m_UssClassNameSelected);
            }
            else
            {
                RemoveFromClassList(m_UssClassNameHighlighted);
                RemoveFromClassList(m_UssClassNameEditing);
                RemoveFromClassList(m_UssClassNameSelected);
            }
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged += OnModelChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnModelChanged;
            m_Model = null;
        }

        void OnModelChanged(SequenceItemViewModel<T> item)
        {
            Update();
        }

        protected virtual void OnClicked(ClickEvent evt)
        {
            m_Model?.Clicked();
        }
    }
}
