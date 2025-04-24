using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Account;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class MotionToTimelineSamplingUI : UITemplateContainer, IUITemplate
    {
        const string k_StyleName = "deeppose-mtt-sampling";
        const string k_ConvertButtonName = "mtt-button-convert";
        const string k_DoneButtonName = "mtt-button-done";
        const string k_SensitivitySliderName = "mtt-sensitivity-slider";
        
        Button m_ConvertButton;
        Button m_DoneButton;
        TouchSliderInt m_SensitivitySlider;

        MotionToTimelineUIModel m_Model;
        
        public MotionToTimelineSamplingUI()
            : base(k_StyleName) { }
        
#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<MotionToTimelineSamplingUI, UxmlTraits> { }
#endif

        public void FindComponents()
        {
            m_ConvertButton = this.Q<Button>(k_ConvertButtonName);
            m_DoneButton = this.Q<Button>(k_DoneButtonName);
            m_SensitivitySlider = this.Q<TouchSliderInt>(k_SensitivitySliderName);
            this.AddManipulator(new SessionStatusTracker());
        }

        public void RegisterComponents()
        {
            m_DoneButton.clicked += OnDoneClicked;
            m_ConvertButton.clicked += OnConvertClicked;
            m_SensitivitySlider.RegisterCallback<ChangeEvent<int>>(OnSensitivityChanged);
        }
        
        public void UnregisterComponents()
        {
            m_DoneButton.clicked -= OnDoneClicked;
            m_ConvertButton.clicked -= OnConvertClicked;
            m_SensitivitySlider.UnregisterCallback<ChangeEvent<int>>(OnSensitivityChanged);
        }

        public void SetModel(MotionToTimelineUIModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
            OnModelChanged(default);
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;
            
            m_Model.OnChanged += OnModelChanged;
        }
        
        void UnregisterModel()
        {
            if (m_Model == null)
                return;
            
            m_Model.OnChanged -= OnModelChanged;
        }

        void OnModelChanged(MotionToTimelineUIModel.Property property)
        {
            if (m_Model == null)
                return;
            
            m_DoneButton.SetEnabled(!m_Model.IsBusy && m_Model.CanConfirm);
            m_ConvertButton.SetEnabled(!m_Model.IsBusy && m_Model.CanConvert);
            m_SensitivitySlider.SetEnabled(!m_Model.IsBusy && (m_Model.CanConfirm||m_Model.CanConvert));
            m_SensitivitySlider.SetValueWithoutNotify(Mathf.RoundToInt(m_Model.KeyFrameSamplingSensitivity));
        }
        
        void OnSensitivityChanged(ChangeEvent<int> evt)
        {
            if (m_Model != null)
                m_Model.KeyFrameSamplingSensitivity = evt.newValue;
        }

        void OnDoneClicked()
        {
            m_Model?.RequestConfirm();
        }

        void OnConvertClicked()
        {
            m_Model.RequestPreview();
        }
    }
}
