using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    /// <summary>
    ///  This operator is deprecated and will be removed in a future release.
    ///  Please use the negative prompt parameter in Prompt Operator instead.
    /// </summary>
    [Serializable]
    internal class NegativePromptOperator : IOperator
    {
        public string OperatorName  => "NegativePromptOperator";
        /// <summary>
        /// Human-readable label for the operator.
        /// </summary>
        public string Label => "Negative Prompt";

        [SerializeField]
        OperatorData m_OperatorData;

        event Action OnDataUpdate;

        public NegativePromptOperator()
        {
            m_OperatorData = new OperatorData(OperatorName, "0.0.1",  new [] { "" }, false);
        }

        public bool IsSavable()
        {
            return false;
        }

        public VisualElement GetCanvasView()
        {
            Debug.Log("NegativePromptOperator.GetCanvasView()");
            return new VisualElement();
        }

        public VisualElement GetOperatorView(Model model)
        {
            return null;
        }

        public OperatorData GetOperatorData()
        {
            return m_OperatorData;
        }

        public void SetOperatorData(OperatorData data)
        {
            m_OperatorData.type = data.type;
            m_OperatorData.version = data.version;
            m_OperatorData.enabled = data.enabled;
            if(data.settings != null && data.settings.Length == 1)
                m_OperatorData.settings = data.settings;

            OnDataUpdate?.Invoke();
        }
        void SetSettings(IReadOnlyList<string> settings)
        {
            m_OperatorData.settings[0] = settings[0];
            OnDataUpdate?.Invoke();
        }

        string[] GetSettings()
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

        public IOperator Clone()
        {
            var result = new NegativePromptOperator();
            var operatorData = new OperatorData();
            operatorData.FromJson(GetOperatorData().ToJson());
            result.SetOperatorData(operatorData);
            return result;
        }
        public void RegisterToEvents(Model model)
        { }

        public void UnregisterFromEvents(Model model)
        { }
        public VisualElement GetSettingsView(Model model, ref bool isCustomSection, Action dismissAction)
        {
            return null;
        }
    }
}
