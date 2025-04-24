using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Clickable = UnityEngine.UIElements.Clickable;
using TextField = Unity.Muse.AppUI.UI.TextField;

namespace Unity.Muse.Animate
{
    class LibraryItemUI : VisualElement
    {
        const string k_UssClassName = "deeppose-library-item";
        const string k_FocusedUssClassName = "deeppose-library-item-focused";
        const string k_UssClassNameHighlighted = "deeppose-library-item-highlighted";
        const string k_UssClassNameEditing = "deeppose-library-item-editing";
        const string k_ThumbnailUssClassName = "deeppose-library-item-thumbnail";
        const string k_CircularProgressUssClassName = "deeppose-library-item-circular-progress";
        const string k_OverlayUssClassName = "deeppose-library-item-overlay";

        const string k_TopRightUssClassName = "deeppose-library-item-top-right";

        const string k_TypeLabelUssClassName = "deeppose-library-item-type-label";
        const string k_DisplayNameUssClassName = "deeppose-library-item-display-name";

        static readonly Dictionary<LibraryItemType, string> k_TakeTypeLabels = new()
        {
            {LibraryItemType.TextToMotionTake, ""},
            {LibraryItemType.KeySequenceTake, "Edited"},
            {LibraryItemType.VideoToMotionTake, ""}
        };

        const string k_FavoriteButtonTooltip = "Add to favorites";
        const string k_ConvertToKeysButtonTooltip = "Convert to frames";
        const string k_EditKeysButtonTooltip = "Edit";

        public LibraryItemUIModel Model => m_Model;

        public VisualElement ContextMenuParent => m_MoreButton;

        LibraryItemUIModel m_Model;

        VisualElement m_Buttons;
        IconButton m_FavoriteButton;
        IconButton m_ConvertToKeysButton;
        IconButton m_MoreButton;

        Image m_ThumbnailElement;
        Text m_TypeLabel;
        Text m_NameLabel;
        TextField m_NameEditField;

        CircularProgress m_CircularProgress;
        VisualElement m_Overlay;

        protected internal LibraryItemUI()
        {
            if (!string.IsNullOrWhiteSpace(k_UssClassName))
            {
                AddToClassList(k_UssClassName);
            }
            InitComponents();
            focusable = true;
        }

        void InitComponents()
        {
            LogVerbose( $"InitComponents()");

            m_ThumbnailElement = new Image() { name = "thumbnail" };
            m_ThumbnailElement.pickingMode = PickingMode.Ignore;
            m_ThumbnailElement.AddToClassList(k_ThumbnailUssClassName);
            m_ThumbnailElement.scaleMode = ScaleMode.StretchToFill;
            m_ThumbnailElement.style.display = DisplayStyle.Flex;
            Add(m_ThumbnailElement);

            m_Overlay = new VisualElement() { name = "overlay" };
            m_Overlay.AddToClassList(k_OverlayUssClassName);
            m_Overlay.pickingMode = PickingMode.Ignore;
            Add(m_Overlay);

            m_FavoriteButton = new IconButton()
            {
                icon = "star",
                style =
                {
                    marginRight = 4
                },
                tooltip = k_FavoriteButtonTooltip
            };

            m_ConvertToKeysButton = new IconButton()
            {
                icon = "pen",
                style =
                {
                    marginRight = 4
                },
                tooltip = k_ConvertToKeysButtonTooltip
            };
            m_MoreButton = new IconButton()
            {
                icon = "ellipsis"
            };

            m_ConvertToKeysButton.clicked += () => m_Model?.RequestEdit();
            m_MoreButton.clicked += () => m_Model?.RequestContextMenu();

            m_Buttons = new VisualElement();
            m_Buttons.AddToClassList(k_TopRightUssClassName);

            // TODO: For now, don't show the "Favorite" button until the feature is available, see below for context.
            // https://github.cds.internal.unity3d.com/unity/UnityMuseAITools/pull/1311
            // https://jira.unity3d.com/browse/MUSEANIM-384
            // m_Buttons.Add(m_FavoriteButton);

            m_Buttons.Add(m_ConvertToKeysButton);
            m_Buttons.Add(m_MoreButton);
            Add(m_Buttons);

            m_TypeLabel = new Text() { name = "type-label", pickingMode = PickingMode.Ignore};
            m_TypeLabel.AddToClassList(k_TypeLabelUssClassName);
            Add(m_TypeLabel);

            m_NameLabel = new Text() { name = "display-name" };
            m_NameLabel.AddToClassList(k_DisplayNameUssClassName);
            var clickable = new Clickable(() =>
            {
                m_Model.IsEditingName = true;
            });
            clickable.activators.Clear();
            clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, clickCount = 2});
            m_NameLabel.AddManipulator(clickable);
            Add(m_NameLabel);

