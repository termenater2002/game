using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate.Toolbar
{
    class Button
    {
        public string Id;
        public string Icon;
        public string Label;
        public string Tooltip;
        public Func<bool> IsVisibleCallback;
        public Func<bool> IsEnabledCallback;
        public Func<bool> IsActiveCallback;
        public Action ClickedCallback;
        public bool HasToRebuild { get; private set; }
        public bool HasToRefresh { get; private set; }
        ActionButton Component { get; set; }
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

        bool IsEnabled
        {
            get => m_IsEnabled;
            set
            {
                if (value == m_IsEnabled)
                    return;

                HasToRefresh = true;
                m_IsEnabled = value;
            }
        }

        public bool IsActive
        {
            get => m_IsActive;
            private set
            {
                if (value == m_IsActive)
                    return;

                HasToRefresh = true;
                m_IsActive = value;
            }
        }

        bool m_IsVisible;
        bool m_IsEnabled;
        bool m_IsActive;

        internal void CheckForChanges()
        {
            // Update Self
            IsActive = IsActiveCallback != null && IsActiveCallback.Invoke();
            IsEnabled = IsEnabledCallback == null || IsEnabledCallback.Invoke();
            IsVisible = IsVisibleCallback == null || IsVisibleCallback.Invoke();
        }

        internal void Rebuild(ActionGroup parentGroup)
        {
            Assert.IsNotNull(parentGroup, "Couldn't rebuild button, provided ActionGroup parentGroup is null");
            HasToRebuild = false;

            Clear();

            if (!IsVisible)
                return;

            Component = new ActionButton();
            Component.name = Id;
            Component.clicked += OnButtonClicked;
            parentGroup.Add(Component);

            HasToRefresh = true;
        }

        internal void Refresh()
        {
            Component.icon = Icon;
            Component.label = Label;
            Component.tooltip = Tooltip;
            Component.SetEnabled(IsEnabled);
        }

        internal void Clear()
        {
            if (Component != null)
            {
                Component.clicked -= OnButtonClicked;
                Component.Clear();
                Component = null;
            }
        }

        void OnButtonClicked()
        {
            ClickedCallback?.Invoke();
        }
    }
}
