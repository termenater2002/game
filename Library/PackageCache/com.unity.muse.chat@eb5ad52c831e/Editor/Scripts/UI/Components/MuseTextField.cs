using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Unity.Muse.Chat.Commands;
using Unity.Muse.Chat.UI.Utils;
using Unity.Muse.Common.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using TextField = UnityEngine.UIElements.TextField;
using Unity.AppUI.Core;

namespace Unity.Muse.Chat.UI.Components
{
    class MuseTextField : ManagedTemplate
    {
        const string k_ChatFocusClass = "mui-mft-input-focused";
        const string k_ChatHoverClass = "mui-mft-input-hovered";
        const string k_ScrollVisibleClass = "mui-mft-scroll-active";
        const string k_ChatSubmitEnabledClass = "mui-submit-enabled";
        const string k_RouteChipVisibleClass = "mui-route-chip-visible";
        const string k_RouteChipHiddenClass = "mui-route-chip-hidden";
        const string k_RouteChipHoveredClass = "mui-route-chip-hovered";

        const string k_RouteChipFocusClass = "mui-route-chip-focused";

        const string k_DefaultAskPlaceholderText = "Ask Muse";
        const string k_DefaultRunPlaceholderText = "Run a command";
        const string k_DefaultCodePlaceholderText = "Use a dedicated code generator";

        VisualElement m_Root;

        VisualElement m_SubmitButton;

        VisualElement m_PopupAnchor;

        ScrollView m_InputScrollView;
        TextField m_ChatInput;
        Label m_ChatCharCount;
        Label m_Placeholder;
        VisualElement m_PlaceholderContent;

        readonly List<string> k_RouteLabels = new();
        bool m_RouteChipVisible;
        VisualElement m_RouteChip;
        TextElement m_RouteChipText;

        Button m_AddContextButton;

        VisualElement m_PopupRoot;
        RoutesPopup m_RoutesPopup;
        PopupTracker m_RoutesPopupTracker;
        Button m_AddRouteButton;
        int m_SelectedRouteItemIndex;
        bool m_RouteEnabled;
        string m_InputAfterSlash;
        int m_CommandSplitIndex;


        bool m_TextHasFocus;
        bool m_ShowPlaceholder;
        bool m_HighlightFocus;
        bool m_IsMusing;

        IMuseChatHost m_Host;

        public MuseTextField()
            : this(null)
        {
        }

        public MuseTextField(IMuseChatHost host)
            : base(MuseChatConstants.UIModulePath)
        {
            m_Host = host;
        }

        public MuseTextField(IMuseChatHost host, VisualElement popupRoot, VisualElement popupAnchor)
            : base(MuseChatConstants.UIModulePath)
        {
            m_Host = host;
            m_PopupRoot = popupRoot;
            m_PopupAnchor = popupAnchor;
            m_RouteEnabled = true;
        }

        public bool HasFocus => m_TextHasFocus;

        public bool ShowPlaceholder
        {
            get => m_ShowPlaceholder;
            set
            {
                if (m_ShowPlaceholder == value)
                {
                    return;
                }

                m_ShowPlaceholder = value;
                RefreshUI();
            }
        }

        public bool HighlightFocus
        {
            get => m_HighlightFocus;
            set
            {
                if (m_HighlightFocus == value)
                {
                    return;
                }

                m_HighlightFocus = value;
                RefreshUI();
            }
        }

        public event Action<string> OnSubmit;

        public Button ContextButton => m_AddContextButton;

        public void ClearText()
        {
            m_ChatInput.SetValueWithoutNotify(string.Empty);
            OnChatValueChanged();
        }

        public void SetText(string text)
        {
            m_ChatInput.SetValueWithoutNotify(text);
            OnChatValueChanged();
            m_ChatInput.Focus();
            m_ChatInput.SelectRange(m_ChatInput.text.Length, m_ChatInput.text.Length);
        }

        public void SelectAllText()
        {
            m_ChatInput.SelectAll();
        }

