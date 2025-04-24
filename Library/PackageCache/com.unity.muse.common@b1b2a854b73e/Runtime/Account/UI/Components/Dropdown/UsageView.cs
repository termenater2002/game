using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class UsageView : VisualElement
    {
        public UsageView()
        {
            Add(new UsageProgressText());
            Add(new UsageProgressBar());
        }
    }
}
