using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate.Toolbar
{
    class Section
    {
        List<Button> Children { get; }
        public string Id { get; }
        internal bool HasToRebuild { get; private set; }
        internal bool HasToRefresh { get; private set; }
        public Func<bool> IsVisibleCallback { get; set; }

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

        bool m_IsVisible;
        bool m_IsLast;
        SelectionType m_SelectionType;

        ActionGroup Component { get; set; }
        List<int> SelectedIndices { get; }
        List<Button> VisibleChildren { get; }

        internal bool IsLast
        {
            get => m_IsLast;
            set
            {
                if (value == m_IsLast)
                    return;

                HasToRefresh = true;
                m_IsLast = value;
            }
        }

        internal SelectionType SelectionType
        {
            get => m_SelectionType;
            set
            {
                if (value == m_SelectionType)
                    return;

                HasToRefresh = true;
                m_SelectionType = value;
            }
        }

        internal Section(string id, SelectionType selectionType)
        {
            Id = id;
            SelectionType = selectionType;
            Children = new List<Button>();
            SelectedIndices = new List<int>();
            VisibleChildren = new List<Button>();
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
                HasToRefresh = HasToRefresh || Children[i].HasToRefresh;
            }
        }

        internal void Rebuild(VisualElement parentToolbar)
        {
            Assert.IsNotNull(parentToolbar, "Couldn't rebuild section, provided parentToolbar is null");
            HasToRebuild = false;

            Clear();

            if (!IsVisible)
                return;

            Component = new ActionGroup();
            Component.AddToClassList("deeppose-toolbar-section");

            parentToolbar.Add(Component);

            for (var i = 0; i < Children.Count; i++)
            {
                var data = Children[i];
                data.Rebuild(Component);

                if (data.IsVisible)
                    VisibleChildren.Add(data);
            }

            if (Component.childCount > 0)
            {
                Component.selectionType = SelectionType;
            }

            HasToRefresh = true;
        }

        internal void Refresh()
        {
            HasToRefresh = false;
            
            // Refresh the styling related to if the section is last of it's toolbar
            Component.EnableInClassList("unity-last-child", IsLast);
            
            SelectedIndices.Clear();

            // Refresh the children components
            for (var i = 0; i < VisibleChildren.Count; i++)
            {
                var data = VisibleChildren[i];

                if (data.HasToRefresh)
                    data.Refresh();
            }

            // Refresh the selected index
            // Note: search amongst the visible children only,
            // since the ActionGroup only contains the visible children
            if (SelectionType != SelectionType.None)
            {
                for (var i = 0; i < VisibleChildren.Count; i++)
                {
                    var data = VisibleChildren[i];

                    if (data.IsActive)
                        SelectedIndices.Add(i);
                }
            }

            // Refresh the ActionGroup's selected index
            // Component.SetSelectionWithoutNotify(SelectedIndices);
            if (SelectedIndices.Count > 0)
            {
                Component.SetSelectionWithoutNotify(SelectedIndices);
            }
            else
            {
                Component.ClearSelectionWithoutNotify();
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

        internal Button AddButton(string id, string icon, string label = "", string tooltip = "")
        {
            var existing = Children.FirstOrDefault(item => item.Id == id);
            Assert.IsNull(existing, $"A button with the same id ({id}) already exists in the section (id:{Id}).");

            var button = new Button()
            {
                Id = id,
                Icon = icon,
                Label = label,
                Tooltip = tooltip
            };

            Children.Add(button);
            return button;
        }
    }
}
