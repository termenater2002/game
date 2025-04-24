using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class MotionToKeysService
    {
        public bool HasRequests => m_Requests.Count > 0;
        public TimelineModel Output => m_Output;
        public BakedTimelineModel BakedOutput => m_BakedOutput;

        public enum Status
        {
            Unknown,
            Loading,
            Ready,
            Failed
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
        
        public MotionToKeysSamplingLogic SamplingLogic => m_SamplingLogic;
        public BakingLogic OutputBakingLogic => m_OutputBakingLogic;
        
        List<MotionToKeysRequest> m_Requests = new();
        
        Status m_State;
        CameraModel m_CameraModel;
        MotionToKeysRequest m_ActiveRequest;
        
        readonly TimelineModel m_Output;
        readonly BakedTimelineModel m_BakedOutput;
        readonly MotionToKeysSamplingLogic m_SamplingLogic;
        readonly BakingLogic m_OutputBakingLogic;

        bool m_IsBusy;
        
        public event Action<MotionToKeysService, Status> OnStateChanged;
        public event Action<MotionToKeysRequest> OnRequestStarted;
        public event Action<MotionToKeysRequest> OnRequestCanceled;
        public event Action<MotionToKeysRequest> OnRequestCompleted;
        public event Action<MotionToKeysRequest, float> OnRequestProgressed;
        public event Action<MotionToKeysRequest, string> OnRequestFailed;
        
        public MotionToKeysService(StageModel stageModel)
        {
            
        }

        public void AddEntity(EntityID entityID, ArmatureMappingComponent referencePhysicsArmature, ArmatureMappingComponent referenceMotionArmature, PhysicsEntityType physicsEntityType)
        {
            m_OutputBakingLogic.AddEntity(entityID, referencePhysicsArmature, referenceMotionArmature, physicsEntityType);
        }

        public void RemoveEntity(EntityID entityID)
        {
            m_OutputBakingLogic.RemoveEntity(entityID);
        }
        
        public MotionToKeysRequest Request(KeySequenceTake target, float sensitivity, bool useMotionCompletion)
        {
            if (TryGetRequest(target, out var recycledRequest))
            {
                QueueRequest(recycledRequest);
                return recycledRequest;
            }
            
            var newRequest = new MotionToKeysRequest(this, target, sensitivity, useMotionCompletion);
            QueueRequest(newRequest);
            return newRequest;
        }

        public void Update(float delta, bool throttle = false)
        {
            if(m_ActiveRequest == null)
                StartNextRequest();

            m_SamplingLogic.Update(delta, throttle);
        }

        void StartNextRequest()
        {
            if (m_Requests.Count == 0)
                return;

            StartRequest(m_Requests[0]);
            m_Requests.RemoveAt(0);
        }

        void StartRequest(in MotionToKeysRequest request)
        {
            if (request.Target != null)
            {
                m_ActiveRequest = request;
                RegisterToActiveRequest();
                m_ActiveRequest.Start();
            }
            else
            {
                throw new Exception("Missing Target in Request.");
            }
        }

        bool TryGetRequest(KeySequenceTake target, out MotionToKeysRequest result)
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
        
        void QueueRequest(MotionToKeysRequest request)
        {
            if (request.IsActive)
            {
                request.Cancel();
            }
            
            if (m_Requests.Contains(request))
            {
                m_Requests.Remove(request);
            }
            
            m_Requests.Add(request);
        }

        public void RemoveRequest(KeySequenceTake target)
        {
            for(var i = 0; i < m_Requests.Count; i++)
            {
                var request = m_Requests[i];

                if (request.Target != target)
                    continue;

                m_Requests[i].Cancel();
                m_Requests.RemoveAt(i);
                break;
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
        
        void OnActiveRequestStarted(MotionToKeysRequest request)
        {
            OnRequestStarted?.Invoke(request);
        }

        void OnActiveRequestProgressed(MotionToKeysRequest request, float progress)
        {
            OnRequestProgressed?.Invoke(request, progress);
        }

        void OnActiveRequestCompleted(MotionToKeysRequest request)
        {
            OnRequestCompleted?.Invoke(request);
            UnregisterFromActiveRequest();
            m_ActiveRequest = null;
        }
        
        void OnActiveRequestFailed(MotionToKeysRequest request, string errorMessage)
        {
            OnRequestFailed?.Invoke(request, errorMessage);
            UnregisterFromActiveRequest();
            m_ActiveRequest = null;
        }
        
        void OnActiveRequestCanceled(MotionToKeysRequest request)
        {
            OnRequestCanceled?.Invoke(request);
            UnregisterFromActiveRequest();
            m_ActiveRequest = null;
        }
    }
}
