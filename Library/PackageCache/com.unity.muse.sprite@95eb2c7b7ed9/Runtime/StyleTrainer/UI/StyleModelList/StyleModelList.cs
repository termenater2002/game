using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Common.Account;
using Unity.Muse.Sprite.Common.DebugConfig;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.StyleModelEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelListUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleTrainerProjectEvents;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.StyleTrainer
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class StyleModelList : ExVisualElement
    {
        ListView m_ListView;
        SearchBar m_SearchBar;
        ExVisualElement m_StyleModelTitleContainer;
        ExVisualElement m_AddStyleContainer;
        Button m_AddStyleButton;
        Button m_AddStyleButtonCollapsed;
        EventBus m_EventBus;
        ContextualMenuManipulator m_DebugMenu;

        void BindElements()
        {
            m_AddStyleButton = this.Q<Button>("styletrainer-stylemodellist__add-button");
            m_AddStyleButton.clicked += OnAddStyleClicked;
            m_AddStyleButtonCollapsed = this.Q<Button>("AddStyleButtonCollapsed");
            m_AddStyleButtonCollapsed.clicked += OnAddStyleClicked;
            if (DebugConfig.developerMode) m_DebugMenu = new ContextualMenuManipulator(DebugMenuBuilder);
            this.AddManipulator(m_DebugMenu);
            SetupListView();
            SetupSearchBar();
            m_StyleModelTitleContainer = this.Q<ExVisualElement>("StyleModelTitleContainer");
            m_AddStyleContainer = this.Q<ExVisualElement>("AddStyleContainer");

            var notifications = this.Q<VisualElement>("notification-area");
            notifications.Add(new SessionStatusNotifications());
            m_AddStyleButton.AddManipulator(new SessionStatusTracker());
            m_AddStyleButtonCollapsed.AddManipulator(new SessionStatusTracker());
        }

        void DebugMenuBuilder(ContextualMenuPopulateEvent obj)
        {
            obj.menu.AppendAction("DEVELOPER: Load Styles", LoadStyles, DropdownMenuAction.AlwaysEnabled);
        }

        void LoadStyles(DropdownMenuAction obj)
        {
            m_EventBus.SendEvent(new LoadStyleProjectEvent());
        }

        void SetupSearchBar()
        {
            m_SearchBar = this.Q<SearchBar>("StyleModelSearchBar");
            m_SearchBar.trailingElement.pickingMode = PickingMode.Position;
            m_SearchBar.trailingElement.RegisterCallback<MouseDownEvent>(OnClearSearch);
            m_SearchBar.trailingElement.EnableInClassList("hidden", string.IsNullOrEmpty(m_SearchBar.value));
            m_SearchBar.RegisterValueChangingCallback(OnSearch);
        }

        void OnClearSearch(MouseDownEvent evt)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                m_SearchBar.value = "";
                SearchInList(m_SearchBar.value);
            }
        }

        void OnSearch(ChangingEvent<string> evt)
        {
            SearchInList(evt.newValue);
        }

        void SearchInList(string searchStyle)
        {
            m_EventBus.SendEvent(new SearchStyleEvent() { search =  searchStyle});
            m_SearchBar.trailingElement.EnableInClassList("hidden", string.IsNullOrEmpty(m_SearchBar.value));
        }

        void SetupListView()
        {
            m_ListView = this.Q<ListView>("StyleModelListView");
            m_ListView.reorderable = false;
            m_ListView.makeItem = MakeItem;
            m_ListView.itemsSource = new List<StyleModelListItem>();
            m_ListView.bindItem = (element, i) =>
            {
                var item = (StyleModelListItem)element;
                item.Init(m_ListView.itemsSource[i] as StyleData, m_EventBus);
            };
            m_ListView.unbindItem = (element, _) =>
            {
                var item = (StyleModelListItem)element;
                item.UnbindItem();
            };
            m_ListView.selectionChanged += OnSelectionChanged;
        }

        void OnSelectionChanged(IEnumerable<object> obj)
        {
            var evt = new StyleModelListSelectionChangedEvent();
            evt.styleData = m_ListView.selectedItem as StyleData;
            m_EventBus.SendEvent(evt);
        }

        static VisualElement MakeItem()
        {
            return StyleModelListItem.CreateFromUxml();
        }

        void OnAddStyleClicked()
        {
            m_EventBus.SendEvent(new AddStyleButtonClickedEvent());
        }

        public void SetEventBus(EventBus eventBus)
        {
            m_EventBus = eventBus;
            m_EventBus.RegisterEvent<StyleModelSourceChangedEvent>(ModelSourcedChanged);
            m_EventBus.RegisterEvent<StyleModelListCollapsedEvent>(OnStyleModelListCollapsed);
        }

        void OnStyleModelListCollapsed(StyleModelListCollapsedEvent arg0)
        {
            m_StyleModelTitleContainer.EnableInClassList("styletrainer-stylemodellist__title-container__collapsed", arg0.collapsed);
            m_AddStyleContainer.EnableInClassList("is-collapsed", arg0.collapsed);
        }

        void ModelSourcedChanged(StyleModelSourceChangedEvent arg0)
        {
            m_ListView.itemsSource = (IList)arg0.styleModels;
            m_ListView.RefreshItems();
            if (arg0.selectedIndex < arg0.styleModels.Count && arg0.selectedIndex >= 0)
            {
                m_ListView.SetSelectionWithoutNotify(new[] { arg0.selectedIndex });
                m_ListView.ScrollToItem(arg0.selectedIndex);
                m_ListView.Focus();
            }

            var evt = new StyleModelListSelectionChangedEvent();
            evt.styleData = m_ListView.selectedItem as StyleData;
            m_EventBus.SendEvent(evt);
        }

        internal static StyleModelList CreateFromUxml()
        {
            var visualTree = ResourceManager.Load<VisualTreeAsset>(PackageResources.styleModelListTemplate);
            var ve = (StyleModelList)visualTree.CloneTree().Q("StyleModelList");
            ve.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.styleModelListStyleSheet));
            ve.BindElements();
            return ve;
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<StyleModelList, UxmlTraits> { }
#endif
    }
}