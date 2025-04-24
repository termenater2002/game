using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Object = UnityEngine.Object;
using TextField = UnityEngine.UIElements.TextField;

namespace Unity.Muse.Common.Editor.Metadata
{
    internal class PngMetadataEditor : EditorWindow
    {
        public const string titleText = "PNG Metadata Editor";

        [MenuItem("internal:Muse/Internals/PNG Metadata Editor")]
        public static void CreateWindow() => EditorWindow.CreateWindow<PngMetadataEditor>().titleContent = new GUIContent(titleText);

        PngMetadataEditorUI m_WatermarkUI;

        void CreateGUI()
        {
            m_WatermarkUI = new PngMetadataEditorUI();
            m_WatermarkUI.StretchToParentSize();
            m_WatermarkUI.onSelectedTexture += OnSelectedTexture;
            rootVisualElement.Add(m_WatermarkUI);
        }

        void OnSelectedTexture(Texture2D newTexture)
        {
            var newTitle = newTexture != null
                ? $"{titleText} ({newTexture.name})"
                : titleText;
            titleContent = new GUIContent(newTitle);
        }
    }

    internal class PngMetadataEditorUI : VisualElement
    {
        public event Action<Texture2D> onSelectedTexture;

        Texture2D m_SelectedTexture;

        Button m_RemoveButton;
        Button m_AddButton;
        Button m_ReadButton;
        Button m_WriteButton;

        MultiColumnListView m_ListView;

        List<MetadataEntry> m_Metadata = new();

        static readonly PngChunkType[] k_AvailableChunkTypes = { PngChunkType.tEXt, PngChunkType.iTXt, PngChunkType.eXIf };
        static readonly List<string> k_AvailableChunkNames = k_AvailableChunkTypes.Select(t => t.ToString()).ToList();

        class MetadataEntry
        {
            public string key;
            public string value;
            public PngChunkType type;
        }

        static class Tooltips
        {
            public const string textureField = "Assign a texture to read/write metadata.";
            public const string dataListField = "List of metadata entries.";
            public const string addButton = "Select to add a new metada entry.";
            public const string readButton = "Select to read texture metadata.";
            public const string writeButton = "Select to write metadata to texture.";
            public const string buttonInactive = "Assing a texture to Texture field to enable metadata modification.";
        }

        public PngMetadataEditorUI()
        {
            var topUI = new VisualElement();
            topUI.style.flexShrink = 0;
            topUI.style.height = 50.0f;
            topUI.style.justifyContent = Justify.Center;
            var textureField = new ObjectField("PNG Texture") { objectType = typeof(Texture2D), tooltip = Tooltips.textureField, allowSceneObjects = false };
            textureField.RegisterValueChangedCallback(OnSetTexture);
            topUI.Add(textureField);
            Add(topUI);

            var entriesUI = new VisualElement();
            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            m_RemoveButton = new Button(OnRemoveEntry) { text = "Remove" };
            m_RemoveButton.style.display = DisplayStyle.None;
            buttons.Add(m_RemoveButton);
            m_AddButton = new Button(OnAddEntry) { text = "Add" };
            buttons.Add(m_AddButton);
            entriesUI.style.flexShrink = 0;
            entriesUI.style.flexDirection = FlexDirection.Row;
            entriesUI.style.justifyContent = Justify.SpaceBetween;
            entriesUI.style.height = 20.0f;
            entriesUI.Add(new Label("Metadata Entries") { tooltip = Tooltips.dataListField });
            entriesUI.Add(buttons);
            Add(entriesUI);

            var columns = new Columns();
            columns.Add(new Column { name = "Key", title = "Key", width = 100.0f, makeCell = MakeTextField, bindCell = BindKeyElement });
            columns.Add(new Column { name = "Value", title = "Value", width = 150.0f, makeCell = MakeTextField, bindCell = BindValueElement });
            columns.Add(new Column { name = "Type", title = "Type", width = 60.0f, makeCell = () => new DropdownField { choices = k_AvailableChunkNames.ToList() }, bindCell = BindTypeElement });
            m_ListView = new MultiColumnListView(columns);
            m_ListView.style.flexGrow = 1;
            m_ListView.style.overflow = Overflow.Hidden;
            m_ListView.selectionChanged += OnSelectionChanged;
            Add(m_ListView);

            var buttonsHolder = new VisualElement();
            buttonsHolder.style.flexShrink = 0;
            buttonsHolder.style.justifyContent = Justify.FlexEnd;
            buttonsHolder.style.height = 30.0f;
            buttonsHolder.style.flexDirection = FlexDirection.Row;
            m_ReadButton = new Button(OnRead) { text = "Read", tooltip = Tooltips.readButton };
            buttonsHolder.Add(m_ReadButton);
            m_WriteButton = new Button(OnWrite) { text = "Write", tooltip = Tooltips.writeButton };
            buttonsHolder.Add(m_WriteButton);
            Add(buttonsHolder);

            OnTextureUpdated();
        }

