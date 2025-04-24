using System;
using System.IO;
using Unity.Muse.Chat.UI.Utils;
using Unity.Muse.Common.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    internal class ChatElementCodeBlock : CommandDisplayTemplate
    {
        const string k_OldFailMessage = "Something went wrong.\nThe generated script was not able to compile in your project.";
        const string k_NewFailMessage = "Something went wrong.\nThe generated script was not able to compile in your project. Try to correct any errors, or generate it again.";
        Label m_Text;
        protected Button m_SaveButton;
        protected Button m_CopyButton;
        VisualElement m_WarningContainer;
        Label m_WarningText;

        public ChatElementCodeBlock()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_Text = view.Q<Label>("codeText");
            m_SaveButton = view.SetupButton("saveCodeButton", OnSaveCodeClicked);
            m_CopyButton = view.SetupButton("copyCodeButton", OnCopyCodeClicked);

            m_WarningContainer = view.Q<VisualElement>("codeWarningContainer");
            m_WarningContainer.SetDisplay(false);
            m_WarningContainer.style.marginBottom = 10;

            m_WarningText = view.Q<Label>("codeWarningText");
        }

        public override void OnSetMessage()
        {
            SetSelectable(true);
        }

        public override void Display()
        {
            UpdateTextWithCode();
            m_SaveButton.SetEnabled(true);
            m_CopyButton.SetEnabled(true);
        }

        protected void UpdateTextWithCode()
        {
            // Update Code preview text with latest code
            string disclaimerHeader = string.Format(MuseChatConstants.DisclaimerText, DateTime.Now.ToShortDateString());
            m_Text.text = CodeSyntaxHighlight.Highlight(string.Concat(disclaimerHeader, ContentGroups[0].Content));
        }

        protected void SetWarningText(string text)
        {
            m_WarningText.text = text;
            m_WarningContainer.SetDisplay(!string.IsNullOrWhiteSpace(text));
        }

        public void SetSelectable(bool selectable)
        {
            m_Text.selection.isSelectable = selectable;
        }

        protected virtual string FormatCodeForSave(string code, string className)
        {
            return CodeExportUtils.Format(code, className);
        }

        void OnCopyCodeClicked(PointerUpEvent evt)
        {
            string disclaimerHeader = string.Format(MuseChatConstants.DisclaimerText, DateTime.Now.ToShortDateString());
            GUIUtility.systemCopyBuffer = string.Concat(disclaimerHeader, ContentGroups[0].Content);

            MuseChatView.ShowNotification("Copied to clipboard", PopNotificationIconType.Info);
        }

        void OnSaveCodeClicked(PointerUpEvent evt)
        {
            string file = EditorUtility.SaveFilePanel("Save Code", Application.dataPath, "code", "cs");
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            EditorUtility.DisplayProgressBar("Saving Code", "Saving code to file", 0.5f);

            try
            {
                string formattedCode = FormatCodeForSave(ContentGroups[0].Content, Path.GetFileNameWithoutExtension(file));
                File.WriteAllText(file, formattedCode);
            }
            catch (Exception)
            {
                MuseChatView.ShowNotification("Failed to save code to file", PopNotificationIconType.Error);
                EditorUtility.ClearProgressBar();
                return;
            }

            MuseChatView.ShowNotification("File Saved", PopNotificationIconType.Info);
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }
    }
}
