using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.Muse.Chat.UI.Components
{
    class SelectionPopup : ManagedTemplate
    {
        internal class ListEntry
        {
            public Object Object;
            public Action<SelectionElement> OnRowClick;
            public Action<SelectionElement> OnFindButtonClick;
            public LogData? LogData;
            public bool IsSelected;
        }

        VisualElement m_Root;
        VisualElement m_AdaptiveListViewContainer;
        ToolbarSearchField m_SearchField;
        AdaptiveListView<ListEntry, SelectionElement> m_ListView;
        Button m_AddEditorSelectionButton;
        Label m_EmptyListHintText;

        double m_LastConsoleCheckTime;
        readonly float k_ConsoleCheckInterval = 0.2f;

        List<LogData> m_LastUpdatedLogReferences = new ();

        public readonly List<Object> ObjectSelection = new();
        public readonly List<Object> CombinedSelection = new();
        public readonly List<LogData> ConsoleSelection = new();

        public Action OnSelectionChanged;
        public Action<Object> OnContextObjectAdded;
        public Action<LogData> OnContextLogAdded;

        const float k_MaxWidth = 500;
        const float k_RightMargin = 60;

        readonly IList<Object> k_SearchResults = new List<Object>();
        bool m_RefreshPending;
        bool m_SearchActive;

        string m_ActiveSearchFilter = string.Empty;

        public SelectionPopup()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        public void SetSelectionFromContext(List<MuseChatContextEntry> context, bool notify = true)
        {
            ObjectSelection.Clear();
            ConsoleSelection.Clear();

            for (var i = 0; i < context.Count; i++)
            {
                var entry = context[i];
                switch (entry.EntryType)
                {
                    case MuseChatContextType.HierarchyObject:
                    case MuseChatContextType.SceneObject:
                    {
                        var target = entry.GetTargetObject();
                        if (target != null)
                        {
                            ObjectSelection.Add(target);
                        }

                        break;
                    }

                    case MuseChatContextType.ConsoleMessage:
                    {
                        var logEntry = new LogData
                        {
                            Message = entry.Value,
                            Type = Enum.Parse<LogDataType>(entry.ValueType)
                        };

                        ConsoleSelection.Add(logEntry);
                        break;
                    }
                }
            }

            if (notify)
            {
                OnSelectionChanged?.Invoke();
            }
        }

        void CheckAndRefilterSearchResults(bool force = false)
        {
            string newFilterValue = m_SearchField.value.Trim();
            if (newFilterValue == m_ActiveSearchFilter && !force)
            {
                return;
            }

            m_ActiveSearchFilter = newFilterValue;
            if (string.IsNullOrEmpty(m_ActiveSearchFilter))
            {
                PopulateListView();

                m_SearchActive = false;
                k_SearchResults.Clear();
                ScheduleRefresh();
                return;
            }

            m_SearchActive = true;
            SearchService.Request(m_ActiveSearchFilter, OnIncomingResults);
        }

        void OnIncomingResults(SearchContext context, IEnumerable<SearchItem> items)
        {
            k_SearchResults.Clear();

            foreach(var item in items)
            {
                var obj = item.ToObject();

                if (obj== null)
                {
                    continue;
                }

                k_SearchResults.Add(obj);
            }

            ScheduleRefresh();
        }

        void ScheduleRefresh()
        {
            if (m_RefreshPending)
            {
                return;
            }

            m_RefreshPending = true;
            EditorApplication.delayCall += OnRefresh;
        }

        void OnRefresh()
        {
            m_RefreshPending = false;
            PopulateListView();
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_Root = view.Q<VisualElement>("popupRoot");

            m_AdaptiveListViewContainer = view.Q<VisualElement>("adaptiveListViewContainer");

            var searchFieldRoot = view.Q<VisualElement>("attachItemSearchFieldRoot");
            m_SearchField = new ToolbarSearchField();
            m_SearchField.AddToClassList("mui-selection-search-bar");
            m_SearchField.RegisterCallback<KeyUpEvent>(_ => CheckAndRefilterSearchResults());
            m_SearchField.RegisterCallback<InputEvent>(_ => CheckAndRefilterSearchResults());
            searchFieldRoot.Add(m_SearchField);

            m_ListView = new()
            {
                EnableDelayedElements = true,
                EnableVirtualization = false,
                EnableScrollLock = true,
                EnableHorizontalScroll = false
            };
            m_ListView.Initialize();
            m_AdaptiveListViewContainer.Add(m_ListView);

            m_ListView.style.minHeight = 200;
            m_ListView.style.minWidth = 200;

            m_AddEditorSelectionButton = view.Q<Button>("addSelectionButton");
            m_AddEditorSelectionButton.focusable = false;
            m_AddEditorSelectionButton.clickable.clicked += () =>
            {
                PopulateListView(true);
            };

            m_EmptyListHintText = view.Q<Label>("emptyListHint");

            Selection.selectionChanged += () => PopulateListView();

            PopulateListView();

            m_LastConsoleCheckTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += DetectLogChanges;
        }

        public override void Show(bool sendVisibilityChanged = true)
        {
            base.Show(sendVisibilityChanged);

            PopulateListView();
        }

        void AddObjectToListView(Object obj, bool addSelection = false)
        {
            if (IsSupportedAsset(obj))
            {
                m_ListView?.AddData(
                    new ListEntry
                    {
                        Object = obj,
                        OnRowClick = (e) => SelectedObject(obj, e),
                        OnFindButtonClick = (e) => PingObject(obj, e),
                        IsSelected = ObjectSelection.Contains(obj) || addSelection
                    }
                );
            }
        }

        void AddObjectsToListView(IList<Object> objs, bool addSelection = false)
        {
            m_ListView.BeginUpdate();
            for (int i = 0; i < objs.Count; i++)
            {
                var obj = objs[i];

                AddObjectToListView(obj, addSelection);

                if (addSelection && !ObjectSelection.Contains(obj))
                    AddObjectToSelection(obj, true);
            }

            m_ListView.EndUpdate();
        }

        void CombinedListPopulate(List<Object> allSelectedObjects, bool addSelection)
        {
            AddObjectsToListView(allSelectedObjects, addSelection);
        }

        public void PopulateListView(bool addSelection = false)
        {
            m_ListView.ClearData();

            if (m_SearchActive || k_SearchResults.Count > 0)
            {
                AddObjectsToListView(k_SearchResults);
                return;
            }

            ConsoleUtils.GetSelectedConsoleLogs(m_LastUpdatedLogReferences);

            // Add selected objects
            ValidateObjectSelection();
            if (Selection.objects.Length > 0 || ObjectSelection.Count > 0)
            {
                // Add combined list of objects currently selected in editor and objects previously selected for context
                if (Selection.objects.Length == 0)
                {
                    CombinedListPopulate(ObjectSelection, addSelection);
                } else if (ObjectSelection.Count == 0)
                {
                    CombinedListPopulate(Selection.objects.ToList(), addSelection);
                } else {
                    CombinedSelection.Clear();
                    CombinedSelection.AddRange(Selection.objects.ToList().Union(ObjectSelection).ToList());
                    CombinedListPopulate(CombinedSelection, addSelection);
                }
            }

            // Add console log entries
            foreach (var logRef in m_LastUpdatedLogReferences)
            {
                var entry = new ListEntry()
                {
                    Object = null,
                    OnRowClick = (SelectionElement e) => SelectedLogReference(logRef, e),
                    LogData = logRef,
                    IsSelected = addSelection || ConsoleUtils.HasEqualLogEntry(ConsoleSelection, logRef)
                };

                if (addSelection && !ConsoleUtils.HasEqualLogEntry(ConsoleSelection, logRef))
                    AddLogReferenceToSelection(logRef, true);

                m_ListView.AddData(entry);
            }

            // Add placeholder text or history as fallback
            if (m_ListView.Data.Count == 0)
            {
                m_EmptyListHintText.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_EmptyListHintText.style.display = DisplayStyle.None;
            }

            RefreshSelectionCount();
        }

        bool IsSupportedAsset(Object obj)
        {
            if (obj is DefaultAsset)
                return false;

            return true;
        }

        void RefreshSelectionCount()
        {
            var nonSelectedItemCount = 0;

            var logs = new List<LogData>();
            ConsoleUtils.GetSelectedConsoleLogs(logs);
            foreach (var log in logs)
                if (!ConsoleUtils.HasEqualLogEntry(ConsoleSelection, log))
                    nonSelectedItemCount++;

            foreach (var obj in Selection.objects)
                if (!ObjectSelection.Contains(obj) && IsSupportedAsset(obj))
                    nonSelectedItemCount++;

            m_AddEditorSelectionButton.text = $"Add Editor selection ({nonSelectedItemCount})";
            m_AddEditorSelectionButton.SetEnabled(nonSelectedItemCount > 0);
        }

        void PingObject(Object obj, SelectionElement e)
        {
            EditorGUIUtility.PingObject(obj);
        }

        void SelectedObject(Object obj, SelectionElement e)
        {
            if (!ObjectSelection.Contains(obj))
            {
                AddObjectToSelection(obj);
                e.SetSelected(true);
            }
            else
            {
                ObjectSelection.Remove(obj);
                e.SetSelected(false);
            }

            OnSelectionChanged?.Invoke();

            RefreshSelectionCount();
        }

        void AddObjectToSelection(Object obj, bool notifySelectionChanged = false)
        {
            ObjectSelection.Add(obj);
            OnContextObjectAdded?.Invoke(obj);

            if (notifySelectionChanged)
                OnSelectionChanged?.Invoke();
        }

        void SelectedLogReference(LogData logRef, SelectionElement e)
        {
            if (!ConsoleUtils.HasEqualLogEntry(ConsoleSelection, logRef))
            {
                AddLogReferenceToSelection(logRef);
                e.SetSelected(true);
            }
            else
            {
                ConsoleSelection.RemoveAll(e => e.Equals(logRef));
                e.SetSelected(false);
            }

            OnSelectionChanged?.Invoke();

            RefreshSelectionCount();
        }

        void AddLogReferenceToSelection(LogData logRef, bool notifySelectionChanged = false)
        {
            ConsoleSelection.Add(logRef);
            OnContextLogAdded?.Invoke(logRef);

            if (notifySelectionChanged)
                OnSelectionChanged?.Invoke();
        }

        void DetectLogChanges()
        {
            if (EditorApplication.timeSinceStartup < m_LastConsoleCheckTime + k_ConsoleCheckInterval)
                return;

            List<LogData> logs = new();
            ConsoleUtils.GetSelectedConsoleLogs(logs);

            if (m_LastUpdatedLogReferences.Count != logs.Count
                || m_LastUpdatedLogReferences.Any(log => !ConsoleUtils.HasEqualLogEntry(logs, log))
                || logs.Any(log => !ConsoleUtils.HasEqualLogEntry(m_LastUpdatedLogReferences, log)) )
            {
                PopulateListView();
            }

            m_LastConsoleCheckTime = EditorApplication.timeSinceStartup;
        }

        void ValidateObjectSelection()
        {
            for (var i = ObjectSelection.Count - 1; i >= 0; i--)
            {
                if (ObjectSelection[i] == null)
                {
                    ObjectSelection.RemoveAt(i);
                }
            }
        }
    }
}
