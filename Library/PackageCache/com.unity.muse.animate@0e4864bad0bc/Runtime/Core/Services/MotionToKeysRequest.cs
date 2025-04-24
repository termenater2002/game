using System;
using System.Diagnostics;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class MotionToKeysRequest
    {
        Status m_State;

        public enum Status
        {
            Stopped,
            Started,
            SamplingKeys,
            BakingOutput,
            Completed,
            Failed,
            Canceled
        }

        public Status State
        {
            get => m_State;
            set
            {
                if (m_State == value)
                    return;

                m_State = value;
                OnStateChanged?.Invoke(this, m_State);
            }
        }

        public bool IsActive => State == Status.SamplingKeys || State == Status.Started || State == Status.BakingOutput;

        public event Action<MotionToKeysRequest,Status> OnStateChanged;
        public event Action<MotionToKeysRequest> OnStarted;
        public event Action<MotionToKeysRequest> OnCanceled;
        public event Action<MotionToKeysRequest> OnCompleted;
        public event Action<MotionToKeysRequest,float> OnProgressed;
        public event Action<MotionToKeysRequest,string> OnFailed;
        float Sensitivity { get; }
        bool UseMotionCompletion { get; }
        MotionToKeysService Service { get; }
        public KeySequenceTake Target { get; }

        public MotionToKeysRequest(MotionToKeysService service, KeySequenceTake target, float sensitivity, bool useMotionCompletion)
        {
            Service = service;
            Target = target;
            Sensitivity = sensitivity;
            UseMotionCompletion = useMotionCompletion;
        }

        public void Start()
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> Start()");

            RegisterToSampling();
            State = Status.Started;
            OnStarted?.Invoke(this);
            
            Service.SamplingLogic.QueueBaking(Sensitivity, UseMotionCompletion);
        }

        public void Cancel()
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> Cancel()");

            Stop();
        }

        void Stop()
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> Stop()");
            
            if (Service.SamplingLogic.IsRunning)
            {
                Service.SamplingLogic.Cancel();
            }
            
            if (Service.OutputBakingLogic.IsRunning)
            {
                Service.OutputBakingLogic.Cancel();
            }
        }

        void RegisterToSampling()
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> RegisterToSampling()");

            Service.SamplingLogic.OnStarted += OnSamplingStarted;
            Service.SamplingLogic.OnProgressed += OnSamplingProgressed;
            Service.SamplingLogic.OnCanceled += OnSamplingCanceled;
            Service.SamplingLogic.OnFailed += OnSamplingFailed;
            Service.SamplingLogic.OnCompleted += OnSamplingCompleted;
        }
        
        void RegisterToBaking()
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> RegisterToBaking()");
            
            Service.OutputBakingLogic.OnBakingStarted += OnBakingStarted;
            Service.OutputBakingLogic.OnBakingProgressed += OnBakingProgressed;
            Service.OutputBakingLogic.OnBakingCanceled += OnBakingCanceled;
            Service.OutputBakingLogic.OnBakingFailed += OnBakingFailed;
            Service.OutputBakingLogic.OnBakingCompleted += OnBakingCompleted;
        }
        
        void UnregisterFromSampling()
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> UnregisterFromSampling()");

            Service.SamplingLogic.OnStarted -= OnSamplingStarted;
            Service.SamplingLogic.OnProgressed -= OnSamplingProgressed;
            Service.SamplingLogic.OnCanceled -= OnSamplingCanceled;
            Service.SamplingLogic.OnFailed -= OnSamplingFailed;
            Service.SamplingLogic.OnCompleted -= OnSamplingCompleted;
        }

        void UnregisterFromBaking()
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> UnregisterFromBaking()");

            Service.OutputBakingLogic.OnBakingStarted -= OnBakingStarted;
            Service.OutputBakingLogic.OnBakingProgressed -= OnBakingProgressed;
            Service.OutputBakingLogic.OnBakingCanceled -= OnBakingCanceled;
            Service.OutputBakingLogic.OnBakingFailed -= OnBakingFailed;
            Service.OutputBakingLogic.OnBakingCompleted -= OnBakingCompleted;
        }
        
        void OnSamplingStarted(MotionToKeysSamplingLogic logic)
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> OnSamplingStarted()");
            
            State = Status.SamplingKeys;
        }

        void OnSamplingProgressed(MotionToKeysSamplingLogic logic, float progress)
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> OnSamplingProgressed(" + progress + ")");

            State = Status.SamplingKeys;
            OnProgressed?.Invoke(this, progress*0.5f);
        }

        void OnSamplingCompleted(MotionToKeysSamplingLogic logic)
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> OnSamplingCompleted()");

            UnregisterFromSampling();
            State = Status.BakingOutput;
            RegisterToBaking();
            OnProgressed?.Invoke(this, 0.5f);
        }

        void OnSamplingFailed(MotionToKeysSamplingLogic logic, string error)
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> OnSamplingFailed(" + error + ")");

            UnregisterFromSampling();
            State = Status.Failed;
            OnFailed?.Invoke(this, error);
        }

        void OnSamplingCanceled(MotionToKeysSamplingLogic logic)
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> OnSamplingCanceled()");

            UnregisterFromSampling();
            State = Status.Canceled;
            OnCanceled?.Invoke(this);
        }
        
        void OnBakingStarted(BakingLogic.BakingEventData eventData)
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> OnBakingStarted()");

            State = Status.BakingOutput;
        }

        void OnBakingProgressed(BakingLogic.BakingEventData eventData)
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> OnBakingProgressed(" + eventData.Progress + ")");

            State = Status.BakingOutput;
            OnProgressed?.Invoke(this, 0.5f + eventData.Progress/2f);
        }

        void OnBakingCompleted(BakingLogic.BakingEventData eventData)
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> OnBakingCompleted()");

            UnregisterFromBaking();
            State = Status.Completed;
            OnCompleted?.Invoke(this);
        }

        void OnBakingFailed(BakingLogic.BakingEventData eventData)
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> OnBakingFailed(" + eventData.Message + ")");

            UnregisterFromBaking();
            State = Status.Failed;
            OnFailed?.Invoke(this, eventData.Message);
        }

        void OnBakingCanceled(BakingLogic.BakingEventData eventData)
        {
            
            DevLogger.LogSeverity(TraceLevel.Verbose, "MotionToKeysRequest -> OnBakingCanceled()");

            UnregisterFromBaking();
            State = Status.Canceled;
            OnCanceled?.Invoke(this);
        }
    }
}
