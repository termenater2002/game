using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
    class TrainingRoundsView : ExVisualElement
    {
        RadioGroup m_RadioGroup;
        List<Text> m_Prompts;

        StyleData m_StyleData;
        EventBus m_EventBus;

        public Action<bool, CheckPointData> OnFavoriteToggleChangedCallback;

        public TrainingRoundsView()
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.trainingRoundsViewStyleSheet));
            CreateUI();
            Refresh();
        }

        public void SetStyleData(StyleData styleData)
        {
            m_StyleData = styleData;
            Refresh();
        }

        public void SetEventBus(EventBus eventBus)
        {
            m_EventBus = eventBus;
            Refresh();
        }

        void CreateUI()
        {
            if (m_StyleData == null || m_EventBus == null) return;

            m_RadioGroup = new RadioGroup();
            var accordion = new Accordion();

            for (int i = 0; i < m_StyleData.checkPoints.Count; i++)
            {
                var item = new TrainingRoundsRowView(i + 1, m_StyleData.checkPoints[i], m_StyleData, m_EventBus);
                m_RadioGroup.Add(item);
                accordion.Add(item);
            }

            Add(accordion);
        }

        void ClearContent()
        {
            Clear();
        }

        void Refresh()
        {
            ClearContent();
            if (m_StyleData != null)
            {
                CreateUI();
            }
        }
    }
}
