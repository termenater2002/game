using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class SequenceView : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-sequence";

        const string k_MenuName = "sequence-menu";
        const string k_AddKeyButtonName = "timeline-add-key";
        const string k_DeleteKeyButtonName = "sequence-delete-keys";
        const string k_GoToPreviousKeyButtonName = "go-to-previous-key";
        const string k_GoToNextKeyButtonName = "go-to-next-key";
        const string k_GoToFirstKeyButtonName = "go-to-first-key";
        const string k_GoToLastKeyButtonName = "go-to-last-key";
        const string k_ToggleTransitionsButtonName = "sequence-toggle-transitions";
        const string k_SequenceAddKeyButtonName = "sequence-add-key";
        const string k_ScrollViewName = "sequence-scrollview";
        const string k_DragDropLayerName = "sequence-drag-layer";
        const string k_TransitionsVisibleIconName = "eye";
        const string k_TransitionsHiddenIconName = "eye-slash";

        SequenceViewModel m_Model;
        SequenceDraggedItemView m_DraggedItem;

        ScrollView m_ScrollView;
        VisualElement m_DragLayer;

        ActionButton m_AddKeyButton;
        ActionButton m_DeleteKeyButton;
        ActionButton m_GoToPreviousKeyButton;
        ActionButton m_GoToNextKeyButton;
        ActionButton m_ToggleTransitionsButton;
        ActionButton m_GoToFirstKeyButton;
        ActionButton m_GoToLastKeyButton;

        SequenceAddKeyButtonView m_SequenceAddKeyButton;
        SequenceKeyContextualMenu m_SequenceKeyContextualMenu;
        
        bool m_IsDragging;
        Vector2 m_MousePos;

        List<SequenceKeyView> m_SequenceKeyViews = new();
        List<SequenceTransitionView> m_SequenceTransitionViews = new();

        SequenceKeyInspectorView m_KeyInspectorView;
        VisualElement m_Menu;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<SequenceView, UxmlTraits> { }
