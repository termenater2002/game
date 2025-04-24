using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    internal class PreviewImage: LoadableImage
    {
        Artifact m_CurrentArtifact;
        public event Action<Artifact> OnSelected;
        public event Action OnLoadedPreview;
        public event Action OnDelete;

        public PreviewImage()
        {
            AddToClassList("preview-image");
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            GenericLoader.OnRetry += OnRetry;
            GenericLoader.OnDelete += OnDeleteClicked;
        }

        private void OnRetry()
        {
            if (!m_CurrentArtifact.IsValid() || !m_CurrentArtifact.isSafe)
            {
                m_CurrentArtifact.RetryGenerate(this.GetContext<Model>());
            }

            SetAsset(m_CurrentArtifact);
        }

        private void OnDeleteClicked()
        {
            OnDelete?.Invoke();
        }

        public void SetAsset(Artifact artifact)
        {
            m_CurrentArtifact = artifact;

            if (artifact.IsValid())
            {
                OnLoading();
                artifact.GetPreview(OnArtifactReceived, true);
            }
            else
            {
                OnLoading();
                artifact.OnGenerationDone -= OnArtifactGenerationDone;
                artifact.OnGenerationDone += OnArtifactGenerationDone;
            }
        }

        void OnArtifactGenerationDone(Artifact artifact, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                var model = this.GetContext<Model>();
                if (model)
                {
#if UNITY_EDITOR
                    // Specific solution to ensure that the model is saved when the artifact is generated.
                    // This is not general enough for every type of action (delete, modification, etc...)
                    // which would require a more complex solution.
                    UnityEditor.EditorUtility.SetDirty(model);
                    UnityEditor.AssetDatabase.SaveAssetIfDirty(model);
#endif
                    model.ArtifactGenerationDone(artifact);
                }

                SetAsset(artifact);
                return;
            }


            OnError("Generation failed.");
        }

        void OnArtifactReceived(Texture2D artifactInstance, byte[] rawData, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                OnLoaded(artifactInstance);
                OnLoadedPreview?.Invoke();
                return;
            }

            OnError(!m_CurrentArtifact.isSafe ? TextContent.potentialInappropriateContentDetected : TextContent.failedToRetrieveArtifact);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if(evt.clickCount == 2)
            {
                OnSelected?.Invoke(m_CurrentArtifact);
            }
        }
    }
}
