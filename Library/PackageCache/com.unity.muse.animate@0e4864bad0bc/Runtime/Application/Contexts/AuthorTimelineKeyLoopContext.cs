using System;
using Unity.Muse.Animate.Toolbar;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds all the objects used when authoring the <see cref="TimelineModel.SequenceKey"/> of a <see cref="TimelineModel"/>, when the key is a "Loop" type.
    /// </summary>
    /// <remarks>
    /// Used by the <see cref="Application.ApplicationHsm.AuthorTimelineKeyLoop"/> state.
    /// </remarks>
    class AuthorTimelineKeyLoopContext
    {
        public LoopAuthoringLogic LoopAuthoringLogic  { get; }
        public AuthoringModel AuthoringModel  { get; }
        public SelectionModel<TimelineModel.SequenceKey> KeySelection  { get; }
        public SelectionModel<EntityID> EntitySelection  { get; }
        public CameraModel Camera  { get; }
        public ToolbarsManager Toolbars { get; }

        public AuthorTimelineKeyLoopContext(LoopAuthoringLogic loopAuthoringLogic, AuthoringModel authoringModel, 
            SelectionModel<TimelineModel.SequenceKey> keySelectionModel, SelectionModel<EntityID> entitySelectionModel,
            CameraContext cameraContext, ToolbarsManager toolbars)
        {
            LoopAuthoringLogic = loopAuthoringLogic;
            AuthoringModel = authoringModel;
            Camera = cameraContext.CameraModel;
            EntitySelection = entitySelectionModel;
            KeySelection = keySelectionModel;
            Toolbars = toolbars;
        }
    }
}
