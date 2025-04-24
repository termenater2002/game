namespace Unity.Muse.Animate
{
    enum PhysicsEntityType
    {
        /// <summary>
        /// Static object that never moves
        /// </summary>
        Kinematic,

        /// <summary>
        /// Physically simulated object, passive
        /// </summary>
        Dynamic,

        /// <summary>
        /// Active ragdoll that applies forces to match target poses
        /// </summary>
        Active
    }
}
