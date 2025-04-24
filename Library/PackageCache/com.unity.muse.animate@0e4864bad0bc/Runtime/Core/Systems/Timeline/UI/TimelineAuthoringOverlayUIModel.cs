using System;

namespace Unity.Muse.Animate
{
    class TimelineAuthoringOverlayUIModel
    {
        public event Action<Property> OnChanged;
        public event Action OnRequestExport;

        public enum Property
        {
            Visibility
        }

        bool m_IsVisible;
        TimelineAuthoringModel m_Model;

        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnChanged?.Invoke(Property.Visibility);
            }
        }

        void RegisterModel(TimelineAuthoringModel model)
        {
            UnregisterModel();
            
            if (model == null)
                return;
            
            m_Model = model;
            m_Model.OnChanged += OnModelChangedProperty;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnModelChangedProperty;
            m_Model = null;
        }

        void OnModelChangedProperty()
        {
            // TODO
        }

        public void RequestExport()
        {
            OnRequestExport?.Invoke();
        }
    }
}