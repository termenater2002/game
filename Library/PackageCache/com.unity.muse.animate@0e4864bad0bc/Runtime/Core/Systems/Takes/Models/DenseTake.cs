using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Base class representing a take containing dense motion.
    /// </summary>
    [Serializable]
    abstract class DenseTake : TakeModel, ISerializationCallbackReceiver
    {
        public BakedTimelineModel BakedTimelineModel => m_BakedTimelineModel;
        public AnimationClip BakedAnimationClip
        {
            get => m_BakedAnimationClip;
            set => m_BakedAnimationClip = value;
        }

        [SerializeField]
        BakedTimelineModel m_BakedTimelineModel;
        
        [SerializeField]
        public AnimationClip m_BakedAnimationClip;

        public override bool CanGenerateThumbnail => BakedTimelineModel.FramesCount > 0;

        public event Action OnBakingComplete;
        public event Action OnBakingFailed;

        protected DenseTake(string title, string description, LibraryItemType itemType, bool hasToBake)
            : base(title, description, itemType, hasToBake)
        {
            m_BakedTimelineModel = new BakedTimelineModel();
            SubscribeToModels();
        }

        public new void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            
            // Events need to be re-registered after deserialization.
            SubscribeToModels();
        }

        void SubscribeToModels()
        {
            Debug.Assert(BakedTimelineModel != null);
            BakedTimelineModel.OnChanged += OnBakedTimelineChanged;
        }
        
        protected void CopyTo(DenseTake item)
        {
            base.CopyTo(item);
            BakedTimelineModel.CopyTo(item.BakedTimelineModel);
            item.BakedAnimationClip = BakingUtils.CloneAnimationClip(BakedAnimationClip);
        }
        
        void OnBakedTimelineChanged(BakedTimelineModel model)
        {
            InvokeChanged(Property.OutputAnimationData);
        }

        void OnRequestProgressed(float overallProgress)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"Request progressed: {overallProgress}");
            Progress = overallProgress;
        }

        void OnRequestCompleted()
        {
            DevLogger.LogInfo("Request completed.");
            IsBaking = false;
            Progress = 1f;
            OnBakingComplete?.Invoke();
        }

        void OnRequestFailed(string error)
        {
            DevLogger.LogError($"Request failed: {error}");
            IsBaking = false;
            IsValid = false;
            OnBakingFailed?.Invoke();
        }

        void OnRequestCanceled()
        {
            IsBaking = false;
            IsValid = false;
            OnBakingFailed?.Invoke();
        }

        public override void TrackRequest<T>(BakingRequest<T> request)
        {
            Progress = 0f;
            IsBaking = true;
            request.OnProgressed += OnRequestProgressed;
            request.OnCompleted += OnRequestCompleted;
            request.OnFailed += OnRequestFailed;
            request.OnCanceled += OnRequestCanceled;
        }

        public override void RequestThumbnailUpdate(ThumbnailModel target, ThumbnailsService thumbnailsService, CameraModel cameraModel)
        {
            if (BakedTimelineModel.FramesCount <= 0)
                return;
            
            var previewFrame = BakedTimelineModel.FramesCount / 2;

            thumbnailsService.RequestThumbnail(
                target,
                BakedTimelineModel,
                previewFrame,
                cameraModel.Position,
                cameraModel.Rotation,
                trailPrev: 3,
                trailPrevSize: 3,
                trailNext: 0,
                trailNextSize: 0);
        }

        public override string GetSearchLabel()
        {
            return $"{Title.ToLower()} - {Description.ToLower()}";
        }
    }
}
