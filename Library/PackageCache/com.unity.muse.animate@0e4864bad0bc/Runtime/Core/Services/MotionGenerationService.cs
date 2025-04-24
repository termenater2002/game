using System;
using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Base class for a service that generates a dense timeline using the provided <see cref="TimelineBakerBase"/>.
    /// </summary>
    /// <typeparam name="TBaker">The class responsible for generating (baking) the dense timeline.</typeparam>
    /// <typeparam name="TTake">The type that will hold the resulting dense timeline.</typeparam>
    /// <typeparam name="TRequest">The type that contains the generation parameters used by <typeparamref name="TBaker"/> .</typeparam>
    abstract class MotionGenerationService<TBaker, TTake, TRequest> where TBaker : TimelineBakerBase, new()
        where TTake : DenseTake
        where TRequest : BakingRequest<TTake>
    {
        public enum Status
        {
            Generating,
            Ready,
            Failed
        }

        public Status State
        {
            get => m_State;
            protected set
            {
                if (m_State == value)
                    return;

                m_State = value;
                OnStateChanged?.Invoke(m_State);
            }
        }

        public event Action<Status> OnStateChanged;
        public event Action<TRequest, float> OnRequestProgressed;
        public event Action<TRequest, string> OnRequestFailed;

        public BakingLogic Baking { get; }

        protected TBaker Baker { get; }

        readonly List<TRequest> m_Requests = new();
        TRequest m_ActiveRequest;
        Status m_State;

        protected MotionGenerationService(TBaker timelineBaker)
        {
            State = Status.Ready;
            Baker = timelineBaker;
            Baking = new BakingLogic(null, new BakedTimelineModel(), null, timelineBaker);
        }

        public void AddEntity(EntityID entityID, ArmatureMappingComponent referencePhysicsArmature, ArmatureMappingComponent referenceMotionArmature, PhysicsEntityType physicsEntityType)
        {
            Baking.AddEntity(entityID, referencePhysicsArmature, referenceMotionArmature, physicsEntityType);
        }

        public void RemoveEntity(EntityID entityID)
        {
            Baking.RemoveEntity(entityID);
        }

        public void QueueRequest(TRequest request, bool first = false)
        {
            if (request.IsActive)
            {
                request.Cancel();
            }

            if (m_Requests.Contains(request))
            {
                m_Requests.Remove(request);
            }

            if (first)
            {
                m_Requests.Insert(0,request);
                return;
            }
            
            m_Requests.Add(request);
        }

        protected bool TryGetRequest(TTake target, out TRequest result)
        {
            for (var i = 0; i < m_Requests.Count; i++)
            {
                var request = m_Requests[i];

                if (request.Target != target)
                    continue;
                result = m_Requests[i];
                return true;
            }

            result = null;
            return false;
        }

        public void Update(float delta, bool throttle = false)
        {
            if (m_ActiveRequest == null)
                StartNextRequest();

            Baking.Update(delta, throttle);
        }

        /// <summary>
        /// Cancel all active requests. 
        /// </summary>
        public void Stop()
        {
            if (m_ActiveRequest != null)
            {
                m_ActiveRequest.Cancel();
                m_ActiveRequest = null;
            }

            foreach (var request in m_Requests)
            {
                request.Cancel();
            }

            m_Requests.Clear();
        }

        void StartNextRequest()
        {
            if (m_Requests.Count == 0)
                return;

            StartRequest(m_Requests[0]);
            m_Requests.RemoveAt(0);
        }

        void StartRequest(TRequest request)
        {
            if (request.Target != null)
            {
                m_ActiveRequest = request;
                RegisterToActiveRequest();
                m_ActiveRequest.Start();
                State = Status.Generating;
            }
            else
            {
                throw new Exception("Missing Target in Request.");
            }
        }

        void RegisterToActiveRequest()
        {
            m_ActiveRequest.OnStarted += OnActiveRequestStarted;
            m_ActiveRequest.OnCompleted += OnActiveRequestCompleted;
            m_ActiveRequest.OnFailed += OnActiveRequestFailed;
            m_ActiveRequest.OnProgressed += OnActiveRequestProgressed;
            m_ActiveRequest.OnCanceled += OnActiveRequestCanceled;
        }

        void UnregisterFromActiveRequest()
        {
            m_ActiveRequest.OnStarted -= OnActiveRequestStarted;
            m_ActiveRequest.OnCompleted -= OnActiveRequestCompleted;
            m_ActiveRequest.OnFailed -= OnActiveRequestFailed;
            m_ActiveRequest.OnProgressed -= OnActiveRequestProgressed;
            m_ActiveRequest.OnCanceled -= OnActiveRequestCanceled;
        }

        void OnActiveRequestStarted() { }

        void OnActiveRequestProgressed(float overallProgress)
        {
            OnRequestProgressed?.Invoke(m_ActiveRequest, overallProgress);
        }

        void OnActiveRequestCompleted()
        {
            UnregisterFromActiveRequest();
            State = Status.Ready;
            m_ActiveRequest = null;
        }

        void OnActiveRequestFailed(string errorMessage)
        {
            UnregisterFromActiveRequest();
            OnRequestFailed?.Invoke(m_ActiveRequest, errorMessage);
            State = Status.Failed;
            m_ActiveRequest = null;
        }

        void OnActiveRequestCanceled()
        {
            UnregisterFromActiveRequest();
            State = Status.Ready;
            m_ActiveRequest = null;
        }
    }
}
