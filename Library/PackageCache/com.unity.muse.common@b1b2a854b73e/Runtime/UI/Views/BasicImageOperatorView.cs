using System;
using System.IO;
using System.Linq;
using Unity.AppUI.Core;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    class BasicImageOperatorView : ExVisualElement
    {
        const string k_ArtifactMimeType = "artifact/guid;";
        const string k_ImageMimeType = "image/png;base64,";
        
        internal event Action dataChanged;
        
        internal event Action closeButtonClicked;
        
        Image m_PreviewImage;

        ActionButton m_CloseButton;

        VisualElement m_DropZoneHelper;
            
        VisualElement m_DropZone;

        VisualElement m_DropzoneContextMenuAnchor;
        
        Text m_DropzoneMessage;
        
        Texture2DDropManipulator m_DropManipulator;
        
        ActionButton m_ClearButton;
        
        readonly Model m_Model;

        string m_Guid;
        internal string GetGuid()
        {
            return m_Guid;
        }
        
        Texture2D m_Image;
        internal Texture2D GetImage()
        {
            return m_Image;
        }

        static string actionKeyLabel =>
            Application.platform is RuntimePlatform.OSXEditor or
                RuntimePlatform.OSXPlayer or
                RuntimePlatform.OSXServer ? "Cmd" : "Ctrl";

        public BasicImageOperatorView(Model model)
        {
            m_Model = model;

            CreateGUI();

            SetImage(null);
        }
        
        void CreateGUI()
        {
            passMask = Passes.Clear | Passes.OutsetShadows | Passes.BackgroundColor;

            AddToClassList("muse-node");
            AddToClassList("appui-elevation-8");
            name = "input-image-node";
            
            var titleRow = new VisualElement();
            titleRow.AddToClassList("row");
            titleRow.AddToClassList("bottom-gap");
            Add(titleRow);
            
            var text = new Text();
            text.text = "Input Image";
            text.AddToClassList("muse-node__title");
            titleRow.Add(text);
            
            m_CloseButton = new ActionButton();
            m_CloseButton.icon = "x";
            m_CloseButton.quiet = true;
            m_CloseButton.clicked += () => closeButtonClicked?.Invoke();
            m_CloseButton.AddToClassList("muse-node__close-button");
            titleRow.Add(m_CloseButton);

            var row = new VisualElement();
            row.AddToClassList("row");
            row.AddToClassList("bottom-gap");
            Add(row);

            var spacer = new VisualElement();
            spacer.AddToClassList("muse-spacer"); 
            row.Add(spacer);

            m_ClearButton = new ActionButton();
            m_ClearButton.icon = "delete";
            m_ClearButton.clicked += OnClearButtonClicked;
            row.Add(m_ClearButton);

            m_DropZone = new VisualElement
            {
                pickingMode = PickingMode.Position,
                focusable = true,
            };
            m_DropZone.AddToClassList("muse-dropzone");
            m_DropZone.AddToClassList("bottom-gap");
            m_DropZone.name = "muse-dropzone";
            Add(m_DropZone);
            m_DropZone.RegisterCallback<GeometryChangedEvent>(ResizeDropZone);

            m_DropzoneContextMenuAnchor = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    width = 0,
                    height = 0,
                }
            };
            m_DropZone.Add(m_DropzoneContextMenuAnchor);
            m_DropZone.RegisterCallback<PointerDownEvent>(OnDropZonePointerDown);
            m_DropZone.RegisterCallback<KeyDownEvent>(OnDropZoneKeyDown);
            m_DropManipulator = new Texture2DDropManipulator(m_Model);
            m_DropManipulator.onDragStart += OnDragStart;
            m_DropManipulator.onDragEnd += OnDragEnd;
            m_DropManipulator.onDrop += OnDrop;
            m_DropZone.AddManipulator(m_DropManipulator);

            m_PreviewImage = new Image { pickingMode = PickingMode.Ignore };
            m_PreviewImage.AddToClassList("muse-dropzone__image");
            m_DropZone.Add(m_PreviewImage);

            m_DropZoneHelper = new VisualElement { pickingMode = PickingMode.Ignore };
            m_DropZoneHelper.AddToClassList("muse-dropzone__helper");
            m_DropZone.Add(m_DropZoneHelper);

            m_DropzoneMessage = new Text { pickingMode = PickingMode.Position, enableRichText = true };
            m_DropzoneMessage.AddToClassList("muse-dropzone__message");
            m_DropzoneMessage.AddToClassList("bottom-gap");
            m_DropZoneHelper.Add(m_DropzoneMessage);

            var dropzoneButton = new ActionButton { label = TextContent.import };
            dropzoneButton.AddToClassList("muse-dropzone__button");
            m_DropZoneHelper.Add(dropzoneButton);
            dropzoneButton.clicked += OnImportButtonClicked;
        }
        

        void OnDropZonePointerDown(PointerDownEvent evt)
        {
            if (evt.button == 1)
            {
                evt.StopImmediatePropagation();
#if !UNITY_2023_2_OR_NEWER
                evt.PreventDefault();
#endif

                m_DropzoneContextMenuAnchor.style.left = evt.localPosition.x;
                m_DropzoneContextMenuAnchor.style.top = evt.localPosition.y;

                if (!m_PreviewImage.image)
                    return;

                var contextMenu = new Menu
                {
                    style =
                    {
                        minWidth = 128
                    }
                };

                var copyAction = new MenuItem
                {
                    label = TextContent.copy,
                    shortcut = $"{actionKeyLabel}+C"
                };
                copyAction.clickable.clicked += CopyImageToClipboard;
                contextMenu.Add(copyAction);

                var pasteAction = new MenuItem
                {
                    label = TextContent.paste,
                    shortcut = $"{actionKeyLabel}+V"
                };
                pasteAction.clickable.clicked += PasteImageFromClipboard;
                contextMenu.Add(pasteAction);

                var menu = MenuBuilder.Build(m_DropzoneContextMenuAnchor, contextMenu);
                menu.dismissed += (builder, type) => m_DropZone.RemoveFromClassList(Styles.focusedUssClassName);
                menu.Show();

                m_DropZone.AddToClassList(Styles.focusedUssClassName);
            }
        }
        
        
        void OnDropZoneKeyDown(KeyDownEvent evt)
        {
            if (evt.actionKey)
            {
                if (evt.keyCode == KeyCode.C)
                {
                    evt.StopImmediatePropagation();
#if !UNITY_2023_2_OR_NEWER
                    evt.PreventDefault();
#endif


                    if (m_PreviewImage.image)
                        CopyImageToClipboard();
                }
                else if (evt.keyCode == KeyCode.V)
                {
                    evt.StopImmediatePropagation();
#if !UNITY_2023_2_OR_NEWER
                    evt.PreventDefault();
#endif


                    PasteImageFromClipboard();
                }
            }
        }
        
        
        void PasteImageFromClipboard()
        {
            var buffer = GUIUtility.systemCopyBuffer;

            if (string.IsNullOrEmpty(buffer))
                return;

            if (buffer.StartsWith(k_ArtifactMimeType))
            {
                var guid = buffer.Substring(k_ArtifactMimeType.Length);
                var artifact = m_Model.AssetsData.FirstOrDefault(a => a.Guid == guid);

                if (artifact is not null && ArtifactCache.IsInCache(artifact))
                {
                    var cachedObj = ArtifactCache.Read(artifact);
                    if (cachedObj is Texture2D img)
                    {
                        SetGuid(guid);
                        SetImage(img);
                        dataChanged?.Invoke();
                    }
                }
            }
            else if (buffer.StartsWith(k_ImageMimeType))
            {
                var b64String = buffer.Substring(k_ImageMimeType.Length);
                var bytes = Convert.FromBase64String(b64String);
                var img = new Texture2D(2, 2);
                img.LoadImage(bytes);
                SetGuid(null);
                SetImage(img);
                dataChanged?.Invoke();
            }
            else
            {
                Debug.Log(buffer);
            }
        }
        
        void CopyImageToClipboard()
        {
            var img = (Texture2D)m_PreviewImage.image;

            if (!img && string.IsNullOrEmpty(m_Guid))
                return;

            GUIUtility.systemCopyBuffer = string.IsNullOrEmpty(m_Guid) ?
                $"{k_ImageMimeType}{Convert.ToBase64String(img.EncodeToPNG())}" : $"{k_ArtifactMimeType}{m_Guid}";

            Toast
                .Build(this, TextContent.inputImageCopiedToClipboard, NotificationDuration.Short)
                .Show();
        }
        
        void OnDrop(Texture2D obj)
        {
            SetGuid(m_DropManipulator.artifact?.Guid);
            SetImage(obj);
            dataChanged?.Invoke();
        }

        void OnDragEnd()
        {
            m_DropZone.RemoveFromClassList("accept-drag");
        }

        void OnDragStart()
        {
            m_DropZone.AddToClassList("accept-drag");
        }
        
        void OnImportButtonClicked()
        {
#if UNITY_EDITOR
            string lastFolderPath = Preferences.lastImportFolderPath;
            if (!Directory.Exists(lastFolderPath))
                lastFolderPath = Preferences.defaultImportFolderPath;

            var path = UnityEditor.EditorUtility.OpenFilePanelWithFilters(
                TextContent.importImages,
                lastFolderPath,
                new[]
                {
                    "Image",
                    "png,jpg,jpeg"
                });
            if (string.IsNullOrEmpty(path))
                return;

            Preferences.lastImportFolderPath = Path.GetDirectoryName(path);
            var img = new Texture2D(2, 2);
            img.LoadImage(System.IO.File.ReadAllBytes(path));
            SetGuid(null);
            SetImage(img);
            dataChanged?.Invoke();
#else
            Debug.LogError("Importing images is not supported in builds");
#endif
        }

        void OnClearButtonClicked()
        {
            SetGuid(null);
            SetImage(null);
            
            dataChanged?.Invoke();
        }

        internal void SetGuid(string guid)
        {
            m_Guid = guid;
        }

        internal void SetImage(Texture2D img)
        {
            if (!Validate(img))
                return;

            m_Image = img;

            RefreshPreview();
        }
        
        internal void SetCloseButtonVisibility(bool visible)
        {
            m_CloseButton.SetEnabled(visible);
        }

        static bool Validate(Texture2D img)
        {
            if (img && !img.isReadable)
            {
                Debug.LogError("<b>[Muse]</b> Input image must be readable, please enable read/write in the import settings");
                return false;
            }

            if (img && IsTextureCompressed(img))
            {
                Debug.LogError($"<b>[Muse]</b> Input image must be not be compressed. Please remove compression from the import settings.");
                return false;
            }

            return true;
        }

        static bool IsTextureCompressed(Texture2D texture)
        {
            var format = texture.format;

            switch (format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ASTC_4x4:
                    return true;
                default:
                    return false;
            }
        }

        void RefreshPreview()
        {
            m_PreviewImage.image = GetImage();
            m_DropzoneMessage.text = TextContent.dragAndDropColorImageMessage;
            m_DropZoneHelper.EnableInClassList(Styles.hiddenUssClassName, m_PreviewImage.image);
            m_ClearButton.SetEnabled(m_PreviewImage.image);
        }

        void ResizeDropZone(GeometryChangedEvent evt)
        {
            var dropZone = (VisualElement)evt.target;
            var size = dropZone.resolvedStyle.width;

            if (!Mathf.Approximately(dropZone.resolvedStyle.height, size))
                dropZone.style.height = size;
        }
    }
}