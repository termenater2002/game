using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat
{
    internal class CodeBlockValidator
    {
        internal const string k_ValidatorAssemblyName = "Unity.Muse.CodeGen";

        readonly DynamicAssemblyBuilder m_Builder = new(k_ValidatorAssemblyName);

        public bool ValidateCode(string code, out string localFixedCode, out string compilationLogs)
        {
            var codeAssembly = m_Builder.CompileAndLoadAssembly(code, out compilationLogs, out localFixedCode);

            return codeAssembly != null;
        }
    }
}
