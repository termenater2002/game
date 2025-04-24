#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common
{
    static class SignInUtility
    {
        [InitializeOnLoadMethod]
        static void Start()
        {
            RefreshSignIn();
            UnityConnectUtils.RegisterConnectStateChangedEvent(_ => RefreshSignIn());
        }

        public static event Action OnChanged;

        /// <summary>
        /// LoggedIn value is only valid if Ready is true, otherwise it is unknown
        /// </summary>
        public static SignInState state => s_State;
        public static bool IsLikelySignedIn => state is SignInState.SignedIn or SignInState.NotReady;
        public static bool IsSignedOut => state is SignInState.SignedOut;
        static SignInState s_State = SignInState.NotReady;

        static void RefreshSignIn()
        {
            if (!UnityConnectUtils.GetIsUserInfoReady())
            {
                EditorApplication.delayCall += RefreshSignIn;
                return;
            }

            var currentState = UnityConnectUtils.GetIsLoggedIn() ? SignInState.SignedIn : SignInState.SignedOut;
            if (s_State == currentState)
                return;

            s_State = currentState;
            OnChanged?.Invoke();
        }
    }
}
#endif