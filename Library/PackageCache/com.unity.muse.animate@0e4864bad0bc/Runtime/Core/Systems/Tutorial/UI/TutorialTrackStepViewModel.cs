using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class TutorialTrackStepViewModel
    {
        public delegate void Changed();
        public event Changed OnChanged;

        public delegate void RequestedAction(TutorialLogic.ActionType actionType);
        public event RequestedAction OnRequestedAction;

        public delegate void ActionFlagsChanged(ActionFlags flag, bool value);
        public event ActionFlagsChanged OnActionFlagsChanged;
        
        [Flags]
        public enum ActionFlags
        {
            CanAccept = 1,
            CanNext = 2,
            CanPrevious = 4,
            CanDismiss = 8,
            CanSkip = 16
        }

        ActionFlags m_ActionFlags;
        
        public string[] ActionButtonNames { get; set; } = Enum.GetNames(typeof(TutorialLogic.ActionType));
        
        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnChanged?.Invoke();
            }
        }

        public bool CanAccept
        {
            get => m_ActionFlags.HasFlag(ActionFlags.CanAccept);
            set => SetActionFlag(ActionFlags.CanAccept, value);
        }
        
        public bool CanNext
        {
            get => m_ActionFlags.HasFlag(ActionFlags.CanNext);
            set => SetActionFlag(ActionFlags.CanNext, value);
        }
        
        public bool CanPrevious
        {
            get => m_ActionFlags.HasFlag(ActionFlags.CanPrevious);
            set => SetActionFlag(ActionFlags.CanPrevious, value);
        }
        
        public bool CanDismiss
        {
            get => m_ActionFlags.HasFlag(ActionFlags.CanDismiss);
            set => SetActionFlag(ActionFlags.CanDismiss, value);
        }
        
        public bool CanSkip
        {
            get => m_ActionFlags.HasFlag(ActionFlags.CanSkip);
            set => SetActionFlag(ActionFlags.CanSkip, value);
        }

        public string Title
        {
            get => m_TitleLabel;
            set
            {
                if (value.Equals(m_TitleLabel))
                    return;

                m_TitleLabel = value;
                OnChanged?.Invoke();
            }
        }

        public string Body
        {
            get => m_BodyLabel;
            set
            {
                if (value.Equals(m_BodyLabel))
                    return;

                m_BodyLabel = value;
                OnChanged?.Invoke();
            }
        }
        
        public string Footer
        {
            get => m_FooterLabel;
            set
            {
                if (value.Equals(m_FooterLabel))
                    return;

                m_FooterLabel = value;
                OnChanged?.Invoke();
            }
        }

        Texture2D m_BackgroundImage;
        public Texture2D BackgroundImage
        {
            get => m_BackgroundImage;
            set
            {
                m_BackgroundImage = value;
                OnChanged?.Invoke();
            }
        }

        bool m_IsVisible;
        string m_TitleLabel = "Title Label";
        string m_BodyLabel = "Body Label";
        string m_FooterLabel = "Footer Label";
        
        readonly TutorialLogic m_TutorialLogic;

        public TutorialTrackStepViewModel(TutorialLogic tutorialLogic)
        {
            m_TutorialLogic = tutorialLogic;
        }

        public void RequestAction(TutorialLogic.ActionType actionType)
        {
            OnRequestedAction?.Invoke(actionType);
        }
        
        public void SetActionButtonName(TutorialLogic.ActionType actionType, string name)
        {
            ActionButtonNames[(int)actionType] = name;
            OnChanged?.Invoke();
        }
        
        public string GetActionButtonName(TutorialLogic.ActionType actionType)
        {
            return ActionButtonNames[(int)actionType];
        }
        
        void SetActionFlag(ActionFlags flag, bool value)
        {
            if (value == m_ActionFlags.HasFlag(flag))
                return;

            if (value)
            {
                m_ActionFlags |= flag;
            }
            else
            {
                m_ActionFlags &= ~flag;
            }

            OnActionFlagsChanged?.Invoke(flag, value);
        }
    }
}