            m_NameEditField = new TextField() { name = "edit-field" };
            m_NameEditField.AddToClassList(k_DisplayNameUssClassName);
            m_NameEditField.style.display = DisplayStyle.None;
            m_NameEditField.RegisterCallback<ChangeEvent<string>>(OnNameEdited);
            m_NameEditField.RegisterCallback<FocusOutEvent>(OnNameFieldFocusOut);

            Add(m_NameEditField);

            m_CircularProgress = new CircularProgress(){ name = "circular-progress" };
            m_CircularProgress.variant = Progress.Variant.Indeterminate;
            m_CircularProgress.bufferValue = 1f;
            m_CircularProgress.pickingMode = PickingMode.Ignore;
            m_CircularProgress.size = Size.L;
            m_CircularProgress.AddToClassList(k_CircularProgressUssClassName);
            Add(m_CircularProgress);

            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.RightMouse && evt.clickCount == 1)
                {
                    RightClicked();
                }

                switch (evt.clickCount)
                {
                    case 1:
                        ClickedLeft();
                        break;
                    case 2:
                        LeftClickedDouble();
                        break;
                }
            });

            var dragDetector = new DragDetector(this);
            dragDetector.OnDragStart += OnDragStarted;

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var dpi = Vector2.one * Mathf.Max(Unity.AppUI.Core.Platform.scaleFactor, 1f);
            var newThumbnailSize = new Vector2(resolvedStyle.width, resolvedStyle.height) * dpi;
            m_Model.AnimatedThumbnailSize = Vector2Int.RoundToInt(newThumbnailSize);
            UpdateState(m_Model, LibraryItemUIModel.ItemState.Visible);
        }

        void ClickedLeft()
        {
            Log("ClickedLeft()");
            LeftClickedSingle();
        }

        void RightClicked()
        {
            Log("RightClicked()");
            m_Model?.RightClicked();
        }

        void LeftClickedDouble()
        {
            Log("LeftClickedDouble()");
            m_Model?.LeftClickedDouble();
        }

        void LeftClickedSingle()
        {
            Log("LeftClickedSingle()");
            m_Model?.LeftClickedSingle();
        }

        void OnDragStarted(VisualElement el)
        {
            LogVerbose("OnDragStarted()");
            m_Model?.DragStarted();
        }

        protected internal void SetModel(LibraryItemUIModel viewModel)
        {
            LogVerbose( $"SetModel({viewModel})");

            if (m_Model != null)
                UnregisterModel();

            m_Model = viewModel;

            RegisterModel();
            Update();
            UpdateThumbnail();
        }

        void Update()
        {
            LogVerbose( $"Update()");

            m_NameLabel.text = m_Model.Target.Title;

            UpdateType();
            UpdateProgress();
            UpdateTooltip();
        }

        void UpdateState(LibraryItemUIModel model, LibraryItemUIModel.ItemState state)
        {
            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;

            if (!Model.IsVisible)
                return;

            UpdateTooltip();
            UpdateButtonVisibility();

            RemoveFromClassList(k_UssClassNameHighlighted);
            RemoveFromClassList(k_UssClassNameEditing);

            if (m_Model.IsSelected)
            {
                AddToClassList(k_UssClassNameEditing);
            }
            else if (m_Model.IsHighlighted)
            {
                AddToClassList(k_UssClassNameHighlighted);
            }

            if (state is LibraryItemUIModel.ItemState.EditingName)
            {
                if (m_Model.IsEditingName)
                {
                    m_Model?.StopAnimatedThumbnail();
                    StartEditingName();
                }
                else
                {
                    StopEditingName();
                    if (m_Model.IsHighlighted && !m_Model.IsInContextualMenu)
                    {
                        m_Model?.PlayAnimatedThumbnail();
                    }
                }
            }

            if (state is LibraryItemUIModel.ItemState.Highlighted)
            {
                if (m_Model.IsHighlighted && !m_Model.IsInContextualMenu && !m_Model.IsEditingName)
                {
                    m_Model?.PlayAnimatedThumbnail();
                }
                else
                {
                    m_Model?.StopAnimatedThumbnail();
                }

                // Progress spinner is sometimes a placeholder for a thumbnail
                UpdateProgress();
            }
        }

        void StartEditingName()
        {
            LogVerbose( $"StartEditingName()");
            m_NameEditField.style.display = DisplayStyle.Flex;
            m_NameLabel.style.display = DisplayStyle.None;
            m_NameEditField.SetValueWithoutNotify(m_Model.Target.Title);
            // HACK: Need to wait for the context menu to close before focusing the text field, or else
            // the text field behaves weirdly.
            schedule.Execute(() => m_NameEditField.Focus()).ExecuteLater(20);
        }

        void StopEditingName()
        {
            LogVerbose( $"StopEditingName()");
            m_NameEditField.style.display = DisplayStyle.None;
            m_NameLabel.style.display = DisplayStyle.Flex;
        }

        void OnNameEdited(ChangeEvent<string> evt)
        {
            LogVerbose( $"OnNameEdited(): evt.newValue = {evt.newValue}");
            // Will trigger StopEditingName
            m_Model.IsEditingName = false;

            // We're getting double events here (not sure why), so we need to check if the value has actually changed.
            if (m_Model.Model.Title == evt.newValue)
                return;

            m_Model.Model.Title = evt.newValue;
        }

        void OnNameFieldFocusOut(FocusOutEvent evt)
        {
            LogVerbose( $"OnNameFieldFocusOut()");
            m_Model.IsEditingName = false;
        }

        void UpdateTooltip()
        {
            var label = !m_Model.IsInContextualMenu ? m_Model.Tooltip : "";
            LogVerbose($"UpdateTooltip({label})");
            tooltip = label;

            if (parent != null)
            {
                parent.tooltip = label;
            }
        }

        void UpdateThumbnail()
        {
            LogVerbose( $"UpdateThumbnail()");

            if (!Model.IsVisible)
                return;

            // Display the thumbnail element
            if (m_Model.Thumbnail is { } thumbnail)
            {
                m_ThumbnailElement.image = thumbnail;
            }
        }

        void UpdateType()
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"TakesLibraryItemUI -> UpdateTypeLabel()");

            var label = k_TakeTypeLabels[m_Model.Target.ItemType];
            if (!string.IsNullOrEmpty(label))
            {
                m_TypeLabel.text = label;
                m_TypeLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_TypeLabel.style.display = DisplayStyle.None;
            }

            m_ConvertToKeysButton.tooltip = m_Model.IsEditable ? k_EditKeysButtonTooltip : k_ConvertToKeysButtonTooltip;
        }

        void UpdateProgress()
        {
            var progress = m_Model.Progress;
            LogVerbose( $"UpdateProgress(" + progress + ")");

            m_CircularProgress.value = progress;
            m_CircularProgress.style.display = progress >= 1 ? DisplayStyle.None : DisplayStyle.Flex;
            
            m_ThumbnailElement.style.display = progress < 1 ? DisplayStyle.None : DisplayStyle.Flex;
        }

        void UpdateButtonVisibility()
        {
            if (m_Model == null)
            {
                return;
            }

            if ((m_Model.IsInContextualMenu || m_Model.IsHighlighted) && !m_Model.Model.IsBaking)
            {
                AddToClassList(k_FocusedUssClassName);
            }
            else
            {
                RemoveFromClassList(k_FocusedUssClassName);
            }
        }

        void RegisterModel()
        {
            if (m_Model == null)
            {
                LogError($"RegisterModel() failed, m_Model is null");
                return;
            }

            LogVerbose( $"RegisterModel()");
            m_Model.OnChanged += OnModelChanged;
            m_Model.OnThumbnailChanged += UpdateThumbnail;
            m_Model.OnStateChanged += UpdateState;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
            {
                LogError($"UnregisterModel() failed, m_Model is null");
                return;
            }

            LogVerbose( $"UnregisterModel()");
            m_Model.OnChanged -= OnModelChanged;
            m_Model.OnThumbnailChanged -= UpdateThumbnail;
            m_Model.OnStateChanged -= UpdateState;
            m_Model = null;
        }

        void OnModelChanged(LibraryItemUIModel item)
        {
            LogVerbose( $"OnModelChanged({item})");
            Update();
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            LogVerbose("OnPointerLeave()");
            if (m_Model == null) return;

            m_Model.IsHighlighted = false;
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            LogVerbose("OnPointerEnter()");
            if (m_Model == null) return;

            m_Model.IsHighlighted = true;
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
        void LogVerbose(string msg)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType().Name} -> {msg}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        void LogError(string msg)
        {
            DevLogger.LogError($"{GetType().Name} -> {msg}");
        }

        
    }
}
