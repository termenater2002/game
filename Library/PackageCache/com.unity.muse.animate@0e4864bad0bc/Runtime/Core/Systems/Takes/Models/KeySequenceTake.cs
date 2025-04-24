using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// A take containing a key sequence.
    /// </summary>
    [Serializable]
    class KeySequenceTake : TakeModel, ICopyable<KeySequenceTake>
    {
        public override bool IsPreviewable => true;
        public TimelineModel TimelineModel => m_TimelineModel;
        public BakedTimelineModel BakedTimelineModel => m_BakedTimelineModel;
        public BakedTimelineMappingModel BakedTimelineMappingModel => m_BakedTimelineMappingModel;
        public AnimationClip BakedAnimationClip
        {
            get => m_BakedAnimationClip;
            set => m_BakedAnimationClip = value;
        }

        public override bool CanGenerateThumbnail => BakedTimelineModel.FramesCount > 0;

        [SerializeReference]
        BakedTimelineModel m_BakedTimelineModel = new();
        
        [SerializeReference]
        TimelineModel m_TimelineModel = new();

        [SerializeReference]
        BakedTimelineMappingModel m_BakedTimelineMappingModel = new();
        
        [SerializeReference]
        public AnimationClip m_BakedAnimationClip;
        
        public KeySequenceTake(string title, string description, TimelineModel timelineModel, BakedTimelineModel bakedTimelineModel, BakedTimelineMappingModel bakedTimelineMappingModel)
            : base(title, description, LibraryItemType.KeySequenceTake)
        {
            timelineModel.CopyTo(m_TimelineModel, true);
            bakedTimelineModel.CopyTo(m_BakedTimelineModel, true);
            bakedTimelineMappingModel.CopyTo(m_BakedTimelineMappingModel, true);

            SubscribeToModels();
            IsEditable = true;
        }

        public void SaveWorkingStage(StageModel stageModel)
        {
            stageModel.WorkingBakedTimelineMapping.CopyTo(m_BakedTimelineMappingModel, false);
            stageModel.WorkingTimeline.CopyTo(m_TimelineModel, false);
            stageModel.WorkingBakedTimeline.CopyTo(m_BakedTimelineModel, false);
        }
        
        public new void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            SubscribeToModels();
            IsEditable = true;
        }

        void SubscribeToModels()
        {
            Debug.Assert(TimelineModel != null);
            Debug.Assert(BakedTimelineModel != null);

            BakedTimelineModel.OnChanged += OnBakedTimelineChanged;
            TimelineModel.OnTimelineChanged += OnTimelineModelChanged;
        }

        public new KeySequenceTake Clone()
        {
            var clone = new KeySequenceTake(Title, Description, TimelineModel, BakedTimelineModel, BakedTimelineMappingModel);
            return clone;
        }

        void ICopyable<KeySequenceTake>.CopyTo(KeySequenceTake item)
        {
            base.CopyTo(item);
            
            Assert.IsNotNull(TimelineModel, "CopyTo() Failed: m_TimelineModel is null");
            Assert.IsNotNull(BakedTimelineModel, "CopyTo() Failed: m_BakedTimelineModel is null");
            Assert.IsNotNull(BakedTimelineMappingModel, "CopyTo() Failed: m_BakedTimelineMappingModel is null");
            
            TimelineModel.CopyTo(item.TimelineModel);
            BakedTimelineModel.CopyTo(item.BakedTimelineModel);
            BakedTimelineMappingModel.CopyTo(item.BakedTimelineMappingModel);
            item.BakedAnimationClip = BakingUtils.CloneAnimationClip(BakedAnimationClip);
        }

        void OnTimelineModelChanged(TimelineModel timelineModel, TimelineModel.Property property)
        {
            Assert.AreEqual(timelineModel, TimelineModel, "Not the same timeline");

            if (property is TimelineModel.Property.InputAnimationData)
            {
                InvokeChanged(Property.InputAnimationData);
            }
        }

        void OnBakedTimelineChanged(BakedTimelineModel model)
        {
            InvokeChanged(Property.OutputAnimationData);
        }

        public void RefreshAllKeysThumbnails(ThumbnailsService thumbnailsService, CameraModel cameraModel)
        {
            for (var i = 0; i < TimelineModel.Keys.Count; i++)
            {
                var key = TimelineModel.Keys[i];
                thumbnailsService.RequestThumbnail(key.Thumbnail, key.Key, cameraModel.Position, cameraModel.Rotation);
            }
        }

        public override void RequestThumbnailUpdate(ThumbnailModel target, ThumbnailsService thumbnailsService, CameraModel cameraModel)
        {
            if (TimelineModel == null)
                return;

            if (TimelineModel.Keys.Count == 0)
                return;

            var key = TimelineModel.Keys[0];
            thumbnailsService.RequestThumbnail(target, key.Key, cameraModel.Position, cameraModel.Rotation);
        }
    }
}
