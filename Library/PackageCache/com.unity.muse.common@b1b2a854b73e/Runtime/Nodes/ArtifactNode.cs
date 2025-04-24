using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    class ArtifactNode : VisualElement, IDisposable
    {
        const string k_MainUssClassName = "muse-canvas-node";

        const string k_PreviewUssClassName = k_MainUssClassName + "__preview";

        Artifact m_Artifact;
        ArtifactView m_CurrentArtifactView;

        public Artifact artifact
        {
            get => m_Artifact;
            set
            {
                if (m_Artifact == value)
                    return;

                m_Artifact = value;
                UpdateView();
            }
        }

        public ArtifactNode()
        {
            usageHints = UsageHints.DynamicTransform;
            AddToClassList(k_MainUssClassName);
        }

        public void UpdateView()
        {
            SetCurrentArtifactView(null);
            Clear();

            if (artifact is null)
                return;

            m_CurrentArtifactView = artifact.CreateCanvasView();
            if (m_CurrentArtifactView == null)
                return;
            m_CurrentArtifactView.AddToClassList(k_PreviewUssClassName);
            var dimensions = m_Artifact.GetVisualDimensions();
            if (dimensions != Vector2.zero)
            {
                style.width = dimensions.x;
                style.height = dimensions.y;
            }
            Add(m_CurrentArtifactView);
        }
        
        void SetCurrentArtifactView(ArtifactView currentArtifactView)
        {
            if (m_CurrentArtifactView is IDisposable disposableArtifact)
            {
                disposableArtifact?.Dispose();
            }

            m_CurrentArtifactView = currentArtifactView;
        }

        public void Dispose()
        {
           SetCurrentArtifactView(null); 
        }
    }
}
