using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Common Interface for Canvas Tools
    /// </summary>
    internal interface ICanvasTool
    {
        /// <summary>
        /// Get the Canvas Manipulator
        /// </summary>
        /// <returns>Canvas Manipulator</returns>
        public CanvasManipulator GetToolManipulator(); 
        
        /// <summary>
        /// Set the Model
        /// </summary>
        /// <param name="model"></param>
        public void SetModel(Model model); 
        
        /// <summary>
        /// Evaluate the Enable State of the Tool
        /// </summary>
        /// <param name="artifact"></param>
        /// <returns>Is the tool enabled or not</returns>
        public bool EvaluateEnableState(Artifact artifact);
        
        /// <summary>
        /// Activate the Operators
        /// </summary>
        public void ActivateOperators();
        
        /// <summary>
        /// Returning the Tool View
        /// </summary>
        public VisualElement GetToolView();
       
       /// <summary>
       /// Tool button data
       /// </summary>
       internal class ToolButtonData
       {
           /// <summary>
           /// Name of the Button
           /// </summary>
           public string Name;
           
           /// <summary>
           /// Icon of the Button
           /// </summary>
           public string Icon;
          
           /// <summary>
           /// Label of the Button
           /// </summary>
           public string Label;
           
           /// <summary>
           /// Tooltip of the Button
           /// </summary>
           public string Tooltip;
       }
    }
}
