using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Text = Unity.Muse.AppUI.UI.Text;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class LoraOperator : IOperator
    {
        public string OperatorName  => "LoraOperator";
        /// <summary>
        /// Human-readable label for the operator.
        /// </summary>
        public string Label => "Style Selector";
        event Action OnDataUpdate;

        [SerializeField]
        OperatorData m_OperatorData;

        public LoraOperator()
        {
            m_OperatorData = new OperatorData(OperatorName, "0.0.1",  new [] { "guid1" }, false);
        }

        public bool IsSavable()
        {
            return true;
        }

        public VisualElement GetCanvasView()
        {
            Debug.Log("LoraOperator.GetCanvasView()");
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

            var dropdown= new Dropdown();
            dropdown.name = "generation-type-dropdown";
            dropdown.AddToClassList("bottom-gap");

            var data = new[] { "guid1", "guid2" };

            //Need to get Labels...
            dropdown.bindItem = (item, i) => item.label = data[i];
            dropdown.sourceItems = data;
            dropdown.SetValueWithoutNotify( new []{ Array.IndexOf(data, m_OperatorData.settings[0]) });
            dropdown.RegisterValueChangedCallback((evt) =>
            {
                using var selection = evt.newValue.GetEnumerator();
                if (selection.MoveNext())
                    m_OperatorData.settings[0] = data[selection.Current];
            });

            OnDataUpdate += () =>
            {
                dropdown.SetValueWithoutNotify(new []{ Array.IndexOf(data, m_OperatorData.settings[0]) });
            };

            UI.Add(dropdown);
            return UI;
        }

        /// <summary>
        /// Get the settings view for this operator.
        /// </summary>
        /// <param name="model">Current Model</param>
        /// <param name="isCustomSection">This VisualElement will override the whole operator section used by the generation settings</param>
        /// <param name="dismissAction">Action to trigger on dismiss</param>
        /// <returns> UI for the operator. Set to Null if the operator should not be displayed in the settings view. Disable the returned VisualElement if you want it to be displayed but not usable.</returns>
        public VisualElement GetSettingsView(Model model, ref bool isCustomSection, Action dismissAction)
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

        public void SetLoraGuid(string guid)
        {
            m_OperatorData.settings[0] = guid;
        }
        public IOperator Clone()
        {
            var result = new LoraOperator();
            var operatorData = new OperatorData();
            operatorData.FromJson(GetOperatorData().ToJson());
            result.SetOperatorData(operatorData);
            return result;
        }
        public void RegisterToEvents(Model model)
        { }

        public void UnregisterFromEvents(Model model)
        { }

    }
}