        void OnSelectionChanged(IEnumerable<object> _)
        {
            var selectedIndex = m_ListView.selectedIndex;
            var validSelection = selectedIndex >= 0 && selectedIndex < m_Metadata.Count;
            m_RemoveButton.style.display = validSelection ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnRemoveEntry()
        {
            var selectedIndex = m_ListView.selectedIndex;
            var validSelection = selectedIndex >= 0 && selectedIndex < m_Metadata.Count;
            if (!validSelection)
                return;

            m_Metadata.RemoveAt(selectedIndex);
            m_ListView.itemsSource = m_Metadata;
            m_ListView.Rebuild();

            m_ListView.SetSelection(new[] { Math.Min(selectedIndex, m_Metadata.Count - 1) });
        }

        void OnAddEntry()
        {
            m_Metadata.Add(new MetadataEntry { key = FindUniqueKeyName("New Key", PngChunkType.tEXt), value = "", type = PngChunkType.tEXt });
            m_ListView.itemsSource = m_Metadata;
            m_ListView.Rebuild();

            m_ListView.SetSelection(new[] { m_Metadata.Count - 1 });
        }

        void OnSetTexture(ChangeEvent<Object> evt)
        {
            m_SelectedTexture = evt.newValue != null ? (Texture2D)evt.newValue : null;
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m_SelectedTexture)))
                m_SelectedTexture = null;

            onSelectedTexture?.Invoke(m_SelectedTexture);

            OnTextureUpdated();

