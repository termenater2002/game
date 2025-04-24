using System;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Common.Account;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelListUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleTrainerMainUIEvents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.StyleTrainer
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class StyleTrainerMainUI : ExVisualElement, IDisposable
    {
        StyleModelInfoEditor m_StyleModelInfoEditor;
        StyleModelList m_StyleModelList;
        EventBus m_EventBus;
        VisualElement m_TopMenu;
        Text m_StyleNameText;
        Button m_LibraryButton;
        VisualElement m_StyleTrainerMainUIContent;
        TwoPaneSplitView m_SplitView;
        VisualElement m_SplitViewAnchor;
        Button m_LoginButton;
        VisualElement m_NoAssetContainer;
        VisualElement m_LoginScreen;
        VisualElement m_LoadingScreen;
        Text m_LoadingScreenText;
        public Action OnStyleTrainerUIEnabled = () => { };

        public StyleTrainerMainUI()
        {
            name = "StyleTrainerMainUI";
        }

        public void SetEventBus(EventBus eventBus)
        {
            m_EventBus = eventBus;
            m_EventBus.RegisterEvent<SignInEvent>(OnSignInEvent);
            m_EventBus.RegisterEvent<ShowDialogEvent>(OnShowDialogEvent);
            m_EventBus.RegisterEvent<ShowLoadingScreenEvent>(OnShowLoadingScreenEvent);
            m_EventBus.RegisterEvent<StyleModelListSelectionChangedEvent>(OnStyleModelListSelectionChangedEvent);
            m_EventBus.RegisterEvent<StyleModelListCollapsedEvent>(OnStyleModelListCollapsed);
            m_EventBus.RegisterEvent<ChangeStyleNameEvent>(OnStyleNameChanged);
            m_StyleModelInfoEditor.SetEventBus(eventBus);
            m_StyleModelList.SetEventBus(eventBus);
        }

        void OnStyleNameChanged(ChangeStyleNameEvent arg0)
        {
            m_StyleNameText.text = arg0.newStyleName;
        }

        void OnStyleModelListCollapsed(StyleModelListCollapsedEvent arg0)
        {
            m_StyleModelList.EnableInClassList("styletrainer-stylemodellist__collapsed", arg0.collapsed);
            m_SplitViewAnchor.Q<VisualElement>("unity-dragline").style.display = arg0.collapsed ? DisplayStyle.None : DisplayStyle.Flex;
            m_LibraryButton.EnableInClassList("is-collapsed", arg0.collapsed);
        }

        void OnStyleModelGeometryChanged(GeometryChangedEvent evt)
        {
            // Workaround for the fact that the SplitView doesn't have a way to set the position of the dragline
            // by code. It is usually calculated during a GeometryChangedEvent.
            var styleModelListWidth = m_StyleModelList.resolvedStyle.width;
            var styleModelListMargins = m_StyleModelList.resolvedStyle.marginLeft + m_StyleModelList.resolvedStyle.marginRight;

            m_SplitViewAnchor.style.left = styleModelListWidth + styleModelListMargins;
        }

        void OnShowDialogEvent(ShowDialogEvent dialogData)
        {
            var dialog = new AlertDialog
            {
                title = dialogData.title,
                description = dialogData.description,
                variant = AlertSemantic.Destructive
            };
            dialog.SetPrimaryAction(1, "Ok", dialogData.confirmAction);
            if (dialogData.cancelAction != null)
            {
                dialog.SetSecondaryAction(2, "Cancel", dialogData.cancelAction);
            }
            var modal = Modal.Build(this, dialog);
            modal.Show();
        }

        void OnShowLoadingScreenEvent(ShowLoadingScreenEvent dialogData)
        {
            if (dialogData.show)
            {
                m_LoadingScreen.style.display = DisplayStyle.Flex;
                m_LoadingScreenText.text = dialogData.description;
            }
            else
            {
                m_LoadingScreen.style.display = DisplayStyle.None;
            }
        }

        void OnSignInEvent(SignInEvent evt)
        {
            RefreshLoggedIn(evt);
        }

        internal static StyleTrainerMainUI CreateFromUxml(StyleModelController controller, VisualElement cloneToElement)
        {
            var visualTree = ResourceManager.Load<VisualTreeAsset>(PackageResources.mainUITemplate);
            visualTree.CloneTree(cloneToElement);
            var styleTrainerMainUI = cloneToElement.Q<StyleTrainerMainUI>();
            styleTrainerMainUI.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.mainUIStyleSheet));
            styleTrainerMainUI.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.iconsStyleSheet));
            styleTrainerMainUI.BindElements();
            controller.InitView(styleTrainerMainUI);
            return styleTrainerMainUI;
        }

        void BindElements()
        {
            m_LoadingScreen = this.Q<VisualElement>("LoadingScreen");
            m_LoadingScreenText = m_LoadingScreen.Q<Text>("LoadingText");
            m_LoadingScreenText.text = "Loading...";
            m_LoadingScreen.style.display = DisplayStyle.None;

            m_StyleTrainerMainUIContent = this.Q<VisualElement>("StyleTrainerContent");
            m_TopMenu = this.Q<VisualElement>("StyleTrainerTopMenu");
            m_StyleNameText = m_TopMenu.Q<Text>("style-name");
            m_LibraryButton = m_TopMenu.Q<Button>("library-button");
            m_LibraryButton.clicked += OnLibraryButtonClicked;

            m_StyleNameText.text = "";
            m_SplitView = this.Q<TwoPaneSplitView>("StyleTrainerUISplitView");
            m_SplitView.fixedPaneIndex = 0;
            m_SplitView.fixedPaneInitialDimension = 300;
            m_SplitView.orientation = TwoPaneSplitViewOrientation.Horizontal;

            m_StyleModelInfoEditor = StyleModelInfoEditor.CreateFromUxml();
            m_StyleModelList = StyleModelList.CreateFromUxml();
            m_StyleModelList.RegisterCallback<GeometryChangedEvent>(OnStyleModelGeometryChanged);

            m_SplitView.Add(m_StyleModelList);
            m_SplitView.Add(m_StyleModelInfoEditor);

            m_StyleModelInfoEditor.AddManipulator(new SessionStatusTracker());

            m_SplitViewAnchor = m_SplitView.Q<VisualElement>("unity-dragline-anchor");

            m_LoginScreen = this.Q<VisualElement>("LoginScreen");
            m_LoginButton = m_LoginScreen.Q<Button>("LoginButton");
            m_LoginButton.clicked += LogInClicked;

            m_NoAssetContainer = this.Q<VisualElement>("NoAssetContainer");
            m_NoAssetContainer.style.display = DisplayStyle.None;

            RefreshLoggedIn(new SignInEvent()
            {
                signIn = true
            });
        }

        void OnLibraryButtonClicked()
        {
            m_EventBus.SendEvent(new StyleModelListCollapsedEvent
            {
                collapsed = !m_StyleModelList.ClassListContains("styletrainer-stylemodellist__collapsed")
            });
        }

        void OnStyleModelListSelectionChangedEvent(StyleModelListSelectionChangedEvent evt)
        {
            m_StyleNameText.text = evt.styleData?.title;
        }

        void RefreshLoggedIn(SignInEvent evt)
        {
#if UNITY_EDITOR
            var isLoggedIn = UnityConnectUtils.GetIsLoggedIn() && evt.signIn;
#else
            var isLoggedIn = true;
#endif
            m_LoginScreen.style.display = isLoggedIn ? DisplayStyle.None : DisplayStyle.Flex;
            m_StyleTrainerMainUIContent.style.display = isLoggedIn ? DisplayStyle.Flex : DisplayStyle.None;
            OnStyleTrainerUIEnabled?.Invoke();
        }

        public bool styleTrainerUIEnabled => m_StyleTrainerMainUIContent.style.display == DisplayStyle.Flex;

        void LogInClicked()
        {
#if UNITY_EDITOR
            CloudProjectSettings.ShowLogin();
            RefreshLoggedIn(new SignInEvent()
            {
                signIn = true
            });
#endif
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<StyleTrainerMainUI, UxmlTraits> { }
#endif

        public void Dispose()
        {
            // not used for now
        }
    }
}