using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

using AppUI = Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// NOTE: Had to take shortcuts for June 26th Release
    /// Started from AppUI code of SliderFloat
    /// Might include some unnecessary code
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class PlaybackSlider : AppUI.UI.BaseSlider<int, int>
    {
#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Factory class to instantiate a <see cref="PlaybackSlider"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<PlaybackSlider, UxmlTraits> { }
#endif

        static readonly int k_SidePadding = 3;
        
        /// <summary>
        /// The Slider main styling class.
        /// </summary>
        public static readonly string ussClassName = "playback-slider";

        /// <summary>
        /// The Slider tick styling class.
        /// </summary>
        public static readonly string tickUssClassName = ussClassName + "__tick";

        /// <summary>
        /// The Slider ticks container styling class.
        /// </summary>
        public static readonly string ticksUssClassName = ussClassName + "__ticks";

        /// <summary>
        /// The Slider track styling class.
        /// </summary>
        public static readonly string trackUssClassName = ussClassName + "__track";

        /// <summary>
        /// The Slider progress styling class.
        /// </summary>
        public static readonly string progressUssClassName = ussClassName + "__progress";

        public static readonly string progressStartUssClassName = progressUssClassName + "-start";

        public static readonly string progressEndUssClassName = progressUssClassName + "-end";

        public static readonly string progressPrimaryUssClassName = progressUssClassName + "-primary";

        /// <summary>
        /// The Slider handle styling class.
        /// </summary>
        public static readonly string handleUssClassName = ussClassName + "__handle";

        /// <summary>
        /// The Slider handle container styling class.
        /// </summary>
        public static readonly string handleContainerUssClassName = ussClassName + "__handle-container";

        /// <summary>
        /// The Slider controls styling class.
        /// </summary>
        public static readonly string controlsUssClassName = ussClassName + "__controls";

        /// <summary>
        /// The Slider control container styling class.
        /// </summary>
        public static readonly string controlContainerUssClassName = ussClassName + "__control-container";

        int m_FillStartValue;

        int m_FillEndValue;

        bool m_Primary;

        readonly AppUI.UI.ExVisualElement m_Handle;

        readonly VisualElement m_Progress;

        readonly VisualElement m_Ticks;

        readonly VisualElement m_HandleContainer;

        readonly VisualElement m_Controls;

        readonly List<VisualElement> m_TickElements = new();
        readonly List<int> m_TickValues = new();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PlaybackSlider()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            var controlContainer = new VisualElement { name = controlContainerUssClassName, pickingMode = PickingMode.Ignore };
            controlContainer.AddToClassList(controlContainerUssClassName);
            hierarchy.Add(controlContainer);

            m_Controls = new VisualElement { name = controlsUssClassName, pickingMode = PickingMode.Ignore };
            m_Controls.AddToClassList(controlsUssClassName);
            controlContainer.hierarchy.Add(m_Controls);

            var track = new VisualElement { name = trackUssClassName, pickingMode = PickingMode.Ignore };
            track.AddToClassList(trackUssClassName);
            m_Controls.hierarchy.Add(track);

            m_Progress = new VisualElement
            {
                name = progressUssClassName,
                usageHints = UsageHints.DynamicTransform,
                pickingMode = PickingMode.Ignore
            };
            
            m_Progress.AddToClassList(progressUssClassName);
            m_Controls.hierarchy.Add(m_Progress);

            m_Ticks = new VisualElement { name = ticksUssClassName, pickingMode = PickingMode.Ignore };
            m_Ticks.AddToClassList(ticksUssClassName);
            m_Controls.hierarchy.Add(m_Ticks);

            m_HandleContainer = new VisualElement
            {
                name = handleContainerUssClassName,
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicTransform,
            };
            
            m_HandleContainer.AddToClassList(handleContainerUssClassName);
            m_Controls.hierarchy.Add(m_HandleContainer);

            m_Handle = new AppUI.UI.ExVisualElement
            {
                name = handleUssClassName,
                pickingMode = PickingMode.Ignore,
                passMask = 0
            };
            
            m_Handle.AddToClassList(handleUssClassName);
            m_HandleContainer.hierarchy.Add(m_Handle);

            primary = true;
            filled = false;
            fillStart = 0;
            fillEnd = 0;

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            m_DraggerManipulator = new AppUI.UI.Draggable(OnTrackClicked, OnTrackDragged, OnTrackUp, OnTrackDown);
            this.AddManipulator(m_DraggerManipulator);
            this.AddManipulator(new AppUI.UI.KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));

            formatString = "#######0";
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_Handle.passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_Handle.passMask = Passes.Clear | Passes.Outline;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.target == this && focusController.focusedElement == this)
            {
                var handled = false;
                var previousValue = value;
                var newValue = previousValue;

                if (evt.keyCode == KeyCode.LeftArrow)
                {
                    newValue = Decrement(newValue);
                    handled = true;
                }
                else if (evt.keyCode == KeyCode.RightArrow)
                {
                    newValue = Increment(newValue);
                    handled = true;
                }

                if (handled)
                {
#if UNITY_2023_2_OR_NEWER
                    evt.StopPropagation();
                    focusController.IgnoreEvent(evt);
#else
                    evt.PreventDefault();
#endif

                    SetValueWithoutNotify(newValue);

                    using var changingEvt = AppUI.UI.ChangingEvent<int>.GetPooled();
                    changingEvt.previousValue = previousValue;
                    changingEvt.newValue = newValue;
                    changingEvt.target = this;
                    SendEvent(changingEvt);
                }
            }
        }

        /// <summary>
        /// If the slider progress is filled.
        /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool filled
        {
            get => m_Progress.visible;
            set
            {
                if (value == m_Progress.visible)
                    return;

                m_Progress.visible = value;
                RefreshUI();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int fillStart
        {
            get => m_FillStartValue;
            set
            {
                if (m_FillStartValue == value)
                    return;

                m_FillStartValue = value;
                RefreshUI();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int fillEnd
        {
            get => m_FillEndValue;
            set
            {
                if (m_FillEndValue == value)
                    return;

                m_FillEndValue = value;
                RefreshUI();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool primary
        {
            get => m_Primary;
            set
            {
                if (m_Primary == value)
                    return;

                m_Primary = value;
                m_Progress.EnableInClassList(progressPrimaryUssClassName, m_Primary);
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool disabled
        {
            get => !enabledSelf;
            set => SetEnabled(!value);
        }
        
        public List<int> tickValues
        {
            get => m_TickValues;
            set
            {
                // Check for change to avoid unnecessary update
                if (m_TickValues.Count == value.Count)
                {
                    var allEqual = true;
                    for (var i = 0; i < m_TickValues.Count; i++)
                    {
                        if (m_TickValues[i] != value[i])
                        {
                            allEqual = false;
                            break;
                        }
                    }

                    if (allEqual)
                        return;
                }

                m_TickValues.Clear();
                foreach (var tickValue in value)
                {
                    m_TickValues.Add(tickValue);
                }

                m_Ticks.EnableInClassList(AppUI.UI.Styles.hiddenUssClassName, m_TickValues.Count == 0);
                m_Ticks.Clear();

                // Increase pool capacity as required
                while (m_TickElements.Count < m_TickValues.Count)
                {
                    var tickItem = new VisualElement { name = tickUssClassName, pickingMode = PickingMode.Ignore };
                    tickItem.AddToClassList(tickUssClassName);
                    m_TickElements.Add(tickItem);
                }

                for (var i = 0; i < m_TickValues.Count; i++)
                {
                    var tickItem = m_TickElements[i];
                    m_Ticks.Add(tickItem);
                }

                RefreshUI();
            }
        }

        /// <summary>
        /// Set the value of the slider without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the slider. </param>
        public override void SetValueWithoutNotify(int newValue)
        {
            newValue = GetClampedValue(newValue);
            m_Value = newValue;

            if (validateValue != null)
                invalid = !validateValue(m_Value);

            RefreshUI();
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.GetSliderRect"/>
        protected override Rect GetSliderRect() => this.WorldToLocal(m_Controls.LocalToWorld(m_Controls.contentRect));

        void RefreshUI()
        {
            if (panel == null || !IsValid(contentRect))
                return;

            var trackWidth = GetSliderRect().width;

            // progress bar
            var startVal = Mathf.Clamp01(SliderNormalizeValue(m_FillStartValue, lowValue, highValue));
            var endVal = Mathf.Max(startVal, Mathf.Clamp01(SliderNormalizeValue(m_FillEndValue, lowValue, highValue)));
            var progressW = trackWidth * (endVal - startVal);
            
            m_Progress.style.width = trackWidth * (endVal - startVal);
            m_Progress.EnableInClassList(progressStartUssClassName, startVal == 0f);
            m_Progress.EnableInClassList(progressEndUssClassName, endVal == 1f);

            if (endVal == 1f || startVal == 0f)
            {
                if (startVal == 0f)
                {
                    progressW += k_SidePadding;
                    m_Progress.style.left = -k_SidePadding;
                }
                
                if (endVal == 1f)
                {
                    progressW += k_SidePadding;
                    m_Progress.style.left = trackWidth * startVal;
                }
            }
            else
            {
                m_Progress.style.left = trackWidth * startVal;
            }

            m_Progress.style.width = progressW;
            
            // handle
            var val = Mathf.Clamp01(SliderNormalizeValue(m_Value, lowValue, highValue));
            m_HandleContainer.style.left = trackWidth * val;
            
            // ticks
            for (var i = 0; i < m_TickValues.Count; i++)
            {
                var tickValue = Mathf.Clamp01(SliderNormalizeValue(m_TickValues[i], lowValue, highValue));
                var tickItem = m_TickElements[i];
                tickItem.style.left = trackWidth * tickValue;
                tickItem.style.display = (tickValue == 0f || tickValue == 1f) ? DisplayStyle.None : DisplayStyle.Flex;
            }

            MarkDirtyRepaint();
        }

        bool IsValid(Rect rect)
        {
            return rect != default &&
                !float.IsNaN(rect.width) && !float.IsNaN(rect.height) &&
                !float.IsInfinity(rect.width) && !float.IsInfinity(rect.height) &&
                !float.IsNegative(rect.width) && !float.IsNegative(rect.height) &&
                !Mathf.Approximately(0, rect.width) && !Mathf.Approximately(0, rect.height);
        }

        /// <summary>
        /// The increment factor used when the slider is interacted with using the keyboard.
        /// </summary>
        public int incrementFactor { get; set; } = 1;

        /// <inheritdoc cref="BaseSlider{TValueType}.ParseStringToValue"/>
        protected override bool ParseStringToValue(string strValue, out int v)
        {
            var ret = int.TryParse(strValue, out var val);
            v = val;
            return ret;
        }
        
        /// <inheritdoc cref="BaseSlider{TValueType}.Clamp"/>
        protected override int Clamp(int v, int lowBound, int highBound)
        {
            return Mathf.Clamp(v, lowBound, highBound);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.ParseValueToString"/>
        protected override string ParseValueToString(int val)
        {
            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.SliderLerpUnclamped"/>
        protected override int SliderLerpUnclamped(int a, int b, float interpolant)
        {
            return Mathf.RoundToInt(Mathf.LerpUnclamped(a, b, interpolant));
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.SliderNormalizeValue"/>
        protected override float SliderNormalizeValue(int currentValue, int lowerValue, int higherValue)
        {
            return Mathf.InverseLerp(lowerValue, higherValue, currentValue);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.Increment"/>
        protected override int Increment(int val)
        {
            return val + incrementFactor;
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.Decrement"/>
        protected override int Decrement(int val)
        {
            return val - incrementFactor;
        }

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Unity.Muse.AppUI.UI.SliderBase{TValueType}"/>.
        /// </summary>
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlIntAttributeDescription m_HighValue = new UxmlIntAttributeDescription
            {
                name = "high-value",
                defaultValue = 100
            };

            readonly UxmlIntAttributeDescription m_LowValue = new UxmlIntAttributeDescription
            {
                name = "low-value",
                defaultValue = 0
            };

            readonly UxmlIntAttributeDescription m_Value = new UxmlIntAttributeDescription
            {
                name = "value",
                defaultValue = 0
            };

            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Primary = new UxmlBoolAttributeDescription
            {
                name = "primary",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Filled = new UxmlBoolAttributeDescription
            {
                name = "filled",
                defaultValue = false
            };

            readonly UxmlIntAttributeDescription m_FillStart = new UxmlIntAttributeDescription
            {
                name = "fill-start",
                defaultValue = 0
            };

            readonly UxmlIntAttributeDescription m_FillEnd = new UxmlIntAttributeDescription
            {
                name = "fill-end",
                defaultValue = 0
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var el = (PlaybackSlider)ve;
                el.filled = m_Filled.GetValueFromBag(bag, cc);
                el.fillStart = m_FillStart.GetValueFromBag(bag, cc);
                el.fillEnd = m_FillEnd.GetValueFromBag(bag, cc);
                el.primary = m_Primary.GetValueFromBag(bag, cc);

                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));

                el.lowValue = m_LowValue.GetValueFromBag(bag, cc);
                el.highValue = m_HighValue.GetValueFromBag(bag, cc);
                el.value = m_Value.GetValueFromBag(bag, cc);
            }
        }
#endif
    }
}
