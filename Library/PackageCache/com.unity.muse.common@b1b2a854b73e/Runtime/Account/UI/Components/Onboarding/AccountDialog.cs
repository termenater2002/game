using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Common.Account
{
    class AccountDialog : VisualElement
    {
        public ScrollView scrollView = new() {name = "muse-dialog-content-scrollview"};
        public VisualElement content = new() {name = "muse-dialog-content"};
        public VisualElement dialogDescription;
        public VisualElement primaryActionContainer = new();
        public VisualElement cancelActionContainer = new();
        public VisualElement actionContainer = new();
        public Action OnClose;
        public VisualElement headerContainer = new() {name = "muse-dialog-header-container"};

        protected Button m_CloseButton;
        protected Button m_PrimaryButton;

        public AccountDialog()
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.accountStyleSheet));

            AddToClassList("muse-subscription-dialog");
            AddToClassList("muse-subscription-message");

            var descriptionContainer = new VisualElement {name = "muse-dialog-description-container"};
            var logoContainer = new VisualElement  {name = "muse-dialog-logo-container"};
            var museLogo = new Image();
            museLogo.scaleMode = ScaleMode.StretchToFill;
            museLogo.image = ResourceManager.Load<Texture2D>(PackageResources.museLogo);
            museLogo.AddToClassList("muse-dialog-logo");
            logoContainer.Add(museLogo);
            descriptionContainer.Add(logoContainer);

            dialogDescription = new();
            dialogDescription.AddToClassList("muse-dialog-description-group");
            descriptionContainer.Add(dialogDescription);

            actionContainer.AddToClassList("muse-dialog-action-container");
            primaryActionContainer.AddToClassList("muse-dialog-secondary-action-container");
            cancelActionContainer.AddToClassList("muse-dialog-secondary-action-container");
            actionContainer.Add(primaryActionContainer);
            actionContainer.Add(cancelActionContainer);

            content.Add(headerContainer);
            content.Add(descriptionContainer);
            content.Add(actionContainer);
            scrollView.Add(content);
            Add(scrollView);
        }

        internal static void SetQuiet(Button button)
        {
            button.quiet = true;
            button.pickingMode = PickingMode.Position;
            button.AddToClassList("appui-dialog__cancel-action");
        }

        public Button AddPrimaryButton(string text, Action clicked, bool isPrimary = true)
        {
            m_PrimaryButton = new Button { title = text };
            if (isPrimary)
                m_PrimaryButton.AddToClassList("appui-dialog__primary-action");
            else
                SetQuiet(m_PrimaryButton);
            m_PrimaryButton.clicked += clicked;
            primaryActionContainer.Add(m_PrimaryButton);

            return m_PrimaryButton;
        }

        public Button CreateQuietButton(string text, Action clicked)
        {
            var button = new Button { title = text };
            SetQuiet(button);
            button.clicked += clicked;
            return button;
        }

        public Button AddCancelButton(string text, Action clicked)
        {
            var button = CreateQuietButton(text, clicked);
            cancelActionContainer.Add(button);

            return button;
        }

        public Button AddCloseButton()
        {
            m_CloseButton = new Button {title = TextContent.subConfirmClose};
            m_CloseButton.quiet = true;
            m_CloseButton.AddToClassList("appui-dialog__cancel-action");
            m_CloseButton.clicked += () => OnClose?.Invoke();
            m_CloseButton.style.marginLeft = 0;
            primaryActionContainer.Add(m_CloseButton);

            return m_CloseButton;
        }

        /// <summary>
        /// Set to disabled while processing
        /// </summary>
        public virtual void SetProcessing()
        {
            m_PrimaryButton.SetEnabled(false);
        }

        protected void AddHeaderImage()
        {
            var heroImage = new Image {image = ResourceManager.Load<Texture2D>(PackageResources.onboardingHeroImage)};
            heroImage.AddToClassList("hero-image");
            heroImage.AddManipulator(new ScaleToFitImageManipulator());

            headerContainer.Add(heroImage);
            headerContainer.AddToClassList("muse-description-section");
        }

        public Modal CreateModal(VisualElement target)
        {
            var modal = Modal.Build(target, this);
            modal.view.AddToClassList("muse-subscription-modal");
            modal.SetKeyboardDismiss(false);
            modal.view.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.accountStyleSheet));

            // Changing passMask and using `--background-color` on the modal's ExVisualElement is necessary to have
            // a larger border radius on the dialog then the default. Otherwise ghosting on the dialog's edges occurs.
            if (modal.view.contentContainer is ExVisualElement exVisualElement)
                exVisualElement.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.BackgroundColor | ExVisualElement.Passes.OutsetShadows;

            return modal;
        }
    }
}
