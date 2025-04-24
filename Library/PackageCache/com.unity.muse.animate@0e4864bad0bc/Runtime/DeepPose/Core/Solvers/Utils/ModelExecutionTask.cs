using System;
using System.Collections;
using Unity.DeepPose.ModelBackend;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// Task for handling model execution
    /// </summary>
    class ModelExecutionTask : ITask
    {

        /// <summary>
        /// Defines the different possible states of this task
        /// </summary>
        public enum State
        {
            /// <summary>Unknown state</summary>
            Unknown,
            /// <summary>Preparing to execute Neural Net</summary>
            Starting,
            /// <summary>Executing Neural Net</summary>
            Running,
            /// <summary>Done</summary>
            Done
        }

        /// <summary>Current state of the task, for monitoring purpose</summary>
        public State CurrentState { get; private set; }

        // From ITask
        public bool IsDone => CurrentState == State.Done;

        // From ITask
        public bool IsRunning => CurrentState != State.Unknown && !IsDone;
        public IModelBackend Backend => m_Backend;
        public float Progress => m_Backend?.scheduleProgress ?? 0f;

        IModelBackend m_Backend;

        bool m_Asynchronous;
        IEnumerator m_BackendIterator;

        /// <summary>
        /// Construct a new task
        /// </summary>
        /// <param name="settings">The task settings</param>
        public ModelExecutionTask()
        {
            CurrentState = State.Unknown;
        }

        // From ITask
        public void Dispose()
        {
            if (CurrentState == State.Running)
                Stop();
        }

        /// <summary>
        /// Starts a new task
        /// </summary>
        public void Start(IModelBackend backend, bool async, int syncEveryNthLayer = 100)
        {
            Assert.IsFalse(IsRunning, "Task is already running");
            m_Backend = backend;
            m_Asynchronous = async;
            CurrentState = State.Starting;
        }

        /// <summary>
        /// Stops a new task
        /// </summary>
        public void Stop()
        {
            Assert.IsTrue(IsRunning, "Task is not running");
            CurrentState = State.Unknown;
        }

        // From ITask
        public bool Step()
        {
            switch (CurrentState)
            {
                case State.Unknown:
                    UnityEngine.Debug.LogError("Start() must be called before Step()");
                    break;

                case State.Starting:
                    if (m_Asynchronous)
                    {
                        m_BackendIterator = m_Backend.StartManualSchedule();
                    }
                    CurrentState = State.Running;
                    break;

                case State.Running:
                    if (m_Asynchronous)
                    {
                        if (!m_BackendIterator.MoveNext())
                        {
                            CurrentState = State.Done;
                        }
                    }
                    else
                    {
                        m_Backend.Execute();
                        CurrentState = State.Done;
                    }
                    break;

                case State.Done:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return IsDone;
        }
    }
}
