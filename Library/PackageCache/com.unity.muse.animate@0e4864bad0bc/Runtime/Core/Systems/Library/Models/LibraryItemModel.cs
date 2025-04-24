using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class LibraryItemModel : ICopyable<LibraryItemModel>, ISerializationCallbackReceiver
    {
        public static readonly string[] TypeIcons = { "", "recorder", "recorder", "", "" };

        public enum Property
        {
            ItemType,
            IsValid,
            IsEditable,
            IsBaking,
            Title,
            Description,
            Progress,
            Thumbnail,
            OutputAnimationData,
            InputAnimationData
        }
        
        [SerializeField]
        LibraryItemType m_ItemType;

        [SerializeField]
        bool m_IsValid;
        
        [SerializeField]
        bool m_IsEditable;
        
        [SerializeField]
        bool m_IsBaking;
        
        [SerializeField]
        string m_Title;
        
        [SerializeField]
        string m_Description;

        [SerializeField]
        float m_Progress;

        public event Action<Property> OnChanged;

        public LibraryItemType ItemType
        {
            get => m_ItemType;
            set
            {
                if (!m_ItemType.Equals(value))
                {
                    m_ItemType = value;
                    InvokeChanged(Property.ItemType);
                }
            }
        }
        
        public string Title
        {
            get => m_Title;
            set
            {
                if (!m_Title.Equals(value) && !string.IsNullOrEmpty(value))
                {
                    m_Title = value;
                    InvokeChanged(Property.Title);
                }
            }
        }
        
        public float Progress
        {
            get => m_Progress;
            set
            {
                if (!Mathf.Approximately(m_Progress, value))
                {
                    m_Progress = value;
                    InvokeChanged(Property.Progress);
                }
            }
        }
        
        public bool IsValid
        {
            get => m_IsValid;
            set
            {
                if (m_IsValid != value)
                {
                    m_IsValid = value;
                    InvokeChanged(Property.IsValid);
                }
            }
        }

        public bool IsEditable
        {
            get => m_IsEditable;
            set
            {
                if (m_IsEditable != value)
                {
                    m_IsEditable = value;
                    InvokeChanged(Property.IsEditable);
                }
            }
        }

        public bool IsBaking
        {
            get => m_IsBaking;
            set
            {
                if (m_IsBaking != value)
                {
                    m_IsBaking = value;
                    InvokeChanged(Property.IsBaking);
                }
            }
        }
        
        public string Description
        {
            get => m_Description;
            set
            {
                if (!m_Description.Equals(value))
                {
                    m_Description = value;
                    InvokeChanged(Property.Description);
                }
            }
        }

        /// <summary>
        /// Whether the item can be "opened" from the library.
        /// </summary>
        /// <remarks>
        /// Usually this is false if the item is currently being generated.
        /// </remarks>
        public virtual bool IsPreviewable => true;

        /// <summary>
        /// Whether the item has sufficient data to generate a thumbnail.
        /// </summary>
        /// <remarks>
        /// For example, a take requires a baked timeline to generate a thumbnail.
        /// </remarks>
        public virtual bool CanGenerateThumbnail => false;
        
        internal LibraryItemModel(string title, string description, LibraryItemType itemType, bool hasToBake)
        {
            m_ItemType = itemType;
            m_Title = title;
            m_Description = description;
            m_IsBaking = false;
            m_Progress = hasToBake ? 1f : 0f;
        }

        public void OnAfterDeserialize()
        {
            
        }

        public void OnBeforeSerialize()
        {
            
        }
        
        public virtual void RequestThumbnailUpdate(ThumbnailModel target, ThumbnailsService thumbnailsService, CameraModel cameraModel)
        {
            // Note: All Inheritors of LibraryItemModel should override RequestThumbnailUpdate()
            LogError("RequestThumbnailUpdate() -> ERROR: All Inheritors of LibraryItemModel should override RequestThumbnailUpdate()");
            throw new NotImplementedException();
        }

        public virtual string GetSearchLabel()
        {
            return Title.ToLower();
        }
        
        public virtual string GetTooltipLabel()
        {
            return $"<b>{Title}</b>\n{Description}";
        }

        void OnThumbnailChanged()
        {
            InvokeChanged(Property.Thumbnail);
        }

        protected void InvokeChanged(Property property)
        {
            OnChanged?.Invoke(property);
        }

        protected void CopyTo(LibraryItemModel item)
        {
            item.m_IsValid = m_IsValid;
            item.m_IsBaking = m_IsBaking;
            item.m_Progress = m_Progress;
            item.m_IsEditable = m_IsEditable;
            item.m_Title = m_Title;
            item.m_Description = m_Description;
        }

        public LibraryItemModel Clone()
        {
            var clone = new LibraryItemModel(Title, Description, ItemType, false);
            CopyTo(clone);
            return clone;
        }

        public static Type GetType(LibraryItemType type)
        {
            switch (type)
            {
                case LibraryItemType.TextToMotionTake:
                    return typeof(TextToMotionTake);
                case LibraryItemType.VideoToMotionTake:
                    return typeof(VideoToMotionTake);
                case LibraryItemType.KeySequenceTake:
                    return typeof(KeySequenceTake);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
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

        void ICopyable<LibraryItemModel>.CopyTo(LibraryItemModel target)
        {
            CopyTo(target);
        }
    }
    
    public enum LibraryItemType
    {
        TextToMotionTake,
        VideoToMotionTake,
        KeySequenceTake,
        Pose
    }
}
