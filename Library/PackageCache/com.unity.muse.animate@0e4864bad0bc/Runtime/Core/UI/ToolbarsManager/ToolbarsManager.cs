using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate.Toolbar
{ 
    class ToolbarsManager
    {
        public Func<bool> IsVisibleCallback { get; set; }
        bool HasToRebuild { get; set; }
        bool HasToRefresh { get; set; }
        
        public bool IsVisible
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
        
        List<Toolbar> Children { get; }
        List<Toolbar> VisibleChildren { get; }
        VisualElement Component { get; set; }
        
        bool m_IsVisible;

        public ToolbarsManager()
        {
            Children = new List<Toolbar>();
            VisibleChildren = new List<Toolbar>();
            Component = null;
            HasToRebuild = true;
        }

        public void SetContainer(VisualElement container)
        {
            Component = container;
        }

        public void Update()
        {
            // Update Self
            IsVisible = IsVisibleCallback == null || IsVisibleCallback.Invoke();

            // Update Children
            for (var i = 0; i < Children.Count; i++)
            {
                Children[i].CheckForChanges();
                HasToRebuild = HasToRebuild || Children[i].HasToRebuild;
            }

            if (HasToRebuild)
                Rebuild();
            
            for (var i = 0; i < Children.Count; i++)
            {
                Children[i].CheckForRefresh();
                HasToRefresh = HasToRefresh || Children[i].HasToRefresh;
            }
            
            if (HasToRefresh)
                Refresh();
        }

        void Refresh()
        {
            HasToRefresh = false;

            for (var i = 0; i < VisibleChildren.Count; i++)
            {
                VisibleChildren[i].Refresh();
            }
        }
        
        void Rebuild()
        {
            Assert.IsNotNull(Component, "Couldn't rebuild toolbars, provided VisualElement Component is null");
            HasToRebuild = false;
            
            Clear();

            if (!IsVisible)
                return;

            for (var i = 0; i < Children.Count; i++)
            {
                Children[i].Rebuild(Component);
                
                if (Children[i].IsVisible)
                    VisibleChildren.Add(Children[i]);
            }

            HasToRefresh = true;
        }

        void Clear()
        {
            VisibleChildren.Clear();

            Component?.Clear();

            for (var i = 0; i < Children.Count; i++)
            {
                var data = Children[i];
                data.Clear();
            }
        }

        public Toolbar AddToolbar(string id)
        {
            var group = new Toolbar(id);
            Children.Add(group);
            return group;
        }
    }
}
