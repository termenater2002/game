using UnityEngine;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class BakingView : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-baking-container";

        const string k_CircularProgressElementName = "baking-circular-progress";
        const string k_ProgressLabelElementName = "baking-progress-label";
        const string k_CircularProgressDoneClassName = "deeppose-baking-circular-progress-done";

        BakingViewModel m_Model;
        Label m_Label;
        CircularProgress m_CircularProgress;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<BakingView, UxmlTraits> { }
#endif

        public BakingView():base(k_UssClassName) {}

        public void FindComponents()
        {
            m_CircularProgress = this.Q<CircularProgress>(k_CircularProgressElementName);
            
            m_CircularProgress.pickingMode = PickingMode.Ignore;
            m_CircularProgress.variant = Progress.Variant.Determinate;
            m_CircularProgress.bufferValue = 1f;
            
            m_Label = this.Q<Label>(k_ProgressLabelElementName);
        }

        public void SetModel(BakingViewModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnBakingProgressChanged += OnChanged;
            Update();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnBakingProgressChanged -= OnChanged;
            m_Model = null;
        }

        void OnChanged()
        {
            Update();
        }

        public void Update()
        {
            m_Label.text = $"{Mathf.FloorToInt(m_Model.Progress*100f)}%";

            m_CircularProgress.value = m_Model.Progress;
            m_CircularProgress.variant = m_Model.Progress > 0f ? Progress.Variant.Determinate : Progress.Variant.Indeterminate;

            if (m_Model.IsDone)
            {
                m_CircularProgress.AddToClassList(k_CircularProgressDoneClassName);
            }
            else
            {
                m_CircularProgress.RemoveFromClassList(k_CircularProgressDoneClassName);
            }

            m_CircularProgress.MarkDirtyRepaint();
        }
    }
}
