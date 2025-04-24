using System;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;

namespace Unity.Muse.StyleTrainer
{
    class SignInEvent : BaseEvent<SignInEvent>
    {
        public bool signIn;
    }

    class SignInController: IDisposable
    {
#if UNITY_EDITOR
        bool m_Initialized;
        ChangeInfo m_Info = new();
        bool? m_LoggedIn;
        EventBus m_EventBus;

        public static Action<bool> ForceSignInTokenRefresh = _ => { };

        public void Init(EventBus evtBus)
        {
            if (m_Initialized) return;
            m_Initialized = true;
            m_EventBus = evtBus;
            m_Info.eventDelegate = UnityConnectUtils.RegisterConnectStateChangedEvent(_ => StateSignOnChange());
            ForceSignInTokenRefresh += ForceSignInTokenRefreshCallback;
        }

        void ForceSignInTokenRefreshCallback(bool obj)
        {
            m_EventBus.SendEvent(new SignInEvent
            {
                signIn = obj
            });
        }

        void RefreshSignIn()
        {
            var loggedIn = UnityConnectUtils.GetIsLoggedIn();
            if (m_LoggedIn is not null && m_LoggedIn.Value == loggedIn)
                return;
            m_LoggedIn = loggedIn;
            m_EventBus.SendEvent(new SignInEvent
            {
                signIn = m_LoggedIn.Value
            });
        }

        void StateSignOnChange()
        {
            RefreshSignIn();
        }
#else
        public void Init(EventBus evtBus) { }
#endif
        public void Dispose()
        {
#if UNITY_EDITOR
            m_Info.eventDelegate = null;
            ForceSignInTokenRefresh -= ForceSignInTokenRefreshCallback;
#endif
        }
    }
}