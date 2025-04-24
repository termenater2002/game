using Unity.Muse.Common.Editor.Integration;
using UnityEngine;
using UnityEngine.Assertions;

#if MUSE_CHAT_PLUGIN_ENABLED

namespace Unity.Muse.Animate.Editor
{
    /// <summary>
    /// Static class which expose an API that can be used by Muse Chat.
    /// This API is found through reflection by Muse Chat and is used to call the methods in this class.
    /// </summary>
    static class MuseChatPlugin
    {
        [Plugin("Plugin for creating a humanoid animation given a prompt.")]
        static void GenerateAnimationsFromPrompt(
            [Parameter("The prompt to guide what animation will be generated")] string prompt)
        {
            int numberOfGenerations = 1;
            float length = 3;
            if (!MuseAnimateEditorWindow.IsWindowShown)
            {
                MuseAnimateEditorWindow.ShowWindow();
                MuseAnimateEditorWindow.Window.AuthoringStarted += AuthoringStarted;
            }
            else
            {
                Generate();
            }
            return;

            void AuthoringStarted()
            {
                MuseAnimateEditorWindow.Window.AuthoringStarted -= AuthoringStarted;
                Generate();
            }

            void Generate()
            {
                var authorState = MuseAnimateEditorWindow.Window.Application.ApplicationFlow.GetState<Application.ApplicationHsm.Root>();
                if (authorState == null)
                {
                    Debug.LogError("Not in Application.ApplicationHsm.Author, should wait to be in authoring state before triggering a generation.");
                    return;
                }

                length = Mathf.Clamp(length, 1, 5);
                numberOfGenerations = Mathf.Clamp(numberOfGenerations, 1, 4);
                authorState.RequestTextToMotionGenerate(prompt, null, numberOfGenerations, length, ITimelineBakerTextToMotion.Model.V2);
            }
        }
    }
}

#endif