        public void SetMusingState(bool isMusing)
        {
            m_IsMusing = isMusing;
            RefreshUI();
        }

        public void SwitchPlaceholderText()
        {
            if (ChatCommands.TryGetCommandHandler(UserSessionState.instance.SelectedCommandMode, out var handler))
            {
                m_Placeholder.text = handler.PlaceHolderText;
            }
        }

        public void Enable()
        {
            SwitchPlaceholderText();
            m_ChatInput.SetEnabled(true);
        }

        public void Disable(string reason)
        {
            m_Placeholder.text = reason;
            m_ChatInput.SetEnabled(false);
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_Root = view.Q<VisualElement>("museTextFieldRoot");

            m_RouteChip = view.Q<VisualElement>("routeChip");
            m_RouteChipText = m_RouteChip.Q<TextElement>();

            m_AddRouteButton = view.SetupButton("addRouteButton", ToggleRoutesPopupShown);
            m_AddContextButton = view.Q<Button>("addContextButton");
            if (m_RouteEnabled)
            {
                InitializeRoutesPopup();
#if ENABLE_ASSISTANT_BETA_FEATURES
                m_AddRouteButton.style.display = DisplayStyle.Flex;
#endif
            }
            else
            {
                m_AddRouteButton.SetDisplay(false);
                m_AddContextButton.SetDisplay(false);
            }

            k_RouteLabels.Clear();
            foreach (var command in ChatCommands.GetCommands())
            {
                ChatCommands.TryGetCommandHandler(command, out var commandHandler);

                if (!commandHandler.ShowInList)
                    continue;
                
                k_RouteLabels.Add(commandHandler.Label);
            }

            m_SubmitButton = view.Q<VisualElement>("submitButton");
            m_SubmitButton.RegisterCallback<PointerUpEvent>(_ =>
            {
                OnSubmit?.Invoke(m_ChatInput.value);
                ResetRoute();
            });

            m_InputScrollView = view.Q<ScrollView>("inputScrollView");

            m_PlaceholderContent = view.Q<VisualElement>("placeholderContent");
            m_Placeholder = view.Q<Label>("placeholderText");

            m_ChatInput = view.Q<TextField>("input");
            m_ChatInput.maxLength = MuseChatConstants.MaxMuseMessageLength;
            m_ChatInput.multiline = true;
            m_ChatInput.selectAllOnFocus = false;
            m_RouteChip.RegisterCallback<ClickEvent>(_ => ToggleRouteChipFocus());
            m_RouteChip.RegisterCallback<MouseEnterEvent>(_ => SetRouteChipHovered(true));
            m_RouteChip.RegisterCallback<MouseLeaveEvent>(_ => SetRouteChipHovered(false));
            m_ChatInput.RegisterCallback<KeyUpEvent>(OnChatKeyUpEvent);
            // TrickleDown.TrickleDown is a workaround for registering KeyDownEvent type with Unity 6
            m_ChatInput.RegisterCallback<KeyDownEvent>(OnChatKeyDownEvent, TrickleDown.TrickleDown);
            m_ChatInput.RegisterValueChangedCallback(_ => OnChatValueChanged());
            m_PlaceholderContent.RegisterCallback<ClickEvent>(_ => m_ChatInput.Focus());
            m_ChatInput.RegisterCallback<FocusInEvent>(_ => SetTextFocused(true));
            m_ChatInput.RegisterCallback<FocusOutEvent>(_ =>
            {
                if (m_RouteChip.ClassListContains(k_RouteChipFocusClass)) ToggleRouteChipFocus();
                SetTextFocused(false);
            });
            m_ChatInput.RegisterCallback<PointerLeaveEvent>(_ => m_SubmitButton.RemoveFromClassList(k_ChatHoverClass));
            m_ChatInput.RegisterCallback<GeometryChangedEvent>(OnInputGeometryChanged);

            m_PlaceholderContent = view.Q<VisualElement>("placeholderContent");
            m_Placeholder = view.Q<Label>("placeholderText");

            m_ChatCharCount = view.Q<Label>("characterCount");

            RefreshChatCharCount();
            ShowPlaceholder = true;
            HighlightFocus = true;
            SetMusingState(false);
        }

