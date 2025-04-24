using Unity.Muse.Chat.WebApi;
using Unity.Muse.Common;
using Unity.Muse.Common.Account;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ServerCompatibility
{
    /// <summary>
    /// Sets the target's enabled state based on server version status.
    /// </summary>
    class ServerCompatibilityTracker : Manipulator
    {
        readonly Model m_Model;
        readonly WebAPI k_WebAPI = new();

        public ServerCompatibilityTracker(Model model = null)
        {
            m_Model = model;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            Chat.ServerCompatibility.Bind(Refresh);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            Chat.ServerCompatibility.OnCompatibilityChanged -= Refresh;
        }

        void Refresh(Chat.ServerCompatibility.CompatibilityStatus status)
        {
            // This is copied from com.unity.muse.common/Runtime/Account/UI/Manipulators/SessionStatusTracker.cs so that
            // in cases where the session state disables the UI, the server compatibility state does not enable it
            var isSessionUsable = m_Model
                ? SessionStatus.GetSessionUsabilityForMode(m_Model.CurrentMode)
                : SessionStatus.IsSessionUsable;

            if(!isSessionUsable)
                return;

            var usable = status != Chat.ServerCompatibility.CompatibilityStatus.Unsupported;
            target?.SetEnabled(usable);
        }
    }
}
