using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Text = Unity.Muse.AppUI.UI.Text;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class UpscaleOperator : IProxyOperator 
    {
        public string OperatorName  => "UpscaleOperator";
        /// <summary>
        /// Human-readable label for the operator.
        /// </summary>
        public string Label => "Upscale Image";

        event Action OnDataUpdate;

        [SerializeField]
        OperatorData m_OperatorData;

        public UpscaleOperator()
        {
            m_OperatorData = new OperatorData(OperatorName, "0.0.1",  new [] { "", "" }, false);
        }

        public bool IsSavable()
        {
            return true;
        }

        public IEnumerable<IOperator> CloneProxyOperators()
        {
            var operatorsData = JsonUtility.FromJson<OperatorDataArray>(m_OperatorData.settings[1]);
            var operators = ModesFactory.GetOperators(operatorsData.operators);
            return operators;
        }

        public VisualElement GetCanvasView()
        {
            Debug.Log("UpscaleOperator.GetCanvasView()");
            return new VisualElement();
        }

        public VisualElement GetOperatorView(Model model)
        {
            var UI = new ExVisualElement
            {
                passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows | ExVisualElement.Passes.BackgroundColor
            };
            UI.AddToClassList("muse-node");
            UI.AddToClassList("appui-elevation-8");
            UI.name = "upscale-node";

            //title
            var text = new Text();
            text.text = Label;
            text.AddToClassList("muse-node__title");
            text.AddToClassList("bottom-gap");
            UI.Add(text);

            //m_Image = new Image();
            // m_Image.AddToClassList("muse-ref-image");
            // m_Image.name = "muse-upscale-image-field";
            //
            // UI.Add(m_Image);
            return UI;
        }

        public OperatorData GetOperatorData()
        {
            return m_OperatorData;
        }

        public void SetOperatorData(OperatorData data)
        {
            m_OperatorData.enabled = data.enabled;
            if (data.settings == null || data.settings.Length < 1)
                return;
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

        public void SetParent(Artifact parent)
        {
            if(parent == null)
                return;
            
            m_OperatorData.settings[0] = parent.Guid;
            var ops = parent.CloneOperators();

            var opsDataArray = new OperatorDataArray
            {
                operators = new OperatorData[ops.Count]
            };
            
            for (var i = 0; i < ops.Count; i++)
            {
                opsDataArray.operators[i] = ops[i].GetOperatorData();
            }
            
            m_OperatorData.settings[1] = JsonUtility.ToJson(opsDataArray);
            OnDataUpdate?.Invoke();
        }
        public IOperator Clone()
        {
            var result = new UpscaleOperator();
            var operatorData = new OperatorData();
            operatorData.FromJson(GetOperatorData().ToJson());
            result.SetOperatorData(operatorData);
            return result;
        }
        public void RegisterToEvents(Model model)
        { }

        public void UnregisterFromEvents(Model model)
        { }

        /// <summary>
        /// Get the settings view for this operator.
        /// </summary>
        /// <param name="model">Current Model</param>
        /// <param name="isCustomSection">This VisualElement will override the whole operator section used by the generation settings</param>
        /// <param name="dismissAction">Action to trigger on dismiss</param>
        /// <returns> UI for the operator. Set to Null if the operator should not be displayed in the settings view. Disable the returned VisualElement if you want it to be displayed but not usable.</returns>
        public VisualElement GetSettingsView(Model model, ref bool isCustomSection, Action dismissAction)
        {
            if(m_OperatorData.settings.Length < 2)
                return null;
            
            isCustomSection = true;
            var container = new VisualElement();
            var upscaleView = new VisualElement
            {
                userData = GenerationSettingsView.HideUse // Don't allow "use" on upscale operator
            };

            container.Add(GenerationSettings.CreateView(this.Label, upscaleView, null));

            var operatorsData = JsonUtility.FromJson<OperatorDataArray>(m_OperatorData.settings[1]);
            
            var operators = ModesFactory.GetOperators(operatorsData.operators);
            foreach (var op in operators)
            {
                var isOpCustomView = false;

                var view = op.GetSettingsView(model, ref isOpCustomView, dismissAction);
                if(view == null)
                    continue;
                
                container.Add(isOpCustomView ? view : GenerationSettings.CreateView(op.Label, view, () =>
                {
                    model.UpdateOperators(op.Clone());
                    dismissAction?.Invoke();
                }));
            }

            return container;
        }

        //Json Utility doesn't provide top level list/array serialization
        [Serializable]
        class OperatorDataArray
        {
            public OperatorData[] operators;
        }
    }
}
