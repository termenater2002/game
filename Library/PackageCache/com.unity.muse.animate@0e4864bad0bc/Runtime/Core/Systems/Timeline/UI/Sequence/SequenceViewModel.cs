using System;
using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    class SequenceViewModel
    {
        public delegate void SequenceItemsChanged();

        public delegate void ItemSelectionChanged();

        public delegate void TransitionsVisibilityChanged();

        public delegate void CurrentKeyChanged();

        public delegate void CurrentTransitionChanged();

        public delegate void KeyDragStarted();

        public delegate void KeyDragEnded();

        public delegate void ControlsChanged(Control control, bool enabled);

        public delegate void RequestedKeyContextualMenuAction(SequenceKeyContextualMenu.ActionType type, ClipboardService clipboard, SelectionModel<TimelineModel.SequenceKey> selectionModel, SequenceItemViewModel<TimelineModel.SequenceKey> target);

        public delegate void RequestedKeyContextualMenu(SequenceItemViewModel<TimelineModel.SequenceKey> key);

        public delegate void RequestedKeyToggle(SequenceItemViewModel<TimelineModel.SequenceKey> key);

        public delegate void RequestedTransitionToggle(SequenceItemViewModel<TimelineModel.SequenceTransition> key);

        public delegate void RequestedAddKey();

        public delegate void RequestedInsertKey(int keyIndex, float transitionProgress);

        public delegate void RequestedSelectKey(TimelineModel.SequenceKey key);

        public delegate void RequestedSelectTransition(TimelineModel.SequenceTransition key);

        public delegate void RequestedDeleteSelectedKeys();

        public event SequenceItemsChanged OnSequenceItemsChanged;
        public event ItemSelectionChanged OnItemSelectionChanged;
        public event TransitionsVisibilityChanged OnTransitionsVisibilityChanged;
        public event CurrentKeyChanged OnCurrentKeyChanged;
        public event CurrentTransitionChanged OnCurrentTransitionChanged;
        public event KeyDragStarted OnKeyDragStarted;
        public event KeyDragEnded OnKeyDragEnded;
        public event ControlsChanged OnControlsChanged;

        public event RequestedKeyContextualMenu OnRequestedKeyContextualMenu;
        public event RequestedKeyContextualMenuAction OnRequestedKeyContextualMenuAction;
        public event RequestedKeyToggle OnRequestedKeyToggle;
        public event RequestedTransitionToggle OnRequestedTransitionToggle;
        public event RequestedAddKey OnRequestedAddKey;
        public event RequestedInsertKey OnRequestedInsertKey;
        public event RequestedSelectKey OnRequestedSelectKey;
        public event RequestedSelectTransition OnRequestedSelectTransition;
        public event RequestedDeleteSelectedKeys OnRequestedDeleteSelectedKeys;

        public event Action<bool> OnEditableChanged;

        [Flags]
        public enum Control
        {
            CanAddKey = 1,
            CanDeleteKey = 2,
            CanGotoFirstKey = 4,
            CanGotoLastKey = 8,
            CanGotoNextItem = 16,
            CanGotoPreviousItem = 32
        }

        public SequenceAddKeyButtonViewModel SequenceAddKeyButton { get; private set; }
        public List<SequenceKeyViewModel> SequenceKeyViewModels { get; } = new();
        public List<SequenceTransitionViewModel> SequenceTransitionViewModels { get; } = new();
        public SequenceDraggedItemViewModel DraggedItemViewModel { get; private set; }

        public int KeyCount => m_TimelineModel?.KeyCount ?? 0;
        public int TransitionCount => m_TimelineModel?.TransitionCount ?? 0;

        public TimelineModel.SequenceKey SelectedKey
        {
            get => m_SelectedKey;
            private set
            {
                if (m_SelectedKey == value)
                    return;

                m_SelectedKey = value;
                UpdateInspectors();
                UpdateControls();
                UpdateKeysSelected();
                UpdateKeysEditing();
                OnItemSelectionChanged?.Invoke();
            }
        }

        public bool IsVisible
        {
            get => m_IsVisible;
            internal set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                UpdateInspectors();
            }
        }

        public bool IsEditable
        {
            get => m_IsEditable;
            internal set
            {
                if (value == m_IsEditable)
                    return;

                m_IsEditable = value;
                OnEditableChanged?.Invoke(value);
            }
        }

        public TimelineModel.SequenceTransition SelectedTransition
        {
            get => m_SelectedTransition;
            private set
            {
                if (m_SelectedTransition == value)
                    return;

                m_SelectedTransition = value;
                UpdateInspectors();
                UpdateControls();
                UpdateTransitionsSelected();
                UpdateTransitionsEditing();
                OnItemSelectionChanged?.Invoke();
            }
        }

        public int SelectedKeyIndex => m_SelectedKey == null ? -1 : m_TimelineModel.IndexOf(m_SelectedKey);
        public int SelectedTransitionIndex => m_SelectedTransition == null ? -1 : m_TimelineModel.IndexOf(m_SelectedTransition);

        public bool IsPlaying
        {
            get => m_IsPlaying;
            set
            {
                if (value == m_IsPlaying)
                    return;

                m_IsPlaying = value;
                UpdateControls();
            }
        }

        public bool IsEditingKey
        {
            get => m_IsEditingKey;
            set
            {
                if (value == m_IsEditingKey)
                    return;

                m_IsEditingKey = value;

                UpdateKeysEditing();
                UpdateTransitionsEditing();
                UpdateControls();
                UpdateInspectors();
            }
        }

        public bool IsEditingTransition
        {
            get => m_IsEditingTransition;
            set
            {
                if (value == m_IsEditingTransition)
                    return;

                m_IsEditingTransition = value;

                UpdateKeysEditing();
                UpdateTransitionsEditing();
                UpdateControls();
                UpdateInspectors();
            }
        }

        public bool TransitionsVisible
        {
            get => m_TransitionsVisible;
            private set
            {
                if (value == m_TransitionsVisible)
                    return;

                m_TransitionsVisible = value;
                UpdateTransitionsVisible();
                OnTransitionsVisibilityChanged?.Invoke();
            }
        }

        public int CurrentKeyIndex
        {
            get => m_CurrentKeyIndex;
            set
            {
                if (value == m_CurrentKeyIndex)
                    return;

                m_CurrentKeyIndex = value;
                UpdateKeysHighlighted();
                UpdateTransitionsHighlighted();
                UpdateControls();
                OnCurrentKeyChanged?.Invoke();
            }
        }

        int CurrentKeyIndexIncludingTransition => m_CurrentKeyIndex != -1 ? m_CurrentKeyIndex : m_CurrentTransitionIndex;

        public int CurrentTransitionIndex
        {
            get => m_CurrentTransitionIndex;
            set
            {
                if (value == m_CurrentTransitionIndex)
                {
                    return;
                }

                m_CurrentTransitionIndex = value;
                UpdateKeysHighlighted();
                UpdateTransitionsHighlighted();
                UpdateControls();
                OnCurrentTransitionChanged?.Invoke();
            }
        }

        public bool CanGotoNextItem
        {
            get => m_ControlsEnabled.HasFlag(Control.CanGotoNextItem);
            private set => SetControlEnabled(Control.CanGotoNextItem, value);
        }

        public bool CanGotoPreviousItem
        {
            get => m_ControlsEnabled.HasFlag(Control.CanGotoPreviousItem);
            private set => SetControlEnabled(Control.CanGotoPreviousItem, value);
        }

        public bool CanGotoFirstKey
        {
            get => m_ControlsEnabled.HasFlag(Control.CanGotoFirstKey);
            private set => SetControlEnabled(Control.CanGotoFirstKey, value);
        }

        public bool CanGotoLastKey
        {
            get => m_ControlsEnabled.HasFlag(Control.CanGotoLastKey);
            private set => SetControlEnabled(Control.CanGotoLastKey, value);
        }

        public bool CanAddKey
        {
            get => m_ControlsEnabled.HasFlag(Control.CanAddKey);
            private set => SetControlEnabled(Control.CanAddKey, value);
        }

        public bool CanDeleteKey
        {
            get => m_ControlsEnabled.HasFlag(Control.CanDeleteKey);
            private set => SetControlEnabled(Control.CanDeleteKey, value);
        }

        public SequenceKeyInspectorViewModel KeyInspectorViewModel => m_KeyInspectorViewModel;
        public SelectionModel<TimelineModel.SequenceKey> KeySelection => m_KeySelectionModel;

        public ClipboardService ClipboardService => m_ClipboardService;

        public TimelineModel TimelineModel
        {
            get => m_TimelineModel;
            set => SetTimelineModel(value);
        }

        TimelineModel m_TimelineModel;
        SelectionModel<TimelineModel.SequenceKey> m_KeySelectionModel;
        SelectionModel<TimelineModel.SequenceTransition> m_TransitionSelectionModel;
        TimelineModel.SequenceKey m_SelectedKey;
        TimelineModel.SequenceTransition m_SelectedTransition;

        int m_CurrentTransitionIndex;
        int m_CurrentKeyIndex;

        bool m_TransitionsVisible;
        bool m_IsVisible;
        bool m_IsPlaying;
        bool m_IsEditingKey;
        bool m_IsEditingTransition;
        bool m_IsEditable;

        Control m_ControlsEnabled;

        readonly SequenceKeyInspectorViewModel m_KeyInspectorViewModel;
        readonly ClipboardService m_ClipboardService;

        public SequenceViewModel(
            TimelineModel timelineModel,
            SelectionModel<TimelineModel.SequenceKey> keySelectionModel,
            SelectionModel<TimelineModel.SequenceTransition> transitionSelectionModel,
            ClipboardService clipboardService,
            InspectorsPanelViewModel inspectorsPanel
        )
        {
            m_KeySelectionModel = keySelectionModel;
            m_TransitionSelectionModel = transitionSelectionModel;
            m_ClipboardService = clipboardService;

            SequenceAddKeyButton = new SequenceAddKeyButtonViewModel(this);

            // Sequence Items Inspectors
            m_KeyInspectorViewModel = new SequenceKeyInspectorViewModel(inspectorsPanel);
            RegisterEvents();

            SetTimelineModel(timelineModel);
        }

        void RegisterEvents()
        {
            m_KeySelectionModel.OnSelectionChanged += OnKeySelectionChanged;
            m_TransitionSelectionModel.OnSelectionChanged += OnTransitionSelectionChanged;
        }

        internal void SetTimelineModel(TimelineModel value)
        {
            if (m_TimelineModel != value)
            {
                if (m_TimelineModel != null)
                    m_TimelineModel.OnTimelineChanged -= OnTimelineModelChanged;

                m_TimelineModel = value;

                if (m_TimelineModel != null)
                    m_TimelineModel.OnTimelineChanged += OnTimelineModelChanged;
            }
            
            PopulateItems();
            OnSequenceItemsChanged?.Invoke();
        }

        void PopulateItems()
        {
            var keysCount = 0;
            var transitionsCount = 0;

            if (m_TimelineModel == null)
            {
                // Hide all keys.
                for (var i = 0; i < SequenceKeyViewModels.Count; i++)
                {
                    SequenceKeyViewModels[i].Target = null;
                    SequenceKeyViewModels[i].IsVisible = false;
                }

                // Hide all transitions.
                for (var i = 0; i < SequenceTransitionViewModels.Count; i++)
                {
                    SequenceTransitionViewModels[i].Target = null;
                    SequenceTransitionViewModels[i].IsVisible = false;
                }
            }
            else
            {
                keysCount = m_TimelineModel.KeyCount;
                transitionsCount = m_TimelineModel.TransitionCount;

                // Add new keys
                for (var i = SequenceKeyViewModels.Count; i < keysCount; i++)
                {
                    var key = new SequenceKeyViewModel(m_TimelineModel.Keys[i]);
                    SequenceKeyViewModels.Add(key);
                    key.OnLeftClicked += OnKeyLeftClicked;
                    key.OnRightClicked += OnKeyRightClicked;
                }

                // Add new transitions
                for (var i = SequenceTransitionViewModels.Count; i < transitionsCount; i++)
                {
                    var transition = new SequenceTransitionViewModel(m_TimelineModel.Transitions[i]);
                    SequenceTransitionViewModels.Add(transition);
                    transition.OnLeftClicked += OnTransitionLeftClicked;
                }

                // Hide extra keys.
                for (var i = keysCount; i < SequenceKeyViewModels.Count; i++)
                {
                    SequenceKeyViewModels[i].Target = null;
                    SequenceKeyViewModels[i].IsVisible = false;
                }

                // Hide extra transitions.
                for (var i = transitionsCount; i < SequenceTransitionViewModels.Count; i++)
                {
                    SequenceTransitionViewModels[i].Target = null;
                    SequenceTransitionViewModels[i].IsVisible = false;
                }

                // Show and set used keys and transitions.
                for (var i = 0; i < keysCount; i++)
                {
                    SequenceKeyViewModels[i].Target = m_TimelineModel.Keys[i];
                    SequenceKeyViewModels[i].IsVisible = true;

                    if (i >= m_TimelineModel.TransitionCount)
                        continue;

                    SequenceTransitionViewModels[i].Target = m_TimelineModel.Transitions[i];
                    SequenceTransitionViewModels[i].IsVisible = true;
                }
            }

            UpdateItems();
            UpdateControls();
        }

        void UpdateControls()
        {
            UpdateCanDeleteKey();
            UpdateCanAddKey();
            UpdateCanGotoFirstKey();
            UpdateCanGotoLastKey();
            UpdateCanGotoNextItem();
            UpdateCanGotoPreviousItem();
        }

        void UpdateCanDeleteKey()
        {
            if (m_TimelineModel == null)
            {
                CanDeleteKey = !m_IsPlaying && m_SelectedKey != null;
            }
            else
            {
                CanDeleteKey = m_TimelineModel.KeyCount > 1 && !m_IsPlaying && m_SelectedKey != null;
            }
        }

        void UpdateCanAddKey()
        {
            CanAddKey = !m_IsPlaying;
        }

        void UpdateCanGotoFirstKey()
        {
            if (m_TimelineModel == null)
            {
                CanGotoFirstKey = false;
                return;
            }

            if (m_TimelineModel.KeyCount <= 1 || m_IsPlaying)
            {
                CanGotoFirstKey = false;
                return;
            }

            if (m_SelectedKey is { PreviousKey: null })
            {
                CanGotoFirstKey = false;
                return;
            }

            CanGotoFirstKey = true;
        }

        void UpdateCanGotoLastKey()
        {
            if (m_TimelineModel == null)
            {
                CanGotoLastKey = false;
                return;
            }
            
            if (m_TimelineModel.KeyCount <= 1 || m_IsPlaying)
            {
                CanGotoLastKey = false;
                return;
            }

            if (m_SelectedKey is { NextKey: null })
            {
                CanGotoLastKey = false;
                return;
            }

            CanGotoLastKey = true;
        }

        void UpdateCanGotoNextItem()
        {
            if (m_TimelineModel == null)
            {
                CanGotoNextItem = false;
                return;
            }

            if (m_TimelineModel.KeyCount <= 1 || m_IsPlaying)
            {
                CanGotoNextItem = false;
                return;
            }

            if (m_SelectedKey != null)
            {
                if (m_SelectedKey.NextKey != null)
                {
                    CanGotoNextItem = true;
                    return;
                }
            }
            else if (m_SelectedTransition != null && m_TransitionsVisible)
            {
                CanGotoNextItem = true;
                return;
            }

            CanGotoNextItem = false;
        }

        void UpdateCanGotoPreviousItem()
        {
            if (m_TimelineModel == null)
            {
                CanGotoPreviousItem = false;
                return;
            }
            
            if (m_TimelineModel.KeyCount <= 1 || m_IsPlaying)
            {
                CanGotoPreviousItem = false;
                return;
            }

            if (m_SelectedKey != null)
            {
                if (m_SelectedKey.PreviousKey != null)
                {
                    CanGotoPreviousItem = true;
                    return;
                }
            }
            else if (m_SelectedTransition != null && m_TransitionsVisible)
            {
                CanGotoPreviousItem = true;
                return;
            }

            CanGotoPreviousItem = false;
        }

        void UpdateItems()
        {
            UpdateKeys();
            UpdateTransitions();
        }

        void UpdateKeys()
        {
            foreach (var key in SequenceKeyViewModels)
                UpdateKey(key);
        }

        void UpdateKey(SequenceKeyViewModel key)
        {
            if (m_TimelineModel == null)
            {
                key.IsHighlighted = false;
            }
            else
            {
                key.IsHighlighted = IsCurrent(key);
            }
            
            key.IsSelected = IsSelected(key);
            key.IsEditing = IsEditing(key);
        }

        void UpdateKeysHighlighted()
        {
            foreach (var key in SequenceKeyViewModels)
                key.IsHighlighted = IsCurrent(key);
        }

        void UpdateKeysSelected()
        {
            foreach (var key in SequenceKeyViewModels)
                key.IsSelected = IsSelected(key);
        }

        void UpdateKeysEditing()
        {
            foreach (var key in SequenceKeyViewModels)
                key.IsEditing = IsEditing(key);
        }

        void UpdateTransitions()
        {
            foreach (var transition in SequenceTransitionViewModels)
                UpdateTransition(transition);
        }

        void UpdateTransition(SequenceTransitionViewModel transition)
        {
            transition.IsVisible = m_TransitionsVisible;
            
            if (m_TimelineModel == null)
            {
                transition.IsHighlighted = false;
            }
            else
            {
                transition.IsHighlighted = IsCurrent(transition);
            }
            
            transition.IsSelected = IsSelected(transition);
            transition.IsEditing = IsEditing(transition);
        }

        void UpdateTransitionsHighlighted()
        {
            foreach (var transition in SequenceTransitionViewModels)
                transition.IsHighlighted = IsCurrent(transition);
        }

        void UpdateTransitionsSelected()
        {
            foreach (var transition in SequenceTransitionViewModels)
                transition.IsSelected = IsSelected(transition);
        }

        void UpdateTransitionsEditing()
        {
            foreach (var transition in SequenceTransitionViewModels)
                transition.IsEditing = IsEditing(transition);
        }

        void UpdateTransitionsVisible()
        {
            foreach (var transition in SequenceTransitionViewModels)
                transition.IsVisible = m_TransitionsVisible;
        }

        void UpdateSelectedKey()
        {
            SelectedKey = m_KeySelectionModel.HasSelection ? m_KeySelectionModel.GetSelection(0) : null;
        }

        void UpdateSelectedTransition()
        {
            SelectedTransition = m_TransitionSelectionModel.HasSelection ? m_TransitionSelectionModel.GetSelection(0) : null;
        }

        bool IsEditing(SequenceKeyViewModel key)
        {
            return IsEditingKey && IsSelected(key);
        }

        bool IsEditing(SequenceTransitionViewModel transition)
        {
            return IsEditingTransition && IsSelected(transition);
        }

        bool IsSelected(SequenceKeyViewModel key)
        {
            return m_KeySelectionModel.IsSelected(key.Target);
        }

        bool IsSelected(SequenceTransitionViewModel transition)
        {
            return m_TransitionSelectionModel.IsSelected(transition.Target);
        }

        bool IsCurrent(SequenceKeyViewModel key)
        {
            if (m_TimelineModel == null)
            {
                return false;
            }

            return CurrentKeyIndexIncludingTransition == m_TimelineModel.IndexOf(key.Target);
        }

        bool IsCurrent(SequenceTransitionViewModel transition)
        {
            if (m_TimelineModel == null)
            {
                return false;
            }
            
            return CurrentTransitionIndex == m_TimelineModel.IndexOf(transition.Target);
        }

        public void StartKeyDrag(TimelineModel.SequenceKey key)
        {
            DraggedItemViewModel ??= new SequenceDraggedItemViewModel();
            DraggedItemViewModel.Key = key;
            DraggedItemViewModel.IsVisible = true;
            OnKeyDragStarted?.Invoke();
        }

        public void EndKeyDrag(int newIndex)
        {
            MoveKey(DraggedItemViewModel.Key, newIndex);
            DraggedItemViewModel.IsVisible = false;
            OnKeyDragEnded?.Invoke();
        }

        public void InsertCopyOfCurrentKey()
        {
            OnRequestedInsertKey?.Invoke(CurrentKeyIndexIncludingTransition + 1, 0f);
        }

        public void AddKeyToEnd()
        {
            OnRequestedAddKey?.Invoke();
        }

        public void RequestKeyContextualMenu(SequenceItemViewModel<TimelineModel.SequenceKey> key)
        {
            OnRequestedKeyContextualMenu?.Invoke(key);
        }

        public void DeleteSelectedKeys()
        {
            OnRequestedDeleteSelectedKeys?.Invoke();
        }

        public void ToggleTransitionVisibility()
        {
            TransitionsVisible = !TransitionsVisible;
        }

        void MoveKey(TimelineModel.SequenceKey key, int newIndex)
        {
            if (newIndex < 0)
            {
                newIndex = m_TimelineModel.KeyCount - 1;
            }

            int oldIndex = m_TimelineModel.IndexOf(key);
            if (oldIndex == newIndex)
                return;

            m_TimelineModel.MoveKey(oldIndex, newIndex, false);
        }

        public void SelectFirstKey()
        {
            SelectKey(m_TimelineModel.GetFirstKey());
        }

        public void SelectLastKey()
        {
            SelectKey(m_TimelineModel.GetLastKey());
        }

        public void SelectPreviousItem()
        {
            if (SelectedKey == null && SelectedTransition == null)
            {
                SelectFirstKey();
                return;
            }

            if (!TransitionsVisible)
            {
                SelectPreviousKey();
                return;
            }

            var targetTransition = SelectedKey?.InTransition;
            if (targetTransition != null)
            {
                SelectTransition(targetTransition);
                return;
            }

            var targetKey = SelectedTransition?.FromKey;
            SelectKey(targetKey);
        }

        public void SelectNextItem()
        {
            if (SelectedKey == null && SelectedTransition == null)
            {
                SelectLastKey();
                return;
            }

            if (!TransitionsVisible)
            {
                SelectNextKey();
                return;
            }

            var targetTransition = SelectedKey?.OutTransition;
            if (targetTransition != null)
            {
                SelectTransition(targetTransition);
                return;
            }

            SelectKey(SelectedTransition?.ToKey);
        }

        void SelectPreviousKey()
        {
            if (m_TimelineModel.KeyCount == 0)
                return;

            var targetKey = m_KeySelectionModel.HasSelection ? m_KeySelectionModel.GetSelection(0) : m_TimelineModel.GetKey(0);
            SelectKey(targetKey.PreviousKey);
        }

        void SelectNextKey()
        {
            if (m_TimelineModel.KeyCount == 0)
                return;

            var targetKey = m_KeySelectionModel.HasSelection ? m_KeySelectionModel.GetSelection(0) : m_TimelineModel.GetKey(0);
            SelectKey(targetKey.NextKey);
        }

        void SelectKey(TimelineModel.SequenceKey item)
        {
            OnRequestedSelectKey?.Invoke(item);
        }

        void SelectTransition(TimelineModel.SequenceTransition item)
        {
            OnRequestedSelectTransition?.Invoke(item);
        }

        void SetControlEnabled(Control state, bool value)
        {
            if (value == m_ControlsEnabled.HasFlag(state))
                return;

            if (value)
            {
                m_ControlsEnabled |= state;
            }
            else
            {
                m_ControlsEnabled &= ~state;
            }

            OnControlsChanged?.Invoke(state, value);
        }

        void OnTimelineModelChanged(TimelineModel model, TimelineModel.Property property)
        {
            PopulateItems();
            OnSequenceItemsChanged?.Invoke();
        }

        void OnKeyRightClicked(SequenceItemViewModel<TimelineModel.SequenceKey> item)
        {
            SelectKey(item.Target);
            RequestKeyContextualMenu(item);
        }

        void OnKeyLeftClicked(SequenceItemViewModel<TimelineModel.SequenceKey> item)
        {
            // If already selected, switch authoring mode between posing and previewing
            if (item.IsSelected)
            {
                OnRequestedKeyToggle?.Invoke(item);
                return;
            }

            SelectKey(item.Target);
        }

        void OnTransitionLeftClicked(SequenceItemViewModel<TimelineModel.SequenceTransition> item)
        {
            // If already selected, switch authoring mode between posing and previewing
            if (item.IsSelected)
            {
                OnRequestedTransitionToggle?.Invoke(item);
                return;
            }

            SelectTransition(item.Target);
        }

        void OnTransitionSelectionChanged(SelectionModel<TimelineModel.SequenceTransition> model)
        {
            UpdateSelectedTransition();
        }

        void OnKeySelectionChanged(SelectionModel<TimelineModel.SequenceKey> model)
        {
            UpdateSelectedKey();
        }

        void UpdateInspectors()
        {
            if (!IsVisible)
            {
                m_KeyInspectorViewModel.Target = null;
                m_KeyInspectorViewModel.IsVisible = false;
                return;
            }

            if (SelectedKey != null && IsEditingKey)
            {
                m_KeyInspectorViewModel.Target = SelectedKey;
                m_KeyInspectorViewModel.IsVisible = m_KeyInspectorViewModel.Target != null;
                return;
            }

            if (SelectedTransition != null && IsEditingTransition)
            {
                m_KeyInspectorViewModel.Target = SelectedTransition.FromKey;
                m_KeyInspectorViewModel.IsVisible = m_KeyInspectorViewModel.Target != null;
                return;
            }

            m_KeyInspectorViewModel.Target = null;
            m_KeyInspectorViewModel.IsVisible = false;
        }

        public void RequestSequenceKeyContextMenuAction(SequenceKeyContextualMenu.ActionType type, ClipboardService clipboard, SelectionModel<TimelineModel.SequenceKey> selectionModel, SequenceItemViewModel<TimelineModel.SequenceKey> target)
        {
            OnRequestedKeyContextualMenuAction?.Invoke(type, clipboard, selectionModel, target);
        }
    }
}
