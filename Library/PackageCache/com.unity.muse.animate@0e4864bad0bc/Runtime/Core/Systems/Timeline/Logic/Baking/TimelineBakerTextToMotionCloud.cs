using System;
using System.Collections.Generic;
using Unity.DeepPose.Cloud;
using UnityEngine;
using UnityEngine.Assertions;
using ProcessState = Unity.Muse.Animate.IProcessingRequest.ProcessState;

namespace Unity.Muse.Animate
{
    class TimelineBakerTextToMotionCloud : TimelineBakerBase, ITimelineBakerTextToMotion
    {
        
        const float k_DefaultFramesPerSecond = 20f;

        public string Prompt { get; set; } = string.Empty;
        
        /// <inheritdoc />
        public int? Seed { get; set; }
        
        /// <inheritdoc />
        public float? Temperature { get; set; }

        public int Length { get; set; }

        public ITimelineBakerTextToMotion.Model ModelType { get; set; } = ITimelineBakerTextToMotion.Model.V1;

        public override ProcessState State => m_State;

        public override float Progress => m_Progress;
        public override float WaitDelay => 0f;
        public override DateTime WaitStartTime { get; protected set; }
        public bool IsRunning => m_State == ProcessState.InProgress;
        
        BakedTimelineModel m_DestinationBakedTimeline;
        Dictionary<EntityID, EntityBakingContext> m_EntityContexts;
        WebAPI m_TextToMotionAPI;
        WebAPI m_MotionDiffusionAPI;

        readonly HashSet<Guid> m_PendingRequests = new();
        ProcessState m_State;

        EntityID m_CurrentEntityID;
        EntityBakingContext m_CurrentEntityContext;
        CloudTextToMotionComponent m_CurrentMotionComponent;
        float m_Progress;

        public TimelineBakerTextToMotionCloud()
        {
            m_State = ProcessState.Unknown;
            m_TextToMotionAPI = new WebAPI(WebUtils.BackendUrl, TextToMotionAPI.ApiName);
            m_MotionDiffusionAPI = new WebAPI(WebUtils.BackendUrl, MotionDiffusionAPI.ApiName);
        }

        public override void Initialize(TimelineModel _, BakedTimelineModel destinationBakedTimeline, BakedTimelineMappingModel destinationMapping, Dictionary<EntityID, EntityBakingContext> entityBakingContexts)
        {
            Assert.IsFalse(IsRunning, "Request is already running. Stop it before initializing.");
            m_DestinationBakedTimeline = destinationBakedTimeline;
            m_EntityContexts = entityBakingContexts;
            WaitStartTime = DateTime.Now;
        }

        public override void Start()
        {
            Assert.IsNotNull(m_DestinationBakedTimeline, "No destination baked timeline. Did you Initialize first?");
            Assert.IsTrue(m_State != ProcessState.InProgress, "Baking already running.");
            Assert.IsTrue(m_State != ProcessState.InWaitDelay, "Baking already in waiting delay.");

            m_DestinationBakedTimeline.Clear();

            // Make sure timeline and baked timeline have the same entities
            if (m_EntityContexts.Count == 0)
            {
                return;
            }

            // Make sure timeline and baked timeline have the same entities
            BakingUtils.SyncTimelineAndBakedTimelineEntities(m_EntityContexts, m_DestinationBakedTimeline);

            // TODO: This solves for the first entity. We should solve for the selected entity.
            using var enumerator = m_EntityContexts.GetEnumerator();
            if (enumerator.MoveNext())
            {
                m_CurrentEntityID = enumerator.Current.Key;
                m_CurrentEntityContext = enumerator.Current.Value;
                m_CurrentEntityContext.MotionArmature.TryGetComponent(out m_CurrentMotionComponent);
            }

            m_Progress = 0f;
            var requestId = Guid.NewGuid();
            m_PendingRequests.Add(requestId);

            if (WaitDelay > 0f)
            {
                WaitStartTime = DateTime.Now;
                m_State = ProcessState.InWaitDelay;
            }
            else
            {
                StartRequest(requestId);
            }
        }

