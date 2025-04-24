using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class GenerateOperator : IOperator
    {
        private const string defaultOperation = "TextToImage";
        private const string defaultGenerationCount = "1";
        public virtual string OperatorName => "GenerateOperator";

        /// <summary>
        /// Human-readable label for the operator.
        /// </summary>
        public string Label => "Generate";

        public virtual string GenerateType => "Images";

        event Action OnDataUpdate;

        [SerializeField]
        internal OperatorData m_OperatorData;

        public GenerateOperator(): this(defaultOperation, defaultGenerationCount){}

        public GenerateOperator(string initialOperation, string initialGenerationCount)
        {
            m_OperatorData = new OperatorData(
                OperatorName,
                "1.0.0",
                new[] { initialOperation, initialGenerationCount },
                true);
        }

        public bool IsSavable()
        {
            return true;
        }

        public int GetCount()
        {
            return int.Parse(m_OperatorData.settings[1]);
        }

        public void SetCount(int count)
        {
            m_OperatorData.settings[1] = count.ToString();
        }

        public void SetDropdownValue(int mode)
        {
            m_OperatorData.settings[0] = ModesFactory.GetModeKeyFromIndex(mode);
        }

        public VisualElement GetCanvasView()
        {
            Debug.Log("PromptOperator.GetCanvasView()");
            return new VisualElement();
        }

        public virtual void SetupGeneratorCountSlider(GenerateOperatorUI ui)
        {
            ui.SetupGeneratorCountSlider(GenerateType);
        }

        public virtual void SetupGenerateButton(GenerateOperatorUI ui)
        {
            ui.SetupGenerateButton();
        }

        public virtual GenerateOperatorUI CreateGenerateOperatorUI()
        {
            return new GenerateOperatorUI();
        }

        public virtual VisualElement GetOperatorView(Model model)
        {
            var ui = CreateGenerateOperatorUI();
            ui.SetupLoading();
            ui.SetupUIBasics(model, m_OperatorData);
            SetupGeneratorCountSlider(ui);
            SetupGenerateButton(ui);
            ui.SubscribeToEvents(OnDataUpdate);
            return ui;
        }

        /// <summary>
        /// Get the settings view for this operator.
        /// </summary>
        /// <param name="model">Current Model</param>
        /// <param name="isCustomSection">This VisualElement will override the whole operator section used by the generation settings</param>
        /// /// <param name="dismissAction">Action to trigger on dismiss</param>
        /// <returns> UI for the operator. Set to Null if the operator should not be displayed in the settings view. Disable the returned VisualElement if you want it to be displayed but not usable.</returns>
        public virtual VisualElement GetSettingsView(Model model, ref bool isCustomSection, Action dismissAction)
        {
            return null;
        }

        public OperatorData GetOperatorData()
        {
            return m_OperatorData;
        }

        public void SetOperatorData(OperatorData data)
        {
            m_OperatorData.enabled = data.enabled;
            if (data.settings == null || data.settings.Length < 2)
                return;
            m_OperatorData.settings = data.settings;
            OnDataUpdate?.Invoke();
        }

        internal void SetSettings(IReadOnlyList<string> settings)
        {
            m_OperatorData.settings[0] = settings[0];
            m_OperatorData.settings[1] = settings[1];

            OnDataUpdate?.Invoke();
        }

        internal string[] GetSettings()
        {
            return m_OperatorData.settings;
        }

        public bool Enabled()
        {
            return m_OperatorData.enabled;
        }

        public void Enable(bool enable)
        {
            m_OperatorData.enabled = enable;
        }

        public bool Hidden { get; set; }

        public virtual GenerateOperator CreateGenerateOperator()
        {
            return new GenerateOperator();
        }

        public virtual IOperator Clone()
        {
            var result = CreateGenerateOperator();
            var operatorData = new OperatorData();
            operatorData.FromJson(GetOperatorData().ToJson());
            result.SetOperatorData(operatorData);
            return result;
        }

        public void RegisterToEvents(Model model) { }

        public void UnregisterFromEvents(Model model) { }
    }
}
