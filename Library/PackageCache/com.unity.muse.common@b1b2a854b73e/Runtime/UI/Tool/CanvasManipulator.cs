using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    abstract class CanvasManipulator: Manipulator
    {
        protected Model m_CurrentModel;
        protected CanvasManipulator(Model model)
        {
            m_CurrentModel = model;
        }
    }
}