        void StartRequest(Guid requestId)
        {
            m_State = ProcessState.InProgress;
            
            switch (ModelType)
            {
                case ITimelineBakerTextToMotion.Model.V1:
                {
                    var request = new TextToMotionAPI.Request()
                    {
                        CharacterID = m_CurrentMotionComponent.CloudCharacterID,
                        Prompt = Prompt,
                        Seed = Seed,
                        Temperature = Temperature
                    };

                    m_TextToMotionAPI.SendJobRequestWithAuthHeaders<TextToMotionAPI.Request, TextToMotionAPI.Response>(request,
                        onSuccess: response =>
                        {
                            OnRequestSuccess(requestId, response);
                            response.Dispose();
                        },
                        onError: error => OnRequestFailed(requestId, error));
                    break;
                }

                case ITimelineBakerTextToMotion.Model.V2:
                {
                    var request = new MotionDiffusionAPI.Request()
                    {
                        CharacterID = m_CurrentMotionComponent.CloudCharacterID,
                        Prompt = Prompt,
                        Seed = Seed,
                        Length = Length
                    };

                    m_MotionDiffusionAPI.SendJobRequestWithAuthHeaders<MotionDiffusionAPI.Request, MotionDiffusionAPI.Response>(request,
                        onSuccess: response =>
                        {
                            OnRequestSuccess(requestId, response);
                            response.Dispose();
                        },
                        onError: error => OnRequestFailed(requestId, error));
                    break;
                }
                default:

                    throw new ArgumentOutOfRangeException();
            }
        }
        
        void OnRequestSuccess(Guid requestId, MotionDiffusionAPI.Response response)
        {
            if (!m_PendingRequests.Contains(requestId))
                return;

            m_PendingRequests.Remove(requestId);

            if (response?.Frames is null || response.Frames.Count == 0)
            {
                OnRequestFailed(requestId, new WebRequestException("No response from server."));
                return;
            }

            var fps = response.FramesPerSecond > 0 ? response.FramesPerSecond : k_DefaultFramesPerSecond;
            BakeTimeline(response.Frames, fps, response.JointNames);
            Seed = response.Seed;
            Length = response.Frames.Count;
            m_Progress = 1f;
        }

        void OnRequestSuccess(Guid requestId, TextToMotionAPI.Response response)
        {
            if (!m_PendingRequests.Contains(requestId))
                return;

            m_PendingRequests.Remove(requestId);

            if (response?.Frames is null || response.Frames.Count == 0)
            {
                OnRequestFailed(requestId, new WebRequestException("No response from server."));
                return;
            }
            
            Seed = response.Seed;
            Temperature = response.Temperature;
            var fps = response.FramesPerSecond > 0 ? response.FramesPerSecond : k_DefaultFramesPerSecond;
            BakeTimeline(response.Frames, fps, response.JointNames);
            m_Progress = 1f;
        }

        void OnRequestFailed(Guid requestId, Exception exception)
        {
            if (!m_PendingRequests.Contains(requestId))
                return;

            m_PendingRequests.Remove(requestId);

            // Stop() must be called before TriggerFailEvent() to ensure that
            // the state is set to Failed before the event is triggered.
            Stop();
            
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
            m_State = ProcessState.Failed;
            m_PendingRequests.Clear();
        }

        public override void Step()
        {
            if (m_State == ProcessState.InWaitDelay)
            {
                m_Progress = GetWaitProgress();
                var timeElapsed = (float)DateTime.Now.Subtract(WaitStartTime).TotalSeconds;

                // Check if done waiting
                if (timeElapsed >= WaitDelay)
                {
                    m_State = ProcessState.InProgress;
                    m_Progress = 0f;
                }
                return;
            }
            
            if (m_Progress >= 1f)
            {
                m_State = ProcessState.Done;
            }
        }

        void BakeTimeline(IReadOnlyList<Frame> responseFrames, float fps, IReadOnlyList<string> jointNames = null)
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

                // Apply locally and update global pose
                destinationPose.LocalPose.ApplyTo(armature.ArmatureMappingData);
                destinationPose.GlobalPose.Capture(armature.ArmatureMappingData);
            }

            if (!fps.NearlyEquals(ApplicationConstants.FramesPerSecond, 0.01f))
            {
                m_DestinationBakedTimeline.Resample(fps, ApplicationConstants.FramesPerSecond);
            }
        }
    }
}
