using System;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    /// <summary>
    /// Sets the target's enabled state based on account, client and connectivity status.
    /// </summary>
    class SessionStatusTracker : Manipulator
    {
        readonly Model m_Model;
        
        public SessionStatusTracker(Model model = null)
        {
            m_Model = model;
        }
        
        protected override void RegisterCallbacksOnTarget()
        {
            SessionStatus.OnUsabilityChanged += OnSessionUsabilityChanged;
            if (m_Model && ExperimentalProgram.IsEnabledForMode(m_Model.CurrentMode))
                ExperimentalProgram.Changed += OnExperimentalProgramChanged;
            Refresh();
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            SessionStatus.OnUsabilityChanged -= OnSessionUsabilityChanged;
            ExperimentalProgram.Changed -= OnExperimentalProgramChanged;
        }

        void OnExperimentalProgramChanged() => Refresh();

        void OnSessionUsabilityChanged(bool _) => Refresh();

        void Refresh()
        {
            var usable = m_Model ? SessionStatus.GetSessionUsabilityForMode(m_Model.CurrentMode) : SessionStatus.IsSessionUsable;
            if (m_Model && ExperimentalProgram.IsEnabledForMode(m_Model.CurrentMode)) 
            {
                ExperimentalProgram.IsUserAuthorized(authorized =>
                {
                    usable &= authorized;
                    if (authorized && ExperimentalProgram.ShouldCheckBalanceForMode(m_Model.CurrentMode))
                    {
                        ExperimentalProgram.GetBalance((balance, errorMessage) =>
                        {
                            var validBalance = string.IsNullOrEmpty(errorMessage) && balance is null or > 0;
                            usable &= validBalance;
                            target?.SetEnabled(usable);
                        });
                    }
                    else
                    {
                        target?.SetEnabled(usable);
                    }
                });
            }
            else
            {
                target?.SetEnabled(usable);
            }
        }
    }
}
