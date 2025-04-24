using System;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    abstract class AdaptiveListViewEntry : ManagedTemplate
    {
        int m_Index;

        protected AdaptiveListViewEntry(string basePath = null)
            : base(basePath ?? MuseChatConstants.UIModulePath)
        {
        }

        public event Action<int, VisualElement> SelectionChanged;

        public int Index => m_Index;

        public virtual void SetData(int index, object data, bool isSelected = false)
        {
            m_Index = index;
        }

        protected void NotifySelectionChanged()
        {
            SelectionChanged?.Invoke(m_Index, this);
        }
    }
}