        void OnInputGeometryChanged(GeometryChangedEvent evt)
        {
            m_SubmitButton.EnableInClassList(k_ScrollVisibleClass, m_InputScrollView.verticalScroller.style.display != DisplayStyle.None);
        }

        void SetTextFocused(bool state)
        {
            m_TextHasFocus = state;
            RefreshUI();
        }

        void RefreshUI()
        {
            if (!ShowPlaceholder || m_TextHasFocus || !string.IsNullOrEmpty(m_ChatInput.value) || m_RouteChipVisible)
            {
                m_PlaceholderContent.style.display = DisplayStyle.None;
            }
            else
            {
                m_PlaceholderContent.style.display = DisplayStyle.Flex;
            }

            m_Root.EnableInClassList(k_ChatFocusClass, m_TextHasFocus && m_HighlightFocus);
        }

        void OnChatValueChanged()
        {
            RefreshChatCharCount();
            m_SubmitButton.EnableInClassList(k_ChatSubmitEnabledClass, !string.IsNullOrEmpty(m_ChatInput.value));
            // Handle copy/pasted values that contain command
            if (m_ChatInput.value.Split(" ").Length > 1)
            {
                string firstWord = m_ChatInput.value.Split(" ")[0];
                bool inputMatch = k_RouteLabels.Contains(firstWord);
                if (inputMatch)
                {
                    SetCommandFromInput(firstWord);
                    SetRouteChipVisible(true, true);
                    m_ChatInput.value = m_ChatInput.value.Remove(0, firstWord.Length + 1);
                }
            }
        }

        void RefreshChatCharCount()
        {
            m_ChatCharCount.text = $"{m_ChatInput.value.Length.ToString()}/{MuseChatConstants.MaxMuseMessageLength}";
        }

        void OnChatKeyDownEvent(KeyDownEvent evt)
        {
            if (string.IsNullOrEmpty(m_ChatInput.value) &&
                (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter ||
                 evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow))
            {
                evt.StopImmediatePropagation();
            }
            // Shift + enter adds a line break.
            // We get 2 similar events when the user presses shift+enter and want to stop both from propagating but only add one line break!
            if (evt.shiftKey &&
                (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter || evt.character == '\n'))
            {
                if (evt.character == '\n')
                {
                    var isAtEnd = m_ChatInput.cursorIndex == m_ChatInput.value.Length;

                    m_ChatInput.SetValueWithoutNotify(m_ChatInput.value.Insert(m_ChatInput.cursorIndex, "\n"));
                    m_ChatInput.cursorIndex += 1;

                    if (isAtEnd)
                    {
                        m_ChatInput.selectIndex = m_ChatInput.cursorIndex + 1;
                    }
                    else
                    {
                        m_ChatInput.selectIndex = m_ChatInput.cursorIndex;
                    }
                }

                evt.StopPropagation();

#if !UNITY_2023_1_OR_NEWER
                evt.PreventDefault();
#endif
            }
            else if (evt.character == '\n')
            {
                // Don't do default behaviour of adding a new line with return:
                evt.StopPropagation();

#if !UNITY_2023_1_OR_NEWER
                evt.PreventDefault();
#endif
            }
        }

        void ToggleRoutesPopupShown(PointerUpEvent evt)
        {
            if (m_RoutesPopup.IsShown)
            {
                HideRoutesPopup();
            }
            else
            {
                ShowRoutesPopup();
            }
        }

        void ShowRoutesPopup()
        {
            m_RoutesPopup.Show();
            m_ChatInput.Focus();
            m_RoutesPopupTracker = new PopupTracker(m_RoutesPopup, m_AddRouteButton, 20, m_PopupAnchor);
            m_RoutesPopupTracker.Dismiss += HideRoutesPopup;
        }

