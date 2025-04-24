using System.ComponentModel;

namespace Unity.Muse.Chat
{
    internal enum ChatCommandType
    {
        [Description("Ask questions and get help")]
        Ask,
        [Description("Give Muse a task to do")]
        Run,
        [Description("Generate code with Muse")]
        Code,
        [Description("Generate Match Three code with Muse")]
        Match3,
    }
}
