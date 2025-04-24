using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class RequestRunner : IProcessingRequest
    {
        public IProcessingRequest.ProcessState State => m_Request?.State ?? IProcessingRequest.ProcessState.Unknown;
        public float Progress => m_Request?.Progress ?? 0f;
        public bool IsRunning => State is IProcessingRequest.ProcessState.InProgress or IProcessingRequest.ProcessState.InWaitDelay;
        public bool IsWaitingDelay => State == IProcessingRequest.ProcessState.InWaitDelay;
        public float WaitDelay => m_Request?.WaitDelay ?? 0f;
        public DateTime WaitStartTime => m_Request?.WaitStartTime ?? DateTime.Now;
        
        bool CanStep => m_Request != null && IsRunning;
        public bool CanSkipToNextFrame => false;
        
        public float TimeBudget
        {
            get => m_TimeBudget;
            set => m_TimeBudget = value;
        }
        
        IProcessingRequest m_Request;
        float m_TimeBudget;
        readonly Stopwatch m_Stopwatch = new();

        public RequestRunner(float timeBudget)
        {
            m_TimeBudget = timeBudget;
        }

        public void Initialize(IProcessingRequest request)
        {
            Assert.IsFalse(IsRunning, "Request is already running. Stop it before initializing.");
            m_Request = request;
        }

        public void Start()
        {
            Assert.IsNotNull(m_Request, "No request. Did you Initialize first?");
            m_Request.Start();
        }

        public void Stop()
        {
            m_Request.Stop();
            m_Request = null;
        }

        public void Step()
        {
            if (!CanStep)
                return;
            
            m_Stopwatch.Reset();
            m_Stopwatch.Start();
            
            if (m_Request.State == IProcessingRequest.ProcessState.InWaitDelay)
            {
                m_Request.Step();
            }
            else
            {
                var maxTime = m_Stopwatch.ElapsedMilliseconds + (long)(m_TimeBudget * 1000);
                var remainingTimeBudget = maxTime - m_Stopwatch.ElapsedMilliseconds;
            
                while (CanStep && remainingTimeBudget > 0)
                {
                    m_Request.Step();

                    // Update remaining time budget
                    remainingTimeBudget = maxTime - m_Stopwatch.ElapsedMilliseconds;

                    if (m_Request.CanSkipToNextFrame)
                    {
                        break;
                    }
                }
            }
        }
    }
}
