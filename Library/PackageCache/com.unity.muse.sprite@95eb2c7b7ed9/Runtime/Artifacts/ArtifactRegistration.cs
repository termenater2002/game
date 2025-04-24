using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Muse.Sprite.Artifacts
{
    internal static class ArtifactRegistration
    {
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        [Preserve]
        public static void RegisterArtifact()
        {
            ArtifactFactory.SetArtifactTypeForMode<SpriteMuseArtifact>(UIMode.UIMode.modeKey);
        }
    }
}
