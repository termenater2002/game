using Unity.Muse.Common.Editor;
using Unity.Muse.Common;
using UnityEngine.UIElements;
using UnityEditor;

namespace Unity.Muse.Animate.Editor
{
    class EditorAnimatePreferencesView : IMuseEditorPreferencesView
    {
        TextField m_AnimateAssetGeneratedPathField;

        public VisualElement CreateGUI()
        {
            var root = new VisualElement();
            var animateAssetRow = new VisualElement();
            animateAssetRow.AddToClassList(MuseProjectSettings.rowUssClassName);
            var assetLabel = new Label {text = TextContent.animateAssetGeneratedPath};
            assetLabel.AddToClassList(MuseProjectSettings.propertyLabelUssClassName);
            animateAssetRow.Add(assetLabel);

            m_AnimateAssetGeneratedPathField = new TextField { isDelayed = true };
            m_AnimateAssetGeneratedPathField.RegisterValueChangedCallback(evt =>
            {
                var newValue = GlobalPreferences.SanitizeMuseGeneratedPath(evt.newValue);
                AnimatePreferences.AssetGeneratedFolderPath = newValue;
                if (newValue != evt.newValue)
                    m_AnimateAssetGeneratedPathField.SetValueWithoutNotify(newValue);
            });

            animateAssetRow.Add(m_AnimateAssetGeneratedPathField);

            var resetAnimatePathButton = new Button(() =>
            {
                m_AnimateAssetGeneratedPathField.value = AnimatePreferences.GetDefaultAnimateGeneratedAssetPath();
            }) { text = TextContent.reset };
            animateAssetRow.Add(resetAnimatePathButton);

            var browseAnimateAssetButton = new Button(() =>
            {
                var newValue = EditorUtility.OpenFolderPanel(
                    TextContent.selectFolder,
                    AnimatePreferences.AssetGeneratedFolderPath, "");
                var sanitizePath = MuseProjectSettings.SanitizePath(newValue);
                if (!string.IsNullOrEmpty(sanitizePath))
                    m_AnimateAssetGeneratedPathField.value = sanitizePath;
            }) {text = TextContent.browse};

            animateAssetRow.Add(browseAnimateAssetButton);
            root.Add(animateAssetRow);

            return root;
        }

        public void Refresh()
        {
            var animateAssetGeneratedPath = AnimatePreferences.AssetGeneratedFolderPath;
            m_AnimateAssetGeneratedPathField.SetValueWithoutNotify(animateAssetGeneratedPath);
        }
    }
}