        void HideRoutesPopup()
        {
            if (m_RoutesPopupTracker == null)
            {
                // Popup is not active
                return;
            }

            m_RoutesPopupTracker.Dismiss -= HideRoutesPopup;
            m_RoutesPopupTracker.Dispose();
            m_RoutesPopupTracker = null;

            m_RoutesPopup.Hide();
        }

        void RouteSelectionChanged()
        {
            SetRouteChipVisible(true, true);
            HideRoutesPopup();
            // Remove first n chars that match command input
            m_ChatInput.value = m_ChatInput.value.Remove(0, GetIndexOfLastCharMatchingCommand(GetRouteLabelFromChosenCommand()));
            RefreshUI();
            m_ChatInput.Focus();
        }

        void InitializeRoutesPopup()
        {
            m_RoutesPopup = new RoutesPopup();
            m_RoutesPopup.Initialize();
            m_RoutesPopup.Hide();

            m_RoutesPopup.Entries.First().SetHovered(true);
            m_RoutesPopup.OnSelectionChanged += RouteSelectionChanged;

            m_PopupRoot.Add(m_RoutesPopup);

            if (m_Host != null)
            {
                m_Host.FocusLost += HideRoutesPopup;
            }
        }

        private int GetIndexOfLastCharMatchingCommand(string commandString)
        {
            string firstWord = m_ChatInput.value.Split(" ").First();
            int firstWordIterator = 0;

            foreach (char c in commandString)
            {
                if (firstWord.Length > firstWordIterator && c == firstWord[firstWordIterator])
                {
                    firstWordIterator++;
                }
                else
                {
                    return firstWordIterator;
                }
            }

            return firstWordIterator;
        }

        private string GetRouteLabelFromChosenCommand()
        {
            if (ChatCommands.TryGetCommandHandler(UserSessionState.instance.SelectedCommandMode, out var handler))
            {
                return handler.Label;
            }
            return "";
        }

        private void SetRouteChipVisible(bool show, bool populateChip = false)
        {
            if (populateChip)
            {
                m_RouteChipText.text = GetRouteLabelFromChosenCommand();
            }
            m_RouteChipVisible = show;
            m_RouteChip.RemoveFromClassList(show ? k_RouteChipHiddenClass: k_RouteChipVisibleClass);
            m_RouteChip.AddToClassList(show ? k_RouteChipVisibleClass: k_RouteChipHiddenClass);
            SetShortcutButtonEnabledState();
        }

        private void FocusRouteChip()
        {
            ShowRoutesPopup();
            m_RouteChip.AddToClassList(k_RouteChipFocusClass);
        }

        private void ToggleRouteChipFocus()
        {
            if (!m_RouteChip.ClassListContains(k_RouteChipFocusClass))
            {
                FocusRouteChip();
                m_ChatInput.Focus();
            }
            else
            {
                m_RouteChip.RemoveFromClassList(k_RouteChipFocusClass);
            }
        }

        void SetRouteChipHovered(bool isHovered)
        {
            if (isHovered && !m_RouteChip.ClassListContains(k_RouteChipFocusClass))
            {
                m_RouteChip.AddToClassList(k_RouteChipHoveredClass);
            }
            else
            {
                m_RouteChip.RemoveFromClassList(k_RouteChipHoveredClass);
            }
        }

        private void SetShortcutButtonEnabledState()
        {
            // Shortcuts button should only be enabled if no content in chat & if no chip
            if (m_ChatInput.value.Length == 0 && !m_RouteChipVisible)
            {
                m_AddRouteButton.SetEnabled(true);
            }
            else
            {
                m_AddRouteButton.SetEnabled(false);
            }
        }


        public void ResetRoute()
        {
            UserSessionState.instance.SelectedCommandMode = AskCommand.k_CommandName;
            SwitchPlaceholderText();
            SetRouteChipVisible(false);
        }

