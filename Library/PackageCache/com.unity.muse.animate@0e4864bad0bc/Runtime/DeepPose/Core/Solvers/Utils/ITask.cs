using System;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// Interface for all ML tasks
    /// </summary>
    interface ITask : IDisposable
    {
        /// <summary>True if the task was running and finished its job, false otherwise</summary>
        bool IsDone { get; }

        /// <summary>True if the task is currently running and not done yet, false otherwise</summary>
        bool IsRunning { get; }

        /// <summary>
        /// Performs one more task step
        /// </summary>
        /// <returns>True if the task is done, false otherwise</returns>
        bool Step();
    }
}
