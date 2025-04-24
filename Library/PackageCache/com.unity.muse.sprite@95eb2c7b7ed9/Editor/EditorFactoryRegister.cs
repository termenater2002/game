using Unity.Muse.Sprite.Artifacts;
using Unity.Muse.Sprite.Editor.DragAndDrop;
using Unity.Muse.Sprite.Operators;
using Unity.Muse.Sprite.Tools;
using Unity.Muse.Sprite.UIMode;

namespace Unity.Muse.Sprite.Editor
{
    static class EditorFactoryRegister
    {
        [UnityEditor.InitializeOnLoadMethod]
        static void RegisterFactory()
        {
            ArtifactRegistration.RegisterArtifact();
            OperatorRegistration.RegisterOperators();
            ToolRegistration.RegisterTools();
            UIModeRegistration.RegisterUIMode();
            SpriteArtifactDragAndDropHandler.Register();
        }
    }
}
