using System;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class UsageProgressText : VisualElement
    {
        Text m_Usage = new() {name = "points"};

        public UsageProgressText()
        {
            Add(new Text {text = TextContent.subUsageUsed});
            Add(m_Usage);
            this.AddManipulator(new UsageTracker(Refresh));
        }

        void Refresh()
        {
            m_Usage.text = AccountInfo.Instance.Usage.Label;
            m_Usage.tooltip = AccountInfo.Instance.Usage.Tooltip;
        }
    }
}
