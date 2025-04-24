using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
    
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class ApplicationMenuUI : UITemplateContainer, IUITemplate
    {
        const string k_ButtonName = "application-menu-button";


#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<ApplicationMenuUI, UxmlTraits> { }
#endif

        ApplicationMenuUIModel m_Model;
        ActionButton m_Button;

        public ApplicationMenuUI()
            : base("deeppose-application-menu")
        {
        }

        public void FindComponents()
        {
            m_Button = this.Q<ActionButton>(k_ButtonName);
        }

        public void RegisterComponents()
        {
            m_Button.clicked += ShowMenu;
        }

        public void UnregisterComponents()
        {
            m_Button.clicked -= ShowMenu;
        }

        void ShowMenu()
        {
            // TODO: In theory, we should be able to cache and reuse the Menu objects. However, there some some issues
            // with opened/closed states in App UI, so it's more reliable to use create them from scratch each time.
            MenuBuilder.Build(m_Button, CreateMenu()).Show();
        }

        public void SetModel(ApplicationMenuUIModel model)
        {
            UnregisterModel();
            m_Model = model;
            if (m_Model != null)
            {
                m_Model.OnChanged += OnMenuChanged;
            }
        }

        void UnregisterModel()
        {
            if (m_Model != null)
            {
                m_Model.OnChanged -= OnMenuChanged;
            }

            m_Model = null;
        }

        void OnMenuChanged()
        {
            // Nothing to do here for now.
        }

        static void AddMenuItems(VisualElement parentMenu, PathTree<MenuItemModel>.Node node)
        {
            var items = node.ChildNodes;
            var sections = items.GroupBy(x => x.Data.SectionName);
            var firstSection = true;
            foreach (var section in sections)
            {
                var sectionParent = parentMenu;
                if (!string.IsNullOrEmpty(section.Key))
                {
                    if (section.Key.StartsWith(ApplicationMenuUIModel.SectionNamePlaceholder))
                    {
                        if (!firstSection)
                            parentMenu.Add(new MenuDivider());
                    }
                    else
                    {
                        var menuSection = new MenuSection
                        {
                            title = section.Key
                        };
                        parentMenu.Add(menuSection);
                        sectionParent = menuSection;
                    }
                }

                foreach (var item in section.OrderBy(x => x.Data.Index))
                {
                    var menuItemInfo = item.Data;
                    var menuItem = new MenuItem
                    {
                        label = menuItemInfo?.DisplayText,
                        icon = menuItemInfo?.IconName ?? "",
                        userData = item.Data,
                        shortcut = menuItemInfo?.ShortcutText ?? ""
                    };
                    
                    var itemEnabled = menuItemInfo?.IsEnabledCallback?.Invoke() ?? true;
                    menuItem.SetEnabled(itemEnabled);

                    if (!item.ChildNodes.Any())
                    {
                        // This is normal menu item
                        Assert.IsNotNull(menuItemInfo);
                        menuItem.RegisterCallback<ClickEvent>(_ => menuItemInfo.Action());
                        sectionParent.Add(menuItem);
                    }
                    else
                    {
                        // This is a sub-menu
                        var subMenu = new Menu();
                        menuItem.subMenu = subMenu;
                        AddMenuItems(subMenu, item);

                        sectionParent.Add(menuItem);
                    }
                }

                firstSection = false;
            }
        }

        Menu CreateMenu()
        {
            var rootMenu = new Menu();
            AddMenuItems(rootMenu, m_Model.MenuTree.Root);
            return rootMenu;
        }
    }
}
