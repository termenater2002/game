using System;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class SubscriptionCallout : VisualElement
    {
        public VisualElement titleContainer;

        public SubscriptionCallout()
        {
            AddToClassList("subscription-callout-container");

            titleContainer = new VisualElement();
            titleContainer.AddToClassList("title-container");

            Add(titleContainer);
        }
    }
}
