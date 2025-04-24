using System;

namespace Unity.Muse.Animate
{
    interface IProcessingRequest
    {
        /// <summary>
        /// The possible states of a processing request.
        /// </summary>
        public enum ProcessState
        {
            Unknown,
            InWaitDelay,
            InProgress,
            Done,
            Failed
        }

        /// <summary>
        /// Get the current state of the process
        /// </summary>
        public ProcessState State { get; }

        /// <summary>
        /// Get the total wait delay before processing the request (in seconds). 
        /// </summary>
        public float WaitDelay { get; }
        
        /// <summary>
        /// Get the exact time when the request started the wait delay.
        /// </summary>
        public DateTime WaitStartTime { get; }
        
        /// <summary>
        /// Get the process progress, 0 meaning no progress, and 1 meaning it completed
        /// </summary>
        public float Progress { get; }

        /// <summary>
        /// Starts the request process. Must be called once before Step()
        /// </summary>
        public void Start();

        /// <summary>
        /// Stops the request process. Request should be running
        /// </summary>
        public void Stop();

        /// <summary>
        /// Ask the process to perform a single step so that the processing can be dispatched in multiple steps
        /// Process must be started first
        /// </summary>
        public void Step();

        /// <summary>
        /// A flag if the processing could skip to the next frame, e.g., when waiting for
        /// web requests to complete.
        /// </summary>
        public bool CanSkipToNextFrame { get; }
    }
}
