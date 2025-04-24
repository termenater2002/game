using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Common.Account;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ServerCompatibility
{
    class ServerCompatibilityNotifications : VisualElement
    {
        VisualElement m_View;
        Model m_Model;

        public ServerCompatibilityNotifications()
        {
            AddToClassList("notifications-area");
            this.AddManipulator(new ServerCompatibilityChanges(Refresh));
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
            if (view == null)
            {
                style.minHeight = 0;
                m_View = null;
                Clear();
                return;
            }

            if (view.GetType() == m_View?.GetType())
                return;     // Don't update if it hasn't changed.

            style.minHeight = 100;
            m_View = view;
            Clear();
            Add(m_View);
        }

        VisualElement View(Model model)
        {
            // This is copied from com.unity.muse.common/Runtime/Account/UI/Manipulators/SessionStatusTracker.cs so that
            // in cases where the session state shows a notification, the server compatibility state does not also show
            // a notification
            var isSessionUsable = model
                ? SessionStatus.GetSessionUsabilityForMode(model.CurrentMode)
                : SessionStatus.IsSessionUsable;

            if (!isSessionUsable)
                return null;

            if(Chat.ServerCompatibility.Status == Chat.ServerCompatibility.CompatibilityStatus.Unsupported)
                return new ServerCompatibilityNotSupportedNotificationView();

            if(Chat.ServerCompatibility.Status == Chat.ServerCompatibility.CompatibilityStatus.Deprecated)
                return new ServerCompatibilityDeprecatedNotificationView(Clear);

            return null;
        }
    }
}
