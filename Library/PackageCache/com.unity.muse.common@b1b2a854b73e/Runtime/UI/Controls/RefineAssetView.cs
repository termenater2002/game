using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    internal partial class RefineAssetView : ExVisualElement, IControl
    {
        const string ussClassName = "muse-refine-asset-view";
        static readonly string rootUssClassName = ussClassName + "__root";

        Model m_CurrentModel;

        public RefineAssetView()
        {
            Init();
        }

        protected virtual void Init()
        {
            var styleSheet = ResourceManager.Load<StyleSheet>(PackageResources.refineAssetViewStyleSheet);
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogError($"RefineAssetView Failed to load style sheet at path");
            }
            AddToClassList(rootUssClassName);
            this.style.display = DisplayStyle.None; // Hide initially
        }

        public virtual void OnAttachToPanel(AttachToPanelEvent evt) {}
        public virtual void OnDetachFromPanel(DetachFromPanelEvent evt) {}

        public void UpdateView() {}

        public void SetModel(Model model)
        {
            UnsubscribeFromEvents();
            m_CurrentModel = model;
            SubscribeToEvents();

            if (!m_CurrentModel)
                return;

            OnArtifactSelected(m_CurrentModel.SelectedArtifact);
        }

        void SubscribeToEvents()
        {
            if (!m_CurrentModel)
                return;

            m_CurrentModel.OnFinishRefineArtifact += OnFinishRefineArtifact;
        }

        void UnsubscribeFromEvents()
        {
            if (!m_CurrentModel)
                return;

            m_CurrentModel.OnFinishRefineArtifact -= OnFinishRefineArtifact;
        }

        void OnRefineArtifact(Artifact artifact)
        {
            Clear();
            var preview = artifact.CreateRefineView();
            if (preview is null)
                return;
            Add(preview);
            this.style.display = DisplayStyle.Flex;
        }

        void OnFinishRefineArtifact(Artifact artifact)
        {
            this.style.display = DisplayStyle.None;
            Clear();
            UnsubscribeFromEvents();
        }

        void OnArtifactSelected(Artifact artifact)
        {
            this.style.display = DisplayStyle.None;
            if (!m_CurrentModel || !m_CurrentModel.isRefineMode)
                return;

            OnRefineArtifact(artifact);
        }
    }
}
