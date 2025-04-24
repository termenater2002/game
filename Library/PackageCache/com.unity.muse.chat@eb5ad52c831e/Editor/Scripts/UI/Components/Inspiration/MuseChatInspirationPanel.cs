using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Chat.Commands;
using Unity.Muse.Chat.UI.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.Inspiration
{
    class MuseChatInspirationPanel : ManagedTemplate
    {
        Button m_RefreshButton;

        VisualElement m_AskContent;
        VisualElement m_RunContent;
        VisualElement m_GenerateContent;

        readonly IDictionary<string, IList<MuseChatInspiration>> k_InspirationCache = new Dictionary<string, IList<MuseChatInspiration>>();

        public MuseChatInspirationPanel()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        public event Action<MuseChatInspiration> InspirationSelected;

        protected override void InitializeView(TemplateContainer view)
        {
            m_RefreshButton = view.SetupButton("refreshButton", _ => RefreshEntries());
            m_AskContent = view.Q<VisualElement>("askInspirationSectionContent");
            m_RunContent = view.Q<VisualElement>("runInspirationSectionContent");
            m_GenerateContent = view.Q<VisualElement>("generateInspirationSectionContent");

            Assistant.instance.OnInspirationsChanged += RefreshEntries;
            BeginRefreshEntries();
        }

        public void RefreshEntries()
        {
            RefreshCache();
            RefreshEntriesForCategory(m_AskContent, AskCommand.k_CommandName, maxEntries: 4);
            RefreshEntriesForCategory(m_RunContent, RunCommand.k_CommandName);
            RefreshEntriesForCategory(m_GenerateContent, CodeCommand.k_CommandName);
        }

        void RefreshCache()
        {
            k_InspirationCache.Clear();
            var entries = Assistant.instance.Inspirations;
            foreach (MuseChatInspiration inspiration in entries)
            {
                if (!k_InspirationCache.TryGetValue(inspiration.Command, out var entryList))
                {
                    entryList = new List<MuseChatInspiration>();
                    k_InspirationCache.Add(inspiration.Command, entryList);
                }

                entryList.Add(inspiration);
            }
        }
        void RefreshEntriesForCategory(VisualElement targetRoot, string command, int maxEntries = 3)
        {
            targetRoot.Clear();

            if (!k_InspirationCache.TryGetValue(command, out var inspirations))
            {
                return;
            }

            int entriesToSelect = Mathf.Min(maxEntries, inspirations.Count);
            var shuffledIndices = Enumerable.Range(0, inspirations.Count)
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(entriesToSelect);

            foreach (var inspiration in shuffledIndices.Select(index => inspirations[index]))
            {
                var entry = new MuseChatInspirationEntry();
                entry.Initialize();
                entry.Value = inspiration;
                entry.Clicked += OnInspirationSelected;
                targetRoot.Add(entry);
            }
        }

        void BeginRefreshEntries()
        {
            _ = Assistant.instance.RefreshInspirations();
        }

        void OnInspirationSelected(MuseChatInspiration value)
        {
            InspirationSelected?.Invoke(value);
        }
    }
}
