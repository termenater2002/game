using Unity.Muse.Common.Editor;
using Unity.Muse.Common;
using UnityEngine.UIElements;
using UnityEditor;

namespace Unity.Muse.Sprite.Editor
{
    class EditorSpritePreferencesView : IMuseEditorPreferencesView
    {
        TextField m_SpriteAssetGeneratedPathField;

        public VisualElement CreateGUI()
        {
            var root = new VisualElement();
            var spriteAssetRow = new VisualElement();
            spriteAssetRow.AddToClassList(MuseProjectSettings.rowUssClassName);
            var spriteAssetLabel = new Label {text = TextContent.spriteAssetGeneratedPath};
            spriteAssetLabel.AddToClassList(MuseProjectSettings.propertyLabelUssClassName);
            spriteAssetRow.Add(spriteAssetLabel);
            m_SpriteAssetGeneratedPathField = new TextField { isDelayed = true };
            m_SpriteAssetGeneratedPathField.RegisterValueChangedCallback(evt =>
            {
                var newValue = GlobalPreferences.SanitizeMuseGeneratedPath(evt.newValue);
                SpritePreferences.assetGeneratedFolderPath = newValue;
                if (newValue != evt.newValue)
                    m_SpriteAssetGeneratedPathField.SetValueWithoutNotify(newValue);
            });
            spriteAssetRow.Add(m_SpriteAssetGeneratedPathField);
            var browseSpriteAssetButton = new Button(() =>
            {
                var newValue = EditorUtility.OpenFolderPanel(
                    TextContent.selectFolder, 
                    SpritePreferences.assetGeneratedFolderPath, "");
                newValue = MuseProjectSettings.SanitizePath(newValue);
                if (!string.IsNullOrEmpty(newValue))
                    m_SpriteAssetGeneratedPathField.value = newValue;
            }) {text = TextContent.browse};
            spriteAssetRow.Add(browseSpriteAssetButton);
            root.Add(spriteAssetRow);
            
            return root;
        }

        public void Refresh()
        {
            var spriteAssetGeneratedPath = SpritePreferences.assetGeneratedFolderPath;
            m_SpriteAssetGeneratedPathField.SetValueWithoutNotify(spriteAssetGeneratedPath);
        }
    }
}
