using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.Muse.Sprite.UIComponents
{
    /// <summary>
    /// SizeInt Field UI element.
    /// </summary>
    internal class SizeIntField : VisualElement, IValidatableElement<Vector2Int>, ISizeableElement
    {
        /// <summary>
        /// The SizeIntField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-vector2field";

        /// <summary>
        /// The SizeIntField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The SizeIntField container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The SizeIntField W NumericalField styling class.
        /// </summary>
        public static readonly string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The SizeIntField H NumericalField styling class.
        /// </summary>
        public static readonly string yFieldUssClassName = ussClassName + "__y-field";

        Size m_Size;
        Vector2Int m_Value;
        readonly IntField m_WidthField;
        readonly IntField m_HeightField;

        Vector2Int m_Min = new Vector2Int(64, 64);
        Vector2Int m_Max = new Vector2Int(512, 512);
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SizeIntField()
        {
            AddToClassList(ussClassName);

            var container = new VisualElement { name = containerUssClassName };
            container.AddToClassList(containerUssClassName);

            m_WidthField = new IntField { name = xFieldUssClassName, unit = "W" };
            m_WidthField.AddToClassList(xFieldUssClassName);

            m_HeightField = new IntField { name = yFieldUssClassName, unit = "H" };
            m_HeightField.AddToClassList(yFieldUssClassName);

            container.Add(m_WidthField);
            container.Add(m_HeightField);

            hierarchy.Add(container);

            size = Size.M;
            SetValueWithoutNotify(Vector2Int.zero);

            m_WidthField.RegisterCallback<ChangingEvent<int>>(OnXFieldChanged);
            m_HeightField.RegisterCallback<ChangingEvent<int>>(OnYFieldChanged);
            m_WidthField.RegisterCallback<FocusOutEvent>(OnXFieldUnfocus);
            m_HeightField.RegisterCallback<FocusOutEvent>(OnYFieldUnfocus);
        }

        public Vector2Int minValue
        {
            set =>m_Min = value;
        }
        public Vector2Int maxValue
        {
            set =>m_Max = value;
        }
        void OnYFieldUnfocus(FocusOutEvent evt)
        {
            var v = value;
            v.y = Math.Min(v.y, m_Max.y);
            v.y = Math.Max(v.y, m_Min.y);
            value = v;
        }

        void OnXFieldUnfocus(FocusOutEvent evt)
        {
            var v = value;
            v.x = Math.Min(v.x, m_Max.x);
            v.x = Math.Max(v.x, m_Min.x);
            value = v;
        }

        /// <summary>
        /// The content container of the SizeIntField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the SizeIntField.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_WidthField.size = m_Size;
                m_HeightField.size = m_Size;
            }
        }

        /// <summary>
        /// Set the value of the SizeIntField without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the SizeIntField. </param>
        public void SetValueWithoutNotify(Vector2Int newValue)
        {
            m_Value = newValue;
            m_WidthField.SetValueWithoutNotify(m_Value.x);
            m_HeightField.SetValueWithoutNotify(m_Value.y);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the SizeIntField.
        /// </summary>
        public Vector2Int value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<Vector2Int>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the SizeIntField.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                EnableInClassList(Styles.invalidUssClassName, value);

                m_WidthField.EnableInClassList(Styles.invalidUssClassName, value);
                m_HeightField.EnableInClassList(Styles.invalidUssClassName, value);
            }
        }

        /// <summary>
        /// The validation function to use to validate the value.
        /// </summary>
        public Func<Vector2Int, bool> validateValue { get; set; }

        void OnYFieldChanged(ChangingEvent<int> evt)
        {
            value = new Vector2Int(value.x, evt.newValue);
        }

        void OnXFieldChanged(ChangingEvent<int> evt)
        {
            value = new Vector2Int(evt.newValue, value.y);
        }
    }
}
