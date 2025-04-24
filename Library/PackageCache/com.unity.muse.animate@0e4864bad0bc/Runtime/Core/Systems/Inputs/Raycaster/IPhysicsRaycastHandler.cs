using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Add this to an event handler to filter raycast and specify priority order for user interactions
    /// </summary>
    interface IPhysicsRaycastHandler
    {
        /// <summary>
        /// Check if the raycast is inside the event handler
        /// </summary>
        /// <param name="screenPosition">The screenPosition at which the raycast was performed</param>
        /// <param name="hit">The raycast hit information</param>
        /// <returns>True if the raycast hit is validated and the UI should interact with the object, False otherwise</returns>
        public bool ValidateRaycastHit(Vector2 screenPosition, RaycastHit hit);

        /// <summary>
        /// Sorting layer order to use, higher means it will have higher priority even if further from camera
        /// Candidates are sorted first by layer, then by order, and then by hit distance
        /// Default layer is 0
        /// </summary>
        public int GetPhysicsRaycastSortingOrder(Vector2 screenPosition, RaycastHit hit);

        /// <summary>
        /// Sorting order to use, higher means it will have higher priority even if further from camera
        /// Candidates are sorted first by layer, then by order, and then by hit distance
        /// Default order is 0
        /// </summary>
        public int GetPhysicsRaycastDepth(Vector2 screenPosition, RaycastHit hit);
    }
}