#endif

        public SequenceView() : base(k_UssClassName) { }

        public void InitComponents()
        {
            m_SequenceAddKeyButton = new SequenceAddKeyButtonView(k_SequenceAddKeyButtonName);
            m_SequenceKeyContextualMenu = new SequenceKeyContextualMenu();
            m_SequenceKeyContextualMenu.OnMenuAction += OnSequenceKeyContextualMenuAction;
            
            m_KeyInspectorView = ApplicationConstants.MotionSynthesisEnabled
                ? new SequenceKeyInspectorView()
                : null;
        }
        
        public void FindComponents()
        {
            m_ScrollView = this.Q<ScrollView>(k_ScrollViewName);
            m_Menu = this.Q<VisualElement>(k_MenuName);
            m_AddKeyButton = this.Q<ActionButton>(k_AddKeyButtonName);
            m_DeleteKeyButton = this.Q<ActionButton>(k_DeleteKeyButtonName);
            m_GoToFirstKeyButton = this.Q<ActionButton>(k_GoToFirstKeyButtonName);
            m_GoToLastKeyButton = this.Q<ActionButton>(k_GoToLastKeyButtonName);
            m_GoToPreviousKeyButton = this.Q<ActionButton>(k_GoToPreviousKeyButtonName);
            m_GoToNextKeyButton = this.Q<ActionButton>(k_GoToNextKeyButtonName);
            m_ToggleTransitionsButton = this.Q<ActionButton>(k_ToggleTransitionsButtonName);
            m_DragLayer = this.Q<VisualElement>(k_DragDropLayerName);
        }

        public void RegisterComponents()
        {
            // The menu is not used for the moment, so it remains hidden
            m_Menu.style.display = DisplayStyle.None;

            m_AddKeyButton.RegisterCallback<ClickEvent>(OnAddKeyButtonClicked);
            m_DeleteKeyButton.RegisterCallback<ClickEvent>(OnDeleteKeyButtonClicked);
            m_GoToFirstKeyButton.RegisterCallback<ClickEvent>(OnGoToFirstKeyButtonClicked);
            m_GoToLastKeyButton.RegisterCallback<ClickEvent>(OnGoToLastKeyButtonClicked);
            m_GoToPreviousKeyButton.RegisterCallback<ClickEvent>(OnGoToPreviousKeyButtonClicked);
            m_GoToNextKeyButton.RegisterCallback<ClickEvent>(OnGoToNextKeyButtonClicked);
            m_ToggleTransitionsButton.RegisterCallback<ClickEvent>(OnToggleTransitionsButtonClicked);
        }

        public void UnregisterComponents()
        {
            m_AddKeyButton.UnregisterCallback<ClickEvent>(OnAddKeyButtonClicked);
            m_DeleteKeyButton.UnregisterCallback<ClickEvent>(OnDeleteKeyButtonClicked);
            m_GoToFirstKeyButton.UnregisterCallback<ClickEvent>(OnGoToFirstKeyButtonClicked);
            m_GoToLastKeyButton.UnregisterCallback<ClickEvent>(OnGoToLastKeyButtonClicked);
            m_GoToPreviousKeyButton.UnregisterCallback<ClickEvent>(OnGoToPreviousKeyButtonClicked);
            m_GoToNextKeyButton.UnregisterCallback<ClickEvent>(OnGoToNextKeyButtonClicked);
            m_ToggleTransitionsButton.UnregisterCallback<ClickEvent>(OnToggleTransitionsButtonClicked);
        }

        public void SetModel(SequenceViewModel model)
        {
            UnregisterModel();
            m_Model = model;
            m_KeyInspectorView?.SetModel(m_Model.KeyInspectorViewModel);
            RegisterModel();
            Update();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnSequenceItemsChanged += OnSequenceItemsChanged;
            m_Model.OnRequestedKeyContextualMenu += OnSequenceRequestedKeyContextualMenu;
            m_Model.OnItemSelectionChanged += OnItemSelectionChanged;
            m_Model.OnCurrentKeyChanged += OnCurrentKeyChanged;
            m_Model.OnKeyDragStarted += OnKeyDragStarted;
            m_Model.OnKeyDragEnded += OnKeyDragEnded;
            m_Model.OnTransitionsVisibilityChanged += OnTransitionsVisibilityChanged;
            m_Model.OnControlsChanged += OnControlsChanged;
            m_Model.OnEditableChanged += OnEditableChanged;
            
            m_SequenceAddKeyButton.SetModel(m_Model.SequenceAddKeyButton);

            Update();
        }

        public void Update()
        {
            if (m_Model == null)
                return;
            
            for (var i = 0; i < m_Model.SequenceKeyViewModels.Count; i++)
            {
                // Create new key views
                if (i >= m_SequenceKeyViews.Count)
                {
                    m_SequenceKeyViews.Add(new SequenceKeyView());
                }

                m_SequenceKeyViews[i].SetModel(m_Model.SequenceKeyViewModels[i]);
            }

            // Create new transition views.
            for (var i = 0; i < m_Model.SequenceTransitionViewModels.Count; i++)
            {
                if (i >= m_SequenceTransitionViews.Count)
                {
                    m_SequenceTransitionViews.Add(new SequenceTransitionView());
                }

                m_SequenceTransitionViews[i].SetModel(m_Model.SequenceTransitionViewModels[i]);
            }

            // Update used views.
            m_ScrollView.Clear();
            
            for (var i = 0; i < m_Model.KeyCount; i++)
            {
                m_ScrollView.Add(m_SequenceKeyViews[i]);
                
                if (i < m_Model.TransitionCount)
                {
                    m_ScrollView.Add((m_SequenceTransitionViews[i]));
                }
            }

            // Add the plus button at the end
            m_ScrollView.Add(m_SequenceAddKeyButton);
            
            OnEditableChanged(m_Model.IsEditable);
        }

        void UpdateControls()
        {
            m_AddKeyButton.SetEnabled(m_Model.CanAddKey);
            m_DeleteKeyButton.SetEnabled(m_Model.CanDeleteKey);
            m_GoToFirstKeyButton.SetEnabled(m_Model.CanGotoFirstKey);
            m_GoToLastKeyButton.SetEnabled(m_Model.CanGotoLastKey);
            m_GoToPreviousKeyButton.SetEnabled(m_Model.CanGotoPreviousItem);
            m_GoToNextKeyButton.SetEnabled(m_Model.CanGotoNextItem);
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnSequenceItemsChanged -= OnSequenceItemsChanged;
            m_Model.OnRequestedKeyContextualMenu -= OnSequenceRequestedKeyContextualMenu;
            m_Model.OnItemSelectionChanged -= OnItemSelectionChanged;
            m_Model.OnCurrentKeyChanged -= OnCurrentKeyChanged;
            m_Model.OnKeyDragStarted -= OnKeyDragStarted;
            m_Model.OnKeyDragEnded -= OnKeyDragEnded;
            m_Model.OnTransitionsVisibilityChanged -= OnTransitionsVisibilityChanged;
            m_Model.OnControlsChanged -= OnControlsChanged;
            m_Model.OnEditableChanged -= OnEditableChanged;

            m_SequenceAddKeyButton.SetModel(null);

            m_Model = null;
        }

        void OnSequenceRequestedKeyContextualMenu(SequenceItemViewModel<TimelineModel.SequenceKey> key)
        {
            if (!m_Model.IsEditable)
                return;

            SequenceKeyView matchingView = null;
            
            foreach (var view in m_SequenceKeyViews)
            {
                if (view.Model == key.Target)
                {
                    matchingView = view;
                    break;
                }
            }
            
            m_SequenceKeyContextualMenu.Open(m_Model.TimelineModel, m_Model.ClipboardService, m_Model.KeySelection, key, matchingView);
        }

        void OnSequenceKeyContextualMenuAction(SequenceKeyContextualMenu.ActionType type, ClipboardService clipboard, SelectionModel<TimelineModel.SequenceKey> selectionModel, SequenceItemViewModel<TimelineModel.SequenceKey> target)
        {
            m_Model.RequestSequenceKeyContextMenuAction(type, clipboard, selectionModel, target);
        }

        void OnControlsChanged(SequenceViewModel.Control control, bool enabled)
        {
            UpdateControls();
        }

        void OnCurrentKeyChanged()
        {
            var currentKeyIndex = m_Model.CurrentKeyIndex;

            if (currentKeyIndex < 0)
                return;

            if (currentKeyIndex >= m_SequenceKeyViews.Count || currentKeyIndex >= m_ScrollView.childCount)
            {
                return;
            }

            m_ScrollView.ScrollTo(m_SequenceKeyViews[currentKeyIndex]);
        }

        void OnItemSelectionChanged()
        {
            if (m_Model.SelectedKey != null && m_Model.SelectedKeyIndex >= 0 && m_Model.SelectedKeyIndex < m_SequenceKeyViews.Count)
            {
                m_ScrollView.ScrollTo(m_SequenceKeyViews[m_Model.SelectedKeyIndex]);
                return;
            }
        }

        void OnSequenceItemsChanged()
        {
            Update();
        }

        void OnMouseMove(MouseMoveEvent evt)
        {
            if (!m_IsDragging || m_DraggedItem == null)
                return;

            m_MousePos = evt.localMousePosition;
            m_DraggedItem.style.position = Position.Absolute;
            m_DraggedItem.style.left = m_MousePos.x - m_DraggedItem.style.width.value.value / 2f;
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            if (!m_IsDragging || m_DraggedItem == null)
                return;

            m_MousePos = evt.localMousePosition;

            var pos = evt.mousePosition;

            // Do nothing if the sequence either contains one or no items, no moving required.
            if (m_ScrollView.childCount < 2)
                return;

            var newIndex = 0;

            if (m_ScrollView[0].worldBound.xMin >= pos.x)
            {
                // dropped before the first key, move key to the beginning of the sequence.
            }

            else if (m_ScrollView[m_ScrollView.childCount - 1].worldBound.xMax <= pos.x)
            {
                // dropped after the last key, move to the end of the sequence.
                newIndex = m_Model.KeyCount - 1;
            }

            else
            {
                // Dropped somewhere in the middle, find the closest index.
                for (var i = 0; i < m_ScrollView.childCount; i++)
                {
                    if (!(m_ScrollView[i] is SequenceKeyView))
                        continue;

                    if (m_ScrollView[i].worldBound.xMin > pos.x || m_ScrollView[i].worldBound.Contains(pos))
                        break;

                    newIndex++;
                }
            }

            m_Model.EndKeyDrag(newIndex);
        }

        void OnKeyDragStarted()
        {
            m_IsDragging = true;
            m_DraggedItem ??= new SequenceDraggedItemView();
            m_DraggedItem.SetModel(m_Model.DraggedItemViewModel);
            m_DragLayer.Add(m_DraggedItem);
            m_DraggedItem.style.position = Position.Absolute;
            m_DraggedItem.style.left = m_MousePos.x;
        }

        void OnKeyDragEnded()
        {
            m_DragLayer.Remove(m_DraggedItem);
            m_IsDragging = false;
        }

        void OnAddKeyButtonClicked(ClickEvent evt)
        {
            m_Model?.InsertCopyOfCurrentKey();
        }

        void OnDeleteKeyButtonClicked(ClickEvent evt)
        {
            m_Model?.DeleteSelectedKeys();
        }

        void OnGoToFirstKeyButtonClicked(ClickEvent evt)
        {
            m_Model?.SelectFirstKey();
        }

        void OnGoToLastKeyButtonClicked(ClickEvent evt)
        {
            m_Model?.SelectLastKey();
        }

        void OnGoToPreviousKeyButtonClicked(ClickEvent evt)
        {
            m_Model?.SelectPreviousItem();
        }

        void OnGoToNextKeyButtonClicked(ClickEvent evt)
        {
            m_Model?.SelectNextItem();
        }

        void OnToggleTransitionsButtonClicked(ClickEvent evt)
        {
            m_Model?.ToggleTransitionVisibility();
        }

        void OnTransitionsVisibilityChanged()
        {
            m_ToggleTransitionsButton.icon = m_Model.TransitionsVisible ? k_TransitionsVisibleIconName : k_TransitionsHiddenIconName;
        }

        void OnEditableChanged(bool enabled)
        {
            m_SequenceAddKeyButton.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
