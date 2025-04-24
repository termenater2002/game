using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    partial class AdaptiveListView<TD, TV> : VisualElement
        where TV: AdaptiveListViewEntry
    {
        const string k_BaseClassName = "adaptive-list-view";

        readonly IList<TD> k_Data = new List<TD>();
        readonly IList<TV> k_VisualElements = new List<TV>();
        readonly List<int> k_PendingUpdates = new();
        readonly List<bool> k_SelectionState = new();

        Scroller m_VerticalScroller;

        ListView m_InnerList;
        ScrollView m_InnerScroll;

        bool m_UpdateInProgress;
        bool m_RefreshRequired;

        public bool EnableSelection = false;
        public bool EnableScrollLock = false;
        public bool EnableVirtualization = true;
        public bool EnableDelayedElements = false;
        public bool EnableHorizontalScroll = false;

        public int DelayedElementOperations = 5;

        public IList<TD> Data => k_Data;

        public event Action<int, TD> SelectionChanged;

        public void Initialize()
        {
            AddToClassList(k_BaseClassName);

            if (EnableVirtualization)
            {
                m_InnerList = new ListView();
                m_InnerList.AddToClassList(k_BaseClassName + "-inner");
                m_InnerList.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                Add(m_InnerList);

                m_VerticalScroller = m_InnerList.Q<Scroller>(null, "unity-scroller--vertical");
                m_VerticalScroller.valueChanged += OnVerticallyScrolled;

                m_InnerList.selectionType = EnableSelection ? SelectionType.Single : SelectionType.None;
                m_InnerList.horizontalScrollingEnabled = false;
                m_InnerList.itemsSource = (IList)k_Data;
                m_InnerList.makeItem = MakeItem;
                m_InnerList.bindItem = BindItem;
                m_InnerList.fixedItemHeight = 100;
                m_InnerList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            }
            else
            {
                if (EnableSelection)
                {
                    throw new NotSupportedException("Selection not supported without virtualization");
                }

                m_InnerScroll = new ScrollView();
                m_InnerScroll.AddToClassList(k_BaseClassName + "-inner");

                if (EnableHorizontalScroll == false)
                    m_InnerScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

                Add(m_InnerScroll);

                m_VerticalScroller = m_InnerScroll.Q<Scroller>(null, "unity-scroller--vertical");
                m_VerticalScroller.valueChanged += OnVerticallyScrolled;
            }
        }

        protected virtual void OnDataSelected(int index, VisualElement element)
        {
            bool selectionChanged = false;
            for (var i = 0; i < k_SelectionState.Count; i++)
            {
                if (k_SelectionState[i])
                {
                    if (i == index)
                    {
                        return;
                    }

                    selectionChanged = true;
                    k_SelectionState[i] = false;
                    RefreshElement(i);
                    continue;
                }

                if (i == index)
                {
                    selectionChanged = true;
                    k_SelectionState[i] = true;
                    RefreshElement(i);
                }
            }

            if (selectionChanged)
            {
                SelectionChanged?.Invoke(index, k_Data[index]);
            }
        }

        TV MakeItem()
        {
            var element = Activator.CreateInstance<TV>();
            element.Initialize();
            element.SelectionChanged += OnDataSelected;
            return element;
        }

        void DestroyItem(TV element)
        {
            element.SelectionChanged -= OnDataSelected;
        }

        public void AddData(TD message)
        {
            k_Data.Add(message);
            k_SelectionState.Add(false);
            m_RefreshRequired = true;

            if (!m_UpdateInProgress)
            {
                k_PendingUpdates.Add(k_Data.Count - 1);
                DoRefreshList(false);
            }
        }

        public void UpdateData(int index, TD data)
        {
            m_RefreshRequired = true;
            k_Data[index] = data;

            if (m_UpdateInProgress)
            {
                return;
            }

            RefreshElement(index);
        }

        void RefreshIfRequired()
        {
            if (m_RefreshRequired)
            {
                DoRefreshList(false);
            }
        }

        void DoRefreshList(bool fullRefresh)
        {
            m_RefreshRequired = false;

            if (EnableVirtualization)
            {
                m_InnerList.RefreshItems();
                EnforceScrollState();
                return;
            }

            if (fullRefresh)
            {
                k_PendingUpdates.Clear();
                for (var i = 0; i < k_Data.Count; i++)
                {
                    QueueUpdate(i);
                }
            }

            if (EnableDelayedElements)
            {
                EditorApplication.delayCall += ContinueCreateElements;
                EditorApplication.delayCall += ContinueUpdateElements;
            }
            else
            {
                ContinueCreateElements();
                ContinueUpdateElements();
            }
        }

        void RefreshElement(int index)
        {
            if (EnableVirtualization)
            {
                m_InnerList.RefreshItem(index);
            }
            else
            {
                QueueUpdate(index);
                if (EnableDelayedElements)
                {
                    EditorApplication.delayCall += ContinueUpdateElements;
                }
                else
                {
                    ContinueUpdateElements();
                }
            }
        }

        public void RemoveData(int index)
        {
            if (EnableVirtualization)
            {
                m_RefreshRequired = true;
                k_Data.RemoveAt(index);
                k_SelectionState.RemoveAt(index);
                DoRefreshList(false);
            }
            else
            {
                m_RefreshRequired = true;
                k_Data.RemoveAt(index);
                k_SelectionState.RemoveAt(index);

                var element = k_VisualElements[index];
                k_VisualElements.RemoveAt(index);
                if (element != null)
                {
                    m_InnerScroll.Remove(element);
                    DestroyItem(element);
                }
            }
        }

        public void BeginUpdate()
        {
            m_UpdateInProgress = true;
        }

        public void EndUpdate(bool scrollToEnd = true)
        {
            m_UpdateInProgress = false;
            DoRefreshList(true);

            if (scrollToEnd)
            {
                ChangeScrollState(ScrollState.ScrollToEnd);
            }
        }

        public void SetSelection(int index, bool state)
        {
            ClearSelection();
            k_SelectionState[index] = state;
            RefreshElement(index);

            SelectionChanged?.Invoke(index, k_Data[index]);
        }

        public void SetSelectionWithoutNotify(int index, bool state)
        {
            ClearSelection(false);
            k_SelectionState[index] = state;
            RefreshElement(index);
        }

        public void ClearSelection(bool notify = true)
        {
            if (EnableVirtualization)
            {
                m_InnerList.ClearSelection();

                bool selectionChanged = false;
                for (var i = 0; i < k_SelectionState.Count; i++)
                {
                    if (k_SelectionState[i])
                    {
                        selectionChanged = true;
                        k_SelectionState[i] = false;
                        RefreshElement(i);
                    }
                }

                if (notify && selectionChanged)
                {
                    SelectionChanged?.Invoke(-1, default);
                }
            }
            else
            {
                throw new NotSupportedException("Selection not supported without virtualization");
            }
        }

        public void ClearData()
        {
            m_RefreshRequired = true;
            k_Data.Clear();
            k_SelectionState.Clear();

            if (EnableVirtualization)
            {
                m_InnerList.RefreshItems();
            }
            else
            {
                for (var i = 0; i < k_VisualElements.Count; i++)
                {
                    TV element = k_VisualElements[i];
                    m_InnerScroll.Remove(element);
                    DestroyItem(element);
                }

                k_VisualElements.Clear();
            }

            ChangeScrollState(ScrollState.None);
        }

        void BindItem(VisualElement element, int messageIndex)
        {
            var wrapper = (TV)element;
            wrapper.SetData(messageIndex, k_Data[messageIndex], k_SelectionState[messageIndex]);
        }

        void ContinueCreateElements()
        {
            int operations = 0;
            while (k_VisualElements.Count < k_Data.Count)
            {
                operations++;
                var element = MakeItem();
                k_VisualElements.Add(element);
                m_InnerScroll.Add(element);

                if (EnableDelayedElements && operations >= DelayedElementOperations)
                {
                    EditorApplication.delayCall += ContinueCreateElements;
                    return;
                }
            }
        }

        void QueueUpdate(int index, bool executeImmediate = false)
        {
            if (k_PendingUpdates.Contains(index))
            {
                return;
            }

            k_PendingUpdates.Add(index);

            if (executeImmediate)
            {
                ContinueUpdateElements();
            }
        }

        void ContinueUpdateElements()
        {
            if (k_PendingUpdates.Count == 0)
            {
                return;
            }

            int operations = 0;
            while (k_PendingUpdates.Count > 0)
            {
                operations++;
                int index = k_PendingUpdates[0];
                k_PendingUpdates.RemoveAt(0);

                if (k_VisualElements.Count <= index)
                {
                    // Not all elements have been created yet, re-queue and try later
                    k_PendingUpdates.Insert(0, index);
                    return;
                }

                TD data = k_Data[index];
                k_VisualElements[index].SetData(index, data, k_SelectionState[index]);

                if (EnableDelayedElements && operations >= DelayedElementOperations)
                {
                    EditorApplication.delayCall += ContinueUpdateElements;
                    break;
                }
            }

            EnforceScrollState();
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            EnforceScrollState();
        }
    }
}
