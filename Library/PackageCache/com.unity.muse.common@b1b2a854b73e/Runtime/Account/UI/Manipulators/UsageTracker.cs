using System;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class UsageTracker : Manipulator
    {
        static Action s_UpdateUsage;
        static Action UpdateUsage => s_UpdateUsage ??= EventServices.IntervalDebounce(AccountInfo.Instance.UpdateUsage, 5f);

        Action m_Callback;

        public UsageTracker(Action callback, bool callImmediately = true)
        {
            m_Callback = callback;
            UpdateUsage();
            if (callImmediately)
                RefreshUsage();
        }

        protected override void RegisterCallbacksOnTarget() => AccountInfo.Instance.OnUsageChanged += RefreshUsage;
        protected override void UnregisterCallbacksFromTarget() => AccountInfo.Instance.OnUsageChanged -= RefreshUsage;

        void RefreshUsage() => m_Callback?.Invoke();
    }
}
