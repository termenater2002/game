using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Unity.DeepPose.Components
{
    abstract class EffectorRecoveryBase : MonoBehaviour, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct PosingEffector : IEquatable<PosingEffector>
        {
            public enum EffectorType
            {
                Position,
                Rotation,
                LookAt
            }

            public EffectorType Type;
            public int ID;
            public int Priority;

            public PosingEffector(EffectorType type, int id, int priority = 0)
            {
                Type = type;
                ID = id;
                Priority = priority;
            }

            public bool Equals(PosingEffector other)
            {
                return Type == other.Type && ID == other.ID && Priority == other.Priority;
            }

            public override bool Equals(object obj)
            {
                return obj is PosingEffector other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine((int)Type, ID, Priority);
            }

            public static bool operator ==(PosingEffector left, PosingEffector right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(PosingEffector left, PosingEffector right)
            {
                return !left.Equals(right);
            }
        }

        struct StepInfo
        {
            public float Error;
            public PosingEffector Effector;

            public StepInfo(float error, PosingEffector effector)
            {
                Error = error;
                Effector = effector;
            }
        }

        struct PoseData
        {
            public Vector3 Position;
            public Quaternion Rotation;

            public PoseData(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }
        }

        /// <summary>
        /// The set of effectors that must be enabled to recover the pose.
        /// </summary>
        public PosingEffector[] MandatoryEffectors;
        
        /// <summary>
        /// The set of effectors that may be enabled to recover the pose.
        /// </summary>
        public PosingEffector[] CandidateEffectors;

        [Range(0f, 0.1f)]
        public float MaxError = 0.03f;

        public int MaxEffectors = -1;

        public bool ErrorOnEffectorsOnly = true;

        /// <summary>
        /// If true, use the <see cref="PosingEffector.Priority"/> values when evaluating the effectors. Otherwise,
        /// all possible combinations of <see cref="CandidateEffectors"/> will be tested iteratively.
        /// </summary>
        /// <remarks>
        /// If true, the effectors are tested in order of priority. It is assumed that effectors of the same priority
        /// are mostly independent and can be enabled in any order (e.g. the solution from enabling A then B is
        /// the same as the solution from enabling B then A).
        /// </remarks>
        public bool UseEffectorPriorities = false;

        PoseData[] m_CapturedPose;
        List<StepInfo> m_Steps = new();

        protected virtual int JointsCount => 0;
        protected virtual int PositionEffectorsCount => 0;
        protected abstract int GetPositionEffectorJoint(int effectorIdx);
        protected abstract bool HasEffector(int jointId);
        protected abstract void EnableEffector(PosingEffector.EffectorType type, int jointId, bool enable = true);
        protected abstract Transform GetJointTransform(int jointIdx);

        protected abstract void SolvePose();
        protected abstract void SnapAllEffectors();
        protected abstract void DisableAllEffectors();

        /// <summary>
        /// The effector candidates groups and sorted according to priority.
        /// </summary>
        readonly SortedDictionary<int, List<PosingEffector>> m_CandidateHierarchy = new();

        public void Solve()
        {
            if (JointsCount == 0)
                return;
            ComputeSteps();

            Apply();
        }

        public void Apply()
        {
            DisableAllEffectors();
            RestorePoseAndSnapEffectors();

            foreach (var step in m_Steps)
            {
                EnableEffector(step.Effector.Type, step.Effector.ID);

                if (step.Error <= MaxError)
                    break;
            }

            SolvePose();
        }

        protected void ComputeSteps()
        {
            DisableAllEffectors();
            RestorePoseAndSnapEffectors();

            m_Steps.Clear();
            
            if (UseEffectorPriorities)
            {
                ComputeStepsHierarchical();
            }
            else
            {
                ComputeStepsIterative();
            }

            RestorePoseAndSnapEffectors();
        }

        void ComputeStepsIterative()
        {
            var effectorsPool = HashSetPool<PosingEffector>.Get();
            
            if (CandidateEffectors.Length > 0)
            {
                foreach (var t in CandidateEffectors)
                {
                    effectorsPool.Add(t);
                }
            }
            else
            {
                for (var i = 0; i < PositionEffectorsCount; i++)
                {
                    var jointId = GetPositionEffectorJoint(i);
                    if (GetJointTransform(jointId) == null)
                        continue;

                    effectorsPool.Add(new PosingEffector(PosingEffector.EffectorType.Position, jointId));
                }
            }

            var hasAtLeastOnePosition = false;
            
            foreach (var effector in MandatoryEffectors)
            {
                // Shouldn't happen, but in case an effector shows up in 2 places.
                effectorsPool.Remove(effector);
                
                EnableEffector(effector.Type, effector.ID);
                m_Steps.Add(new StepInfo(float.PositiveInfinity, effector));
                
                hasAtLeastOnePosition |= effector.Type == PosingEffector.EffectorType.Position;
            }

            var error = float.PositiveInfinity;
            if (hasAtLeastOnePosition)
            {
                SolvePose();
                error = ComputeError(ErrorOnEffectorsOnly);
            }

            while (effectorsPool.Count > 0 && (MaxEffectors < 0 || m_Steps.Count < MaxEffectors))
            {
                var minError = float.PositiveInfinity;
                PosingEffector minEffector = default;

                foreach (var effector in effectorsPool)
                {
                    EnableEffector(effector.Type, effector.ID);

                    SolvePose();
                    var newError = ComputeError(ErrorOnEffectorsOnly);

                    if (newError < minError)
                    {
                        minError = newError;
                        minEffector = effector;
                    }

                    EnableEffector(effector.Type, effector.ID, false);
                }

                if (minError < error)
                    error = minError;

                EnableEffector(minEffector.Type, minEffector.ID);
                m_Steps.Add(new StepInfo(error, minEffector));

                effectorsPool.Remove(minEffector);
            }
            
            HashSetPool<PosingEffector>.Release(effectorsPool);
        }

        void ComputeStepsHierarchical()
        {
            var hasAtLeastOnePosition = false;
            foreach (var effector in MandatoryEffectors)
            {
                hasAtLeastOnePosition |= effector.Type == PosingEffector.EffectorType.Position;
                EnableEffector(effector.Type, effector.ID);
                m_Steps.Add(new StepInfo(float.PositiveInfinity, effector));
            }

            // Get the initial error with only the mandatory effectors enabled
            var error = float.PositiveInfinity;
            if (hasAtLeastOnePosition)
            {
                SolvePose();
                error = ComputeError(ErrorOnEffectorsOnly);
            }

            // Test effectors group by priority, starting with the highest
            // We assume that we can enable each effector in priority group independently
            // (i.e. order of enabling does not matter)
            foreach (var (_, candidates) in m_CandidateHierarchy)
            {
                var effectorsAdded = false;
                foreach (var effector in candidates)
                {
                    EnableEffector(effector.Type, effector.ID);

                    SolvePose();
                    var newError = ComputeError(ErrorOnEffectorsOnly);

                    if (newError < error)
                    {
                        error = newError;
                        m_Steps.Add(new StepInfo(error, effector));
                        effectorsAdded = true;
                    }
                    else
                    {
                        EnableEffector(effector.Type, effector.ID, false);
                    }
                }
                
                if (!effectorsAdded)
                {
                    // Skip lower-priority effectors if none of the current priority group improved the error.
                    break;
                }
            }
        }
        
        public void CapturePose()
        {
            m_CapturedPose = new PoseData[JointsCount];

            for (var i = 0; i < JointsCount; i++)
            {
                var jointTransform = GetJointTransform(i);
                if (jointTransform == null)
                    continue;

                m_CapturedPose[i] = new PoseData(jointTransform.position, jointTransform.rotation);
            }
        }

        public void RestorePoseAndSnapEffectors()
        {
            if (m_CapturedPose == null || m_CapturedPose.Length != JointsCount)
                return;

            for (var i = 0; i < JointsCount; i++)
            {
                var jointTransform = GetJointTransform(i);
                if (jointTransform == null)
                    continue;

                jointTransform.position = m_CapturedPose[i].Position;
                jointTransform.rotation = m_CapturedPose[i].Rotation;
            }

            SnapAllEffectors();
        }

        float ComputeError(bool includeEffectorsOnly = true)
        {
            if (m_CapturedPose == null || JointsCount != m_CapturedPose.Length)
                return 0f;

            var error = 0f;
            var count = 0;

            for (var i = 0; i < JointsCount; i++)
            {
                var jointTransform = GetJointTransform(i);
                if (jointTransform == null)
                    continue;

                if (includeEffectorsOnly && !HasEffector(i))
                    continue;

                var jointError = (jointTransform.position - m_CapturedPose[i].Position).magnitude;

                count++;
                error += jointError;
            }

            return count > 0 ? error / count : 0f;
        }

        public void OnBeforeSerialize()
        {
            // No work to do here
        }

        public void OnAfterDeserialize()
        {
            foreach (var (_, list) in m_CandidateHierarchy)
            {
                ListPool<PosingEffector>.Release(list);
            }
            
            m_CandidateHierarchy.Clear();
            
            foreach (var effector in CandidateEffectors)
            {
                if (m_CandidateHierarchy.TryGetValue(effector.Priority, out var list))
                {
                    list.Add(effector);
                }
                else
                {
                    list = ListPool<PosingEffector>.Get();
                    list.Clear();
                    list.Add(effector);
                    m_CandidateHierarchy.Add(effector.Priority, list);
                }
            }
        }
    }
}
