using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
    internal class SampleOutputListRowItem : ExVisualElement
    {
        float m_Ratio = 1.0f;
        public SampleOutputListRowItem(VisualElement item, float ratio)
        {
            m_Ratio = ratio;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Add(item);
        }

        public float ratio
        {
            get => m_Ratio;
            set
            {
                if (m_Ratio != value)
                {
                    m_Ratio = value;
                    style.width = resolvedStyle.height * m_Ratio;
                }
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            style.width = resolvedStyle.height * m_Ratio;
        }
    }
}