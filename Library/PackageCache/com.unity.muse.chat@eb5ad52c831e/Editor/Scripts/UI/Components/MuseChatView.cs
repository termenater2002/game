using System.Diagnostics;
using Unity.Muse.Chat.Commands;
using Unity.Muse.Chat.UI.Components.ChatElements;
using Unity.Muse.Chat.UI.Components.History;
using Unity.Muse.Chat.UI.Components.Inspiration;
using Unity.Muse.Chat.UI.Components.ServerCompatibility;
using Unity.Muse.Chat.UI.Utils;
using Unity.Muse.Common;
using Unity.Muse.Common.Account;
using Unity.Muse.Common.Utils;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    partial class MuseChatView : ManagedTemplate
    {
        static readonly char[] k_MessageTrimChars = { ' ', '\n', '\r', '\t' };

        VisualElement m_RootMain;
        VisualElement m_RootPanel;
        VisualElement m_NotificationContainer;

        Button m_NewChatButton;
        Button m_HistoryButton;

        Label m_ConversationName;

        MuseConversationPanel m_ConversationPanel;

        MuseChatNotificationBanner m_Banner;
        VisualElement m_BannerRoot;

        VisualElement m_HistoryPanelRoot;
        VisualElement m_MusingElementRoot;
        MusingElement m_MusingElement;
        ChatElementUser m_LastUserElement;

        VisualElement m_InspirationRoot;
        MuseChatInspirationPanel m_InspirationPanel;

        HistoryPanel m_HistoryPanel;

        VisualElement m_HeaderRoot;
        VisualElement m_FooterRoot;

        VisualElement m_ChatInputRoot;
        MuseTextField m_ChatInput;
        VisualElement m_CommandActionGroup;

        VisualElement m_PopupRoot;
        SelectionPopup m_SelectionPopup;
        PopupTracker m_SelectionPopupTracker;

        Button m_ClearContextButton;

        ScrollView m_SelectedContextScrollView;
        VisualElement m_SelectedContextScrollViewContent;

        VisualElement m_ExceedingSelectedConsoleMessageLimitRoot;
        int m_SelectedConsoleMessageNum;
        string m_SelectedConsoleMessageContent;
        string m_SelectedGameObjectName;

        bool m_MusingInProgress;
        IMuseChatHost m_Host;

        /// <summary>
        /// Constructor for the MuseChatView.
        /// </summary>
        public MuseChatView()
            : this(null)
        {
        }

        public MuseChatView(IMuseChatHost host)
            : base(MuseChatConstants.UIModulePath)
        {
            m_Host = host;

            RegisterAttachEvents(OnAttachToPanel, OnDetachFromPanel);
        }

        public void InitializeThemeAndStyle()
        {
            LoadStyle(m_RootPanel, EditorGUIUtility.isProSkin ? MuseChatConstants.MuseChatSharedStyleDark : MuseChatConstants.MuseChatSharedStyleLight);
            LoadStyle(m_RootPanel, MuseChatConstants.MuseChatBaseStyle, true);
        }

        /// <summary>
        /// Initialize the view and its component, called by the managed template
        /// </summary>
        /// <param name="view">the template container of the current element</param>
        protected override void InitializeView(TemplateContainer view)
        {
            this.style.flexGrow = 1;
            view.style.flexGrow = 1;

            m_HeaderRoot = view.Q<VisualElement>("headerRoot");
            m_HeaderRoot.AddSessionAndCompatibilityStatusManipulators();

            m_RootMain = view.Q<VisualElement>("root-main");
            m_RootMain.RegisterCallback<MouseEnterEvent>(UpdateSelectedContextWarning);
            m_NotificationContainer = view.Q<VisualElement>("notificationContainer");

            m_RootPanel = view.Q<VisualElement>("root-panel");

            m_NewChatButton = view.SetupButton("newChatButton", OnNewChatClicked);
            m_NewChatButton.AddSessionAndCompatibilityStatusManipulators();
            m_HistoryButton = view.SetupButton("historyButton", OnHistoryClicked);
            m_HistoryButton.AddSessionAndCompatibilityStatusManipulators();

            m_ConversationName = view.Q<Label>("conversationNameLabel");
            m_ConversationName.enableRichText = false;

            var panelRoot = view.Q<VisualElement>("chatPanelRoot");
            m_ConversationPanel = new MuseConversationPanel();
            m_ConversationPanel.Initialize();
            m_ConversationPanel.RegisterCallback<MouseUpEvent>(OnConversationPanelClicked);
            panelRoot.Add(m_ConversationPanel);

            m_HistoryPanelRoot = view.Q<VisualElement>("historyPanelRoot");
            m_HistoryPanel = new HistoryPanel();
            m_HistoryPanel.Initialize();
            m_HistoryPanelRoot.Add(m_HistoryPanel);
            m_HistoryPanelRoot.style.display = UserSessionState.instance.IsHistoryOpen ? DisplayStyle.Flex : DisplayStyle.None;

            m_MusingElementRoot = view.Q<VisualElement>("musingElementContainer");
            m_MusingElement = new MusingElement();
            m_MusingElement.Initialize();
            m_MusingElementRoot.Add(m_MusingElement);

            var contentRoot = view.Q<VisualElement>("chatContentRoot");
            contentRoot.AddSessionAndCompatibilityStatusManipulators();

            m_InspirationRoot = view.Q<VisualElement>("inspirationPanelRoot");
            m_InspirationPanel = new MuseChatInspirationPanel();
            m_InspirationPanel.Initialize();
            m_InspirationPanel.InspirationSelected += OnInspirationSelected;
            m_InspirationRoot.Add(m_InspirationPanel);

            m_HeaderRoot = view.Q<VisualElement>("headerRoot");
            m_FooterRoot = view.Q<VisualElement>("footerRoot");
            m_FooterRoot.AddSessionAndCompatibilityStatusManipulators();

            m_SelectedContextScrollView = view.Q<ScrollView>("userSelectedContextListView");

            m_ChatInputRoot = view.Q<VisualElement>("chatTextFieldRoot");

            m_PopupRoot = view.Q<VisualElement>("chatModalPopupRoot");
            InitializeSelectionPopup();

            m_ChatInput = new MuseTextField(null, m_PopupRoot, m_SelectedContextScrollView);
            m_ChatInput.Initialize();
            m_ChatInput.OnSubmit += OnMuseRequestSubmit;
            m_ChatInput.ContextButton.RegisterCallback<PointerUpEvent>(x => ToggleSelectionPopup());
            m_ChatInputRoot.Add(m_ChatInput);

            m_CommandActionGroup = view.Q<VisualElement>("commandGroup");

            view.SetupButton("commandAsk", x => OnToggleMode(AskCommand.k_CommandName, 0));
            view.SetupButton("commandRun", x => OnToggleMode(RunCommand.k_CommandName, 1));
            view.SetupButton("commandCode", x => OnToggleMode(CodeCommand.k_CommandName, 2));

            // Hide commands until features are ready
            m_CommandActionGroup.style.display = DisplayStyle.None;

            m_ClearContextButton = view.SetupButton("clearContextButton", ClearContext);

            var sessionNotifications = new SessionStatusNotifications { pickingMode = PickingMode.Ignore };
            var serverCompatibilityNotifications = new ServerCompatibilityNotifications { pickingMode = PickingMode.Ignore };

            var notificationBanner = view.Q<VisualElement>("account-notifications");
            notificationBanner.Add(serverCompatibilityNotifications);
            notificationBanner.Add(sessionNotifications);

            m_BannerRoot = view.Q<VisualElement>("notificationBannerRoot");
            m_Banner = new MuseChatNotificationBanner();
            m_Banner.Initialize(false);
            m_BannerRoot.Add(m_Banner);

            m_SelectedContextScrollViewContent = m_SelectedContextScrollView.Q<VisualElement>("unity-content-container");
            m_SelectedContextScrollViewContent.style.flexDirection = FlexDirection.Row;
            m_SelectedContextScrollViewContent.style.flexWrap = Wrap.Wrap;

            m_ExceedingSelectedConsoleMessageLimitRoot = view.Q<VisualElement>("userSelectedContextWarningRoot");

            UpdateMuseEditorDriverContext();
            UpdateSelectedContextWarning();

            EditorApplication.hierarchyChanged += OnHierarchChanged;

            ClearChat();
            SearchService.Refresh();
            m_DropZoneRoot = view.Q<VisualElement>("chatDropZone");
            m_DropZone = new ChatDropZone();
            m_DropZone.Initialize();
            m_DropZoneRoot.Add(m_DropZone);
            m_DropZone.SetupDragDrop(m_DropZoneRoot, OnDropped);
            m_DropZone.SetupDragDrop(m_RootMain, OnDropped);

            m_DropZone.SetDropZoneActive(false);

            view.RegisterCallback<GeometryChangedEvent>(OnViewGeometryChanged);

            Assistant.instance.OnDataChanged += OnDataChanged;
            Assistant.instance.OnConnectionChanged += OnConnectionChanged;

            Assistant.instance.OnConversationHistoryChanged += OnConversationHistoryChanged;
            Assistant.instance.OnConversationTitleChanged += OnConversationTitleChanged;
            SetMusingActive(false);

            ClientStatus.Instance.OnClientStatusChanged += CheckClientStatus;
            CheckClientStatus(ClientStatus.Instance.Status);

            s_ShowNotificationEvent += OnShowNotification;

            UpdateContextSelectionElements();

            Assistant.instance.ViewInitialized();

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

#if UNITY_2022_3_20 || UNITY_2022_3_21 || UNITY_2022_3_22 || UNITY_2022_3_23 || UNITY_2022_3_24 || UNITY_2022_3_25
            bool showWarning = true;
            string versionsToUse = "2022.3.19f1 or lower, or 2022.3.26f1";
#elif UNITY_2023_2_16 || UNITY_2023_2_17 || UNITY_2023_2_18 || UNITY_2023_2_19
            bool showWarning = true;
            string versionsToUse = "2023.2.15f1 or lower, or 2023.2.20f1";
#else
            bool showWarning = false;
            string versionsToUse = "";
#endif

            if (showWarning)
            {
                const string warningShownKey = "MUSE_CHAT_WARNING_SHOWN";
                if (!SessionState.GetBool(warningShownKey, false))
                {
                    m_Banner.Show("This Unity version has performance issues with Muse Chat",
                        $"For the best experience, use version {versionsToUse} or later.", dismissCallback:
                        () =>
                        {
                            SessionState.SetBool(warningShownKey, true);
                        });
                }
            }
        }

        void OnToggleMode(string commandName, int index)
        {
            UserSessionState.instance.SelectedCommandMode = commandName;
            m_ChatInput.Enable();
        }

        void OnConversationPanelClicked(MouseUpEvent evt)
        {
            SetHistoryDisplay(false);
        }

        void OnSuggestionRootClicked(MouseUpEvent evt)
        {
            SetHistoryDisplay(false);
        }

        public void Deinit()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;

            Assistant.instance.OnDataChanged -= OnDataChanged;
            Assistant.instance.OnConversationTitleChanged -= OnConversationTitleChanged;
            Assistant.instance.OnConnectionChanged -= OnConnectionChanged;
            Assistant.instance.OnConversationHistoryChanged -= OnConversationHistoryChanged;
            Assistant.instance.ClearForNewConversation();

            ClientStatus.Instance.OnClientStatusChanged -= CheckClientStatus;

            s_ShowNotificationEvent -= OnShowNotification;

            UserSessionState.instance.Clear();
        }

        public void ChangeConversation(MuseConversation conversation)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                ClearChat();
                m_ConversationName.text = conversation.Title;

                m_ConversationPanel.Populate(conversation);
            } finally{
                sw.Stop();

                InternalLog.Log($"PopulateConversation took {sw.ElapsedMilliseconds}ms ({conversation.Messages.Count} Messages)");
            }
        }

        public void OnConversationTitleChanged(string title)
        {
            m_ConversationName.text = title;
        }

        public void ClearChat()
        {
            m_ConversationName.text = "New chat";
            m_ChatInput.ClearText();
            m_ConversationPanel.ClearConversation();
            HideMusingElement();
            SetSuggestionVisible(true);
        }

        void ShowMusingElement()
        {
            m_MusingElementRoot.SetDisplay(true);
            m_MusingElement.Start();
        }

        void HideMusingElement()
        {
            m_MusingElementRoot.SetDisplay(false);
            m_MusingElement.Stop();
        }

        void OnHistoryClicked(PointerUpEvent evt)
        {
            _ = Assistant.instance.RefreshConversationsAsync();

            bool status = !(m_HistoryPanelRoot.style.display == DisplayStyle.Flex);
            SetHistoryDisplay(status);
        }

        void SetHistoryDisplay(bool isVisible)
        {
            m_HistoryPanelRoot.style.display = isVisible
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            UserSessionState.instance.IsHistoryOpen = isVisible;
        }

        void OnNewChatClicked(PointerUpEvent evt)
        {
            Assistant.instance.ClearForNewConversation();
            ClearChat();
            SetMusingActive(false);

            ShowNotification("New Chat created", PopNotificationIconType.Info);
        }

        void OnHierarchChanged()
        {
            UpdateContextSelectionElements();
        }

        void OnAssetDeletes(string[] paths)
        {
            CheckContextForDeletedAssets(paths);
        }

        void OnInspirationSelected(MuseChatInspiration inspiration)
        {
            m_ChatInput.SetText(inspiration.Value);
        }

        void OnDataChanged(MuseChatUpdateData data)
        {
            switch (data.Type)
            {
                case MuseChatUpdateType.ConversationChange:
                {
                    // Clear the dynamic content and regen based on the conversation
                    ChangeConversation(Assistant.instance.GetActiveConversation());
                    break;
                }

                case MuseChatUpdateType.ConversationClear:
                {
                    ClearChat();
                    break;
                }

                default:
                {
                    m_ConversationPanel.UpdateData(data);
                    break;
                }
            }

            SetSuggestionVisible(false);
            SetMusingActive(data.IsMusing);
        }

        void SetMusingActive(bool state)
        {
            m_ChatInput.SetMusingState(state);

            m_MusingInProgress = state;
            if (state && Assistant.instance.CurrentPromptState != Assistant.PromptState.Streaming)
            {
                ShowMusingElement();
            }
            else
            {
                HideMusingElement();
                m_ChatInput.ResetRoute();
            }
        }

        void OnConnectionChanged(string message, bool connected)
        {
            if (!connected)
            {
                m_ChatInput.Disable(message);
            }
            else
            {
                m_ChatInput.Enable();
            }
        }

        void OnMuseRequestSubmit(string message)
        {
            message = message.Trim(k_MessageTrimChars);
            // If musing is in progress and the submit button is pressed, stop the current request:
            if (m_MusingInProgress || string.IsNullOrEmpty(message))
            {
                if (m_MusingInProgress)
                {
                    Assistant.instance.AbortPrompt();
                    SetMusingActive(false);
                    Assistant.instance.ConversationReload();
                }

                m_ChatInput.ClearText();
                return;
            }

            // Disable edit mode if the user is editing a message in the chat element while submitting a prompt in the chat text field:
            if (m_LastUserElement != null)
            {
                m_LastUserElement.EditEnabled = false;
            }

            m_ChatInput.ClearText();
            _ = Assistant.instance.ProcessPrompt(message);
            SetMusingActive(true);
        }

        void ToggleEnabled(bool enabled)
        {
            m_HeaderRoot.SetEnabled(enabled);
            m_InspirationPanel.SetEnabled(enabled);
            m_FooterRoot.SetDisplay(enabled);
        }

        void CheckClientStatus(ClientStatusResponse clientStatus)
        {
            if (clientStatus.IsDeprecated)
            {
                ToggleEnabled(false);
                m_Banner.Show("Update required", TextContent.clientStatusDeprecatedMessage, ClientStatus.Instance.OpenInPackageManager, "Update Packages", false);
                return;
            }

            ToggleEnabled(true);

            if (clientStatus.WillBeDeprecated)
            {
                m_Banner.Show("Update required soon", TextContent.ClientStatusWillBeDeprecatedMessage(ClientStatus.Instance.Status.ObsoleteDate), ClientStatus.Instance.OpenInPackageManager, "Update Packages");
            }
        }

        void SetSuggestionVisible(bool value)
        {
            m_InspirationRoot.style.display = value ? DisplayStyle.Flex:DisplayStyle.None;
        }

        void OnViewGeometryChanged(GeometryChangedEvent evt)
        {
            bool isCompactView = evt.newRect.width < MuseChatConstants.CompactWindowThreshold;

            m_HistoryButton.EnableInClassList(MuseChatConstants.CompactStyle, isCompactView);
            m_NewChatButton.EnableInClassList(MuseChatConstants.CompactStyle, isCompactView);

            m_ConversationName.EnableInClassList(MuseChatConstants.CompactStyle, isCompactView);

            m_FooterRoot.EnableInClassList(MuseChatConstants.CompactStyle, isCompactView);
        }

        void OnConversationHistoryChanged()
        {
            MuseChatHistoryBlackboard.HistoryPanelReloadRequired?.Invoke();
        }

        void OnBeforeAssemblyReload()
        {
            var activeConversation = Assistant.instance.GetActiveConversation();
            UserSessionState.instance.LastActiveConversationId = activeConversation == null
                ? null
                : activeConversation.Id.Value;
        }

        void ToggleSelectionPopup()
        {
            if (m_SelectionPopup.IsShown)
            {
                HideSelectionPopup();
            }
            else
            {
                ShowSelectionPopup();
            }
        }

        void ShowSelectionPopup()
        {
            // Restore previous context selection
            m_SelectionPopup.SetSelectionFromContext(k_SelectedContext);

            m_ChatInput.ContextButton.EnableInClassList("mui-selected-context-button-open", true);
            m_ChatInput.ContextButton.EnableInClassList("mui-selected-context-button-default-behavior", false);

            m_SelectionPopup.Show();

            m_SelectionPopupTracker = new PopupTracker(m_SelectionPopup, m_ChatInput.ContextButton, MuseChatConstants.RoutesPopupOffset, m_SelectedContextScrollView);
            m_SelectionPopupTracker.Dismiss += HideSelectionPopup;
        }

        void HideSelectionPopup()
        {
            if (m_SelectionPopupTracker == null)
            {
                // Popup is not active
                return;
            }

            m_SelectionPopupTracker.Dismiss -= HideSelectionPopup;
            m_SelectionPopupTracker.Dispose();
            m_SelectionPopupTracker = null;

            m_SelectionPopup.Hide();

            m_ChatInput.ContextButton.EnableInClassList("mui-selected-context-button-open", false);
            m_ChatInput.ContextButton.EnableInClassList("mui-selected-context-button-default-behavior", true);
        }

        void InitializeSelectionPopup()
        {
            m_SelectionPopup = new SelectionPopup();
            m_SelectionPopup.Initialize();
            m_SelectionPopup.Hide();
            m_SelectionPopup.OnSelectionChanged += () =>
            {
                // Memorize current context selection
                SyncContextSelection(m_SelectionPopup.ObjectSelection, m_SelectionPopup.ConsoleSelection);

                UpdateContextSelectionElements();
            };

            m_PopupRoot.Add(m_SelectionPopup);

            if (m_Host != null)
            {
                m_Host.FocusLost += HideSelectionPopup;
            }
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            MuseChatAssetsModificationProcessors.AssetDeletes -= OnAssetDeletes;
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            MuseChatAssetsModificationProcessors.AssetDeletes += OnAssetDeletes;
        }
    }
}
