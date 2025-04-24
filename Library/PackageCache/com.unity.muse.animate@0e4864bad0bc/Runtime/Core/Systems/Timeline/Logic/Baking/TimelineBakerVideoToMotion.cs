using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.DeepPose.Cloud;
using UnityEngine;
using UnityEngine.Assertions;
using ProcessState = Unity.Muse.Animate.IProcessingRequest.ProcessState;

namespace Unity.Muse.Animate
{
    class TimelineBakerVideoToMotion : TimelineBakerBase
    {
        public enum Model
        {
            V1,
            V2
        }

        public string FilePath { get; set; } = string.Empty;
        public string ConvertedFilePath { get; private set; }
        public int? StartFrame { get; set; }
        public int? FrameCount { get; set; }

        public Model ModelType { get; set; }

        public override ProcessState State => m_State;
        public override float WaitDelay => 0;
        public override DateTime WaitStartTime { get; protected set; }
        public override float Progress => m_Progress;

        BakedTimelineModel m_DestinationBakedTimeline;
        Dictionary<EntityID, EntityBakingContext> m_EntityContexts;

        WebAPI m_VideoToMotionAPI = new WebAPI(ApplicationConstants.VideoInferenceHost, VideoToMotionAPI.ApiName);
        ProcessState m_State = ProcessState.Unknown;

        Task<VideoToMotionAPI.Response> m_BackendComputeTask;
        CancellationTokenSource m_BackendComputeCancellationTokenSource;

        EntityID m_CurrentEntityID;
        EntityBakingContext m_CurrentEntityContext;

        // TODO: This component is useful for any baking process that needs to map one armature to another, so it
        // should probably be renamed.
        CloudTextToMotionComponent m_CurrentMotionComponent;
        float m_Progress;
        BakingStep m_CurrentStep;

        static readonly List<KeyValuePair<string, string>> k_RequestHeaders = new()
        {
            new(ApplicationConstants.AuthorizationHeaderName, $"Bearer {ApplicationConstants.VideoInferenceAuthorizationToken}")
        };

        enum BakingStep
        {
            Start,
            WaitingForConversion,
            ConversionComplete,
            WaitingForBaking,
        }

        public override void Initialize(TimelineModel _, BakedTimelineModel destination, BakedTimelineMappingModel destinationMapping, Dictionary<EntityID, EntityBakingContext> entityBakingContexts)
        {
            m_DestinationBakedTimeline = destination;
            m_EntityContexts = entityBakingContexts;
        }

        public override void Start()
        {
            Stop();

            m_DestinationBakedTimeline.Clear();

            // Make sure timeline and baked timeline have the same entities
            if (m_EntityContexts.Count == 0)
            {
                return;
            }

            BakingUtils.SyncTimelineAndBakedTimelineEntities(m_EntityContexts, m_DestinationBakedTimeline);

            // TODO: This solves for the first entity. We should solve for the selected entity.
            using var enumerator = m_EntityContexts.GetEnumerator();
            if (enumerator.MoveNext())
            {
                m_CurrentEntityID = enumerator.Current.Key;
                m_CurrentEntityContext = enumerator.Current.Value;
                m_CurrentEntityContext.MotionArmature.TryGetComponent(out m_CurrentMotionComponent);
            }

            m_CurrentStep = BakingStep.Start;
            m_Progress = 0f;
            m_State = ProcessState.InProgress;
        }

        void StartRequest()
        {
            m_CurrentStep = BakingStep.WaitingForBaking;
            // Populate the form data with the video
            var fileBytes = System.IO.File.ReadAllBytes(ConvertedFilePath);
            var fileName = System.IO.Path.GetFileName(ConvertedFilePath);

            var request = new VideoToMotionAPI.Request
            {
                VideoData = fileBytes,
                VideoName = fileName,
                StartFrame = StartFrame,
                FrameCount = FrameCount
            };

            m_BackendComputeCancellationTokenSource = new CancellationTokenSource();
            m_BackendComputeTask = m_VideoToMotionAPI.PostJobRequestFormAsync<VideoToMotionAPI.Request, VideoToMotionAPI.Response>(request, 5000, k_RequestHeaders);
        }

        void OnRequestSuccess(VideoToMotionAPI.Response response)
        {
            if (response?.Frames is null || response.Frames.Count == 0)
            {
                OnRequestFailed(new WebRequestException("Empty response from server."));
                return;
            }

            BakeTimeline(response.Frames, response.FramesPerSecond);
            m_State = ProcessState.Done;
            m_Progress = 1f;
        }

        void OnRequestFailed(Exception exception)
        {
            // State must be set to Failed before triggering the event
            m_State = ProcessState.Failed;
            TriggerFailEvent(exception.Message);

            if (exception is ApiVersionMismatchException)
            {
                // If there's a problem with the package version, we need
                // to propagate this outside the application.
                DevLogger.LogError("API version mismatch. Please update the package.");
                Application.Instance.PublishMessage(new VersionErrorMessage());
            }
        }

