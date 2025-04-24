using UnityEngine;

namespace Unity.Muse.Animate
{
    class BakingViewModel
    {
        public delegate void BakingProgressChanged();
        public event BakingProgressChanged OnBakingProgressChanged;

        public float Progress => m_Progress;
        public bool IsDone => m_Progress.Equals(1f);

        float m_Progress;
        BakingLogic m_Logic;

        public BakingViewModel(BakingLogic bakingLogic)
        {
            m_Logic = bakingLogic;
            m_Logic.OnBakingProgressed += OnBakingProgressed;
            m_Logic.OnBakingFailed += OnBakingFailed;

            UpdateBakingProgress();
        }

        void OnBakingFailed(BakingLogic.BakingEventData bakingEventData)
        {
            m_Progress = 1f;
            OnBakingProgressChanged?.Invoke();
        }

        void OnBakingProgressed(BakingLogic.BakingEventData bakingEventData)
        {
            UpdateBakingProgress();
            OnBakingProgressChanged?.Invoke();
        }

        void UpdateBakingProgress()
        {
            m_Progress = m_Logic.BakingProgress;
        }
    }
}
