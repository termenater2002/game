
using Unity.Muse.Animate.UserActions;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class SequenceKeyInspectorViewModel : SequenceItemInspectorViewModel<TimelineModel.SequenceKey>
    {
        public override TimelineModel.SequenceKey Target
        {
            get => m_Target;
            set
            {
                UnregisterFromTarget();
                base.Target = value;
                RegisterToTarget();
            }
        }

        TransitionModel InTransition => m_Target?.InTransition?.Transition;
        TransitionModel OutTransition => m_Target?.OutTransition?.Transition;

        public bool HasTransition => OutTransition != null;
        public float TransitionDuration => OutTransition?.Duration ?? 0f;
        public bool CanLoop => InTransition != null && OutTransition == null;

        public bool CanExtrapolate => InTransition != null;

        public bool IsLooping => HasTarget && Target.Key.Type == KeyData.KeyType.Loop;
        public bool IsExtrapolating => HasTarget && Target.Key.Type == KeyData.KeyType.Empty;

        public SequenceKeyInspectorViewModel(InspectorsPanelViewModel inspectorsPanel)
            : base(inspectorsPanel) {}

        void RegisterToTarget()
        {
            if (m_Target != null)
            {
                m_Target.Key.OnChanged += OnTargetModelChanged;
                
                if (m_Target.OutTransition != null)
                {
                    m_Target.OutTransition.Transition.OnChanged += OnTargetModelOutTransitionChanged;
                }
            }
        }
        
        void UnregisterFromTarget()
        {
            if (m_Target != null)
            {
                m_Target.Key.OnChanged -= OnTargetModelChanged;
                
                if (m_Target.OutTransition != null)
                {
                    m_Target.OutTransition.Transition.OnChanged -= OnTargetModelOutTransitionChanged;
                }
            }
        }
        
        void OnTargetModelChanged(KeyModel keyModel, KeyModel.Property property)
        {
            NotifyTargetChange();
        }
        
        void OnTargetModelOutTransitionChanged(TransitionModel outTransitionModel, TransitionModel.Property property)
        {
            NotifyTargetChange();
        }
        
        public void SetTransitionDuration(float speed)
        {
            UserActionsManager.Instance.StartUserEdit("Speed");
            OutTransition.Duration = Mathf.RoundToInt(speed);
        }

        public void SetLooping(bool isLooping)
        {
            UserActionsManager.Instance.StartUserEdit("Looping");
            if (isLooping)
            {
                Target.Key.Type = KeyData.KeyType.Loop;
                Target.Key.Loop.StartFrame = 0;
                Target.Key.Loop.NumBakingLoopbacks = 1;
            }
            else
            {
                Target.Key.Type = KeyData.KeyType.FullPose;
            }
        }

        public void SetExtrapolating(bool isExtrapolating)
        {
            UserActionsManager.Instance.StartUserEdit("Extrapolating");
            Target.Key.Type = isExtrapolating ? KeyData.KeyType.Empty : KeyData.KeyType.FullPose;
        }
    }
}
