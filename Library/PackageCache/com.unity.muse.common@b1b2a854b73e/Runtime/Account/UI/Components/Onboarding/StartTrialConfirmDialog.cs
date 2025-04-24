using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Common.Account
{
    class StartTrialConfirmDialog : AccountDialog
    {
        public Action OnAccept;
        readonly Button m_PrimaryAction;
        readonly Text m_TermsOfServiceText;
        readonly Text m_PrivacyPolicyText;
        readonly Checkbox m_TermsOfServiceCheck;
        readonly Checkbox m_PrivacyPolicyCheck;

        public StartTrialConfirmDialog(OrganizationInfo org)
        {
            AddToClassList("muse-subscription-dialog-start-confirm");
            AddHeaderImage();

            var dialogTitle = new Text {text = TextContent.subConfirmTitle, name = "muse-description-title"};

            var descriptionText = new Text {text = TextContent.subConfirmDescription1, name = "muse-description-secondary", enableRichText = true};
            var descriptionContainer = new VisualElement();
            descriptionContainer.Add(descriptionText);
            descriptionContainer.AddToClassList("muse-description-section");

            dialogDescription.Add(dialogTitle);
            dialogDescription.Add(descriptionContainer);

            m_PrimaryAction = AddPrimaryButton(TextContent.subConfirmStart, () => OnAccept?.Invoke());
            AddCancelButton(TextContent.subConfirmLearnMore, AccountLinks.TrialLearnMore);
            m_PrimaryAction.SetEnabled(false);

            m_TermsOfServiceCheck = new Checkbox {name = "trial-confirm-checkbox"};
            m_PrivacyPolicyCheck = new Checkbox {name = "trial-confirm-checkbox"};

            void CheckboxCallback(ChangeEvent<CheckboxState> _) =>
                m_PrimaryAction.SetEnabled(
                    m_TermsOfServiceCheck.value == CheckboxState.Checked &&
                    m_PrivacyPolicyCheck.value == CheckboxState.Checked);

            m_TermsOfServiceCheck.RegisterValueChangedCallback(CheckboxCallback);
            m_PrivacyPolicyCheck.RegisterValueChangedCallback(CheckboxCallback);

            var termsOfService = new VisualElement {name = "muse-terms-of-service"};
            termsOfService.AddToClassList("muse-dialog-description-group");
            termsOfService.AddToClassList("muse-checkbox-group");
            termsOfService.AddToClassList("muse-terms-of-service-section");
            termsOfService.Add(m_TermsOfServiceCheck);
            m_TermsOfServiceText = new Text {text = TextContent.subConfirmTermsOfService, enableRichText = true};
            m_TermsOfServiceText.RegisterCallback<PointerDownLinkTagEvent>(TermsOfServiceClick);
            m_TermsOfServiceText.RegisterCallback<PointerOverLinkTagEvent>(LinkEnter);
            m_TermsOfServiceText.RegisterCallback<PointerOutLinkTagEvent>(LinkLeave);
            termsOfService.Add(m_TermsOfServiceText);
            dialogDescription.Add(termsOfService);

            var privacyPolicy = new VisualElement {name = "muse-privacy-policy"};
            privacyPolicy.AddToClassList("muse-dialog-description-group");
            privacyPolicy.AddToClassList("muse-checkbox-group");
            privacyPolicy.Add(m_PrivacyPolicyCheck);
            m_PrivacyPolicyText = new Text {text = TextContent.subConfirmPrivacy, enableRichText = true};
            m_PrivacyPolicyText.RegisterCallback<PointerDownLinkTagEvent>(PrivacyPolicyClick);
            m_PrivacyPolicyText.RegisterCallback<PointerOverLinkTagEvent>(LinkEnter);
            m_PrivacyPolicyText.RegisterCallback<PointerOutLinkTagEvent>(LinkLeave);
            privacyPolicy.Add(m_PrivacyPolicyText);
            dialogDescription.Add(privacyPolicy);

            // Customize the copy based on the current context
            if (org is {Status: SubscriptionStatus.FreeTrial})
            {
                dialogTitle.text = TextContent.subConfirmTitleTrial;
                m_PrimaryAction.title = TextContent.subConfirmJoinTrial;
            }
            else if (org is {IsExpired: true} or {IsEntitled: true})
            {
                dialogTitle.text = TextContent.subConfirmTitleSubscribed;
                m_PrimaryAction.title = TextContent.subConfirmStartSubscribed;
            }
        }

        void LinkEnter(PointerOverLinkTagEvent evt)
        {
            m_TermsOfServiceText.AddToClassList("muse-link-hover");
            m_PrivacyPolicyText.AddToClassList("muse-link-hover");
        }

        void LinkLeave(PointerOutLinkTagEvent pointerOutLinkTagEvent)
        {
            m_TermsOfServiceText.RemoveFromClassList("muse-link-hover");
            m_PrivacyPolicyText.RemoveFromClassList("muse-link-hover");
        }

        static void PrivacyPolicyClick(PointerDownLinkTagEvent evt)
        {
            if (evt.linkID == "policy")
                AccountLinks.PrivacyPolicy();
            else if (evt.linkID == "supplemental")
                AccountLinks.PrivacyStatement();
        }

        static void TermsOfServiceClick(PointerDownLinkTagEvent evt)
        {
            if (evt.linkID == "terms")
                AccountLinks.TermsOfService();
            else if (evt.linkID == "legal")
                AccountLinks.LegalInfo();
        }

        public override void SetProcessing()
        {
            m_PrimaryAction.SetEnabled(false);
            m_TermsOfServiceCheck.SetEnabled(false);
            m_PrivacyPolicyCheck.SetEnabled(false);
            m_CloseButton?.SetEnabled(false);
            cancelActionContainer.SetEnabled(false);
        }
    }
}
