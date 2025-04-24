using UnityEngine.UIElements;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using Toggle = Unity.Muse.AppUI.UI.Toggle;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class SequenceKeyInspectorView : SequenceItemInspectorView<TimelineModel.SequenceKey>, IUITemplate
    {
        const string k_SpeedSliderName = "transition-speed";
        const string k_ExtrapolateToggleName = "extrapolate-pose-toggle";
        const string k_LoopToggleName = "loop-pose-toggle";

        const string k_ExtrapolateToggleLabelName = "extrapolate-pose-toggle-label";
        const string k_LoopToggleLabelName = "loop-pose-toggle-label";

        new SequenceKeyInspectorViewModel Model => base.Model as SequenceKeyInspectorViewModel;

        SliderFloat m_SpeedSlider;
        Toggle m_ExtrapolateToggle;
        Toggle m_LoopToggle;
        InputLabel m_LoopToggleLabel;
        InputLabel m_ExtrapolateToggleLabel;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<SequenceKeyInspectorView, UxmlTraits> { }
#endif

        public void FindComponents()
        {
            m_SpeedSlider = this.Q<SliderFloat>(k_SpeedSliderName);
            m_ExtrapolateToggle = this.Q<Toggle>(k_ExtrapolateToggleName);
            m_LoopToggle = this.Q<Toggle>(k_LoopToggleName);
            m_LoopToggleLabel = this.Q<InputLabel>(k_LoopToggleLabelName);
            m_ExtrapolateToggleLabel = this.Q<InputLabel>(k_ExtrapolateToggleLabelName);

            var label = m_SpeedSlider.Q<LocalizedTextElement>("appui-slider__inline-valuelabel");
            label.AddToClassList("deeppose-slider-no-inline-value");
        }

        public void RegisterComponents()
        {
            m_SpeedSlider.RegisterValueChangedCallback(OnTransitionSpeedChanged);
            m_ExtrapolateToggle.RegisterValueChangedCallback(OnExtrapolateToggled);
            m_LoopToggle.RegisterValueChangedCallback(OnLoopToggled);
        }

        public void UnregisterComponents()
        {
            m_SpeedSlider.UnregisterValueChangedCallback(OnTransitionSpeedChanged);
            m_ExtrapolateToggle.UnregisterValueChangedCallback(OnExtrapolateToggled);
            m_LoopToggle.UnregisterValueChangedCallback(OnLoopToggled);
        }
        
        void OnTransitionSpeedChanged(ChangeEvent<float> evt)
        {
            Model?.SetTransitionDuration(evt.newValue);
        }

        void OnExtrapolateToggled(ChangeEvent<bool> evt)
        {
            Model?.SetExtrapolating(evt.newValue);
        }

        void OnLoopToggled(ChangeEvent<bool> evt)
        {
            Model?.SetLooping(evt.newValue);
        }

        public void SetModel(SequenceKeyInspectorViewModel model)
        {
            base.SetModel(model);
        }

        protected override void UpdateUI()
        {
            if (Model is not { HasTarget: true, IsVisible: true })
                return;

            m_SpeedSlider.SetValueWithoutNotify(Model.TransitionDuration);
            m_ExtrapolateToggle.SetValueWithoutNotify(Model.IsExtrapolating);
            m_LoopToggle.SetValueWithoutNotify(Model.IsLooping);
            m_SpeedSlider.SetEnabled(Model.HasTransition);
            m_ExtrapolateToggle.SetEnabled(Model.CanExtrapolate);
            m_ExtrapolateToggleLabel.SetEnabled(Model.CanExtrapolate);
            m_LoopToggle.SetEnabled(Model.CanLoop);
            m_LoopToggleLabel.SetEnabled(Model.CanLoop);
        }
    }
}
