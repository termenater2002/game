using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Unity.Muse.Agent.Dynamic;
using Unity.Muse.Chat.BackendApi.Model;
using UnityEngine;

namespace Unity.Muse.Chat
{
    class RunCommandInterpreter
    {
        internal const string k_DynamicAssemblyName = "Unity.Muse.Agent.Dynamic";
        internal const string k_DynamicCommandNamespace = "Unity.Muse.Agent.Dynamic";
        internal const string k_DynamicCommandClassName = "CommandScript";

        internal static readonly Regex k_CsxMarkupRegex = new("```csx(.*?)```", RegexOptions.Compiled | RegexOptions.Singleline);

        const string k_DynamicCommandFullClassName = k_DynamicCommandNamespace + "." + k_DynamicCommandClassName;

        const string k_DummyCommandScript =
            "\nusing UnityEngine;\nusing UnityEditor;\n\ninternal class CommandScript : IRunCommand\n{\n    public void Execute(ExecutionResult result) {}\n    public void Preview(PreviewBuilder builder) {}\n}";


        readonly DynamicAssemblyBuilder m_Builder = new(k_DynamicAssemblyName, k_DynamicCommandNamespace);
        Dictionary<int, ExecutionResult> m_CommandExecutions = new();

        static string[] k_UnsafeMethods = new[]
        {
            "UnityEditor.AssetDatabase.DeleteAsset",
            "UnityEditor.FileUtil.DeleteFileOrDirectory",
            "System.IO.File.Delete",
            "System.IO.Directory.Delete",
            "System.IO.File.Move",
            "System.IO.Directory.Move"
        };

        public RunCommandInterpreter()
        {
            Task.Run(InitCacheWithDummyCompilation);
        }

        void InitCacheWithDummyCompilation()
        {
            // To enable the internal assembly cache we start a compilation of an empty command
            m_Builder.Compile(k_DummyCommandScript, out _);
        }

        public AgentRunCommand BuildRunCommand(string commandScript)
        {
            // Remove embedded MonoBehaviours that already exist in the project
            var embeddedMonoBehaviours = ExtractRequiredMonoBehaviours(ref commandScript);

            var agentAssembly = m_Builder.CompileAndLoadAssembly(commandScript, out var compilationLogs, out var updatedScript);
            var runCommand = new AgentRunCommand { CompilationLogs = compilationLogs, Script = updatedScript };

            if (agentAssembly != null)
            {
                var commandInstance = CreateRunCommand(agentAssembly, out var commandDescription);

                runCommand.SetInstance(commandInstance, commandDescription);

                if (commandInstance != null)
                {
                    CheckForUnsafeCalls(updatedScript, runCommand);

                    // Save embedded MonoBehaviours list
                    runCommand.SetRequiredMonoBehaviours(embeddedMonoBehaviours);
                }
                else
                {
                    InternalLog.LogWarning($"Unable to find a valid CommandScript in the assembly");
                }
            }
            else
            {
                InternalLog.LogWarning($"Unable to compile the command:\n{compilationLogs}");
            }

            return runCommand;
        }

        static List<ClassCodeTextDefinition> ExtractRequiredMonoBehaviours(ref string commandScript)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(commandScript);
            var embeddedMonoBehaviours = tree.ExtractTypesByInheritance<MonoBehaviour>();
            for (var i = embeddedMonoBehaviours.Count - 1; i >= 0; i--)
            {
                var monoBehaviour = embeddedMonoBehaviours[i];
                if (UserAssemblyContainsType(monoBehaviour.ClassName))
                {
                    commandScript = tree.RemoveType(monoBehaviour.ClassName).GetText().ToString();
                    embeddedMonoBehaviours.RemoveAt(i);
                }
            }

            return embeddedMonoBehaviours;
        }

        void CheckForUnsafeCalls(string commandScript, AgentRunCommand runCommand)
        {
            var compilation = m_Builder.Compile(commandScript, out var tree);
            var model = compilation.GetSemanticModel(tree);

            var root = tree.GetCompilationUnitRoot();
            var walker = new PublicMethodCallWalker(model);
            walker.Visit(root);

            foreach (var methodCall in walker.PublicMethodCalls)
            {
                if (k_UnsafeMethods.Contains(methodCall))
                {
                    runCommand.Unsafe = true;
                    break;
                }
            }
        }

        IRunCommand CreateRunCommand(Assembly dynamicAssembly, out string commandDescription)
        {
            var type = dynamicAssembly.GetType(k_DynamicCommandFullClassName);
            if (type == null)
            {
                commandDescription = null;
                return null;
            }

            var attribute = type.GetCustomAttribute<CommandDescriptionAttribute>();
            commandDescription = attribute?.Description;

            return Activator.CreateInstance(type) as IRunCommand;
        }

        public void StoreExecution(ExecutionResult executionResult)
        {
            m_CommandExecutions.Add(executionResult.Id, executionResult);
        }

        public ExecutionResult RetrieveExecution(int id)
        {
            return m_CommandExecutions.GetValueOrDefault(id);
        }

        static bool UserAssemblyContainsType(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyCSharp = assemblies.FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");

            if (assemblyCSharp != null)
            {
                var type = assemblyCSharp.GetType(typeName);
                return type != null;
            }

            return false;
        }
    }
}
