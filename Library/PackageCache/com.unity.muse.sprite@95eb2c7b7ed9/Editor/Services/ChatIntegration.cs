using Unity.Muse.Common.Editor;
using Unity.Muse.Common.Editor.Integration;
using Unity.Muse.Common.Services;

namespace Unity.Muse.Sprite.Editor.Services
{
    static class ChatIntegration
    {
        [Plugin("Plugin for creating a sprite given a prompt.")]
        public static void GenerateSprite(
            [Parameter("The prompt to guide what sprite will be generated")]
            string prompt)
        {
            GenerationService.GenerateImage(prompt,
                result => { },
                ImageModel.Sprite,
                model =>
                {
                    EditorModelAssetEditor.OpenEditorTo(model);
                });
        }
    }
}
