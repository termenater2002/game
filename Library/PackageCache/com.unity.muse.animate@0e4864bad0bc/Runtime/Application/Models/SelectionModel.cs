using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Defines a selection state and handles requests related to it.
    /// </summary>
    [Serializable]
    class SelectionModel<T> : ICopyable<SelectionModel<T>>
    {
        [SerializeField]
        SelectionData<T> m_Data;

        public bool HasSelection => m_Data.HasSelection;
        public int Count => m_Data.Count;

        public delegate void SelectionChanged(SelectionModel<T> model);
        public event SelectionChanged OnSelectionChanged;

        public delegate void SelectionStateChanged(SelectionModel<T> model, T index, bool isSelected);
        public event SelectionStateChanged OnSelectionStateChanged;

        public List<T> Selection => m_Data.Selected;

        public SelectionModel(int capacity = 64)
        {
            m_Data = new SelectionData<T>(capacity);
        }

        public SelectionModel(SelectionData<T> initData)
        {
            m_Data = new SelectionData<T>(initData);
        }

        public T GetSelection(int index)
        {
            if (index < 0 || index >= m_Data.Selected.Count)
                AssertUtils.Fail($"Invalid index: {index.ToString()}");
            return m_Data.Selected[index];
        }

        public void Select(T index)
        {
            Assert.IsTrue(m_Data.IsValid);

            if (m_Data.Selected.Contains(index))
                return;

            m_Data.Selected.Add(index);
            OnSelectionStateChanged?.Invoke(this, index, true);
            OnSelectionChanged?.Invoke(this);
        }
        
        public void SetSelection(T index)
        {
            Assert.IsTrue(m_Data.IsValid);
            
            var changed = false;
            using var oldSelection = TempList<T>.Allocate();
            oldSelection.List.AddRange(m_Data.Selected);
            m_Data.Selected.Clear();
            m_Data.Selected.Add(index);
            
            foreach (var oldIndex in oldSelection)
            {
                if (!m_Data.Selected.Contains(oldIndex))
                {
                    // Previous selection removed
                    OnSelectionStateChanged?.Invoke(this, oldIndex, false);
                    changed = true;
                }
            }

            if (!oldSelection.Contains(index))
            {
                // Is a new selection
                OnSelectionStateChanged?.Invoke(this, index, true);
                changed = true;
            }
            
            if (changed)
            {
                OnSelectionChanged?.Invoke(this);
            }
        }
        
        public void SetSelection(IEnumerable<T> indices)
        {
            Assert.IsTrue(m_Data.IsValid);
            
            var changed = false;
            using var oldSelection = TempList<T>.Allocate();
            oldSelection.List.AddRange(m_Data.Selected);
            m_Data.Selected.Clear();
            
            foreach (var index in indices)
            {
                // Don't allow duplicate selections
                if (m_Data.Selected.Contains(index))
                    continue;

                m_Data.Selected.Add(index);
                if (!oldSelection.Contains(index))
                {
                    // New selection added
                    OnSelectionStateChanged?.Invoke(this, index, true);
                    changed = true;
                }
            }

            foreach (var index in oldSelection)
            {
                if (!m_Data.Selected.Contains(index))
                {
                    // Old selection removed
                    OnSelectionStateChanged?.Invoke(this, index, false);
                    changed = true;
                }
            }

            if (changed)
            {
                OnSelectionChanged?.Invoke(this);
            }
        }

        public void Select(ICollection<T> indices)
        {
            Assert.IsTrue(m_Data.IsValid);
            if (indices == null || indices.Count == 0)
                return;

            var changed = false;
            foreach (var index in indices)
            {
                if (m_Data.Selected.Contains(index))
                    continue;

                m_Data.Selected.Add(index);
                OnSelectionStateChanged?.Invoke(this, index, true);
                changed = true;
            }

            if (changed)
            {
                OnSelectionChanged?.Invoke(this);
            }
        }

        public void Unselect(T index)
        {
            Assert.IsTrue(m_Data.IsValid);

            if (m_Data.Selected.Remove(index))
            {
                OnSelectionStateChanged?.Invoke(this, index, false);
                OnSelectionChanged?.Invoke(this);
            }
        }

        public void Unselect(ICollection<T> indices)
        {
            Assert.IsTrue(m_Data.IsValid);
            if (indices == null || indices.Count == 0)
                return;

            var selectionChanged = false;
            foreach (var index in indices)
            {
                if (m_Data.Selected.Remove(index))
                {
                    OnSelectionStateChanged?.Invoke(this, index, false);
                    selectionChanged = true;
                }
            }

            if (selectionChanged)
                OnSelectionChanged?.Invoke(this);
        }

        public void SetSelected(T index, bool isSelected)
        {
            if (isSelected)
            {
                Select(index);
            }
            else
            {
                Unselect(index);
            }
        }

        public void Clear()
        {
            Assert.IsTrue(m_Data.IsValid);

            if (m_Data.Selected.Count == 0)
                return;

            using var tmpList = TempList<T>.Allocate();
            tmpList.List.AddRange(m_Data.Selected);

            m_Data.Selected.Clear();

            foreach (var index in tmpList)
            {
                OnSelectionStateChanged?.Invoke(this, index, false);
            }
            OnSelectionChanged?.Invoke(this);
        }

        public bool IsSelected(T index)
        {
            Assert.IsTrue(m_Data.IsValid);
            return m_Data.Selected.Contains(index);
        }

        public void CopyTo(SelectionModel<T> target)
        {
            m_Data.CopyTo(ref target.m_Data);
        }

        public SelectionModel<T> Clone()
        {
            return new SelectionModel<T>(m_Data);
        }
    }
}
