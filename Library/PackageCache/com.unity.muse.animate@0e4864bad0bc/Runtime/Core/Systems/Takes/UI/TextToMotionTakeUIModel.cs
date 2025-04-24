using System;

namespace Unity.Muse.Animate
{
    class TextToMotionTakeUIModel
    {   
        TextToMotionTake m_TakeModel;
        public event Action OnTitleChanged;

        public TextToMotionTake Target
        {
            get => m_TakeModel;
            set
            {
                if (m_TakeModel == value)
                    return;
                
                UnregisterEvents();
                m_TakeModel = value;
                RegisterEvents();
                OnTitleChanged?.Invoke();
            }
        }

        public string Title
        {
            get => m_TakeModel?.Title ?? string.Empty;
            set
            {
                if (m_TakeModel == null)
                    return;
                
                m_TakeModel.Title = value;
            }
        }

        public string Description => m_TakeModel?.Description ?? string.Empty;

        void RegisterEvents()
        {
            if (m_TakeModel == null)
                return;
            
            m_TakeModel.OnChanged += OnTakeChanged;
        }
        
        void UnregisterEvents()
        {
            if (m_TakeModel == null)
                return;
            
            m_TakeModel.OnChanged -= OnTakeChanged;
        }

        void OnTakeChanged(LibraryItemModel.Property property)
        {
            if (property is LibraryItemModel.Property.Title)
            {
                OnTitleChanged?.Invoke();
            }
        }
    }
}
