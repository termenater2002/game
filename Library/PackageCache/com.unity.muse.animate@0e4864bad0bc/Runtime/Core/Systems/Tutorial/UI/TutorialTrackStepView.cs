using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{
    class TutorialTrackStepView : UITemplateContainer, IUITemplate
    {
        TutorialTrackStepViewModel m_Model;

        const string k_TitleLabelName = "title-label";
        const string k_BodyLabelName = "body-label";
        const string k_FooterLabelName = "footer-label";
        
        const string k_AcceptButtonName = "accept-button";
        const string k_NextNameButton = "next-button";
        const string k_PrevButtonName = "previous-button";
        const string k_DismissButtonName = "dismiss-button";
        const string k_SkipButtonName = "skip-button";

        Heading m_TitleLabel;
        Text m_BodyLabel;

        // TODO: (@james.mccafferty) I have temporarily commented out the footer label because it is not used in the
        // tutorial design. I have a hunch it may return. I will leave it here for uncommenting if the time comes.

        // Label m_FooterLabel;

        bool m_Attached;

        private static DisplayStyle GetStyle(bool shouldShow) => shouldShow ? DisplayStyle.Flex : DisplayStyle.None;

        private class TutorialButton
        {
            public readonly Action UpdateLabel;
            public readonly Action UpdateVisibility;

            public TutorialButton(Button button, TutorialLogic.ActionType actionType, Func<TutorialTrackStepViewModel> getModel, Func<TutorialTrackStepViewModel, bool> getVisibility)
            {
                button.clickable.clickedWithEventInfo += (eventBase) =>
                {
                    button.Blur();
                    getModel()?.RequestAction(actionType);
                };

                UpdateLabel = () =>
                {
                    var title = getModel()?.GetActionButtonName(actionType);

                    if (!string.IsNullOrEmpty(title))
                    {
                        button.title = title;
                    }
                };

                UpdateVisibility = () =>
                {
                    var model = getModel();
                    if (model != null)
                    {
                        button.style.display = GetStyle(getVisibility(model));
                    }
                };
            }
        }

        List<TutorialButton> m_Buttons;

        public TutorialTrackStepView()
            : base("deeppose-track-step")
        {
            m_Attached = true;
            style.display = DisplayStyle.Flex;
        }
        
        void IUITemplate.InitComponents()
        {
            TutorialTrackStepViewModel GetModel() => m_Model;
            Button Get(string buttonName) => this.Q<Button>(buttonName);

            m_Buttons = new List<TutorialButton>
            {
                new (Get(k_AcceptButtonName), TutorialLogic.ActionType.Accept, GetModel, m => m.CanAccept),
                new (Get(k_NextNameButton), TutorialLogic.ActionType.Next, GetModel, m => m.CanNext),
                new (Get(k_PrevButtonName), TutorialLogic.ActionType.Previous, GetModel, m => m.CanPrevious),
                new (Get(k_DismissButtonName), TutorialLogic.ActionType.Dismiss, GetModel, m => m.CanDismiss),
                new (Get(k_SkipButtonName), TutorialLogic.ActionType.Skip, GetModel, m => m.CanSkip)
            };
        }

        
        void IUITemplate.FindComponents()
        {
            m_TitleLabel = this.Q<Heading>(k_TitleLabelName);
            m_BodyLabel = this.Q<Text>(k_BodyLabelName);
            m_BodyLabel.enableRichText = true;
        }
        
        void IUITemplate.RegisterComponents()
        {
            
        }
        
        void IUITemplate.UnregisterComponents()
        {
            
        }

        public void SetModel(TutorialTrackStepViewModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged += OnModelChanged;
            m_Model.OnActionFlagsChanged += OnActionFlagsChanged;
            
            ((IUITemplate)this).Update();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnModelChanged;
            m_Model.OnActionFlagsChanged -= OnActionFlagsChanged;
            
            m_Model = null;
        }

        void IUITemplate.Update()
        {
            if (m_Model == null)
                return;
            
            style.display = GetStyle(m_Model.IsVisible);
            
            if (!m_Attached)
                return;
            
            UpdateLabels();
            UpdateButtons();

            style.backgroundImage = m_Model.BackgroundImage;
        }

        void UpdateButtons() => m_Buttons.ForEach(button => button.UpdateVisibility());

        void UpdateLabels()
        {
            m_TitleLabel.text = m_Model.Title;
            m_BodyLabel.text = m_Model.Body;
            
            // TODO: (@james.mccafferty) I have temporarily commented out the footer label because it is not used in the
            // tutorial design. I have a hunch it may return. I will leave it here for uncommenting if the time comes.

            // if (!string.IsNullOrEmpty(m_Model.Footer))
            // {
            //     m_FooterLabel.text = m_Model.Footer;
            //     m_FooterLabel.style.display = DisplayStyle.Flex;
            // }
            // else
            // {
            //     m_FooterLabel.style.display = DisplayStyle.None;
            //     m_FooterLabel.text = "";
            // }

            m_Buttons.ForEach(button => button.UpdateLabel());
        }

        void OnModelChanged() => ((IUITemplate)this).Update();

        void OnActionFlagsChanged(TutorialTrackStepViewModel.ActionFlags flag, bool value) => ((IUITemplate)this).Update();
    }
}
