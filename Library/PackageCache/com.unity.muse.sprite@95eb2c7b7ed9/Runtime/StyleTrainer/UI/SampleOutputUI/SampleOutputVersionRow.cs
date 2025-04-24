using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
    class SampleOutputVersionRowState : RowStateBase
    {
        public bool foldout;
    }

#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class SampleOutputVersionRow : ExVisualElement, ISampleOutputRow
    {
        Text m_VersionName;
        Text m_Status;
        ActionButton m_FavouriteButton;
        VisualElement m_FavouritedContainer;
        VisualElement m_NotFavouriteContainer;
        VisualElement m_FavouriteContainer;
        Icon m_ErrorIcon;
        CircularProgress m_TrainingIcon;
        ExVisualElement m_Content;
        Foldout m_Foldout;
        CheckPointData m_CheckPointData;
        CheckPointData m_RequestLoadCheckPointData;
        int m_ItemIndex = 0;
        ImageArtifact[] m_PreviewImages;
        public Action<bool, int> OnFavouriteToggleCallback;

        const string k_HeaderString = "Version {0}:";
        const string k_DescriptionString = "Trained with {0} steps.";
        const string k_DescriptionWithStatusString = "Trained with {0} steps. {1}";
        SampleOutputVersionRowState m_RowState = new()
        {
            foldout = true
        };
        public int itemIndex
        {
            get => m_ItemIndex;
            set
            {
                m_ItemIndex = value;
                CheckPointLoaded(m_CheckPointData);
            }
        }

        public void UpdateRowHeight(float height)
        {
            m_Content.style.height = VersionRowHeight(height);
            m_Content.style.maxHeight = VersionRowHeight(height);
        }

        public void Unbind()
        {
            OnFavouriteToggleCallback = null;
        }

        public void CanModify(bool canModify)
        {
            // nothing to do here.
        }

        public bool UpdateCheckPointData(CheckPointData checkPointData)
        {
            if (m_CheckPointData == checkPointData)
            {
                CheckPointLoaded(checkPointData);
                return true;
            }

            return false;
        }

        public bool SetFavouriteCheckpoint(string checkpoint)
        {
            var isCheckPoint = m_CheckPointData.guid == checkpoint;
            m_FavouritedContainer.style.display = isCheckPoint ? DisplayStyle.Flex : DisplayStyle.None;
            m_NotFavouriteContainer.style.display = isCheckPoint ? DisplayStyle.None : DisplayStyle.Flex;
            return isCheckPoint;
        }

        public void SelectItems(IList<int> indices)
        {
            //nothing to select here
        }

        public RowStateBase GetRowState()
        {
            return m_RowState;
        }

        void ShowFavourite(bool show)
        {
            m_FavouriteContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void ShowTraining(bool show)
        {
            m_TrainingIcon.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void ShowError(bool show)
        {
            m_ErrorIcon.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            m_Foldout.SetEnabled(!show);
        }

        void SetHeaderText(string title, string status)
        {
            m_VersionName.text = title;
            m_Status.text = status;
        }

        void BindElements(CheckPointData checkPoint, float rowHeight, RowStateBase rowData)
        {
            m_Foldout = this.Q<Foldout>("Foldout");
            m_Foldout.contentContainer.AddToClassList("sampleoutputv2-listview-foldout__content");
            m_Foldout.RegisterValueChangedCallback(OnFoldOutValueChanged);
            var foldoutInput = m_Foldout.Q(className:Foldout.inputUssClassName);
            foldoutInput.AddToClassList("sampleoutputv2-listview-foldout__header");
            var foldoutHeader = SampleOutputVersionFoldoutHeader.CreateFromUxml();
            foldoutInput.Add(foldoutHeader);

            m_VersionName = foldoutHeader.Q<Text>("VersionName");
            m_Status = foldoutHeader.Q<Text>("Status");
            m_ErrorIcon = foldoutHeader.Q<Icon>("ErrorIcon");
            m_TrainingIcon = foldoutHeader.Q<CircularProgress>("TrainingIcon");

            m_FavouriteButton = foldoutHeader.Q<ActionButton>("FavouriteButton");
            m_FavouriteButton.clickable.clicked += OnFavouriteClicked;
            m_FavouritedContainer = foldoutHeader.Q<VisualElement>("Favourited");
            m_NotFavouriteContainer = foldoutHeader.Q<VisualElement>("NotFavourite");
            m_FavouriteContainer = foldoutHeader.Q<VisualElement>("Favourite");

            m_Content = this.Q<ExVisualElement>("Content");
            UpdateRowHeight(rowHeight);
            SetHeaderText(string.Format(k_HeaderString, itemIndex + 1), "(Checking...)");

            m_RowState.foldout = m_Foldout.value;
            m_RowState.guid = checkPoint.guid;
            if (rowData is SampleOutputVersionRowState rd)
            {
                if (rd.guid == checkPoint.guid)
                {
                    m_RowState.foldout = rd.foldout;
                    m_Foldout.SetValueWithoutNotify(m_RowState.foldout);
                }
            }

            m_CheckPointData = checkPoint;
            CheckPointLoaded(checkPoint);
        }

        void OnFavouriteClicked()
        {
            OnFavouriteToggleCallback?.Invoke(true, itemIndex);
        }

        void OnFoldOutValueChanged(ChangeEvent<bool> evt)
        {
            if(m_CheckPointData?.state != EState.Training)
                m_RowState.foldout = evt.newValue;
        }

        void CheckPointLoaded(CheckPointData obj)
        {
            if (obj == null)
                return;
            ShowError(false);
            ShowTraining(false);
            ShowFavourite(false);
            switch (obj.state)
            {
                case EState.Error:
                    ShowError(true);
                    SetHeaderText(string.Format(k_HeaderString, itemIndex + 1), string.Format(k_DescriptionWithStatusString, obj.trainingSteps, "(Error)"));
                    m_Foldout.SetValueWithoutNotify(false);
                    m_Foldout.SetEnabled(false);
                    break;
                case EState.Initial:
                    ShowTraining(true);
                    SetHeaderText(string.Format(k_HeaderString, itemIndex + 1), "(Loading...)");
                    LoadCheckPoint(obj);
                    break;
                case EState.Loaded:
                    ShowFavourite(true);
                    SetHeaderText(string.Format(k_HeaderString, itemIndex + 1), string.Format(k_DescriptionString, obj.trainingSteps));
                    m_Foldout.SetValueWithoutNotify(m_RowState.foldout);
                    break;
                case EState.Training:
                    ShowTraining(true);
                    SetHeaderText(string.Format(k_HeaderString, itemIndex + 1), "Training...");
                    LoadCheckPoint(obj);
                    m_Foldout.SetValueWithoutNotify(false);
                    break;
                case EState.Loading:
                    ShowTraining(true);
                    SetHeaderText(string.Format(k_HeaderString, itemIndex + 1), "(Loading...)");
                    LoadCheckPoint(obj);
                    break;
            }

            SetupValidationImage(obj);
        }

        void LoadCheckPoint(CheckPointData obj)
        {
            if (m_RequestLoadCheckPointData != obj)
            {
                if (m_RequestLoadCheckPointData != null)
                {
                    m_RequestLoadCheckPointData.OnDataChanged -= OnCheckPointDataChanged;
                    m_RequestLoadCheckPointData.OnStateChanged -= OnCheckPointDataChanged;
                }

                m_RequestLoadCheckPointData = obj;
                m_CheckPointData = m_RequestLoadCheckPointData;
                m_RequestLoadCheckPointData.OnDataChanged += OnCheckPointDataChanged;
                m_RequestLoadCheckPointData.OnStateChanged += OnCheckPointDataChanged;
                m_RequestLoadCheckPointData.GetArtifact(RequestLoadCheckPointLoaded, false);
            }
        }

        void RequestLoadCheckPointLoaded(CheckPointData obj)
        {
            if (m_RequestLoadCheckPointData != obj)
            {
                m_RequestLoadCheckPointData = null;
                CheckPointLoaded(obj);
            }
        }

        void OnCheckPointDataChanged(CheckPointData obj)
        {
            if (obj.state == EState.Loaded || obj.state == EState.Error)
            {
                m_RequestLoadCheckPointData.OnStateChanged -= OnCheckPointDataChanged;
                m_RequestLoadCheckPointData.OnDataChanged -= OnCheckPointDataChanged;
            }
            CheckPointLoaded(obj);
        }

        void SetupValidationImage(CheckPointData obj)
        {
            if (obj == m_CheckPointData)
            {
                if (obj?.validationImageData?.Count == 0 || obj.state == EState.Error)
                {
                    ClearPreviewImages();
                    m_Foldout.SetValueWithoutNotify(false);
                    m_Foldout.SetEnabled(false);
                }
                else
                {
                    if (m_PreviewImages != null)
                    {
                        // Check if the images are still the same. If not rebuild it.
                        if(m_PreviewImages.Length != obj.validationImageData.Count)
                        {
                            ClearPreviewImages();
                        }
                        for(int i = 0; i < m_PreviewImages.Length; ++i)
                        {
                            if(m_PreviewImages[i].guid != obj.validationImageData[i].imageArtifact.guid)
                            {
                                ClearPreviewImages();
                                break;
                            }
                        }
                    }

                    m_Foldout.SetEnabled(true);
                    if (m_PreviewImages == null)
                    {
                        m_PreviewImages = new ImageArtifact[obj.validationImageData.Count];
                        for (int i = 0; i < obj.validationImageData.Count; ++i)
                        {
                            var validation = obj.validationImageData[i];
                            var previewImage = new PreviewImage();
                            previewImage.AddToClassList("sampleoutputv2-listview-version--image");
                            m_PreviewImages[i] = validation.imageArtifact;
                            validation.imageArtifact.disablePlaceHolder = true;
                            previewImage.SetArtifact(validation.imageArtifact);
                            m_Content.Add(CreateRowItem(previewImage));
                        }
                    }
                }
            }
        }

        SampleOutputListRowItem CreateRowItem(VisualElement child)
        {
            var item = new SampleOutputListRowItem(child, 1);
            item.AddToClassList("sampleoutputv2-listview-rowitem");
            return item;
        }

        void ClearPreviewImages()
        {
            m_Content.Clear();
            m_PreviewImages = null;
        }

        static public float VersionRowHeight(float height)
        {
            return SampleOutputPromptRow.PromptItemWidth(height);
        }

        internal static SampleOutputVersionRow CreateFromUxml(CheckPointData checkPoint, float rowHeight, RowStateBase rowData)
        {
            var visualTree = ResourceManager.Load<VisualTreeAsset>(PackageResources.sampleOutputVersionRowTemplate);
            var ve = (SampleOutputVersionRow)visualTree.CloneTree().Q("SampleOutputVersionRow");
            ve.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.sampleOutputVersionRowStyleSheet));
            ve.BindElements(checkPoint, rowHeight, rowData);
            return ve;
        }
        
#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<SampleOutputVersionRow, UxmlTraits> { } 
#endif
    }
}