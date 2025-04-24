using System;
using UnityEngine;

namespace Unity.Muse.StyleTrainer
{
    [Serializable]
    class TrainingData : Artifact<TrainingData, TrainingData>
    {
        [SerializeReference]
        ImageArtifact m_ImageArtifact;

        public TrainingData(EState state, string guid)
            : base(state)
        {
            this.guid = guid;
            m_ImageArtifact = new ImageArtifact(state);
            m_ImageArtifact.guid = guid;
        }

        public override void OnDispose()
        {
            m_ImageArtifact?.OnDispose();
            base.OnDispose();
        }

        public void SetImageArtifact(ImageArtifact imageArtifact)
        {
            m_ImageArtifact?.OnDispose();
            m_ImageArtifact = imageArtifact;
        }

        public override void GetArtifact(Action<TrainingData> onDoneCallback, bool useCache)
        {
            // we have nothing to load
            if (Utilities.ValidStringGUID(guid))
                state = EState.Loaded;
            onDoneCallback?.Invoke(this);
        }

        public ImageArtifact imageArtifact => m_ImageArtifact;

        public string Base64Image()
        {
            return m_ImageArtifact.Base64Image();
        }

        public void DuplicateNew(Action<TrainingData> duplicateDone)
        {
            // attempt to load the image artifact since the duplicate will need it later
            var data = new TrainingData(EState.New, Utilities.emptyGUID);
            data.SetImageArtifact(new ImageArtifact(EState.New));
            data.imageArtifact.guid = Utilities.CreateTempGuid();
            imageArtifact.GetArtifact((_) =>
            {
                data.imageArtifact.SetTexture(imageArtifact.GetRawData());
                duplicateDone?.Invoke(data);
            }, true);
        }

        public void Delete()
        {
            m_ImageArtifact?.DeleteCache();
        }
    }
}