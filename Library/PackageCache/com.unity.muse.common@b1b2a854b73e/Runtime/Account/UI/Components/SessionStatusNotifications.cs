using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class SessionStatusNotifications : VisualElement
    {
        static readonly HashSet<string> k_DisabledEntitlementNotifications = new();

        VisualElement m_View;

        Model m_Model;

        public SessionStatusNotifications()
        {
            AddToClassList("notifications-area");
            this.AddManipulator(new SessionChangesTracker(Refresh));

            this.RegisterContextChangedCallback<Model>(OnModelChanged);
        }

        void OnModelChanged(ContextChangedEvent<Model> evt)
        {
            SetModel(evt?.context);
        }

        void SetModel(Model model)
        {
            m_Model = model;
            Refresh();
        }

        void Refresh()
        {
            var view = View(m_Model);
            if (view.GetType() == m_View?.GetType())
                return;     // Don't update if it hasn't changed.

            m_View = view;

            Clear();
            Add(m_View);
        }

        static VisualElement View(Model model)
        {
            if (!NetworkState.IsAvailable)
                return new NetworkNotificationView();
            if (SignInUtility.IsSignedOut)
                return new SignInNotificationView(true);
            if (ClientStatus.Instance.Status.IsDeprecated)
                return new ClientDeprecatedNotificationView(true);

            if (!AccountInfo.Instance.IsEntitled)
            {
                if (model && k_DisabledEntitlementNotifications.Contains(model.CurrentMode))
                    return new();

                if (AccountInfo.Instance.Organization is {Status: SubscriptionStatus.SubscriptionExpired})
                    return new SubscriptionExpiredNotificationView(true);
                if (AccountInfo.Instance.Organization is {Status: SubscriptionStatus.TrialExpired})
                    return new TrialExpiredNotificationView(true);
                return new TrialNotificationView(true);
            }

            if (ClientStatus.Instance.Status.WillBeDeprecated)
                return new ClientWillBeDeprecatedNotificationView(ClientStatus.Instance.Status.ObsoleteDate, true);
            if (ClientStatus.Instance.Status.NeedsUpdate)
                return new ClientUpdateNotificationView(true);

            return new();
        }

        internal static void DisableEntitlementsNotificationsForMode(string mode) => k_DisabledEntitlementNotifications.Add(mode);

        internal static void EnableEntitlementsNotificationsForMode(string mode) => k_DisabledEntitlementNotifications.Remove(mode);
    }
}
