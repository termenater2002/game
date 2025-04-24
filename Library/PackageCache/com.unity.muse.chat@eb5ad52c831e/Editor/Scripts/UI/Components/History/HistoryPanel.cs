using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common.Utils;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.History
{
    class HistoryPanel : ManagedTemplate
    {
        static readonly List<MuseConversationInfo> k_ConversationCache = new();
        static readonly IDictionary<string, List<MuseConversationInfo>> k_GroupCache = new Dictionary<string, List<MuseConversationInfo>>();

        readonly IList<object> k_TempList = new List<object>();

        ToolbarSearchField m_SearchBar;
        VisualElement m_ContentRoot;
        AdaptiveListView<object, HistoryPanelEntry> m_ContentList;

        MuseConversationId m_SelectedConversation;

        string m_SearchFilter;

        public HistoryPanel()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_ContentRoot = view.Q<VisualElement>("historyContentRoot");
            m_ContentList = new AdaptiveListView<object, HistoryPanelEntry>
            {
                EnableVirtualization = true
            };
            m_ContentList.Initialize();
            m_ContentList.SelectionChanged += SelectionChanged;
            m_ContentRoot.Add(m_ContentList);

            m_SearchBar = new ToolbarSearchField();
            m_SearchBar.AddToClassList("mui-history-panel-search-bar");
            view.Q<VisualElement>("historySearchBarRoot").Add(m_SearchBar);
            m_SearchBar.RegisterCallback<KeyUpEvent>(OnSearchTextChanged);
            m_SearchBar.RegisterValueChangedCallback(OnSearchValueChanged);

            // Schedule a history update every 5 minutes
            schedule.Execute(() => _ = Assistant.instance.RefreshConversationsAsync()).Every(1000 * 60 * 5);

            MuseChatHistoryBlackboard.HistoryPanelRefreshRequired += OnRefreshRequired;
            MuseChatHistoryBlackboard.HistoryPanelReloadRequired += OnReloadRequired;
        }

        static void LoadData(IList<object> result, long nowRaw, string searchFilter = null)
        {
            bool searchActive = !string.IsNullOrEmpty(searchFilter);
            k_GroupCache.Clear();
            result.Clear();
            foreach (var conversationInfo in k_ConversationCache)
            {
                if (searchActive && conversationInfo.Title.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                string groupKey;
                if (MuseChatHistoryBlackboard.GetFavoriteCache(conversationInfo.Id))
                {
                    groupKey = "000000#Favorites";
                }
                else
                {
                    groupKey = MessageUtils.GetMessageTimestampGroup(conversationInfo.LastMessageTimestamp, nowRaw);
                }

                if (!k_GroupCache.TryGetValue(groupKey, out var groupInfos))
                {
                    groupInfos = new List<MuseConversationInfo>();
                    k_GroupCache.Add(groupKey, groupInfos);
                }

                groupInfos.Add(conversationInfo);
            }

            var orderedKeys = k_GroupCache.Keys.OrderBy(x => x).ToArray();
            for (var i = 0; i < orderedKeys.Length; i++)
            {
                var title = orderedKeys[i].Split('#')[1];
                result.Add(title);

                var groupContent = k_GroupCache[orderedKeys[i]];
                groupContent.Sort((e1, e2) => DateTimeOffset.Compare(DateTimeOffset.FromUnixTimeMilliseconds(e2.LastMessageTimestamp), DateTimeOffset.FromUnixTimeMilliseconds(e1.LastMessageTimestamp)));
                foreach (var info in groupContent)
                {
                    result.Add(info);
                }
            }
        }

        void Reload(bool fullReload = true, bool resetScrollPosition = false)
        {
            if (fullReload)
            {
                // Full reload let's get a fresh list of conversations from the driver
                k_ConversationCache.Clear();
                k_ConversationCache.AddRange(Assistant.instance.History);

                // Update the cache
                foreach (var conversationInfo in k_ConversationCache)
                {
                    MuseChatHistoryBlackboard.SetFavoriteCache(conversationInfo.Id, conversationInfo.IsFavorite);
                }
            }

            var nowRaw = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var activeConversation = Assistant.instance.GetActiveConversation();

            m_ContentList.ClearData();
            m_ContentList.ClearSelection();

            k_TempList.Clear();
            LoadData(k_TempList, nowRaw, m_SearchFilter);

            int selectedIndex = -1;
            m_ContentList.BeginUpdate();
            for (var i = 0; i < k_TempList.Count; i++)
            {
                var entry = k_TempList[i];
                if (activeConversation != null && entry is MuseConversationInfo info && info.Id == activeConversation.Id)
                {
                    selectedIndex = i;
                }

                m_ContentList.AddData(entry);
            }

            m_ContentList.EndUpdate(false);
            if (selectedIndex >= 0)
            {
                m_ContentList.SetSelectionWithoutNotify(selectedIndex, true);
            }

            m_SelectedConversation = activeConversation?.Id ?? default;

            m_ContentList.SetDisplay(m_ContentList.Data.Count != 0);

            if (resetScrollPosition)
            {
                m_ContentList.ScrollToStartIfNotLocked();
            }
        }

        void OnRefreshRequired()
        {
            Reload(fullReload: false);
        }

        void OnReloadRequired()
        {
            Reload(fullReload: true);
        }

        void OnSearchTextChanged(KeyUpEvent evt)
        {
            SetSearchFilter(m_SearchBar.value);
        }

        void SelectionChanged(int index, object data)
        {
            if (index == -1 || data is string)
            {
                return;
            }

            var conversationInfo = (MuseConversationInfo)data;
            m_SelectedConversation = conversationInfo.Id;

            _ = Assistant.instance.ConversationLoad(m_SelectedConversation);
        }

        void OnSearchValueChanged(ChangeEvent<string> evt)
        {
            SetSearchFilter(evt.newValue);
        }

        void SetSearchFilter(string filter)
        {
            if (m_SearchFilter == filter)
            {
                return;
            }

            m_SearchFilter = filter;
            Reload(false, true);
        }
    }
}
