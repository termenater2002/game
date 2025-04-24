using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents an item in the application menu.
    /// </summary>
    class MenuItemModel
    {
        /// <summary>
        /// A '/' delimited path to the item in the menu hierarchy.
        /// </summary>
        /// <remarks>
        /// You can use the delimiter to group items into submenus. For example, specifying a path of
        /// "Game/Objective" will create a menu item called "Objective" in a submenu called "Game". It is not
        /// to necessary to create the "Game" menu item explicitly.
        /// </remarks>
        public string Path { get; }
            
        /// <summary>
        /// The label of the menu item to display. This is the last part of the <see cref="Path"/> (without the
        /// parent menu names).
        /// </summary>
        public string DisplayText => Path.Substring(Path.LastIndexOf('/') + 1);
            
        /// <summary>
        /// The action to execute when the menu item is clicked.
        /// </summary>
        public Action Action { get; }
            
        /// <summary>
        /// A shortcut text to display next to the label.
        /// </summary>
        /// <remarks> For display purposes only. Does not create the shortcut.</remarks>
        public string ShortcutText { get; }
            
        /// <summary>
        /// Function to retrieve the enabled state of the menu item.
        /// </summary>
        /// <remarks>
        /// Default is null (item always enabled). If specified, the function is called every time the menu is
        /// opened.
        /// </remarks>
        public Func<bool> IsEnabledCallback { get; }
            
        /// <summary>
        /// The icon to display next to the label.
        /// </summary>
        public string IconName { get; }
            
        /// <summary>
        /// Name of the section to place the item in.
        /// </summary>
        /// <remarks>
        /// Default is no section. Use a name beginning <see cref="ApplicationMenuUIModel.SectionNamePlaceholder"/>
        /// to create a divider (no name displayed).
        /// </remarks>
        public string SectionName { get; }
            
        /// <summary>
        /// Determines the ordering of items.
        /// </summary>
        /// <remarks>
        /// Default is 0. Items with the same index appear in the order they were added.
        /// </remarks>
        public int Index { get; }
            
        /// <summary>
        /// Create a new <see cref="MenuItemModel"/>.
        /// </summary>
        /// <param name="path">A '/' delimited path to the item in the menu hierarchy.</param>
        /// <param name="action">The action to execute when the menu item is clicked.</param>
        /// <param name="shortcutText">A shortcut text to display next to the label.</param>
        /// <param name="isEnabledCallback">Function to retrieve the enabled state of the menu item.</param>
        /// <param name="iconName">The icon to display next to the label.</param>
        /// <param name="sectionName">Name of the section to place the item in.</param>
        /// <param name="index">Determines the ordering of items.</param>
        public MenuItemModel(string path,
            Action action,
            string shortcutText = "",
            Func<bool> isEnabledCallback = null,
            string iconName = "",
            string sectionName = null,
            int index = 0)
        {
            Path = path;
            IconName = iconName;
            ShortcutText = shortcutText;
            Action = action;
            Index = index;
            IsEnabledCallback = isEnabledCallback;
            SectionName = sectionName;
            IsEnabledCallback = isEnabledCallback;
        }
    }
    
    class ApplicationMenuUIModel
    {
        public const string SectionNamePlaceholder = "--";
        
        /// <summary>
        /// Menu items changed.
        /// </summary>
        public event Action OnChanged;
        
        public PathTree<MenuItemModel> MenuTree => m_CachedMenuTree ??= PathTree<MenuItemModel>.Build(m_Items);
        
        readonly Dictionary<string, MenuItemModel> m_Items = new();
        
        PathTree<MenuItemModel> m_CachedMenuTree;
        
        public MenuItemModel GetItem(string path)
        {
            return m_Items.TryGetValue(path, out var item) ? item : null;
        }

        void OnItemsChanged()
        {
            m_CachedMenuTree = null;
            OnChanged?.Invoke();
        }
        
        public void AddItem(MenuItemModel item)
        {
            if (m_Items.ContainsKey(item.Path))
            {
                Debug.LogWarning($"Duplicate menu item: {item.Path}");
                return;
            }
            
            m_Items.Add(item.Path, item);
            OnItemsChanged();
        }
        
        public void AddItems(IEnumerable<MenuItemModel> items)
        {
            foreach (var item in items)
            {
                if (m_Items.ContainsKey(item.Path))
                {
                    Debug.LogWarning($"Duplicate menu item: {item.Path}");
                    continue;
                }
                m_Items.Add(item.Path, item);
            }
            OnItemsChanged();
        }

        public void RemoveItem(string path)
        {
            m_Items.Remove(path);
            OnItemsChanged();
        }
        
        public void RemoveItem(MenuItemModel item)
        {
            m_Items.Remove(item.Path);
            OnItemsChanged();
        }
        
        public void RemoveItems(IEnumerable<MenuItemModel> items)
        {
            foreach (var item in items)
            {
                m_Items.Remove(item.Path);
            }
            OnItemsChanged();
        }
    }
}
