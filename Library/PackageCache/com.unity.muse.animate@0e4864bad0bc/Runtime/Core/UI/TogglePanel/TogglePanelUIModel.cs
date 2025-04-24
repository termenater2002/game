using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class TogglePanelUIModel
    {
        
        public event Action OnSelectedPageIndexChanged;
        public event Action OnDefaultWidthChanged;
        public event Action OnChanged;
        
        public delegate void PanelAdded(int pageIdx, VisualElement panel);
        public event PanelAdded OnPanelAdded;
        
        public delegate void PanelRemoved(int pageIdx, VisualElement panel);
        public event PanelRemoved OnPanelRemoved;
        
        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnChanged?.Invoke();
            }
        }
        
        public int SelectedPageIndex
        {
            get => m_SelectedPageIndex;
            set
            {
                if (m_SelectedPageIndex != value)
                {
                    m_SelectedPageIndex = value;
                    OnSelectedPageIndexChanged?.Invoke();
                }
            }
        }
        
        public int DefaultWidth
        {
            get => m_DefaultWidth;
            set
            {
                if (m_DefaultWidth != value)
                {
                    m_DefaultWidth = value;
                    OnDefaultWidthChanged?.Invoke();
                }
            }
        }
        
        bool m_IsVisible;
        int m_SelectedPageIndex = -1;
        int m_DefaultWidth;

        protected TogglePanelUIModel()
        {
            IsVisible = true;
        }
        
        protected void AddPanel(int pageIdx, VisualElement panel)
        {
            OnPanelAdded?.Invoke(pageIdx, panel);
        }

        protected void RemovePanel(int pageIdx, VisualElement panel)
        {
            OnPanelRemoved?.Invoke(pageIdx, panel);
        }
    }
}
