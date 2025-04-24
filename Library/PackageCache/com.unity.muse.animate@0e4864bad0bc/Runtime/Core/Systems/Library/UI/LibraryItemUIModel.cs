using System;
using System.Diagnostics;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class LibraryItemUIModel
    {
        // Events for transmitting commands to the library model
        public event Action<LibraryItemUIModel> OnLeftClickedSingle;
        public event Action<LibraryItemUIModel> OnRequestEdit;
        public event Action<LibraryItemUIModel> OnRightClicked;
        public event Action<LibraryItemUIModel> OnDragStarted;
        public event Action<LibraryItemUIModel> OnContextMenuRequested;
        public event Action<RenderTexture, LibraryItemAsset> OnPlayThumbnail;
        public event Action<RenderTexture> OnStopThumbnail;
        
        // Events for views to update the UI
        public event Action<LibraryItemUIModel> OnChanged;
        public event Action<LibraryItemUIModel, ItemState> OnStateChanged;
        public event Action OnThumbnailChanged;

        [Flags]
        public enum ItemState
        {
            Visible = 1,
            Selected = 2,
            Highlighted = 4,
            EditingName = 8,
            InContextualMenu = 16,
            PreviewAble = 32,
        }

        public LibraryItemAsset Target => m_Target;

        public Texture Thumbnail
        {
            get
            {
                ThrowIfThumbnailIsNull();
                if (m_Target == null || m_Target.Thumbnail == null)
                    return null;

                if (m_PlayingAnimatedThumbnail)
                    return m_AnimatedThumbnail;

                return m_Target.Thumbnail.Texture;
            }
        }
        
        [Conditional("UNITY_MUSE_DEV")]
        void ThrowIfThumbnailIsNull()
        {
            if (m_Target == null)
                throw new NullReferenceException("LibraryItemUIModel: Target is null");
            if (m_Target.Preview == null)
                throw new NullReferenceException("LibraryItemUIModel: Preview asset is null");
            if (m_Target.Preview.Thumbnail == null)
                throw new NullReferenceException("LibraryItemUIModel: Thumbnail is null");
        }

        public bool IsVisible
        {
            get => m_State.HasFlag(ItemState.Visible);
            set => SetState(ItemState.Visible, value);
        }

        public bool IsSelected
        {
            get => m_State.HasFlag(ItemState.Selected);
            set => SetState(ItemState.Selected, value);
        }

        public bool IsHighlighted
        {
            get => m_State.HasFlag(ItemState.Highlighted);
            set => SetState(ItemState.Highlighted, value);
        }

        public bool IsInContextualMenu
        {
            get => m_State.HasFlag(ItemState.InContextualMenu);
            set => SetState(ItemState.InContextualMenu, value);
        }

        public bool IsEditingName
        {
            get => m_State.HasFlag(ItemState.EditingName);
            set => SetState(ItemState.EditingName, value);
        }

        public bool IsEditable => Target.Data.Model.IsEditable;

        public string Tooltip
        {
            get
            {
                return m_Tooltip;
            }
            set
            {
                if (m_Tooltip.Equals(value))
                    return;

                m_Tooltip = value;
                OnChanged?.Invoke(this);
            }
        }

        public Vector2Int AnimatedThumbnailSize
        {
            get => m_AnimatedThumbnailSize;
            set
            {
                if (value == m_AnimatedThumbnailSize)
                    return;

                m_AnimatedThumbnailSize = value;
                EnsureValidThumbnailTexture();

                if (m_PlayingAnimatedThumbnail)
                    InvokeThumbnailChanged();
            }
        }

        public LibraryItemModel Model => Target.Data.Model;

        public float Progress
        {
            get
            {
                if (Model is TakeModel takeModel)
                {
                    if (takeModel.IsBaking)
                    {
                        return takeModel.Progress;
                    }

                    // Show a progress bar if we can't display a thumbnail
                    if (!takeModel.CanGenerateThumbnail && m_PlayingAnimatedThumbnail)
                    {
                        return 0f;
                    }
                }

                return 1f;
            }
        }

        LibraryItemAsset m_Target;
        ItemState m_State = ItemState.Visible;
        RenderTexture m_AnimatedThumbnail;
        Vector2Int m_AnimatedThumbnailSize;
        bool m_PlayingAnimatedThumbnail;

        string m_Tooltip = "";

        void EnsureValidThumbnailTexture()
        {
            var isAllocated = m_AnimatedThumbnail != null;
            var shouldReallocate = !isAllocated ||
                m_AnimatedThumbnail.width != m_AnimatedThumbnailSize.x ||
                m_AnimatedThumbnail.height != m_AnimatedThumbnailSize.y;

            if (shouldReallocate)
            {
                if (isAllocated)
                {
                    m_AnimatedThumbnail.Release();
                }

                m_AnimatedThumbnail = new RenderTexture(m_AnimatedThumbnailSize.x, m_AnimatedThumbnailSize.y, 24);
            }
        }

        void SetState(ItemState state, bool value)
        {
            if (value == m_State.HasFlag(state))
                return;

            if (value)
            {
                m_State |= state;
            }
            else
            {
                m_State &= ~state;
            }

            OnStateChanged?.Invoke(this, state);
        }

        internal void SetTarget(LibraryItemAsset value)
        {
            if (m_Target != null)
                UnregisterCallbacks();

            m_Target = value;

            if (m_Target != null)
            {
                RegisterCallbacks();
                InvokeThumbnailChanged();
            }

            InvokeChanged();
        }

        void RegisterCallbacks()
        {
            Tooltip = m_Target.Tooltip;

            // Override to register/unregister to the item's specific models.
            // See SequenceKeyViewModel.cs for an example
            m_Target.OnSubAssetPropertyChanged += OnTargetPropertyChanged;
            m_Target.OnTooltipChanged += OnTargetTooltipChanged;
            m_Target.Preview.Thumbnail.OnChanged += OnThumbnailModelChanged;
        }

        void OnTargetTooltipChanged(LibraryItemAsset item, string tooltip)
        {
            Tooltip = m_Target.Tooltip;
        }

        void UnregisterCallbacks()
        {
            // Override to register/unregister to the item's specific models.
            // See SequenceKeyViewModel.cs for an example
            m_Target.OnSubAssetPropertyChanged -= OnTargetPropertyChanged;
            m_Target.OnTooltipChanged -= OnTargetTooltipChanged;
            m_Target.Preview.Thumbnail.OnChanged -= OnThumbnailModelChanged;
        }

        public void RightClicked()
        {
            OnRightClicked?.Invoke(this);
        }

        public void LeftClickedSingle()
        {
            OnLeftClickedSingle?.Invoke(this);
        }

        public void LeftClickedDouble()
        {
            RequestEdit();
        }

        public void DragStarted()
        {
            OnDragStarted?.Invoke(this);
        }

        public void PlayAnimatedThumbnail()
        {
            EnsureValidThumbnailTexture();
            OnPlayThumbnail?.Invoke(m_AnimatedThumbnail, Target);

            // The thumbnail is updated when a new frame is rendered for the animated thumbnail.
            m_PlayingAnimatedThumbnail = true;
            InvokeThumbnailChanged();
        }

        public void StopAnimatedThumbnail()
        {
            OnStopThumbnail?.Invoke(m_AnimatedThumbnail);
            m_PlayingAnimatedThumbnail = false;
            InvokeThumbnailChanged();
        }

        void InvokeChanged()
        {
            OnChanged?.Invoke(this);
        }

        void OnThumbnailModelChanged()
        {
            if (m_PlayingAnimatedThumbnail)
            {
                PlayAnimatedThumbnail();
            }
            else
            {
                InvokeThumbnailChanged();
            }
        }

        void InvokeThumbnailChanged()
        {
            OnThumbnailChanged?.Invoke();
        }

        void OnTargetPropertyChanged(LibraryItemAsset asset, LibraryItemModel.Property property)
        {
            if (property is
                LibraryItemModel.Property.Title or
                LibraryItemModel.Property.Description or
                LibraryItemModel.Property.Thumbnail or
                LibraryItemModel.Property.IsBaking or
                LibraryItemModel.Property.Progress)
            {
                InvokeChanged();
            }
        }

        public void RequestContextMenu()
        {
            OnContextMenuRequested?.Invoke(this);
        }

        public void RequestEdit()
        {
            if (!Model.IsBaking)
            {
                StopAnimatedThumbnail();
                OnRequestEdit?.Invoke(this);
            }
        }
    }
}
