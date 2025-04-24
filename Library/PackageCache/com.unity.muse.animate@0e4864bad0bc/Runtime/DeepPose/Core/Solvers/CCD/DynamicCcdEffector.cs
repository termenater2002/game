using System;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// Tuple of a Reference to a Transform component and a weight number.
    /// </summary>
    [Serializable]
    struct DynamicCcdEffector : IEquatable<DynamicCcdEffector>
    {
        /// <summary>Reference to a Transform component.</summary>
        [SerializeField]
        public Transform Transform;
        /// <summary>Weight of this effector. This is a number be in between 0 and 1.</summary>
        [SerializeField]
        public float Weight;
        /// <summary>Tolerance of this effector. This is a positive number.</summary>
        [SerializeField]
        public float Tolerance;
        /// <summary>ID of this effector.</summary>
        [SerializeField]
        public int Id;
        /// <summary>If the solver is to keep the original offset from this effector and the tip of the CCD chain.</summary>
        [SerializeField]
        public bool MaintainOriginalOffset;

        public bool IsEnabled => Weight > 0f && Transform != null && Id >= 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="transform">Reference to a Transform component.</param>
        /// <param name="id">ID of this effector.</param>
        /// <param name="weight">Weight. This is a number in between 0 and 1.</param>
        /// <param name="tolerance">Distance tolerance between the tip of the chain and the effector.</param>
        /// <param name="maintainOriginalOffset">If the original distance from the tip of the chain and the effector is to be maintained.</param>
        public DynamicCcdEffector(Transform transform, int id, float weight, float tolerance, bool maintainOriginalOffset)
        {
            Transform = transform;
            Id = id;
            Weight = Mathf.Clamp01(weight);
            Tolerance = Mathf.Max(0f, tolerance);
            MaintainOriginalOffset = maintainOriginalOffset;
        }

        /// <summary>
        /// Returns a WeightedTransform object with an null Transform component reference and the specified parameters.
        /// </summary>
        /// <param name="id">ID of this effector.</param>
        /// <param name="weight">Weight. This is a number in between 0 and 1.</param>
        /// <param name="tolerance">Distance tolerance between the tip of the chain and the effector.</param>
        /// <param name="maintainOriginalOffset">If the original distance from the tip of the chain and the effector is to be maintained.</param>
        /// <returns>Returns a new <see cref="DynamicCcdEffector"/></returns>
        public static DynamicCcdEffector Default(int id, float weight, float tolerance, bool maintainOriginalOffset) => new DynamicCcdEffector(null, id, weight, tolerance, maintainOriginalOffset);

        /// <summary>
        /// Compare two Effector objects for equality.
        /// </summary>
        /// <param name="other">A Effector object</param>
        /// <returns>Returns true if both Effector have the same values. False otherwise.</returns>
        public bool Equals(DynamicCcdEffector other)
        {
            return (Transform == other.Transform
                && Weight == other.Weight
                && Tolerance == other.Tolerance
                && Id == other.Id
                && MaintainOriginalOffset == other.MaintainOriginalOffset);
        }
    }
}
