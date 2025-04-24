using UnityEngine.UIElements;

namespace Unity.Muse.Common.Editor
{
    /// <summary>
    /// Interface to implement your own section in the Muse Preferences window.
    /// </summary>
    interface IMuseEditorPreferencesView
    {
        /// <summary>
        /// Called to create the GUI for the preferences section.
        /// </summary>
        /// <returns> The root VisualElement for the preferences section. </returns>
        VisualElement CreateGUI();
        
        /// <summary>
        /// Called to refresh the preferences section.
        /// </summary>
        /// <remarks>
        /// This is usually where you want to update the GUI with the current preferences values.
        /// </remarks>
        void Refresh();
    }
}
