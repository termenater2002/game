using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Unity.Muse.Chat.Commands;
using Unity.Muse.Chat.UI.Utils;
using Unity.Muse.Common.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    class ChatElementRunCommandBlock : CommandDisplayTemplate
    {
        public static Action<string, string> OnDevToolClicked;

        Label m_Title;
        Button m_ExecuteButton;
        Button m_DebugButton;
        Label m_CodePreview;
        Foldout m_CodePreviewFoldout;
        VisualElement m_PreviewContainer;
        VisualElement m_WarningContainer;
        Label m_WarningText;

        AgentRunCommand m_RunCommand;

        public ChatElementRunCommandBlock()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_Title = view.Q<Label>("actionTitle");
            m_Title.text = "New command";

            m_WarningContainer = view.Q<VisualElement>("warningContainer");
            m_WarningContainer.SetDisplay(false);
            m_WarningText = view.Q<Label>("warningText");

            m_ExecuteButton = view.SetupButton("executeButton", OnExecuteCodeClicked);
            m_ExecuteButton.SetEnabled(false);

            var devToolButton = view.Q<Button>("devToolButton");
            devToolButton.SetDisplay(false);

            if (OnDevToolClicked != null)
                InitializeDevTool(devToolButton);

            var overviewFoldout = view.Q<Foldout>("overviewFoldout");

            m_CodePreviewFoldout = view.Q<Foldout>("codePreviewFoldout");
            m_CodePreviewFoldout.SetDisplay(false);

            m_CodePreview = view.Q<Label>("actionCode");

            m_PreviewContainer = view.Q<VisualElement>("previewContainer");
        }

        void InitializeDevTool(Button devToolButton)
        {
            devToolButton.SetDisplay(true);
            devToolButton.clicked += () =>
            {
                string userQuery = string.Empty;
                var conversation = Assistant.instance.GetActiveConversation();
                for (var i = conversation.Messages.Count - 1; i >= 1; i--)
                {
                    var message = conversation.Messages[i];
                    if (message.Id == m_ParentMessage.Id)
                    {
                        userQuery = conversation.Messages[i - 1].Content;
                        break;
                    }
                }

                OnDevToolClicked.Invoke(userQuery, ContentGroups[0].Content);
            };
        }

        protected override bool ValidateInternal(int index, out string logs)
        {
            m_RunCommand = Assistant.instance.Agent.BuildRunCommand(ContentGroups[index].Content);
            if (m_RunCommand == null)
            {
                logs = "No command present";
                return false;
            }
            logs = m_RunCommand.CompilationLogs;
            return m_RunCommand.CompilationSuccess;
        }

        public override void Display()
        {
            var content = ContentGroups[0];

            switch (content.State)
            {
                case DisplayState.Success:
                    GeneratePreview();
                    break;
                case DisplayState.Fail:
                    DisplayCompilationWarning();
                    break;
            }
        }

        void DisplayCompilationWarning()
        {
            //Still display the code
            m_CodePreviewFoldout.text = "Failed command attempt";
            m_CodePreviewFoldout.SetDisplay(true);
            m_CodePreview.text = CodeSyntaxHighlight.Highlight(CodeExportUtils.Format(m_RunCommand.Script));

            m_WarningContainer.SetDisplay(true);

            if (m_RunCommand.HasUnauthorizedNamespaceUsage())
                m_WarningText.text =  "<b>Sorry, I can’t help with that.</b>\nA script was generated that does triggers an unauthorized API. As a safety precaution, this is not permitted.";
            else
                m_WarningText.text =  "<b>Can we try that again?</b>\nIt helps to be detailed and add an attachment to add context to your request. If you keep getting this message, try to ask something else.";

            m_WarningText.tooltip = $"Unable to compile the command:\n {m_RunCommand.CompilationLogs}";
        }

        void DisplayUnsafeWarning()
        {
            m_WarningContainer.SetDisplay(true);
            m_WarningText.text =  "This command is performing operations that cannot be undone.";
        }

        void GeneratePreview()
        {
            if (m_RunCommand.Unsafe)
                DisplayUnsafeWarning();

            m_Title.text = m_RunCommand.Description;

            // Update Code preview text with latest code
            m_CodePreviewFoldout.SetDisplay(true);
            m_CodePreview.text = FormatDisplayScript();
            m_CodePreviewFoldout.value = false; // collapse by default

            if (!m_RunCommand.PreviewIsDone)
            {
                // Update preview content with DryRun
                m_RunCommand.BuildPreview(out var previewBuilder);

                if (m_RunCommand.RequiredMonoBehaviours.Any())
                {
                    foreach (var requiredComponent in m_RunCommand.RequiredMonoBehaviours)
                    {
                        var commandEntry = new ChatElementRunCommandEntry( $"A new C# component <b>{requiredComponent.ClassName}</b> is required to perform this command.", m_RunCommand);
                        commandEntry.Initialize(false);
                        commandEntry.RegisterAction(() =>
                        {
                            string file = EditorUtility.SaveFilePanel("Save new Component", Application.dataPath, requiredComponent.ClassName, "cs");
                            if (!string.IsNullOrEmpty(file))
                            {
                                File.WriteAllText(file, requiredComponent.Code);
                                EditorUtility.DisplayProgressBar("New components", "Recompiling assembly", 0);
                                AssetDatabase.Refresh();
                                EditorUtility.RequestScriptReload();
                            }
                        });

                        m_PreviewContainer.Add(commandEntry);
                    }

                }

                foreach (var previewLine in previewBuilder.Preview)
                {
                    var commandEntry = new ChatElementRunCommandEntry(previewLine, m_RunCommand);
                    commandEntry.Initialize(false);

                    m_PreviewContainer.Add(commandEntry);
                }

                if (!EditorApplication.isPlaying && !m_RunCommand.RequiredMonoBehaviours.Any())
                    m_ExecuteButton.SetEnabled(true);
            }
        }

        string FormatDisplayScript()
        {
            // Update Code preview with Muse AI disclaimer
            var scriptDisclaimer = string.Format(MuseChatConstants.DisclaimerText, DateTime.Now.ToShortDateString());

            // Remove namespaces from display
            var tree = SyntaxFactory.ParseSyntaxTree(scriptDisclaimer + m_RunCommand.Script);
            tree = tree.RemoveNamespaces();

            return CodeSyntaxHighlight.Highlight(tree.GetText().ToString());
        }

        void OnExecuteCodeClicked(PointerUpEvent evt)
        {
            ExecuteCommand();
        }

        private void ExecuteCommand()
        {
            m_RunCommand.Execute(out var executionResult);

            var agent = Assistant.instance.Agent;
            agent.StoreExecution(executionResult);

            Assistant.instance.AddInternalMessage($"```{ChatElementRunExecutionBlock.FencedBlockTag}\n{executionResult.Id}\n```", Assistant.k_SystemRole, false, author: RunCommand.k_CommandName);
        }
    }
}
