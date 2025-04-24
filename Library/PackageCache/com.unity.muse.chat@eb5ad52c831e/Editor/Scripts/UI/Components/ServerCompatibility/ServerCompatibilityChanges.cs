using System;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ServerCompatibility
{
    /// <summary>
    /// Updates elements base on the compatibility of this frontend against the current live server.
    /// </summary>
    class ServerCompatibilityChanges : Manipulator
    {
        Action m_Callback;

        public ServerCompatibilityChanges(Action callback, bool callImmediately = true)
        {
            m_Callback = callback;
            if (callImmediately)
                Refresh(Chat.ServerCompatibility.Status);
        }

        protected override void RegisterCallbacksOnTarget()
            => Chat.ServerCompatibility.OnCompatibilityChanged += Refresh;

        protected override void UnregisterCallbacksFromTarget()
            => Chat.ServerCompatibility.OnCompatibilityChanged -= Refresh;

        void Refresh(Chat.ServerCompatibility.CompatibilityStatus compatibilityStatus) => m_Callback?.Invoke();
    }
}
