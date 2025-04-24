using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate.Toolbar
{
    class Toolbar
    {
        public Func<bool> IsVisibleCallback { get; set; }
        public string Id { get; }
        internal bool HasToRebuild { get; private set; }
        internal bool HasToRefresh { get; private set; }
        internal bool IsVisible
        {
            get => m_IsVisible;
            private set
            {
                if (value == m_IsVisible)
                    return;

                HasToRebuild = true;
                m_IsVisible = value;
            }
        }
        
        List<Section> Children { get; }
        List<Section> VisibleChildren { get; }
        VisualElement Component { get; set; }
        
        bool m_IsVisible;

        internal Toolbar(string id)
        {
            Id = id;
            Children = new List<Section>();
            VisibleChildren = new List<Section>();
            HasToRebuild = true;
        }

        internal void CheckForChanges()
        {
            // Check Self
            IsVisible = IsVisibleCallback == null || IsVisibleCallback.Invoke();

            // Check Children
            for (var i = 0; i < Children.Count; i++)
            {
                Children[i].CheckForChanges();
                HasToRebuild = HasToRebuild || Children[i].HasToRebuild;
            }
        }
        
        internal void CheckForRefresh()
        {
            // Check Children
            for (var i = 0; i < Children.Count; i++)
            {
                Children[i].CheckForRefresh();
                HasToRefresh = HasToRefresh || Children[i].HasToRefresh;
            }
        }
        
        internal void Rebuild(VisualElement parentContainer)
        {
            Assert.IsNotNull(parentContainer, "Couldn't rebuild toolbar, provided VisualElement container is null");
            HasToRebuild = false;

            Clear();

            if (!IsVisible)
                return;

            Component = new VisualElement();
            Component.AddToClassList("deeppose-toolbar");
            parentContainer.Add(Component);

            for (var i = 0; i < Children.Count; i++)
            {
                Children[i].Rebuild(Component);
                
                if (Children[i].IsVisible)
                    VisibleChildren.Add(Children[i]);
            }

            HasToRefresh = true;
        }

        internal void Refresh()
        {
            HasToRefresh = false;
            
            if (VisibleChildren.Count > 1)
            {
                for (var i = 0; i < VisibleChildren.Count-1; i++)
                {
                    VisibleChildren[i].IsLast = false;
                }

                VisibleChildren[^1].IsLast = true;
            }
            
            for (var i = 0; i < VisibleChildren.Count; i++)
            {
                VisibleChildren[i].Refresh();
            }
        }

        internal void Clear()
        {
            VisibleChildren.Clear();

            if (Component != null)
            {
                Component.Clear();
                Component = null;
            }

            for (var i = 0; i < Children.Count; i++)
            {
                var data = Children[i];
                data.Clear();
            }
        }

        internal Section AddSection(string id, SelectionType selectionType = SelectionType.None)
        {
            var existing = Children.FirstOrDefault(item => item.Id == id);
            Assert.IsNull(existing, $"A section with the same id ({id}) already exists in the toolbar (id:{Id}).");
            var child = new Section(id, selectionType);
            child.IsLast = true;
            Children.Add(child);
            return child;
        }
    }
}
