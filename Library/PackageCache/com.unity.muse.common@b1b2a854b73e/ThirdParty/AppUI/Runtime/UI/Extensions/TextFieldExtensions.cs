using System;
using UnityEngine.UIElements;

namespace Unity.Muse.AppUI.UI
{
    /// <summary>
    /// Extensions for the <see cref="TextField"/> class.
    /// </summary>
    internal static class TextFieldExtensions
    {
        /// <summary>
        /// Make the cursor blink.
        /// </summary>
        /// <param name="tf">The <see cref="TextField"/> object.</param>
        [Obsolete("Use Unity.Muse.AppUI.UI.BlinkingCursor manipulator instead.")]
        public static void BlinkingCursor(this UnityEngine.UIElements.TextField tf)
        {
            tf.AddManipulator(new BlinkingCursor());
        }
    }
}
