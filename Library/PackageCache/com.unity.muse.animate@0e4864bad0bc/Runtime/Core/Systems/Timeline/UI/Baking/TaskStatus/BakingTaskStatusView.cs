using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

using AppUI = Unity.Muse.AppUI.UI;
using AppCore = Unity.AppUI.Core;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class BakingTaskStatusView : UITemplateContainer, IUITemplate
    {
        static readonly CustomStyleProperty<Color> k_UssColor = new CustomStyleProperty<Color>("--progress-color");
        
        const string k_UssClassName = "deeppose-task-status";
        const string k_ProgressElementName = "task-status-circular-progress";
        const string k_ProgressWaitingUssClassName = "appui-circular-progress.waiting";
        const string k_ProgressDoneUssClassName = "done";
        BakingTaskStatusViewModel m_Model;
        CircularProgress m_CircularProgress;
        Color m_OriginalProgressColor;
        
#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<BakingTaskStatusView, UxmlTraits> { }
#endif

        public BakingTaskStatusView()
            : base(k_UssClassName) { }

        public void FindComponents()
        {
            m_CircularProgress = this.Q<CircularProgress>(k_ProgressElementName);
            m_CircularProgress.variant = Progress.Variant.Determinate;
            m_CircularProgress.bufferValue = 1f;
        }
        
        public void SetModel(BakingTaskStatusViewModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
            Update();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged += OnModelChanged;
            m_Model.OnError += OnModelError;
            Update();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnModelChanged;
            m_Model.OnError -= OnModelError;
            m_Model = null;
        }
        
        public void Update()
        {
            if (m_Model == null)
                return;
            
            UpdateVisibility();
            
            if(m_CircularProgress.customStyle.TryGetValue(k_UssColor, out var color))
            {
                m_OriginalProgressColor = color;
            }
            else
            {
                m_OriginalProgressColor = Color.red;
            }
            
            m_CircularProgress.value = m_Model.Progress;
            m_CircularProgress.variant = m_Model.Progress > 0f ? Progress.Variant.Determinate : Progress.Variant.Indeterminate;
            m_CircularProgress.colorOverride = m_Model.IsWaitingDelay? Color.grey:m_OriginalProgressColor;
        }

        void UpdateVisibility()
        {
            if (m_Model == null)
                return;
            
            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnModelChanged()
        {
            Update();
        }

        void OnModelError(string error)
        {
            var toast = Toast.Build(this, error, AppCore.NotificationDuration.Short);
            toast.SetStyle(NotificationStyle.Negative);
            toast.SetAnimationMode(AnimationMode.Slide);
            toast.Show();
        }
    }
}
