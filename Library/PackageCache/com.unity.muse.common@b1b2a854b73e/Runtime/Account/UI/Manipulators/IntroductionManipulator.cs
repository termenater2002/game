#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    /// <summary>
    /// Control flow for user introduction to Muse.
    /// </summary>
    class IntroductionManipulator : Manipulator
    {
        internal static IntroductionManipulator current;

        public Action OnStateChanged;
        public AccountState State => m_State;

        AccountState m_State = AccountState.Default;
        StateTransition m_OnStateTransition;
        TrialForm m_TrialForm;
        AccountDialog m_Dialog;
        EditorWindow m_Window;

        public IntroductionManipulator(EditorWindow windowToClose = null)
        {
            m_Window = windowToClose;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            current = this;

            AccountInfo.Instance.OnOrganizationChanged += StateChanged;
            AccountInfo.Instance.OnLegalConsentChanged += StateChanged;
            SignInUtility.OnChanged += StateChanged;

            target.RegisterCallback<AttachToPanelEvent>(AttachToPanel);

            StateChanged();
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            current = null;

            AccountInfo.Instance.OnOrganizationChanged -= StateChanged;
            AccountInfo.Instance.OnLegalConsentChanged -= StateChanged;
            SignInUtility.OnChanged -= StateChanged;
        }

        void AttachToPanel(AttachToPanelEvent evt) => Apply();

        void StateChanged()
        {
            var updateToState = ResolveCurrentState();                                          // Apply internal state change logic
            m_State = m_OnStateTransition?.Invoke(updateToState, m_State) ?? updateToState;       // Apply External client's state change logic
            Apply();                                                                            // Apply new state
            OnStateChanged?.Invoke();
        }

        // Always allow all usage. Used mainly for testing purposes.
        public static bool ForceAllowUsage { get; set; }

        // Always show sign-in dialog if requested
        static bool IsSignedOut => SignInUtility.state == SignInState.SignedOut;

        // If the user does not have seats but his organization is determined to have available ones.
        static bool ShouldRequestSeats => AccountInfo.Instance.RequestSeat;

        // If the user is currently filling up dialogs
        static bool IsFillingForm(TrialForm trialForm) => trialForm is not null;

        // If user has ever used Muse (entitled or expired subscription)
        static bool HasMuseAccount => AccountInfo.Instance.HasMuseAccount;

        // Keep in current state if processing trial form dialogs (eg: clicked start trial)
        // Don't change state until we have entitlements+legal information
        // Otherwise we run the risk of displaying partial information
        //  eg: Entitlement is set, so dialog disappear but legal consent is not yet received so consent dialog is shown again briefly
        static bool IsProcessingTrialForm(TrialForm trialForm) => trialForm?.processing ?? false;

        AccountState TrialFormState
        {
            get => m_TrialForm.state;
            set
            {
                m_TrialForm.state = value;
                StateChanged();
            }
        }

        AccountState ResolveCurrentState() => ResolveCurrentState(m_State, m_TrialForm);

        // Resolve current state based on current user/account information
        internal static AccountState ResolveCurrentState(AccountState currentState, TrialForm trialForm)
        {
            if (ForceAllowUsage)
                return AccountState.Default;
            if (IsSignedOut)
                return AccountState.SignIn;
            if (IsProcessingTrialForm(trialForm))
                return currentState;

            if (HasMuseAccount)
            {
                if (AccountInfo.Instance.LegalConsent.HasConsented)
                    return AccountState.Default;

                if (trialForm?.state == AccountState.DataOptIn)
                    return AccountState.DataOptIn;

                return AccountState.TrialConfirm;
            }

            if (ShouldRequestSeats)
                return AccountState.RequestSeat;
            if (IsFillingForm(trialForm))
                return trialForm.state;

            return AccountState.Trial;

        }

        /// <summary>
        /// Apply current state
        /// </summary>
        void Apply()
        {
            if (State == AccountState.Default)
            {
                // If started trial form and changed organization to a valid one, ensure trialForm is reset
                // Otherwise changing back to an organization that's not entitled would pursue the old form.
                m_TrialForm = null;
                TryDismissCurrentModal();

                m_Window?.Close();              // Close window if any.

                if (!GlobalPreferences.exploreShown)
                {
                    GlobalPreferences.exploreShown = true;
                    DisplayExplore();
                }
            }
            else if (State == AccountState.Trial)
            {
                m_TrialForm = new() {startTrial = !AccountInfo.Instance.IsEntitled};
                DisplayStartTrial();
            }
            else if (State == AccountState.TrialConfirm)
            {
                // startTrial will be false since in this case we only need legal consent and opt-in
                m_TrialForm ??= new()
                {
                    organization = AccountInfo.Instance.Organization,
                    state = AccountState.TrialConfirm
                };
                DisplayStartTrialConfirm();
            }
            else if (State == AccountState.DataOptIn)
                DisplayDataOptIn();
            else if (State == AccountState.TrialStarted)
                DisplayTrialStarted();
            else if (State == AccountState.SignIn)
                DisplaySignIn();
            else if (State == AccountState.RequestSeat)
                DisplayRequestSeat();
        }

        public void TryDismissCurrentModal()
        {
            if (target != null)
            {
                target.Clear();
                m_Dialog = null;
            }
        }

        internal void DisplayStartTrial()
        {
            ShowModal(new StartTrialDialog
            {
                OnAccept = org =>
                {
                    m_TrialForm.organization = org;
                    m_TrialForm.state = AccountState.TrialConfirm;

                    // Apply the organization change
                    if (m_TrialForm.organization is not null)
                        AccountInfo.Instance.Organization = m_TrialForm.organization;

                    StateChanged();
                },
                OnClose = StateChanged
            });
        }

        internal void DisplayStartTrialConfirm() => ShowModal(new StartTrialConfirmDialog(m_TrialForm.organization)
            {
                OnAccept = () =>
                {
                    m_TrialForm.legalConsent.terms_of_service_legal_info = true;
                    m_TrialForm.legalConsent.privacy_policy_gen_ai = true;

                    if (AccountInfo.Instance.LegalConsent.HasConsented)
                        ProcessTrialForm(m_TrialForm);      // Start trial without showing usage opt-in if the user has already consented to the legal terms
                    else
                        TrialFormState = AccountState.DataOptIn;
                },
                OnClose = () => TrialFormState = AccountState.Trial
            });

        internal void DisplayDataOptIn() => ShowModal(new DataOpInDialog {OnAccept = usage =>
            {
                m_TrialForm.legalConsent.content_usage_data_training = usage;
                ProcessTrialForm(m_TrialForm);
            }
        });

        void DisplayTrialStarted() => ShowModal(new SubscriptionStartedDialog
        {
            OnAccept = StateChanged
        });
        internal void DisplaySignIn() => ShowModal(new SignInDialog());
        void DisplayRequestSeat() => ShowModal(new RequestSeatDialog());
        internal void DisplayExplore() => ExploreWindow.ShowExplore();

        void ShowModal(AccountDialog dialog)
        {
            TryDismissCurrentModal();
            m_Dialog = dialog;
            target.Add(m_Dialog);
        }

        void ProcessTrialForm(TrialForm trialForm)
        {
            AsyncUtils.SafeExecute(ProcessTrialFormAsync(trialForm));
        }

        async Task ProcessTrialFormAsync(TrialForm trialForm)
        {
            m_Dialog.SetProcessing();       // Block dialog buttons
            await trialForm.Apply();
            m_TrialForm = null;

            // Normally there should have been a refresh at the correct event, but this acts
            // as a failsafe to ensure the state is always refreshed once all information is known.
            StateChanged();
        }
    }
}
#endif
