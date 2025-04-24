using System;
using System.Diagnostics;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Base class for connecting the result of a <see cref="BakingLogic"/> to a <see cref="TakeModel"/>.
    /// </summary>
    /// <typeparam name="TTake"></typeparam>
    abstract class BakingRequest<TTake> where TTake : DenseTake
    {
        Status m_State;

        public enum Status
        {
            Stopped,
            WaitingForBaking,
            Baking,
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
                OnStateChanged?.Invoke(m_State);
            }
        }

        public bool IsActive => State is Status.Baking or Status.WaitingForBaking;

        public event Action<Status> OnStateChanged;
        
        public event Action OnStarted;
        public event Action OnCanceled;
        public event Action OnCompleted;
        public event Action<float> OnProgressed;
        public event Action<string> OnFailed;
        
        BakingLogic Baking { get; }

        public TTake Target { get; protected set; }

        protected BakingRequest(BakingLogic baking)
        {
            Baking = baking;
        }

        protected abstract void InitializeParameters();
        
        protected abstract void FinalizeBaking(BakedTimelineModel output);

        public void Start()
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType()} -> Start()");
            InitializeParameters();
            RegisterToBaking();

            State = Status.WaitingForBaking;
            OnStarted?.Invoke();

            Baking.QueueBaking(false);
        }

        public void Cancel()
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType()} -> Cancel()");

            Stop();
        }

        void Stop()
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType()} -> Stop()");

            if (Baking.IsRunning)
            {
                // Stop the baking logic. This will trigger the OnBakingCanceled event.
                Baking.Cancel();
            }
            else
            {
                // We are not baking, so we need to manually trigger the OnBakingCanceled event.
                OnBakingCanceled(Baking.GetEventData(BakingLogic.BakingEventData.BakingEventType.Canceled));
            }
        }

        void RegisterToBaking()
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType()} -> RegisterToBaking()");

            Baking.OnBakingStarted += OnBakingStarted;
            Baking.OnBakingCanceled += OnBakingCanceled;
            Baking.OnBakingFailed += OnBakingFailed;
            Baking.OnBakingCompleted += OnBakingCompleted;
            Baking.OnBakingProgressed += OnBakingProgressed;
        }

        void UnregisterFromBaking()
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType()} -> UnregisterFromBaking()");

            Baking.OnBakingStarted -= OnBakingStarted;
            Baking.OnBakingCanceled -= OnBakingCanceled;
            Baking.OnBakingFailed -= OnBakingFailed;
            Baking.OnBakingCompleted -= OnBakingCompleted;
            Baking.OnBakingProgressed -= OnBakingProgressed;
        }

        void OnBakingStarted(BakingLogic.BakingEventData eventData)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType()} -> OnBakingStarted()");

            State = Status.Baking;
            OnStarted?.Invoke();
        }

        void OnBakingProgressed(BakingLogic.BakingEventData eventData)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType()} -> OnBakingProgressed(" + eventData.Progress + ")");

            State = Status.Baking;
            OnProgressed?.Invoke(eventData.Progress);
        }

        void OnBakingCompleted(BakingLogic.BakingEventData eventData)
        {
            DevLogger.LogSeverity(TraceLevel.Info, $"{GetType()} -> OnBakingCompleted()");

            UnregisterFromBaking();
            
            eventData.BakedTimelineModel.CopyTo(Target.BakedTimelineModel);
            FinalizeBaking(eventData.BakedTimelineModel);
            
            // Change state to Completed at the end
            State = Status.Completed;
            OnCompleted?.Invoke();
        }

        void OnBakingFailed(BakingLogic.BakingEventData eventData)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType()} -> OnBakingFailed(" + eventData.Message + ")");

            UnregisterFromBaking();
            State = Status.Failed;
            OnFailed?.Invoke(eventData.Message);
        }

        void OnBakingCanceled(BakingLogic.BakingEventData eventData)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType()} -> OnBakingCanceled()");

            UnregisterFromBaking();
            State = Status.Canceled;
            OnCanceled?.Invoke();
        }
    }
}
