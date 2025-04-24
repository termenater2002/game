using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Defines a selection state.
    /// </summary>
    [Serializable]
    struct SelectionData<T>
    {
        public List<T> Selected;

        public bool HasSelection => Selected.Count > 0;
        public int Count => Selected.Count;

        public bool IsValid => Selected != null;

        public SelectionData(int capacity)
        {
            Selected = new List<T>(capacity);
        }

        public SelectionData(SelectionData<T> other)
        {
            Selected = new List<T>();
            other.CopyTo(ref this);
        }

        public void CopyTo(ref SelectionData<T> other)
        {
            other.Selected.Clear();
            foreach (var v in Selected)
            {
                other.Selected.Add(v);
            }
        }
    }
}
