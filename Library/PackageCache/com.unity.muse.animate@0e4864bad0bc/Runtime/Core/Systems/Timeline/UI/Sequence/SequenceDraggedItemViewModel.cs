using UnityEngine;

namespace Unity.Muse.Animate
{
    class SequenceDraggedItemViewModel
    {
        public delegate void Changed();
        public event Changed OnChanged;

        public TimelineModel.SequenceKey Key { get; set; }

        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                {
                    return;
                }

                m_IsVisible = value;
                OnChanged?.Invoke();
            }
        }

        public Texture2D Thumbnail => Key.Thumbnail.Texture;

        bool m_IsVisible;
    }
}
