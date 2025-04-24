using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Toggle = Unity.Muse.AppUI.UI.Toggle;

namespace Unity.Muse.Sprite.UIComponents
{
    /// <summary>
    /// Seed Field UI element.
    /// </summary>
    internal class MuseToggle : VisualElement
    {
        readonly Toggle m_Toggle;
        public event Action<bool> OnToggle;
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MuseToggle(string label)
        {
            var mainContainer = new VisualElement { };
            var container = new InputLabel(label)
            {
                inputAlignment = Align.FlexEnd
            };
            container.AddToClassList("muse-input-label--toggle-flexend");
            mainContainer.Add(container);
            m_Toggle = new Toggle {label = null};
            m_Toggle.RegisterValueChangedCallback(OnChangeMode);
            container.Add(m_Toggle);
            container.AddToClassList("bottom-gap");

            hierarchy.Add(mainContainer);
        }

        public void SetLabel(string label)
        {
            this.Q<InputLabel>().label = label;
        }

        void OnChangeMode(ChangeEvent<bool> evt)
        {
            OnToggle?.Invoke(evt.newValue);
        }

        /// <summary>
        /// The value of the SeedField.
        /// </summary>
        public bool value
        {
            get => m_Toggle.value;
            set => m_Toggle.SetValueWithoutNotify(value);
        }

        public void Show(bool b)
        {
            style.display = b ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}