        public override void Stop()
        {
            if (m_BackendComputeCancellationTokenSource != null)
            {
                DevLogger.LogWarning("Cancelling in-progress request");
                m_BackendComputeCancellationTokenSource.Cancel();
                m_BackendComputeCancellationTokenSource.Dispose();
                m_BackendComputeCancellationTokenSource = null;
                m_State = ProcessState.Failed;
                TriggerFailEvent("Request cancelled.");
            }

            m_BackendComputeTask = null;
        }

        void StartVideoConversion()
        {
            if (!Locator.TryGet(out IVideoConverter converter))
            {
                return;
            }

            if (!Locator.TryGet(out ICoroutineRunner coroutineRunner))
            {
                return;
            }

            m_CurrentStep = BakingStep.WaitingForConversion;

            // Default settings for the video converter are correct.
            var outputDir = converter.GetTempOutputPath();
            var fileName = System.IO.Path.GetFileName(FilePath);

            if (!System.IO.Directory.Exists(outputDir))
                System.IO.Directory.CreateDirectory(outputDir);

            ConvertedFilePath = System.IO.Path.Combine(outputDir, fileName);

            var startFrame = StartFrame ?? 0;
            var endFrame = StartFrame + FrameCount ?? -1;
            DevLogger.LogInfo($"V2M Baker: saving converted video to {ConvertedFilePath}");

            converter.conversionCompleted += ConversionComplete;
            converter.conversionFailed += ConversionFailed;
            coroutineRunner.StartCoroutine(converter.Convert(FilePath, ConvertedFilePath, startFrame, endFrame));

            void ConversionComplete()
            {
                converter.conversionCompleted -= ConversionComplete;
                converter.conversionFailed -= ConversionFailed;
                m_CurrentStep = BakingStep.ConversionComplete;
            }

            void ConversionFailed(string error)
            {
                converter.conversionCompleted -= ConversionComplete;
                converter.conversionFailed -= ConversionFailed;
                OnRequestFailed(new Exception(error));
            }
        }

        public override void Step()
        {
            if (m_CurrentStep is BakingStep.Start)
            {
                // Do video conversion
                StartVideoConversion();
            }
            else if (m_CurrentStep is BakingStep.ConversionComplete)
            {
                m_Progress = 0.5f;

                DevLogger.LogInfo("V2M Baker: Conversion complete");

                StartRequest();
            }
            else if (m_CurrentStep is BakingStep.WaitingForBaking)
            {
                // TODO: Update progress number
                // https://jira.unity3d.com/browse/MUSEANIM-365
                if (m_BackendComputeTask is { IsCompleted: true })
                {
                    try
                    {
                        OnRequestSuccess(m_BackendComputeTask.Result);
                    }
                    catch (AggregateException ex)
                    {
                        OnRequestFailed(ex.InnerException);
                    }
                    finally
                    {
                        m_BackendComputeCancellationTokenSource.Dispose();
                        m_BackendComputeCancellationTokenSource = null;
                        m_BackendComputeTask = null;
                    }
                }
            }
        }

        void BakeTimeline(IReadOnlyList<Frame> responseFrames, float fps)
        {
            m_DestinationBakedTimeline.FramesCount = responseFrames.Count;
            var armature = m_CurrentEntityContext.MotionArmature;
            Assert.IsNotNull(m_CurrentMotionComponent);

            var characterToMotionMapping = m_CurrentMotionComponent.CharacterToMotionArmatureMapping;
            Assert.IsNotNull(characterToMotionMapping);

            for (var frameIndex = 0; frameIndex < responseFrames.Count; ++frameIndex)
            {
                var responseFrame = responseFrames[frameIndex];
                var destinationFrame = m_DestinationBakedTimeline.GetFrame(frameIndex);
                if (!destinationFrame.TryGetModel(m_CurrentEntityID, out var destinationPose))
                {
                    continue;
                }

                // Capture initial pose to get local position of all joints
                destinationPose.LocalPose.Capture(armature.ArmatureMappingData);

                // Read frame data
                var rootJointPosition = responseFrame.Positions[0];

                destinationPose.LocalPose.SetPosition(0, rootJointPosition);

                destinationPose.LocalPose.CopyFrom(responseFrame.Rotations,
                    characterToMotionMapping,
                    inverseMapping: true);
            }

            if (!fps.NearlyEquals(ApplicationConstants.FramesPerSecond, 0.01f))
            {
                m_DestinationBakedTimeline.Resample(fps, ApplicationConstants.FramesPerSecond);
            }

            // Apply locally and update global pose
            for (var frameIndex = 0; frameIndex < m_DestinationBakedTimeline.FramesCount; ++frameIndex)
            {
                var destinationFrame = m_DestinationBakedTimeline.GetFrame(frameIndex);
                if (!destinationFrame.TryGetModel(m_CurrentEntityID, out var destinationPose))
                {
                    continue;
                }

                destinationPose.LocalPose.ApplyTo(armature.ArmatureMappingData);
                destinationPose.GlobalPose.Capture(armature.ArmatureMappingData);
            }
        }
    }
}
