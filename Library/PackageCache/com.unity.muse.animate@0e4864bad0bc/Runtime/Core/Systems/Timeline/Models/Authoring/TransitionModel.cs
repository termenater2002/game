using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents a transition
    /// </summary>
    [Serializable]
    class TransitionModel
    {
        public enum Property
        {
            Duration,
            Type
        }

        [SerializeField]
        TransitionData m_Data;

        /// <summary>
        /// Checks if the transition is in a valid state
        /// </summary>
        public bool IsValid => m_Data.Duration > 0;

        /// <summary>
        /// Duration of the transition, in frames
        /// </summary>
        public int Duration
        {
            get => m_Data.Duration;
            set => SetDuration(value, false);
        }

        /// <summary>
        /// The type of the transition
        /// </summary>
        public TransitionData.TransitionType Type
        {
            get => m_Data.Type;
            set => SetType(value, false);
        }

        /// <summary>
        /// The index of this transition in its parent TimelineModel
        /// </summary>
        public int ListIndex { get; set; }
        
        public delegate void Changed(TransitionModel model, Property property);
        public event Changed OnChanged;

        /// <summary>
        /// Creates a new transition
        /// </summary>
        public TransitionModel()
        {
            m_Data.Duration = 30;
            m_Data.Type = TransitionData.TransitionType.MotionSynthesis;
        }

        /// <summary>
        /// Copies the transition to another transition.
        /// </summary>
        /// <param name="other">The other transition to which the state will be copied.</param>
        /// <param name="silent">If the other object we are copying to will emit change events or not.</param>
        public void CopyTo(TransitionModel other, bool silent = false)
        {
            other.SetDuration(Duration, silent);
            other.SetType(Type, silent);
            other.ListIndex = ListIndex;
        }

        void SetType(TransitionData.TransitionType value, bool silent)
        {
            if (value == m_Data.Type)
                return;

            m_Data.Type = value;

            if (!silent)
                OnChanged?.Invoke(this, Property.Type);
        }

        void SetDuration(int value, bool silent)
        {
            var previousValue = m_Data.Duration;
            var correctedValue = Mathf.Max(1, value);
            if (correctedValue == previousValue)
                return;

            m_Data.Duration = correctedValue;
            
            if(!silent)
                OnChanged?.Invoke(this, Property.Duration);
        }

        /// <summary>
        /// Create a duplicate of this transition
        /// </summary>
        /// <returns>A new transition instance that is a copy of this transition</returns>
        public TransitionModel Clone()
        {
            var clone = new TransitionModel();
            CopyTo(clone);
            return clone;
        }
        
        /// <summary>
        /// Merge a source transition into that transition.
        /// </summary>
        /// <param name="source">The source transition to merge. Left unchanged.</param>
        public void Fuse(TransitionModel source)
        {
            Duration += source.Duration;
            // Type is left unchanged for now until we have to deal with it
        }

        /// <summary>
        /// Splits a transition to a target transition.
        /// This transition will be updated to store the first half and the target transition the second half
        /// </summary>
        /// <param name="target">The target transition that will store the second half of the split result</param>
        /// <param name="timePercent">An indication of where to perform the split. Must be between 0 and 1.</param>
        public void Split(TransitionModel target, float timePercent = 0.5f)
        {
            if (timePercent < 0f || timePercent > 1f)
                AssertUtils.Fail($"Invalid splitting percent: {timePercent}. Must be between 0 and 1.");

            target.Type = Type;
            var remainder = Mathf.RoundToInt((1f-timePercent) * Duration);
            target.Duration = remainder;
            Duration -= remainder;

            target.Type = Type;
        }
    }
}
