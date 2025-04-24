using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Utils;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class ExperimentalProgramNotifications : VisualElement
    {
        readonly ExperimentalProgramLimitReachedNotificationView m_LimitReached;
        
        readonly ExperimentalProgramSignUpNotificationView m_SignUp;
        
        NotificationView m_GetBalanceError;

        Model m_Model;

        public ExperimentalProgramNotifications()
        {
            AddToClassList("notifications-area");
            
            m_SignUp = new ExperimentalProgramSignUpNotificationView(true);
            m_SignUp.SetDisplay(false);
            m_LimitReached = new ExperimentalProgramLimitReachedNotificationView(true);
            m_LimitReached.SetDisplay(false);

            Add(m_SignUp);
            Add(m_LimitReached);
            
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent _)
        {
            SetModel(this.GetContext<Model>());
            this.RegisterContextChangedCallback<Model>(OnModelContextChanged);
        }
        
        void OnDetachFromPanel(DetachFromPanelEvent _)
        {
            this.UnregisterContextChangedCallback<Model>(OnModelContextChanged);
            SetModel(null);
        }

        void OnModelContextChanged(ContextChangedEvent<Model> evt)
        {
            SetModel(evt.context);
        }

        void SetModel(Model model)
        {
            if (model == m_Model)
                return;

            if (m_Model)
                UnsubscribeFromEvents();
            
            m_Model = model;
            
            if (m_Model)
                SubscribeToEvents();
            
            Refresh();
        }
        
        void SubscribeToEvents()
        {
            ExperimentalProgram.Changed += Refresh;
            
            if (!m_Model)
                return;
            
            m_Model.OnModeChanged += OnModeChanged;
        }

        void UnsubscribeFromEvents()
        {
            ExperimentalProgram.Changed -= Refresh;
            
            if (!m_Model)
                return;
            
            m_Model.OnModeChanged -= OnModeChanged;
        }
        
        void OnModeChanged(int _)
        {
            Refresh();
        }

        void Refresh()
        {
            m_LimitReached.SetDisplay(false);
            m_SignUp.SetDisplay(false);
            if (m_Model && ExperimentalProgram.IsConfigured && ExperimentalProgram.IsEnabledForMode(m_Model.CurrentMode))
                ExperimentalProgram.IsUserAuthorized(OnUserAuthorized);
        }

        void OnUserAuthorized(bool authorized)
        {
            m_SignUp.SetDisplay(!authorized && SignInUtility.IsLikelySignedIn);
            if (authorized && m_Model && ExperimentalProgram.ShouldCheckBalanceForMode(m_Model.CurrentMode))
                ExperimentalProgram.GetBalance(OnBalanceReceived);
        }

        void OnBalanceReceived(int? balance, string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                m_GetBalanceError?.RemoveFromHierarchy();
                m_GetBalanceError = new NotificationView(new ()
                {
                    titleText = "Experimental Program Error",
                    description = errorMessage,
                });
                Add(m_GetBalanceError);
                return;
            }
            
            m_LimitReached.SetDisplay(balance == 0);
        }
    }
}
