using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Common UI for Canvas Tools
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class MuseToolbar : VisualElement
    {
        private const string k_ClassName = "muse-toolbar";
        private ActionGroup m_ButtonGroup;

        /// <summary>
        /// Pan Button
        /// </summary>
        public ActionButton PanBtn { get; private set; }
        /// <summary>
        /// Paint Button
        /// </summary>
        public ActionButton PaintBtn { get; private set; }
        /// <summary>
        /// Erase Button
        /// </summary>
        public ActionButton EraseBtn { get; private set; }
        /// <summary>
        /// Delete Button
        /// </summary>
        public ActionButton DeleteBtn { get; private set; }
        /// <summary>
        /// Size Slider
        /// </summary>
        public TouchSliderFloat SizeSlider { get; private set; }
        
        public MuseToolbar()
        {
            InitializeVisualTree();
        }

        void InitializeVisualTree()
        {
            AddToClassList(k_ClassName);
            var styleSheet = ResourceManager.Load<StyleSheet>(PackageResources.toolbarStyleSheet);
            styleSheets.Add(styleSheet);
            m_ButtonGroup = new ActionGroup()
            {
                compact = true,
                justified = false,
                selectionType = SelectionType.Single,
                allowNoSelection = false,
                style =
                {
                    flexGrow = 0f
                }
            };

            PanBtn = new ActionButton()
            {
                name = "PanBtn",
                tooltip = "Pan (1 or P)",
                icon = "pan"
            };
            m_ButtonGroup.Add(PanBtn);

            PaintBtn = new ActionButton()
            {
                name = "PaintBtn",
                tooltip = "Paint (2 or B)",
                icon = "paint-brush"
            };
            m_ButtonGroup.Add(PaintBtn);

            EraseBtn = new ActionButton()
            {
                name = "EraseBtn",
                tooltip = "Erase (3 or E)",
                icon = "eraser"
            };
            m_ButtonGroup.Add(EraseBtn);

            Add(m_ButtonGroup);

            SizeSlider = new TouchSliderFloat()
            {
                label = "Size",
                value = 5,
                lowValue = 0,
                highValue = 10,
                style =
                {
                    width = 138
                }
            };
            Add(SizeSlider);

            DeleteBtn = new ActionButton
            {
                icon = "delete",
                tooltip = "Clear"
            };

            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                DeleteBtn.tooltip += " (Command+Delete)";
            }
            else
            {
                DeleteBtn.tooltip += " (Del)";
            }

            Add(DeleteBtn);

            PanBtn.clickable.clicked += OnPanClicked;
            PaintBtn.clickable.clicked += OnPaintClicked;
            EraseBtn.clickable.clicked += OnEraseClicked;

            SelectButton(PanBtn);
        }

        /// <summary>
        /// Set the button's selected state
        /// </summary>
        /// <param name="button">The specific button</param>
        public void SelectButton(ActionButton button)
        {
            var index = button.parent?.IndexOf(button) ?? -1;
            if (index >= 0)
                m_ButtonGroup.SetSelection(new []{ index });
            DeleteBtn.SetEnabled(button == PaintBtn || button == EraseBtn);
            SizeSlider.SetEnabled(button == PaintBtn || button == EraseBtn);
        }

        internal bool IsPaintButtonSelected()
        {
            return PaintBtn.ClassListContains(Styles.selectedUssClassName);
        }

        internal bool IsEraserButtonSelected()
        {
            return EraseBtn.ClassListContains(Styles.selectedUssClassName);
        }

        void OnPaintClicked()
        {
            SelectButton(PaintBtn);
        }

        void OnEraseClicked()
        {
            SelectButton(EraseBtn);
        }

        void OnPanClicked()
        {
            SelectButton(PanBtn);
        }
        
#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<MuseToolbar, UxmlTraits> { }
#endif

        }
    }
