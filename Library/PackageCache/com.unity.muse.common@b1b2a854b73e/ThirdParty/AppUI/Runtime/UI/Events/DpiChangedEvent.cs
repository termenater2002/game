using UnityEngine.UIElements;

namespace Unity.Muse.AppUI.UI
{
    /// <summary>
    /// The DPI has changed.
    /// </summary>
    internal class DpiChangedEvent : EventBase<DpiChangedEvent>
    {
        /// <summary>
        /// The previous value.
        /// </summary>
        public float previousValue { get; set; }

        /// <summary>
        /// The new value.
        /// </summary>
        public float newValue { get; set; }
    }
}
