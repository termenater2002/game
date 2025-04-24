using UnityEngine;

namespace Unity.Muse.Animate
{
    class ToleranceHandleViewModel : HandleViewModel
    {
        public RadiusControlViewModel RadiusControlViewModel => m_RadiusControlViewModel;

        public bool IsDragging => m_RadiusControlViewModel.IsDragging;

        RadiusControlViewModel m_RadiusControlViewModel;
        ToleranceHandleModel m_ToleranceHandleModel;

        public ToleranceHandleViewModel(ToleranceHandleModel toleranceHandleModel, CameraModel cameraModel) :
            base(cameraModel)
        {
            m_ToleranceHandleModel = toleranceHandleModel;
            m_RadiusControlViewModel = new RadiusControlViewModel(m_ToleranceHandleModel.RadiusControlModel, CameraModel);
            OnVisibilityChanged += UpdateElementsVisibility;
            UpdateElementsVisibility();
        }

        void UpdateElementsVisibility()
        {
            m_RadiusControlViewModel.IsVisible = IsVisible;
        }
        
        public void CancelDrag()
        {
            m_RadiusControlViewModel.CancelDrag();
        }
    }
}
