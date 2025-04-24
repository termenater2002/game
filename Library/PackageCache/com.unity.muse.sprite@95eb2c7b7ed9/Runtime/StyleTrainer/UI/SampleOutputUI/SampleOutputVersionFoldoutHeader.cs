using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class SampleOutputVersionFoldoutHeader : ExVisualElement
    {
        internal static SampleOutputVersionFoldoutHeader CreateFromUxml()
        {
            var visualTree = ResourceManager.Load<VisualTreeAsset>(PackageResources.sampleOutputVersionFoldoutHeaderTemplate);
            var ve = (SampleOutputVersionFoldoutHeader)visualTree.CloneTree().Q("SampleOutputVersionFoldoutHeader");
            ve.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.sampleOutputPromptInputStyleSheet));
            return ve;
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<SampleOutputVersionFoldoutHeader, UxmlTraits> { }
#endif
    }
}