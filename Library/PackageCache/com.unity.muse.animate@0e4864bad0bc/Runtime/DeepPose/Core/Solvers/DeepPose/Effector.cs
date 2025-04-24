using System;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// Tuple of a Reference to a Transform component and a weight number.
    /// </summary>
    [Serializable]
    struct Effector : IEquatable<Effector>
    {
        /// <summary>Reference to a Transform component.</summary>

        public Transform transform;
        /// <summary>Weight of this effector. This is a number be in between 0 and 1.</summary>

        public float weight;
        /// <summary>Tolerance of this effector. This is a positive number.</summary>

        public float tolerance;
        /// <summary>ID of this effector.</summary>

        public int id;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="transform">Reference to a Transform component.</param>
        /// <param name="id">ID of this effector.</param>
        /// <param name="weight">Weight. This is a number in between 0 and 1.</param>
        /// <param name="tolerance">Tolerance. This is a positive number.</param>
        public Effector(Transform transform, int id, float weight, float tolerance)
        {
            this.transform = transform;
            this.id = id;
            this.weight = Mathf.Clamp01(weight);
            this.tolerance = Mathf.Max(0f, tolerance);
        }

        /// <summary>
        /// Returns a WeightedTransform object with an null Transform component reference and the specified parameters.
        /// </summary>
        /// <param name="id">ID of this effector.</param>
        /// <param name="weight">Weight. This is a number in between 0 and 1.</param>
        /// /// <param name="tolerance">Tolerance. This is a positive number.</param>
        /// <returns>Returns a new Effector</returns>
        public static Effector Default(int id, float weight, float tolerance) => new Effector(null, id, weight, tolerance);

        /// <summary>
        /// Compare two Effector objects for equality.
        /// </summary>
        /// <param name="other">A Effector object</param>
        /// <returns>Returns true if both Effector have the same values. False otherwise.</returns>
        public bool Equals(Effector other)
        {
            return (transform == other.transform
                && weight == other.weight
                && tolerance == other.tolerance
                && id == other.id);
        }
    }
}
