using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class SubscriptionView : VisualElement
    {
        public Action OnDismiss;

        public SubscriptionView()
        {
            var subscriptionType = new Text(
                AccountInfo.Instance.Organization.Status == SubscriptionStatus.FreeTrial ?
                    TextContent.subscriptionTypeTitleTrial:
                    TextContent.subscriptionTypeTitleSubscribed);
            subscriptionType.AddToClassList("dropdown-subscription-type");
            Add(subscriptionType);

            Add(new UsageView());

            var menuItems = new VisualElement();
            menuItems.AddToClassList("menu-items");

            menuItems.Add(new ExternalLinkMenuItem
            {
                label = TextContent.goToMuseAccount,
                clickable = new Pressable(() =>
                {
                    OnDismiss?.Invoke();
                    AccountUtility.GoToMuseAccount();
                })
            });

            if (AccountInfo.Instance.Organization.Status != SubscriptionStatus.Entitled)
            {
                menuItems.Add(new ExternalLinkMenuItem
                {
                    name = "subscribe-to-muse-menu-item",
                    label = TextContent.subNotificationSubscribeAction,
                    clickable = new Pressable(() =>
                    {
                        OnDismiss?.Invoke();
                        AccountLinks.StartSubscription();
                    })
                });
            }
            
            menuItems.Add(new MenuDivider());
            
            menuItems.Add(new ExternalLinkMenuItem
            {
                label = TextContent.experimentalProgram,
                clickable = new Pressable(() =>
                {
                    OnDismiss?.Invoke();
                    AccountLinks.ExperimentalProgram();
                })
            });

            Add(menuItems);
        }
    }
}
