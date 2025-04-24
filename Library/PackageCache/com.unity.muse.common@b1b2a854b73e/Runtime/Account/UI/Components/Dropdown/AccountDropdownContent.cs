using System;
using Unity.Muse.Common.Account;
using Unity.Muse.Common.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    class AccountDropdownContent : VisualElement
    {
        public Action OnAction;
        
        readonly ExperimentalProgramLimitReachedNotificationView m_ExperimentalProgramView;
        
        readonly ExperimentalProgramSignUpNotificationView m_SignUp;
        
        public AccountDropdownContent()
        {
            AddToClassList("muse-account-settings");
            
            m_ExperimentalProgramView = new ExperimentalProgramLimitReachedNotificationView(false);
            m_SignUp = new ExperimentalProgramSignUpNotificationView(false);
            
            this.AddManipulator(new SessionChangesTracker(Refresh, false));
            this.AddManipulator(new ExperimentalProgramChangedTracker(Refresh));
        }

        void Refresh()
        {
            Clear();
            Add(View());

            m_ExperimentalProgramView.SetDisplay(false);
            m_SignUp.SetDisplay(false);
            if (ExperimentalProgram.IsConfigured)
            {
                Add(m_ExperimentalProgramView);
                Add(m_SignUp);
                if (SignInUtility.IsLikelySignedIn)
                    ExperimentalProgram.IsUserAuthorized(OnUserAuthorized);
            }
        }

        VisualElement View()
        {
            if (!NetworkState.IsAvailable)
                return new NetworkNotificationView();
            if (SignInUtility.IsSignedOut)
                return new SignInNotificationView();

            if (!AccountInfo.Instance.IsEntitled)
            {
                if (AccountInfo.Instance.Organization is {Status: SubscriptionStatus.SubscriptionExpired})
                    return new SubscriptionExpiredNotificationView(false, OnAction);
                if (AccountInfo.Instance.Organization is {Status: SubscriptionStatus.TrialExpired})
                    return new TrialExpiredNotificationView(false, OnAction);
                return new TrialNotificationView(false, OnAction);
            }

            return new SubscriptionView {OnDismiss = OnAction};
        }
        
        void OnUserAuthorized(bool authorized)
        {
            m_SignUp.SetDisplay(!authorized);
            if (authorized)
                ExperimentalProgram.GetBalance(OnExperimentalProgramBalanceReceived);
        }

        void OnExperimentalProgramBalanceReceived(int? balance, string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                var errorView = new NotificationView(new ()
                {
                    titleText = "Experimental Program Error",
                    description = errorMessage,
                });
                Add(errorView);
                return;
            }
            
            m_ExperimentalProgramView.SetDisplay(balance == 0);
        }
    }
}
