using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    class PanTool : ICanvasTool
    {
        Model m_CurrentModel;
        
        public CanvasManipulator GetToolManipulator()
        {
            return null;
        }

        public void SetModel(Model model)
        {
            m_CurrentModel = model;
        }

        public bool EvaluateEnableState(Artifact artifact)
        {
            return m_CurrentModel.isRefineMode && ArtifactCache.IsInCache(artifact);
        }

        public void ActivateOperators()
        {
            // Do nothing
        }

        public VisualElement GetToolView()
        {
            return null;
        }
    }
}
