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
    internal class SeedField : VisualElement, IValidatableElement<int>
    {
        /// <summary>
        /// The SeedField main styling class.
        /// </summary>
        public static readonly string ussClassName = "muse-seedfield";

        /// <summary>
        /// The SeedField X NumericalField styling class.
        /// </summary>
        public static readonly string seedFieldUssClassName = ussClassName + "__seed-field";

        Size m_Size;

        int m_Value;

        readonly IntField m_SeedField;
        readonly Toggle m_Checkbox;
        bool m_UserSpecified;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SeedField()
        {
            AddToClassList(ussClassName);

            var mainContainer = new VisualElement { };
            var container = new InputLabel(TextContent.customSeed)
            {
                inputAlignment = Align.FlexEnd
            };
            container.AddToClassList("muse-input-label--toggle-flexend");
            mainContainer.Add(container);
            m_Checkbox = new Toggle {label = null};
            m_Checkbox.RegisterValueChangedCallback(OnChangeMode);
            container.Add(m_Checkbox);
            container.AddToClassList("bottom-gap");

            m_SeedField = new IntField { name = seedFieldUssClassName };
            m_SeedField.style.flexGrow = 1;
            m_SeedField.lowValue = ushort.MinValue;
            m_SeedField.highValue = ushort.MaxValue;
            mainContainer.Add(m_SeedField);

            hierarchy.Add(mainContainer);

            SetValueWithoutNotify(0);

            m_SeedField.RegisterCallback<ChangingEvent<int>>(OnIntFieldChanged);

            UpdateSeedVisual();
        }

        void OnChangeMode(ChangeEvent<bool> evt)
        {
            userSpecified = evt.newValue;
            UpdateSeedVisual();
        }

        void UpdateSeedVisual()
        {
            m_Checkbox.SetValueWithoutNotify(userSpecified);
            m_SeedField.style.display = userSpecified ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// The content container of the SeedField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Set the value of the SeedField without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the Vector2IntField. </param>
        public void SetValueWithoutNotify(int newValue)
        {
            m_Value = newValue;
            m_SeedField.SetValueWithoutNotify(m_Value);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the SeedField.
        /// </summary>
        public int value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<int>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the SeedField.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                EnableInClassList(Styles.invalidUssClassName, value);

                m_SeedField.EnableInClassList(Styles.invalidUssClassName, value);
            }
        }

        /// <summary>
        /// The validation function to use to validate the value.
        /// </summary>
        public Func<int, bool> validateValue { get; set; }

        void OnIntFieldChanged(ChangingEvent<int> evt)
        {
            value = evt.newValue;
        }

        public bool userSpecified
        {
            get => m_UserSpecified;
            set
            {
                if (m_UserSpecified == value)
                    return;

                m_UserSpecified = value;
                UpdateSeedVisual();

                using var evt = ChangeEvent<int>.GetPooled(m_Value, m_Value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}