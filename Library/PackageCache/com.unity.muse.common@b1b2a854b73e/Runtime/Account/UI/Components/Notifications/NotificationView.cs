using System;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class NotificationView : VisualElement
    {
        Action m_OnAction;
        SubscriptionCallout m_Callout;

        public NotificationView(NotificationOptions options)
        {
            AddToClassList("notification-view");
            AddToClassList($"style-{options.style.ToString().ToLower()}");

            if (options.action != null)
                m_OnAction += options.action;

            m_Callout = new SubscriptionCallout();
            if (options.style == MuseNotificationStyle.Alert)
            {
                m_Callout.titleContainer.Add(new Icon
                {
                    name = AlertDialog.iconUssClassName,
                    iconName = "warning",
                    variant = IconVariant.Fill,
                    pickingMode = PickingMode.Ignore
                });
            }
            m_Callout.titleContainer.Add(new Text {text = options.titleText});
            m_Callout.Add(new Text {text = options.description});
            Add(m_Callout);

            AddButton(options.buttonText, options.buttonIcon, m_OnAction, options.inlineButton);

            if (options.showOrganizations)
                m_Callout.Add(new OrganizationSelection());
        }

        protected void AddButton(string text, string icon, Action action, bool inline)
        {
            var button = new SizeToContentButton(action, inline)
            {
                button =
                {
                    title = text,
                    trailingIcon = icon,
                    size = Size.S
                }
            };

            if (!string.IsNullOrEmpty(text))
            {
                if (inline)
                    m_Callout.Add(button);
                else
                    Add(button);
            }
        }
    }
}
