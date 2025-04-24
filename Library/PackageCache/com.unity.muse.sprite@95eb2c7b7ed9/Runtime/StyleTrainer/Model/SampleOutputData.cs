using System;
using UnityEngine;

namespace Unity.Muse.StyleTrainer
{
    [Serializable]
    class SampleOutputData : Artifact<SampleOutputData, SampleOutputData>
    {
        [SerializeField]
        ImageArtifact m_ImageArtifact;
        [SerializeField]
        string m_Prompt;

        public SampleOutputData(EState state, string prompt)
            : base(state)
        {
            m_ImageArtifact = new ImageArtifact(state);
            m_Prompt = prompt;
        }

        public override void OnDispose()
        {
            m_ImageArtifact?.OnDispose();
            base.OnDispose();
        }

        protected override void GUIDChanged()
        {
            m_ImageArtifact.guid = guid;
            base.GUIDChanged();
        }

        public ImageArtifact imageArtifact => m_ImageArtifact;

        public string prompt
        {
            get => m_Prompt;
            set
            {
                m_Prompt = value;
                DataChanged(this);
            }
        }

        public override void GetArtifact(Action<SampleOutputData> onDoneCallback, bool useCache)
        {
            onDoneCallback?.Invoke(this);
        }

        public SampleOutputData Duplicate()
        {
            var data = new SampleOutputData(EState.New, m_Prompt);
            return data;
        }

        public void Delete()
        {
            if (m_ImageArtifact?.state == EState.Loaded)
                m_ImageArtifact?.DeleteCache();
        }
    }
}
