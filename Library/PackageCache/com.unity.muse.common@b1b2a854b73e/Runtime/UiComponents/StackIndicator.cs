using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    class StackIndicator : VisualElement
    {
        readonly Image m_StackIndicatorImage;

        readonly Text m_CounterText;

        public override VisualElement contentContainer => m_StackIndicatorImage;
        
        int m_Counter;

        public int count
        {
            get => m_Counter;
            set
            {
                m_Counter = value;
                m_CounterText.text = m_Counter.ToString();
            }
        }

        public StackIndicator()
        {
            AddToClassList("muse-stack-indicator");
            pickingMode = PickingMode.Ignore;
            focusable = false;
            
            m_StackIndicatorImage = new Image ();
            m_StackIndicatorImage.AddToClassList("muse-stack-indicator__image");
            hierarchy.Add(m_StackIndicatorImage);
            
            m_CounterText = new Text();
            m_CounterText.AddToClassList("muse-stack-indicator__counter");
            hierarchy.Add(m_CounterText);
            
            count = 0;
        }
    }
}
