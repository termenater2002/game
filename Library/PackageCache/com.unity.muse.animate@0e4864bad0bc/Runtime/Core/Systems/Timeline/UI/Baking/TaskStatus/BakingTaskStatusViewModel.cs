using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class BakingTaskStatusViewModel
    {
        public delegate void Changed();

        public event Changed OnChanged;

        public delegate void Error(string error);

        public event Error OnError;

        public bool IsVisible
        {
            get => m_IsVisible;
            private set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnChanged?.Invoke();
            }
        }

        public float Progress
        {
            get => m_Progress;
            private set
            {
                if (Math.Abs(value - m_Progress) < Mathf.Epsilon)
                    return;

                m_Progress = value;
                OnChanged?.Invoke();
            }
        }
        
        public bool IsWaitingDelay
        {
            get => m_IsWaitingDelay;
            private set
            {
                if (m_IsWaitingDelay == value)
                    return;

                m_IsWaitingDelay = value;
                OnChanged?.Invoke();
            }
        }

        public bool IsDone => Progress.Equals(1f);
        List<MotionToKeysSamplingLogic> m_TrackedSamplingLogics = new();
        List<BakingLogic> m_TrackedBakingLogics = new();
        
        bool m_IsVisible;
        float m_Progress;
        bool m_IsWaitingDelay;

        public BakingTaskStatusViewModel()
        {
            IsVisible = false;
        }
        
        /// <summary>
        /// Track one or many BakingLogic instances.
        /// The task status will update itself automatically
        /// based on what the tracked baking and sampling logics are doing.
        /// </summary>
        /// <param name="logics">The BakingLogic instances to track.</param>
        public void TrackBakingLogics(params BakingLogic[] logics)
        {
            foreach (var logic in logics)
            {
                TrackBakingLogic(logic);
            }
        }
        
        /// <summary>
        /// Track one or many MotionToKeysSamplingLogic instances.
        /// The task status will update itself automatically
        /// based on what the tracked baking and sampling logics are doing.
        /// </summary>
        /// <param name="logics">The MotionToKeysSamplingLogic instances to track.</param>
        public void TrackSamplingLogics(params MotionToKeysSamplingLogic[] logics)
        {
            foreach (var logic in logics)
            {
                TrackSamplingLogic(logic);
            }
        }
        
        /// <summary>
        /// Stop tracking one or many BakingLogic instances.
        /// </summary>
        /// <param name="logics">The BakingLogic instances to stop tracking.</param>
        public void UntrackBakingLogics(params BakingLogic[] logics)
        {
            foreach (var logic in logics)
            {
               UntrackBakingLogic(logic);
            }
        }
        
        /// <summary>
        /// Stop tracking one or many MotionToKeysSamplingLogic instances.
        /// </summary>
        /// <param name="logics">The MotionToKeysSamplingLogic instances to stop tracking.</param>
        public void UntrackSamplingLogics(params MotionToKeysSamplingLogic[] logics)
        {
            foreach (var logic in logics)
            {
                UntrackSamplingLogic(logic);
            }
        }
        
        void TrackBakingLogic(BakingLogic logic)
        {
            if (m_TrackedBakingLogics.Contains(logic))
                return;
            
            m_TrackedBakingLogics.Add(logic);
    
            if(logic.IsRunning)
                OnBakingStarted(logic.GetEventData(BakingLogic.BakingEventData.BakingEventType.Started));
    
            logic.OnBakingProgressed += OnBakingProgressed;
            logic.OnBakingStarted += OnBakingStarted;
            logic.OnBakingCompleted += OnBakingCompleted;
            logic.OnBakingCanceled += OnBakingCanceled;
            logic.OnBakingFailed += OnBakingFailed;
            
            Update();
        }
        
        void TrackSamplingLogic(MotionToKeysSamplingLogic logic)
        {
            if (m_TrackedSamplingLogics.Contains(logic))
                return;
            
            m_TrackedSamplingLogics.Add(logic);
            
            logic.OnProgressed += OnSamplingProgressed;
            logic.OnStarted += OnSamplingStarted;
            logic.OnCompleted += OnSamplingCompleted;
            logic.OnFailed += OnSamplingFailed;
            logic.OnCanceled += OnSamplingCanceled;
            
            Update();
        }
        
        void UntrackBakingLogic(BakingLogic logic)
        {
            if (!m_TrackedBakingLogics.Contains(logic))
                return;
            
            logic.OnBakingProgressed -= OnBakingProgressed;
            logic.OnBakingStarted -= OnBakingStarted;
            logic.OnBakingCompleted -= OnBakingCompleted;
            logic.OnBakingCanceled -= OnBakingCanceled;
            logic.OnBakingFailed -= OnBakingFailed;

            m_TrackedBakingLogics.Remove(logic);
            
            Update();
        }
        
        void UntrackSamplingLogic(MotionToKeysSamplingLogic logic)
        {
            if (!m_TrackedSamplingLogics.Contains(logic))
                return;
            
            m_TrackedSamplingLogics.Remove(logic);

            logic.OnProgressed -= OnSamplingProgressed;
            logic.OnStarted -= OnSamplingStarted;
            logic.OnCompleted -= OnSamplingCompleted;
            logic.OnFailed -= OnSamplingFailed;
            logic.OnCanceled -= OnSamplingCanceled;

            Update();
        }

        void OnBakingFailed(BakingLogic.BakingEventData eventData)
        {
            Update();
            OnError?.Invoke(eventData.Message);
        }

        void OnBakingStarted(BakingLogic.BakingEventData eventData)
        {
            Update();
        }

        void OnBakingCompleted(BakingLogic.BakingEventData eventData)
        {
            Update();
        }

        void OnBakingProgressed(BakingLogic.BakingEventData eventData)
        {
            Update();
        }
        
        void OnBakingCanceled(BakingLogic.BakingEventData eventData)
        {
            Update();
        }
        
        void OnSamplingCanceled(MotionToKeysSamplingLogic logic)
        {
            Update();
        }
        
        void OnSamplingFailed(MotionToKeysSamplingLogic logic, string error)
        {
            IsVisible = false;
            Update();
            OnError?.Invoke(error);
        }

        void OnSamplingStarted(MotionToKeysSamplingLogic logic)
        {
            Update();
        }
        void OnSamplingCompleted(MotionToKeysSamplingLogic logic)
        {
            Update();
        }
        void OnSamplingProgressed(MotionToKeysSamplingLogic logic, float progress)
        {
            Update();
        }

        void Update()
        {
            var progress = 0f;
            var visible = false;
            var isWaitingDelay = false;
            
            foreach (var logic in m_TrackedBakingLogics)
            {
                if (logic.IsRunning)
                {
                    progress = Mathf.Max(progress, logic.BakingProgress);
                    visible = true;
                }

                if (logic.IsWaitingDelay)
                {
                    isWaitingDelay = true;
                }
            }
            
            foreach (var logic in m_TrackedSamplingLogics)
            {
                if (logic.IsRunning)
                {
                    progress = Mathf.Max(progress, logic.SamplingProgress);
                    visible = true;
                }
                
                if (logic.IsWaitingDelay)
                {
                    isWaitingDelay = true;
                }
            }

            IsWaitingDelay = isWaitingDelay;
            Progress = progress;
            IsVisible = visible;
        }
    }
}