        void AdjustPopupHighlight()
        {
            foreach (var entry in m_RoutesPopup.Entries)
            {
                entry.SetHovered(false);
            }

            m_SelectedRouteItemIndex = 0;
            if (m_RoutesPopup.Entries.Count > 0) {
                m_RoutesPopup.Entries.First().SetHovered(true);
            }
        }

        private void SetCommandFromInput(string firstWord)
        {
            if (ChatCommands.TryGetCommandHandler(firstWord, out var handler))
            {
                UserSessionState.instance.SelectedCommandMode = handler.Command;
            }
        }

        private void DetectRouteInput(KeyUpEvent evt)
        {
            // Detect forward slash as first character "/" and show popup even if rest of input does not match a route
                if (
                    m_ChatInput.value.Length > 0
                    && m_ChatInput.value[0] == '/'
                    && !m_RouteChipVisible
                    && evt.keyCode != KeyCode.Backspace
                )
                {
                    ShowRoutesPopup();
                    m_CommandSplitIndex += 1;
                }

                if (m_ChatInput.value.Length > 0 && m_ChatInput.value[0] == '/' && !m_RouteChipVisible &&
                    evt.keyCode == KeyCode.Backspace)
                {
                    m_CommandSplitIndex -= 1;
                }

                // Allow consecutive filter input
                if (m_ChatInput.cursorIndex != m_CommandSplitIndex)
                {
                    m_CommandSplitIndex = 0;
                }

                bool inputMatch = StringUtils.StartsWithAnyLinq(m_ChatInput.value, k_RouteLabels);

                // Show routes popup if any matching prefix but not if chat input is empty
                if (m_ChatInput.value.Length > 0 && inputMatch && !m_RouteChipVisible)
                {
                    ShowRoutesPopup();
                }

                // Hide routes popup if space pressed (new word started)
                if (evt.keyCode == KeyCode.Space && m_RoutesPopup.IsShown)
                {
                    HideRoutesPopup();
                }

                // allow Enter key for selection of route
                if ((evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) && m_RoutesPopup.IsShown &&
                    inputMatch)
                {
                    var selectedEntry = m_RoutesPopup.Entries[m_SelectedRouteItemIndex];
                    UserSessionState.instance.SelectedCommandMode = selectedEntry.Command.Command;
                    RouteSelectionChanged();
                    m_RouteChip.RemoveFromClassList(k_RouteChipFocusClass);
                    return;
                }

                bool arrowKeyInput = evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow;

                // Hide non-relevant items as command is typed - unhide all when command is input
                if (!m_RouteChipVisible && !arrowKeyInput)
                {
                    bool routesExist = m_RoutesPopup.DisplayRoutes(
                        m_CommandSplitIndex == 0
                            ? m_ChatInput.value.Split(" ").First()
                            : m_ChatInput.value.Substring(0, m_CommandSplitIndex)
                    );
                    if (!routesExist)
                    {
                        HideRoutesPopup();
                    }
                    m_RoutesPopupTracker?.RealignPopup();
                    AdjustPopupHighlight();
                }
                else if (!arrowKeyInput)
                {
                    m_RoutesPopup.DisplayRoutes(string.Empty);
                    AdjustPopupHighlight();
                    m_RoutesPopupTracker?.RealignPopup();
                }


                // Allow arrow key navigation of popup
                if (evt.keyCode == KeyCode.UpArrow && m_RoutesPopup.IsShown)
                {
                    m_RoutesPopup.Entries[m_SelectedRouteItemIndex].SetHovered(false);
                    if (m_SelectedRouteItemIndex == 0)
                    {
                        m_SelectedRouteItemIndex = m_RoutesPopup.Entries.Count - 1;
                    }
                    else
                    {
                        m_SelectedRouteItemIndex--;
                    }

                    m_RoutesPopup.Entries[m_SelectedRouteItemIndex].SetHovered(true);
                    evt.StopImmediatePropagation();
                }

                if (evt.keyCode == KeyCode.DownArrow && m_RoutesPopup.IsShown)
                {
                    m_RoutesPopup.Entries[m_SelectedRouteItemIndex].SetHovered(false);
                    if (m_SelectedRouteItemIndex == m_RoutesPopup.Entries.Count - 1)
                    {
                        m_SelectedRouteItemIndex = 0;
                    }
                    else
                    {
                        m_SelectedRouteItemIndex++;
                    }

                    m_RoutesPopup.Entries[m_SelectedRouteItemIndex].SetHovered(true);
                    evt.StopImmediatePropagation();
                }

                // If route chip is focused and backspace is pressed reset to default route
                if (evt.keyCode == KeyCode.Backspace && m_RouteChip.ClassListContains(k_RouteChipFocusClass))
                {
                    ResetRoute();
                }

                // Shortcuts button should only be enabled if no content in chat & if no chip
                SetShortcutButtonEnabledState();

                if (string.IsNullOrEmpty(m_ChatInput.value) && !m_RoutesPopup.IsShown)
                {
                    HideRoutesPopup();
                }

                bool backspaceOnEmptyField = evt.keyCode == KeyCode.Backspace && m_ChatInput.value.Length == 0;
                bool backspaceAtInputStart = evt.keyCode == KeyCode.Backspace && m_ChatInput.cursorIndex == 0;

                if (backspaceOnEmptyField && !m_RouteChipVisible)
                {
                    HideRoutesPopup();
                }
                else if (m_ChatInput.value.Length > 0 && m_RouteChipVisible && !backspaceAtInputStart)
                {
                    HideRoutesPopup();
                    m_RouteChip.RemoveFromClassList(k_RouteChipFocusClass);
                    // detect backspace on empty field with selected chip - hide chat route chip/hide popup
                }
                else if (m_RouteChipVisible && backspaceOnEmptyField &&
                         m_RouteChip.ClassListContains(k_RouteChipFocusClass))
                {
                    ResetRoute();
                    HideRoutesPopup();
                    // detect backspace on empty field with non-selected chip - select route chip/display popup
                }
                else if (m_RouteChipVisible && (backspaceOnEmptyField || backspaceAtInputStart) &&
                         !m_RouteChip.ClassListContains(k_RouteChipFocusClass))
                {
                    FocusRouteChip();
                    ShowRoutesPopup();
                }
                else if (m_ChatInput.value.Length > 0 && m_RouteChip.ClassListContains(k_RouteChipFocusClass))
                {
                    m_RouteChip.RemoveFromClassList(k_RouteChipFocusClass);
                }

                if (k_RouteLabels.Contains(m_ChatInput.value))
                {
                    SetCommandFromInput(m_ChatInput.value);
                    SwitchPlaceholderText();
                    m_RouteChipText.text = m_ChatInput.value;
                    SetRouteChipVisible(true);
                    HideRoutesPopup();
                    ClearText();
                }
        }

        private void OnChatKeyUpEvent(KeyUpEvent evt)
        {
#if ENABLE_ASSISTANT_BETA_FEATURES
            if (m_RouteEnabled)
            {
                bool cursorAtOrBeforeFirstWord = m_ChatInput.value.Length == 0 || m_ChatInput.cursorIndex <= m_ChatInput.value.Split(" ")[0].Length;

                if (!cursorAtOrBeforeFirstWord && m_RoutesPopup.IsShown)
                {
                    HideRoutesPopup();
                }

                if (cursorAtOrBeforeFirstWord)
                {
                    DetectRouteInput(evt);
                }
            }
#endif

            RefreshChatCharCount();
            OnChatValueChanged();

            switch (evt.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                {
                    break;
                }

                default:
                {
                    return;
                }
            }

            if (evt.ctrlKey || evt.altKey || evt.shiftKey)
            {
                return;
            }

            evt.StopPropagation();

            // Do not allow submit while waiting for a response:
            if (!m_IsMusing)
            {
                OnSubmit?.Invoke(m_ChatInput.value);
                m_AddRouteButton.SetEnabled(true);
                ResetRoute();
            }
        }
    }
}
