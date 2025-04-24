using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    internal interface IOperator
    {
        public string OperatorName { get; }
        /// <summary>
        /// Human-readable label for the operator.
        /// </summary>
        public string Label { get; }
        public bool Enabled();
        public void Enable(bool enable);
        /// <summary>
        /// Whether the operator should be hidden from the UI.
        /// </summary>
        public bool Hidden { get; set; }
        public VisualElement GetCanvasView();
        public VisualElement GetOperatorView(Model model);

        /// <summary>
        /// Get the settings view for this operator.
        /// </summary>
        /// <param name="model">Current Model</param>
        /// <param name="isCustomSection">This VisualElement will override the whole operator section used by the generation settings</param>
        /// <param name="dismissAction">Action to trigger on dismiss</param>
        /// <returns> UI for the operator. Set to Null if the operator should not be displayed in the settings view. Disable the returned VisualElement if you want it to be displayed but not usable.</returns>
        public VisualElement GetSettingsView(Model model, ref bool isCustomSection, Action dismissAction);
   
        public OperatorData GetOperatorData();

        public void SetOperatorData(OperatorData data);

        public IOperator Clone();

        public void RegisterToEvents(Model model);
        public void UnregisterFromEvents(Model model);
        public bool IsSavable();
    }
}
