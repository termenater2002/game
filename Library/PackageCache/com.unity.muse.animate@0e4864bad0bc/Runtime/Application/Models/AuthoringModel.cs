using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents the Authoring Model of the Application.
    /// </summary>
    /// <remarks>
    /// Handles requests for actions related to Authoring in general.
    /// </remarks>
    class AuthoringModel
    {
        // Events meant to be used by both View Models and Authoring States
        public event Action OnModeChanged;
        public event Action OnPosingToolChanged;
        public event Action OnLoopToolChanged;
        public event Action OnTitleChanged;

        // Events meant to be used by the Authoring View Models
        public event Action OnChanged;

        // Events meant to be used by the Authoring States
        // - Library (Takes, timelines, poses, etc) interactions

        // - Timeline and Sequence Keys interactions
        public event Action<ThumbnailModel, KeyModel> OnRequestedGenerateKeyThumbnail;
        public event Action<ThumbnailModel, BakedTimelineModel, int> OnRequestedGenerateFrameThumbnail;

        /// <summary>
        /// Authoring modes
        /// </summary>
        public enum AuthoringMode
        {
            /// <summary>
            /// No authoring mode set
            /// </summary>
            Unknown,

            /// <summary>
            /// Editing / Viewing a Timeline
            /// </summary>
            Timeline,

            /// <summary>
            /// Editing / Viewing a text to motion take
            /// </summary>
            TextToMotionTake,

            /// <summary>
            /// Editing / Viewing a video to motion take
            /// </summary>
            VideoToMotionTake,

            /// <summary>
            /// Extracting a timeline (keys) from a baked timeline
            /// </summary>
            ConvertMotionToTimeline
        }

        /// <summary>
        /// Authoring full pose tool
        /// </summary>
        public enum PosingToolType
        {
            /// <summary>
            /// No tool set
            /// </summary>
            None,

            /// <summary>
            /// Dragging effectors
            /// </summary>
            Drag,

            /// <summary>
            /// Translating effectors by dragging or through Gizmo
            /// </summary>
            Translate,

            /// <summary>
            /// Rotating effectors
            /// </summary>
            Rotate,

            /// <summary>
            /// Translating and rotation effectors
            /// </summary>
            Universal,

            /// <summary>
            /// Setting effector tolerance
            /// </summary>
            Tolerance
        }

        /// <summary>
        /// Authoring loop tool
        /// </summary>
        public enum LoopToolType
        {
            /// <summary>
            /// No tool set
            /// </summary>
            None,

            /// <summary>
            /// Set loop translation offset
            /// </summary>
            Translate,

            /// <summary>
            /// Set loop rotation offset
            /// </summary>
            Rotate
        }

        public enum SelectionType
        {
            Entity,
            Effector,
            SequenceKey,
            SequenceTransition
        }

        public SelectionType LastSelectionType { get; set; }

        public PosingToolType PosingTool
        {
            get => m_PosingTool;
            set
            {
                // Dev note: For the moment we prevent this tool from being activated at all
                if (value == PosingToolType.Universal)
                {
                    return;
                }

                if (m_PosingTool != value)
                {
                    m_PosingTool = value;
                    OnPosingToolChanged?.Invoke();
                }
            }
        }

        public LoopToolType LoopTool
        {
            get => m_LoopTool;
            set
            {
                if (m_LoopTool != value)
                {
                    m_LoopTool = value;
                    OnLoopToolChanged?.Invoke();
                }
            }
        }

        public AuthoringMode Mode
        {
            get => m_Mode;
            set
            {
                if (m_Mode == value)
                    return;

                m_Mode = value;
                OnModeChanged?.Invoke();
                OnChanged?.Invoke();
            }
        }

        public string Title
        {
            get => m_Title;
            set
            {
                if (m_Title == value)
                    return;

                m_Title = value;
                OnTitleChanged?.Invoke();
            }
        }

        public string TargetName
        {
            get => m_TargetName;
            set
            {
                if (m_TargetName == value)
                    return;

                m_TargetName = value;
                OnChanged?.Invoke();
            }
        }

        public AuthorContext Context { get; }
        public TextToMotionAuthoringModel TextToMotion => m_TextToMotionAuthoringModel;
        public VideoToMotionAuthoringModel VideoToMotionAuthoringModel { get; }
        public MotionToTimelineAuthoringModel MotionToTimeline => m_MotionToTimelineAuthoringModel;
        public TimelineAuthoringModel Timeline => m_TimelineAuthoringModel;
        public PoseAuthoringLogic PoseAuthoringLogic { get; set; }

        public bool IsEditingTimeline() => Mode == AuthoringMode.Timeline;

        PosingToolType m_PosingTool = PosingToolType.None;
        LoopToolType m_LoopTool = LoopToolType.None;
        AuthoringMode m_Mode = AuthoringMode.Unknown;

        bool m_CanCopyPose;
        bool m_CanEstimatePose;
        bool m_CanDeleteSelectedEntities;

        string m_Title = "Untitled";
        string m_TargetName = "Undefined";
        
        readonly TextToMotionAuthoringModel m_TextToMotionAuthoringModel;
        readonly MotionToTimelineAuthoringModel m_MotionToTimelineAuthoringModel;
        readonly TimelineAuthoringModel m_TimelineAuthoringModel;

        public AuthoringModel(AuthorContext context)
        {
            Context = context;
            m_TextToMotionAuthoringModel = new TextToMotionAuthoringModel();
            m_MotionToTimelineAuthoringModel = new MotionToTimelineAuthoringModel();
            m_TimelineAuthoringModel = new TimelineAuthoringModel(this);
            VideoToMotionAuthoringModel = new VideoToMotionAuthoringModel();
        }

        public void RegisterToUI()
        {
            // Track the T2M baking logic (Spinning Circle)
            Context.TextToMotionTakeContext.BakingTaskStatusUI.TrackBakingLogics(Context.TextToMotionService.Baking);
        }

        public void UnregisterFromUI()
        {
            // Untrack the T2M baking logic (Spinning Circle)
            Context.TextToMotionTakeContext.BakingTaskStatusUI.UntrackBakingLogics(Context.TextToMotionService.Baking);
        }

        internal void QueueTimelineBaking()
        {
            Context.TimelineContext.TimelineBakingLogic.QueueBaking(true);
        }

        // [Section] Request Methods

        public void RequestGenerateKeyThumbnail(ThumbnailModel target, KeyModel key)
        {
            OnRequestedGenerateKeyThumbnail?.Invoke(target, key);
        }

        public void RequestGenerateFrameThumbnail(ThumbnailModel target, BakedTimelineModel timeline, int frame)
        {
            OnRequestedGenerateFrameThumbnail?.Invoke(target, timeline, frame);
        }
        
        

        // [Section] Do Methods

        internal void DoRefreshKeyThumbnail(ThumbnailModel target, KeyModel key)
        {
            Context.ThumbnailsService.RequestThumbnail(target, key, Context.Camera.Position, Context.Camera.Rotation);
        }

        internal void DoRefreshFrameThumbnail(ThumbnailModel target, BakedTimelineModel timeline, int frame)
        {
            Context.ThumbnailsService.RequestThumbnail(target, timeline, frame, Context.Camera.Position, Context.Camera.Rotation, 3, 3, 0, 0);
        }

        // [Section] UI Events Handlers

        
    }
}
