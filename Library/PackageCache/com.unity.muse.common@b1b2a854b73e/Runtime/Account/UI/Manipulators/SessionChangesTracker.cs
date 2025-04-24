using System;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    /// <summary>
    /// Updates element on any potential changes to the various session states such as current organization, account status, connectivity, etc...
    /// </summary>
    class SessionChangesTracker : Manipulator
    {
        Action m_Callback;

        public SessionChangesTracker(Action callback, bool callImmediately = true)
        {
            m_Callback = callback;
            if (callImmediately)
                Refresh();
        }

        protected override void RegisterCallbacksOnTarget() => SessionStatus.OnChanges += Refresh;
        protected override void UnregisterCallbacksFromTarget() => SessionStatus.OnChanges -= Refresh;

        void Refresh() => m_Callback?.Invoke();
    }
}
