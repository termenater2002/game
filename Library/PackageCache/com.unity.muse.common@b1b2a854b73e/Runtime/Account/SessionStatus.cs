using System;
using System.Collections.Generic;

namespace Unity.Muse.Common.Account
{
    static class SessionStatus
    {
        public static bool IsSessionUsable { get; private set; }
        public static Action<bool> OnUsabilityChanged;
        public static Action OnChanges;

        static readonly HashSet<string> k_ModesWithoutEntitlements = new();

        static SessionStatus()
        {
            AccountInfo.Instance.OnOrganizationChanged += Refresh;
            ClientStatus.Instance.OnClientStatusChanged += _ => Refresh();
            NetworkState.OnChanged += Refresh;
            SignInUtility.OnChanged += Refresh;
            GlobalPreferences.OnReady += Refresh;
            ExperimentalProgram.Changed += Refresh;
            Refresh();
        }

        static void Refresh()
        {
            if (!GlobalPreferences.IsReady)
                return;

            var isSessionUsable =
                AccountInfo.Instance.IsEntitled &&
                ClientStatus.Instance.Status.IsValid &&
                NetworkState.IsAvailable &&
                SignInUtility.IsLikelySignedIn;

            if (IsSessionUsable != isSessionUsable)
            {
                IsSessionUsable = isSessionUsable;
                OnUsabilityChanged?.Invoke(IsSessionUsable);
            }

            // Invoke on any potential changes to session status
            OnChanges?.Invoke();
        }

        internal static bool GetSessionUsabilityForMode(string mode)
        {
            if (string.IsNullOrEmpty(mode) || !GlobalPreferences.IsReady)
                return IsSessionUsable;

            var entitled = k_ModesWithoutEntitlements.Contains(mode) || AccountInfo.Instance.IsEntitled;
            var isSessionUsable =
                entitled &&
                ClientStatus.Instance.Status.IsValid &&
                NetworkState.IsAvailable &&
                SignInUtility.IsLikelySignedIn;

            return isSessionUsable;
        }

        internal static void RegisterModeWithoutEntitlements(string mode) => k_ModesWithoutEntitlements.Add(mode);

        internal static void UnregisterModeWithoutEntitlements(string mode) => k_ModesWithoutEntitlements.Remove(mode);
    }
}
