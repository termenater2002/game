using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Muse.Chat.UI.Utils;
using Unity.Muse.Common.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    class ChatElementUser : ChatElementBase
    {
        const string k_ConnectAssembly = "UnityEditor.Connect.UnityConnect";
        const string k_UserInfoType = "UnityEditor.Connect.UserInfo";
        const string k_UserInfoMethod = "GetUserInfo";
        const string k_UserInstanceProperty = "instance";
        const string k_UserInfoDisplayNameProperty = "displayName";
        const string k_UserInfoIdProperty = "userId";

        const string k_EditModeActiveClass = "mui-um-edit-mode-active";
        const string k_EditModeRWTextFieldClass = "mui-user-edit-read-write-field";
        const string k_ContextElementClass = "mui-context-entry-user-message";

        readonly IList<VisualElement> m_TextFields = new List<VisualElement>();

        VisualElement m_ChatRoot;

        VisualElement m_EditControls;
        Button m_EditButton;
        Button m_EditCancelButton;

        VisualElement m_TextFieldRoot;
        MuseTextField m_EditField;
        MuseChatImage m_UserIcon;
        VisualElement m_UserIconFrame;
        Label m_UserName;
        Foldout m_ContextFoldout;
        VisualElement m_ContextContent;

        bool m_EditEnabled = true;
        bool m_EditModeActive;
        bool m_EditButtonVisible = false;

        public bool EditEnabled
        {
            get => m_EditEnabled;
            set
            {
                if (m_EditEnabled == value)
                {
                    return;
                }

                m_EditEnabled = value;

                if(!m_EditEnabled)
                {
                    SetEditMode(false);
                }

                RefreshUI();
            }
        }

        /// <summary>
        /// Set the user data used by this element
        /// </summary>
        /// <param name="message">the message to display</param>
        /// <param name="id">id of the message, used for edit callbacks</param>
        public override void SetData(MuseMessage message)
        {
            base.SetData(message);

            EditEnabled = true;

            RefreshText(m_TextFieldRoot, m_TextFields);
            RefreshUI();
            RefreshContext();
        }

        string GetUserName()
        {
            try
            {
                var connectAssembly = TypeDef<CloudProjectSettings>.Assembly;
                var unityConnectType = connectAssembly.GetType(k_ConnectAssembly);
                var userInfoMethod = unityConnectType.GetMethod(k_UserInfoMethod);
                var instanceProperty = unityConnectType.GetProperty(k_UserInstanceProperty, BindingFlags.Public | BindingFlags.Static);
                var instance = instanceProperty.GetValue(null, null);

                var userInfo = userInfoMethod.Invoke(instance, null);

                var userInfoType = connectAssembly.GetType(k_UserInfoType);
                var displayNameProp = userInfoType.GetProperty(k_UserInfoDisplayNameProperty);
                var displayName = (string)displayNameProp.GetValue(userInfo);

                var userIdProp = userInfoType.GetProperty(k_UserInfoIdProperty);
                var userId = (string)userIdProp.GetValue(userInfo);

                SetUserAvatar(userId);
                return displayName;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return string.Empty;
        }

        void SetUserAvatar(string userId)
        {
            UserAvatarHelper.GetUserAvatar(userId, (icon) =>
            {
                if (icon != null)
                {
                    m_UserIconFrame.SetDisplay(true);
                    m_UserIcon.SetTexture(icon);
                }
            });
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_ChatRoot = view.Q<VisualElement>("chatRoot");

            m_ContextFoldout = view.Q<Foldout>("contextFoldout");

            m_ContextContent = view.Q<VisualElement>("contextContent");

            m_EditControls = view.Q<VisualElement>("editControls");
            m_EditButton = view.SetupButton("editButton", OnEditClicked);
            m_EditCancelButton = view.SetupButton("editCancelButton", x => { SetEditMode(false); });

            m_TextFieldRoot = view.Q<VisualElement>("userMessageTextFieldRoot");

            // Hide the icon until we find a way to display that:
            m_UserIcon = view.SetupImage("userIcon");
            m_UserIconFrame = view.Q<VisualElement>("userIconFrame");
            m_UserIconFrame.SetDisplay(false);

            m_UserName = view.Q<Label>("userName");
            m_UserName.text = GetUserName();

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerExit);
        }

        static string TrimDisplayString(string s)
        {
            return s.Trim('\n', '\r');
        }

        void InitializeEditField()
        {
            if (m_EditField != null)
            {
                return;
            }

            m_EditField = new MuseTextField();
            m_EditField.Initialize();
            m_EditField.ShowPlaceholder = false;
            m_EditField.HighlightFocus = false;
            m_EditField.OnSubmit += OnEditFieldSubmit;
            m_EditField.style.display = DisplayStyle.None;
            m_EditField.AddToClassList(k_EditModeRWTextFieldClass);
            m_TextFieldRoot.parent.Add(m_EditField);
        }

        void OnEditFieldSubmit(string value)
        {
            if (!m_EditModeActive)
            {
                return;
            }

            value = TrimDisplayString(value);

            string changedText = value;
            if (changedText != TrimDisplayString(Message.Content))
            {
                Assistant.instance.ProcessEditPrompt(changedText, Message.Id);
            }

            SetEditMode(false);
        }

        void OnEditClicked(PointerUpEvent evt)
        {
            SetEditMode(!m_EditModeActive);
        }

        void SetEditMode(bool state)
        {
            // Never allow going into edit state if editing is disabled:
            if (!EditEnabled)
            {
                state = false;
            }

            InitializeEditField();

            m_EditModeActive = state;
            if (m_EditModeActive)
            {
                // Editing started, we only show the first chunk of the message
                m_EditField.SetText(TrimDisplayString(MessageChunks[0]));
            }
            else
            {
                // Clear out the edit field to save some memory, message could be large
                m_EditField.ClearText();
            }

            RefreshUI();
        }

        void RefreshUI()
        {
            m_EditControls.style.display = EditEnabled ? DisplayStyle.Flex : DisplayStyle.None;
            m_ChatRoot.EnableInClassList(k_EditModeActiveClass, m_EditModeActive);

            m_EditButton.SetDisplay(m_EditButtonVisible && !m_EditModeActive);
            m_EditCancelButton.SetDisplay(m_EditModeActive);
            m_TextFieldRoot.SetDisplay(!m_EditModeActive);

            if (m_EditField != null)
            {
                m_EditField.SetDisplay(m_EditModeActive);
                m_EditField.SelectAllText();
            }
        }

        void RefreshContext()
        {
            if (ContextEntries == null || ContextEntries.Count == 0)
            {
                m_ContextFoldout.style.display = DisplayStyle.None;
                return;
            }

            m_ContextFoldout.style.display = DisplayStyle.Flex;

            m_ContextContent.Clear();
            for (var index = 0; index < ContextEntries.Count; index++)
            {
                var contextEntry = ContextEntries[index];
                var entry = new ContextElement();
                entry.Initialize();
                entry.SetData(contextEntry, extraStyle: k_ContextElementClass);
                m_ContextContent.Add(entry);
            }
        }

        void OnPointerExit(PointerLeaveEvent evt)
        {
            m_EditButtonVisible = false;
            RefreshUI();
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            m_EditButtonVisible = true;
            RefreshUI();
        }
    }
}
