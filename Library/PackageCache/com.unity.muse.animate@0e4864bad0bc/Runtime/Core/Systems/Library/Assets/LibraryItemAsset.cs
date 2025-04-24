using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Asset holding the persistent data of items that can be authored and browsed from the application library.
    /// </summary>
    [Serializable]
    class LibraryItemAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        public event Action<LibraryItemAsset, LibraryItemModel.Property> OnSubAssetPropertyChanged;
        public event Action<LibraryItemAsset, string> OnTooltipChanged;

        const int k_CurrentVersion = 1;

        [SerializeReference, HideInInspector]
        LibraryItemAssetData m_Data;

        [SerializeReference, HideInInspector]
        LibraryItemAssetPreview m_Preview;

        public LibraryItemAssetData Data => m_Data;
        public LibraryItemAssetPreview Preview => m_Preview;

        [SerializeField, HideInInspector]
        int m_Version;

        [SerializeField, HideInInspector]
        LibraryItemType m_ItemType;

        [SerializeField, HideInInspector]
        string m_Title;

        [SerializeField, HideInInspector]
        string m_Description;

        [SerializeField, HideInInspector]
        string m_Path;
        public ThumbnailModel Thumbnail => m_Preview == null ? null : Preview.Thumbnail;

        [SerializeField, HideInInspector]
        string m_AssetName;

        [SerializeField, HideInInspector]
        public StageModel StageModel;

        [SerializeField, HideInInspector]
        string m_Tooltip = "";

        [NonSerialized]
        bool m_IsSubscribedToPreview;

        [NonSerialized]
        bool m_IsSubscribedToData;

        public int Version => m_Version;

        public string Tooltip
        {
            get => m_Tooltip;
            set
            {
                if (m_Tooltip.Equals(value))
                    return;

                m_Tooltip = value;
                OnTooltipChanged?.Invoke(this, m_Tooltip);
            }
        }

        public bool IsPreviewable => m_Data.Model.IsPreviewable;

        public LibraryItemType ItemType => m_ItemType;

        public string Title
        {
            get => m_Title;
            internal set => m_Title = value;
        }

        public string Description => m_Description;

        public string Path
        {
            get => m_Path;
            internal set => m_Path = value;
        }

        public string AssetName
        {
            get => m_AssetName;
            internal set => m_AssetName = value;
        }

        internal void SetupNewAsset<TModel>(string path, TModel item, StageModel stageModel, LibraryItemAssetPreview preview, LibraryItemAssetData data) where TModel : LibraryItemModel
        {
            Log("SetupNewAsset()");
            m_Version = k_CurrentVersion;
            m_Preview = preview;
            m_Data = data;
            StageModel = new StageModel(stageModel, $"Item Stage ({data.Model.Title})");
            m_ItemType = item.ItemType;
            m_Path = path;
            m_Title = item.Title;
            m_Description = item.Description;
            RefreshTooltip();
            SubscribeToModels();
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            Log("OnAfterDeserialize()");
            SubscribeToModels();
        }

        void SubscribeToModels()
        {
            Log("SubscribeToModels()");

            if (!m_IsSubscribedToPreview)
            {
                if (m_Preview != null)
                {
                    m_Preview.OnThumbnailChanged += OnPreviewThumbnailChangedInternal;
                    m_IsSubscribedToPreview = true;
                    Log("SubscribeToModels() -> Subscribed to Preview.");
                }
                else
                {
                    Log("SubscribeToModels() -> Failed, Preview was null.");
                }
            }
            else
            {
                Log("SubscribeToModels() -> Failed, was already subscribed to Preview.");
            }

            if (!m_IsSubscribedToData)
            {
                if (m_Data != null)
                {
                    m_Data.OnModelChanged += OnDataModelChangedInternal;
                    m_IsSubscribedToData = true;
                    Log("SubscribeToModels() -> Subscribed to Data.");
                }
                else
                {
                    Log("SubscribeToModels() -> Failed, Data was null.");
                }
            }
            else
            {
                Log("SubscribeToModels() -> Failed, was already subscribed to Data.");
            }
        }

        public void OnValidate()
        {
            Log("OnValidate()");
            SubscribeToModels();
        }

        void OnPreviewThumbnailChangedInternal(ThumbnailModel thumbnail)
        {
            OnSubAssetPropertyChanged?.Invoke(this, LibraryItemModel.Property.Thumbnail);
        }

        void OnDataModelChangedInternal(LibraryItemAssetData data, LibraryItemModel model, LibraryItemModel.Property property)
        {
            var needToRefreshTooltip = false;
            var needToApplyTitle = false;

            switch (property)
            {
                case LibraryItemModel.Property.ItemType:
                    break;
                case LibraryItemModel.Property.IsValid:
                    break;
                case LibraryItemModel.Property.IsEditable:
                    needToRefreshTooltip = true;
                    break;
                case LibraryItemModel.Property.IsBaking:
                    needToRefreshTooltip = true;
                    break;
                case LibraryItemModel.Property.Title:
                    needToApplyTitle = true;
                    break;
                case LibraryItemModel.Property.Description:
                    m_Description = model.Description;
                    needToRefreshTooltip = true;
                    break;
                case LibraryItemModel.Property.Progress:
                    needToRefreshTooltip = true;
                    break;
                case LibraryItemModel.Property.OutputAnimationData:
                case LibraryItemModel.Property.Thumbnail:
                case LibraryItemModel.Property.InputAnimationData:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(property), property, null);
            }

            if (needToApplyTitle)
            {
                // Note: We skip RefreshTooltip() here because ApplyTitleToPath() will trigger
                // RefreshTooltip() anyway later down the line, after the asset gets renamed.
                LibraryRegistry.QueueProcessItem(this);
            }
            else if (needToRefreshTooltip)
            {
                RefreshTooltip();
            }

            OnSubAssetPropertyChanged?.Invoke(this, property);
        }

        internal void RefreshTooltip()
        {
            var tooltip = Data.Model.GetTooltipLabel();

            Log($"RefreshTooltip({tooltip})");

            if (Data == null)
            {
                LogError("RefreshTooltip() -> Error: Data is null");
                return;
            }

            Tooltip = tooltip;
        }

        internal void RequestThumbnailUpdate(ThumbnailsService contextThumbnailsService, CameraModel contextCamera)
        {
            if (Data == null)
            {
                LogError("RequestThumbnailUpdate() -> Error: Data is null");
                return;
            }

            if (Data.Model == null)
            {
                LogError("RequestThumbnailUpdate() -> Error: Data.Model is null");
                return;
            }

            Data.Model.RequestThumbnailUpdate(Preview.Thumbnail, contextThumbnailsService, contextCamera);
        }

        internal string GetSearchLabel()
        {
            return $"{Title.ToLower()} {Description.ToLower()}";
        }

        public void Save(StageModel stage)
        {
            Log("Save()");

            stage.CopyTo(StageModel);
            m_Preview.Save();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        // [Section] Debugging

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        void Log(string msg)
        {
            DevLogger.LogInfo($"{GetType().Name} -> {msg}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        void LogError(string msg)
        {
            DevLogger.LogError($"{GetType().Name} -> {msg}");
        }
    }
}
