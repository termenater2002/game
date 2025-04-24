using UnityEngine;

namespace Unity.Muse.Animate
{
    class SequenceAddKeyButtonViewModel
    {
        public delegate void Changed();
        public event Changed OnChanged;

        TimelineModel m_TimelineModel;
        SequenceViewModel m_Sequence;

        public SequenceAddKeyButtonViewModel(SequenceViewModel sequence)
        {
            m_Sequence = sequence;
        }

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

        bool m_IsVisible = true;

        public void AddKey()
        {
            m_Sequence.AddKeyToEnd();
        }
    }
}