            OnRead();
        }

        void OnTextureUpdated()
        {
            var enabled = m_SelectedTexture != null;
            m_AddButton.SetEnabled(enabled);
            m_AddButton.tooltip = enabled ? Tooltips.addButton : Tooltips.buttonInactive;
            m_ReadButton.SetEnabled(enabled);
            m_ReadButton.tooltip = enabled ? Tooltips.readButton : Tooltips.buttonInactive;
            m_WriteButton.SetEnabled(enabled);
            m_WriteButton.tooltip = enabled ? Tooltips.writeButton : Tooltips.buttonInactive;
        }

        void OnRead()
        {
            m_Metadata.Clear();
            if (m_SelectedTexture != null)
            {
                var data = File.ReadAllBytes(AssetDatabase.GetAssetPath(m_SelectedTexture));
                var metadataEncoder = new PngMetadataEncoder(data);

                foreach (var (key, value) in metadataEncoder.data.GetEntries(PngChunkType.tEXt))
                    m_Metadata.Add(new MetadataEntry { key = key, value = value, type = PngChunkType.tEXt });
                foreach (var (key, value) in metadataEncoder.data.GetEntries(PngChunkType.iTXt))
                    m_Metadata.Add(new MetadataEntry { key = key, value = value, type = PngChunkType.iTXt });
                foreach (var (key, value) in metadataEncoder.data.GetEntries(PngChunkType.eXIf))
                    m_Metadata.Add(new MetadataEntry { key = key, value = value, type = PngChunkType.eXIf });
            }

            m_ListView.itemsSource = m_Metadata;
            m_ListView.Rebuild();
        }

        void OnWrite()
        {
            if (m_SelectedTexture == null)
                return;

            var path = AssetDatabase.GetAssetPath(m_SelectedTexture);
            path = EditorUtility.SaveFilePanel("Save texture", Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), Path.GetExtension(path).Replace(".", string.Empty));
            if (string.IsNullOrEmpty(path))
                return;

            var tEXt = new Dictionary<string, string>();
            var iTXt = new Dictionary<string, string>();
            var eXIf = new Dictionary<string, string>();
            for (var i = 0; i < m_Metadata.Count; i++)
            {
                var entry = m_Metadata[i];
                if (string.IsNullOrEmpty(entry.key) || entry.key.Length <= 1)
                {
                    Debug.Log($"Skipping '{entry.key}' entry ({i}) because the key is null, empty, or too short");
                    continue;
                }

                if (entry.key.Length > 79)
                {
                    Debug.Log($"Skipping '{entry.key}' entry ({i}) because the key is too long ( > 79).");
                    continue;
                }

                switch (entry.type)
                {
                    case PngChunkType.tEXt:
                        if (!tEXt.TryAdd(entry.key, entry.value))
                            Debug.Log($"Skipping {entry.key} entry ({i}). Make sure that key values are not duplicated.");
                        break;
                    case PngChunkType.iTXt:
                        if (!iTXt.TryAdd(entry.key, entry.value))
                            Debug.Log($"Skipping {entry.key} entry ({i}). Make sure that key values are not duplicated.");
                        break;
                    case PngChunkType.eXIf:
                        if(!eXIf.TryAdd(entry.key, entry.value))
                            Debug.Log($"Skipping {entry.key} entry ({i}). Make sure that key values are not duplicated.");
                        break;
                }
            }

            var metadata = new PngMetadata();
            metadata.SetEntries(tEXt, PngChunkType.tEXt);
            metadata.SetEntries(iTXt, PngChunkType.iTXt);
            metadata.SetEntries(eXIf, PngChunkType.eXIf);

            var pngData = File.ReadAllBytes(AssetDatabase.GetAssetPath(m_SelectedTexture));
            var metadataEncoder = new PngMetadataEncoder(pngData);
            metadataEncoder.SetMetadata(metadata);

            var dataWithMetadata = metadataEncoder.GetData();
            File.WriteAllBytes(path, dataWithMetadata);

            AssetDatabase.Refresh();
        }

        static TextField MakeTextField()
        {
            var textField = new TextField();
            textField.style.whiteSpace = WhiteSpace.Normal;
            return textField;
        }

        void BindKeyElement(VisualElement visualElement, int i)
        {
            if (m_Metadata == null || i < 0 || i >= m_Metadata.Count)
                return;

            if (visualElement is TextField textField)
            {
                var metadata = m_Metadata[i];
                textField.value = textField.tooltip = metadata.key;
                textField.RegisterValueChangedCallback(evt => m_Metadata[i].key = evt.newValue);
            }
        }

        void BindValueElement(VisualElement visualElement, int i)
        {
            if (m_Metadata == null || i < 0 || i >= m_Metadata.Count)
                return;

            if (visualElement is TextField textField)
            {
                var metadata = m_Metadata[i];
                textField.value = textField.tooltip = metadata.value;
                textField.RegisterValueChangedCallback(evt => m_Metadata[i].value = evt.newValue);
            }
        }

        void BindTypeElement(VisualElement visualElement, int i)
        {
            if (m_Metadata == null || i < 0 || i >= m_Metadata.Count)
                return;

            if (visualElement is DropdownField dropdownField)
            {
                var metadata = m_Metadata[i];
                var typeName = metadata.type.ToString();
                dropdownField.tooltip = typeName;
                dropdownField.index = k_AvailableChunkNames.IndexOf(typeName);
                dropdownField.RegisterValueChangedCallback(_ => m_Metadata[i].type = k_AvailableChunkTypes[dropdownField.index]);
            }
        }

        string FindUniqueKeyName(string preferredName, PngChunkType type)
        {
            var metadata = m_Metadata.Where(e => e.type == type).Select(i => i.key);
            if (metadata.All(key => key != preferredName))
                return preferredName;

            var highestCount = 0;
            while (metadata.Any(key => key == $"{preferredName} {highestCount}"))
                highestCount++;
            return $"{preferredName} {highestCount}";
        }
    }
}
