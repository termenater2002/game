using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.Muse.AppUI.UI
{
    /// <summary>
    /// PageIndicator UI element.
    /// This element is used to display a list of dots that can be used to navigate between pages.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class PageIndicator : BaseVisualElement, INotifyValueChanged<int>
    {
#if ENABLE_RUNTIME_DATA_BINDINGS

        internal static readonly BindingId directionProperty = nameof(direction);

        internal static readonly BindingId countProperty = nameof(count);

        internal static readonly BindingId valueProperty = nameof(value);

#endif

        /// <summary>
        /// The PageIndicator main styling class.
        /// </summary>
        public const string ussClassName = "appui-page-indicator";

        /// <summary>
        /// The PageIndicator direction styling class.
        /// </summary>
        [EnumName("GetDirectionUssClassName", typeof(Direction))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The PageIndicator dot styling class.
        /// </summary>
        public const string dotUssClassName = ussClassName + "__dot";

        /// <summary>
        /// The PageIndicator dot background styling class.
        /// </summary>
        public const string dotBackgroundUssClassName = ussClassName + "__dot-background";

        /// <summary>
        /// The PageIndicator dot content styling class.
        /// </summary>
        public const string dotContentUssClassName = ussClassName + "__dot-content";

        Direction m_Direction;

        int m_Value;

        readonly List<Pressable> m_Clickables = new List<Pressable>();

        readonly List<KeyboardFocusController> m_KeyboardFocusControllers = new List<KeyboardFocusController>();

        /// <summary>
        /// The number of dots.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int count
        {
            get => hierarchy.childCount;
            set
            {
                if (value == hierarchy.childCount)
                    return;

                BuildDots(value);

#if ENABLE_RUNTIME_DATA_BINDINGS
                NotifyPropertyChanged(in countProperty);
#endif
            }
        }

        /// <summary>
        /// The currently selected dot index.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int value
        {
            get => m_Value;

            set
            {
                if (value < 0 || value > hierarchy.childCount - 1)
                    return;

                var previousValue = m_Value;
                SetValueWithoutNotify(value);
                if (previousValue != m_Value)
                {
                    using var evt = ChangeEvent<int>.GetPooled(previousValue, m_Value);
                    evt.target = this;
                    SendEvent(evt);
                }

#if ENABLE_RUNTIME_DATA_BINDINGS
                NotifyPropertyChanged(in valueProperty);
#endif
            }
        }

        /// <summary>
        /// The PageIndicator direction (horizontal or vertical).
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public Direction direction
        {
            get => m_Direction;
            set
            {
                var changed = m_Direction != value;
                RemoveFromClassList(GetDirectionUssClassName(m_Direction));
                m_Direction = value;
                AddToClassList(GetDirectionUssClassName(m_Direction));

#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in directionProperty);
#endif
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PageIndicator()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
            focusable = false;
            direction = Direction.Horizontal;

            RegisterCallback<KeyDownEvent>(OnDotKeyDown);
        }

        void OnDotKeyDown(KeyDownEvent evt)
        {
            var handled = false;

            if (evt.target is VisualElement dot && dot.hierarchy.parent == this)
            {
                if (direction == Direction.Horizontal)
                {
                    if (evt.keyCode == KeyCode.LeftArrow)
                        handled = GoToPrevious();
                    else if (evt.keyCode == KeyCode.RightArrow)
                        handled = GoToNext();
                }
                else
                {
                    if (evt.keyCode == KeyCode.UpArrow)
                        handled = GoToPrevious();
                    else if (evt.keyCode == KeyCode.DownArrow)
                        handled = GoToNext();
                }
            }

            if (handled)
            {

                evt.StopPropagation();
            }
        }

        void OnDotClicked(EventBase evt)
        {
            if (evt.target is VisualElement dot && dot.hierarchy.parent == this)
            {
                value = hierarchy.IndexOf(dot);

                evt.StopPropagation();
            }
        }

        /// <summary>
        /// The content container of the PageIndicator. This is always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Set the value of the PageIndicator without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(int newValue)
        {
            if (m_Value >= 0 && m_Value < hierarchy.childCount)
                hierarchy.ElementAt(m_Value).RemoveFromClassList(Styles.selectedUssClassName);
            m_Value = newValue;
            if (m_Value >= 0 && m_Value < hierarchy.childCount)
                hierarchy.ElementAt(m_Value).AddToClassList(Styles.selectedUssClassName);
        }

        /// <summary>
        /// Go to the next dot.
        /// </summary>
        /// <returns> True if the dot has been changed. </returns>
        public bool GoToNext()
        {
            var nextIndex = Mathf.Min(Mathf.Max(0, m_Value + 1), hierarchy.childCount - 1);
            if (nextIndex == m_Value)
                return false;
            value = nextIndex;
            return true;
        }

        /// <summary>
        /// Go to the previous dot.
        /// </summary>
        /// <returns> True if the dot has been changed. </returns>
        public bool GoToPrevious()
        {
            var nextIndex = Mathf.Min(Mathf.Max(0, m_Value - 1), hierarchy.childCount - 1);
            if (nextIndex == m_Value)
                return false;
            value = nextIndex;
            return true;
        }

        void BuildDots(int dotCount)
        {
            for (var i = hierarchy.childCount - 1; i >= 0; i--)
            {
                var dot = hierarchy.ElementAt(i);
                dot.RemoveManipulator(m_Clickables[i]);
                dot.RemoveManipulator(m_KeyboardFocusControllers[i]);
                hierarchy.Remove(dot);
                m_Clickables.RemoveAt(i);
            }
            hierarchy.Clear();
            m_Clickables.Clear();
            for (var i = 0; i < dotCount; i++)
            {
                var dot = new ExVisualElement { name = $"dot-{i}", focusable = true, tabIndex = 0, pickingMode = PickingMode.Position, passMask = 0 };
                dot.AddToClassList(dotUssClassName);
                var clickable = new Pressable(OnDotClicked);
                var keyboardFocus = new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn);
                m_Clickables.Add(clickable);
                m_KeyboardFocusControllers.Add(keyboardFocus);
                dot.AddManipulator(clickable);
                dot.AddManipulator(keyboardFocus);
                var dotBg = new VisualElement { name = dotBackgroundUssClassName, pickingMode = PickingMode.Ignore };
                dotBg.AddToClassList(dotBackgroundUssClassName);
                dot.Add(dotBg);
                var dotContent = new VisualElement { name = dotContentUssClassName, pickingMode = PickingMode.Ignore };
                dotContent.AddToClassList(dotContentUssClassName);
                dot.Add(dotContent);
                hierarchy.Add(dot);
            }
            SetValueWithoutNotify(value);
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            ((ExVisualElement)evt.target).passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            ((ExVisualElement)evt.target).passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Outline;
        }

#if ENABLE_UXML_TRAITS

        /// <summary>
        /// Factory class to instantiate a <see cref="PageIndicator"/> using the data read from a UXML file.
        /// </summary>
        internal new class UxmlFactory : UxmlFactory<PageIndicator, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="PageIndicator"/>.
        /// </summary>
        internal new class UxmlTraits : BaseVisualElement.UxmlTraits
        {
            readonly UxmlEnumAttributeDescription<Direction> m_Direction = new UxmlEnumAttributeDescription<Direction>()
            {
                name = "direction",
                defaultValue = Direction.Horizontal,
            };

            readonly UxmlIntAttributeDescription m_Count = new UxmlIntAttributeDescription
            {
                name = "count",
                defaultValue = 1,
            };

            readonly UxmlIntAttributeDescription m_DefaultValue = new UxmlIntAttributeDescription
            {
                name = "default-value",
                defaultValue = -1,
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                m_PickingMode.defaultValue = PickingMode.Ignore;
                base.Init(ve, bag, cc);

                var el = (PageIndicator)ve;
                el.direction = m_Direction.GetValueFromBag(bag, cc);
                el.count = m_Count.GetValueFromBag(bag, cc);
                var defaultValue = 0;
                if (m_DefaultValue.TryGetValueFromBag(bag, cc, ref defaultValue))
                    el.SetValueWithoutNotify(defaultValue);
            }
        }

#endif
    }
}
