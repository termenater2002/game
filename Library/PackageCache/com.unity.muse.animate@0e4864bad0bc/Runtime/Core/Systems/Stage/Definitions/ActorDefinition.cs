using Unity.Muse.Animate;
using UnityEngine;

namespace DefaultNamespace
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "ActorDefinition", menuName = "Muse Animate Dev/Actor Definition")]
#endif
    class ActorDefinition : ScriptableObject
    {
        public string ID;
        public string Label;
        public ActorDefinitionComponent Prefab;
        public Texture2D Thumbnail;
        public bool IsHidden;
    }
}
