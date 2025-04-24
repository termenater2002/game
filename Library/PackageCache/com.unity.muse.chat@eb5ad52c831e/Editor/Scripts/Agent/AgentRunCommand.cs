using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Muse.Agent.Dynamic;
using UnityEngine;

namespace Unity.Muse.Chat
{
    class AgentRunCommand
    {
        static readonly string[] k_UnauthorizedNamespaces = { "System.Net", "System.Diagnostics" };

        IRunCommand m_ActionInstance;

        string m_Description;

        List<ClassCodeTextDefinition> m_RequiredMonoBehaviours;

        public string Script { get; set; }
        public string CompilationLogs { get; set; }

        public bool PreviewIsDone { get; set; }

        public bool Unsafe { get; set; }

        public string Description => m_Description;

        public IRunCommand Instance => m_ActionInstance;

        public bool CompilationSuccess => m_ActionInstance != null;

        public IEnumerable<ClassCodeTextDefinition> RequiredMonoBehaviours => m_RequiredMonoBehaviours;

        public void SetInstance(IRunCommand instance, string description)
        {
            m_ActionInstance = instance;
            m_Description = description;
        }

        public bool Execute(out ExecutionResult executionResult)
        {
            executionResult = new ExecutionResult(m_Description);

            if (m_ActionInstance == null)
                return false;

            executionResult.Start();

            var chatObjectSelection = Assistant.instance.GetValidAttachment(Assistant.instance.k_ObjectAttachments);
            var actionAttachment = new CommandAttachment(chatObjectSelection);

            try
            {
                m_ActionInstance.Execute(actionAttachment, executionResult);
            }
            catch (Exception e)
            {
                executionResult.LogError(e.ToString());
            }

            executionResult.End();

            return true;
        }

        public void BuildPreview(out PreviewBuilder builder)
        {
            builder = new PreviewBuilder();

            if (m_ActionInstance == null)
                return;

            PreviewIsDone = true;
            m_ActionInstance.BuildPreview(builder);
        }

        public void SetRequiredMonoBehaviours(List<ClassCodeTextDefinition> requiredMonoBehaviours)
        {
            m_RequiredMonoBehaviours = requiredMonoBehaviours;
        }

        public bool HasUnauthorizedNamespaceUsage()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(Script);
            return tree.ContainsNamespaces(k_UnauthorizedNamespaces);
        }
    }
}
