using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "UITemplateDefinition", menuName = "Muse Animate Dev/UI/UI Template")]
#endif
    class UITemplateDefinition : ScriptableObject
    {
        public string Name;
        public StyleSheet StyleSheet;
        public VisualTreeAsset Uxml;
    }
}
