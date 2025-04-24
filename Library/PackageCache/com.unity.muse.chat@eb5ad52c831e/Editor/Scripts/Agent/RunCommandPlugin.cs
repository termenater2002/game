using Unity.Muse.Common.Editor.Integration;
using UnityEngine;

namespace Unity.Muse.Chat
{
#if ENABLE_ASSISTANT_BETA_FEATURES
    /// <summary>
    /// Static class which exposes Muse Agent to the plugin system.
    /// </summary>
    static class RunCommandPlugin
    {
        /// <summary>
        /// Triggers Muse Agent to conduct in-editor action based on the original user message.
        /// </summary>
        /// <param name="userMessage">The original user message to send to Muse Agent.</param>
        [Plugin("Plugin to trigger Muse Agent to execute in-editor actions based on the original user message. The in-editor actions include manipulating the scene, changing project settings, organizing the project, creating primitive game object, etc.")]
        static void TriggerAgentFromPrompt([Parameter("The original user message to instruct Muse Agent to conduct in-editor action.")] string userMessage)
        {
            userMessage = "/" + ChatCommandType.Run.ToString().ToLower() + " " + userMessage;
            _ = Assistant.instance.ProcessPrompt(userMessage);
        }
    }
#endif
}
