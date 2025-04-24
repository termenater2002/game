using UnityEngine;

namespace Unity.Muse.Animate.Prop
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "PropDefinition", menuName = "Muse Animate Dev/Prop Definition")]
#endif
    class PropDefinition : ScriptableObject
    {
        public string ID;
        public string Label;
        public PropDefinitionComponent Prefab;
        public Texture2D Thumbnail;
        public bool IsHidden;
    }
}
