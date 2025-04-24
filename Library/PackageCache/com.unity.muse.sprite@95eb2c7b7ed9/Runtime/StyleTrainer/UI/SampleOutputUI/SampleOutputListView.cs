using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
    class RowStateBase
    {
        public string guid;
    }

    interface ISampleOutputRow
    {
        void UpdateRowHeight(float height);
        void Unbind();
        void CanModify(bool canModify);
        bool UpdateCheckPointData(CheckPointData checkPointData);
        bool SetFavouriteCheckpoint(string checkpoint);
        void SelectItems(IList<int> indices);
        RowStateBase GetRowState();
    }

    class SampleOutputListView : ExVisualElement
    {
        SampleOutputPromptRow m_PromptRow;
        VisualElement m_PromptRowContent;
        ScrollView m_ScrollView;
        VisualElement m_Content;

        StyleData m_StyleData;
        const float k_RowHeight = 250;
        float m_RowHeight = k_RowHeight;
        public Action<int> OnDeleteClickedCallback;
        public Action<bool, CheckPointData> OnFavoriteToggleChangedCallback;
        bool m_CanModify;

        List<ISampleOutputRow> m_Rows = new();
        List<RowStateBase> m_RowState = new();

        public SampleOutputListView()
        {
            m_ScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            m_ScrollView.horizontalScroller.RegisterCallback<ChangeEvent<float>>(OnScrollViewHorizontalChanged);

            m_Content = new VisualElement()
            {
                name = "Content"
            };
            m_Content.AddToClassList("sampleoutputv2-listview-content");
            m_ScrollView.Add(m_Content);
            m_ScrollView.AddToClassList("sampleoutputv2-listview-scrollview");
            AddToClassList("sampleoutputv2-listview");
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.sampleOutputListViewStyleSheet));

            m_PromptRowContent = new VisualElement()
            {
                name = "PromptRowContent"
            };
            m_PromptRowContent.AddToClassList("sampleoutputv2-listview-promptrowcontent");
            Add(m_PromptRowContent);
            Add(m_ScrollView);

            RefreshItems();
        }

        void OnScrollViewHorizontalChanged(ChangeEvent<float> evt)
        {
            if (m_PromptRowContent.Contains(m_PromptRow))
            {
                var p = m_PromptRow.transform.position;
                p.x = m_ScrollView.contentContainer.transform.position.x;
                m_PromptRow.transform.position = p;
            }
        }

        void ClearContent()
        {
            m_Content.Clear();
            m_RowState.Clear();
            for(int i = 0; i < m_Rows.Count; i++)
            {
                m_RowState.Add(m_Rows[i].GetRowState());
                m_Rows[i].Unbind();
            }

            m_Rows.Clear();
        }

        void RefreshItems()
        {
            ClearContent();
            if (m_StyleData != null)
            {
                if (m_StyleData.sampleOutputPrompts.Count > 0)
                {
                    if (m_PromptRow == null)
                    {
                        m_PromptRow = BindPrompt();
                    }
                    else
                    {
                        m_PromptRow.BindElements(m_StyleData);
                        m_PromptRow.CanModify(m_CanModify);
                    }


                    if (m_StyleData.checkPoints.Count > 0)
                    {
                        if(m_Content.Contains(m_PromptRow))
                            m_Content.Remove(m_PromptRow);
                        if(!m_PromptRowContent.Contains(m_PromptRow))
                            m_PromptRowContent.Add(m_PromptRow);
                    }
                    else
                    {
                        if(m_PromptRowContent.Contains(m_PromptRow))
                            m_PromptRowContent.Remove(m_PromptRow);
                        if(!m_Content.Contains(m_PromptRow))
                            m_Content.Add(m_PromptRow);
                    }
                }
            }
            UpdateFavouriteCheckpoint();
        }

        SampleOutputPromptRow BindPrompt()
        {
            var ve = SampleOutputPromptRow.CreateFromUxml(m_StyleData, m_RowHeight);
            ve.OnSampleOutputPromptDeleteClicked += OnSampleOutputPromptDeleteClicked;
            ve.AddToClassList("sampleoutputv2-listview-item");
            m_Rows.Add(ve);
            ve.CanModify(m_CanModify);
            return ve;
        }

        void OnSampleOutputPromptDeleteClicked(int obj)
        {
            OnDeleteClickedCallback?.Invoke(obj);
        }

        public void SetRowSize(float thumbnailSize)
        {
            m_RowHeight = thumbnailSize * k_RowHeight;
            for (int i = 0; i < m_Rows.Count; ++i)
            {
                m_Rows[i].UpdateRowHeight(m_RowHeight);
            }
            m_PromptRow?.UpdateRowHeight(m_RowHeight);
        }

        public void SetStyleData(StyleData styleData)
        {
            m_StyleData = styleData;
            RefreshItems();
        }

        public void CanModify(bool canModify)
        {
            m_CanModify = canModify;
            m_PromptRow?.CanModify(canModify);
            for (int i = 0; i < m_Rows.Count; ++i)
            {
                m_Rows[i].CanModify(canModify);
            }
        }

        public void CheckPointSourceDataChanged()
        {
            RefreshItems();
        }

        public void CheckPointDataChanged(CheckPointData arg0CheckPointData)
        {
            for (int i = 0; i < m_Rows.Count; ++i)
            {
                // there should be only 1 row interested.
                if (m_Rows[i].UpdateCheckPointData(arg0CheckPointData))
                    break;
            }
        }

        public void UpdateFavouriteCheckpoint()
        {
            for (int i = 0; i < m_Rows.Count; ++i)
            {
                m_Rows[i].SetFavouriteCheckpoint(m_StyleData.favoriteCheckPoint);
            }
        }

        public void SelectItems(IList<int> indices)
        {
            m_PromptRow.SelectItems(indices);
            for (int i = 0; i < m_Rows.Count; ++i)
            {
                m_Rows[i].SelectItems(indices);
            }
        }
    }
}