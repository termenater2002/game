using Unity.Muse.Agent.Dynamic;
using Unity.Muse.Chat.UI.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    class ChatElementRunExecutionBlock : CommandDisplayTemplate
    {
        public const string FencedBlockTag = "csx_execute";

        Button m_UndoButton;
        Label m_Title;

        public ChatElementRunExecutionBlock()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        VisualElement m_ExecutionContainer;

        protected override void InitializeView(TemplateContainer view)
        {
            m_Title = view.Q<Label>("runActionTitle");

            m_ExecutionContainer = view.Q<VisualElement>("executionContainer");

            m_UndoButton = view.SetupButton("undoButton", _ => UndoHistoryUtils.OpenHistory());
            m_UndoButton.SetEnabled(false);
        }

        public static void FormatExecutionResult(ExecutionResult executionResult, VisualElement container)
        {
            if (!executionResult.SuccessfullyStarted)
            {
                var executionEntry = new ChatElementRunExecutionEntry(new ExecutionLog($"<color={ExecutionResult.WarningTextColor}>{executionResult.ConsoleLogs}</color>", LogType.Error));
                executionEntry.Initialize(false);
                container.Add(executionEntry);

                return;
            }

            var resultLogs = executionResult.Logs;
            if (resultLogs.Count == 0)
            {
                var executionEntry = new ChatElementRunExecutionEntry(new ExecutionLog("Executed without logs", LogType.Log));
                executionEntry.Initialize(false);
                container.Add(executionEntry);
                return;
            }

            foreach (var executionLog in resultLogs)
            {
                var executionEntry = new ChatElementRunExecutionEntry(executionLog);
                executionEntry.Initialize(false);
                container.Add(executionEntry);
            }
        }

        public override void Display()
        {
            var content = ContentGroups[0].Content;
            if (int.TryParse(content, out var executionId))
            {
                var execution = Assistant.instance.Agent.RetrieveExecution(executionId);

                m_Title.text = execution.CommandName ?? "Command completed";
                FormatExecutionResult(execution, m_ExecutionContainer);
                m_UndoButton.SetEnabled(execution.SuccessfullyStarted);
            }
        }
    }
}
