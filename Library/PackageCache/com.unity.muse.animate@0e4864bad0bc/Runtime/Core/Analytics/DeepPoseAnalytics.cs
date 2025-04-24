using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.Muse.Animate
{
    class DeepPoseAnalytics : MonoBehaviour
    {
        const string k_EventCloudLabSessionStarted = "cloudLabSessionStarted";
        const string k_EventTutorialStepOpened = "tutorialStepOpened";
        const string k_EventPerformance = "performance";
        const string k_EventActionOfInterest = "actionOfInterest";
        const string k_EventFrameCameraOnEffectorSelection = "frameCameraOnEffectorSelection";
        const string k_EventEntityEffectorUsed = "entityEffectorUsed";
        const string k_EventEffectorUsed = "effectorUsed";
        const string k_EventTimelineSetPlaybackSpeed = "SetPlaybackSpeed";
        const string k_EventSetCurrentFrame = "SetCurrentFrame";
        const string k_EventKeyAction = "KeyAction";
            
        const string k_ParameterTutorialStepId = "tutorialStepId";
        const string k_ParameterTutorialPercentageComplete = "tutorialPercentageComplete";
        const string k_ParameterUserAgent = "userAgent";
        const string k_ParameterMemoryUsed = "memoryUsed";
        const string k_ParameterAverageFramerate = "averageFramerate";
        const string k_ParameterActionOfInterestName = "actionOfInterestName";
        const string k_ParameterSelectedEffectorNames = "selectedEffectorNames";
        const string k_ParameterEffectorAction = "effectorAction";
        const string k_ParameterEffectorNames = "effectorNames";
        const string k_ParameterTimelineSetPlaybackSpeed = "PlaybackSpeed";
        const string k_ParameterTimelineKeyIndex = "timelineKeyIndex";
        const string k_ParameterFrameIndex = "frameIndex";
        const string k_ParameterKeyActionName = "keyActionName";
        
        const string k_UnknownUserAgentMessage = "Unknown";

        const float k_PerformanceTrackingIntervalSeconds = 60;

        static bool s_IsServiceInitialized;
        static readonly List<Action> k_EarlyAnalyticsEvents = new();

        // Global switch to control if analytics should be logged in UGS when in the Editor
        // or instead be logged to the Console. It's best to keep it disabled while writing
        // new analytics, and enable it temporarily to validate with UGS.
        const bool k_AnalyticsEnabled = false;
        
        void Start()
        {
        }

        public static void SendTutorialStepOpenedEvent(string stepId, float percentageComplete)
        {
            void TutorialStepOpenedEvent()
            {
                var dict = new Dictionary<string, object>
                {
                    { k_ParameterTutorialStepId, stepId },
                    { k_ParameterTutorialPercentageComplete, percentageComplete }
                };

                SendAnalyticsCustomData(k_EventTutorialStepOpened, dict);
            }

            CacheOrRunEvent(TutorialStepOpenedEvent);
        }

        public static void SendActionOfInterestEvent(ActionOfInterest action)
        {
            void ActionOfInterestEvent()
            {
                var dict = new Dictionary<string, object>
                {
                    { k_ParameterActionOfInterestName, Enum.GetName(typeof(ActionOfInterest), action) }
                };
                
                SendAnalyticsCustomData(k_EventActionOfInterest, dict);
            }

            CacheOrRunEvent(ActionOfInterestEvent);
        }

        static void CacheOrRunEvent(Action evt)
        {
            if (s_IsServiceInitialized)
            {
                evt();
            }
            else
            {
                // In the event that the service never initializes, guard against a memory leak by limited to 100
                // pre-initialization cached events
                if (k_EarlyAnalyticsEvents.Count > 100)
                    k_EarlyAnalyticsEvents.RemoveAt(0);

                k_EarlyAnalyticsEvents.Add(evt);
            }
        }

        public static void SendFrameCameraOnEffectorSelection(string selectedEffectorNames)
        {
            void FrameCameraOnEffectorSelection()
            {
                var dict = new Dictionary<string, object>
                {
                    { k_ParameterSelectedEffectorNames, selectedEffectorNames }
                };                
                
                SendAnalyticsCustomData(k_EventFrameCameraOnEffectorSelection, dict);
            }

            CacheOrRunEvent(FrameCameraOnEffectorSelection);
        }

        public static void SendEntityEffectorUsed(EffectorAction effectorAction)
        {
            void EffectorUsed()
            {
                var dict = new Dictionary<string, object>
                {
                    {
                        k_ParameterEffectorAction,
                        Enum.GetName(typeof(EffectorAction), effectorAction)
                    }
                };

                SendAnalyticsCustomData(k_EventEntityEffectorUsed, dict);
            }

            CacheOrRunEvent(EffectorUsed);
        }
        
        public static void SendEffectorUsed(EffectorAction effectorAction, string effectorNames)
        {
            void EffectorUsed()
            {
                var dict = new Dictionary<string, object>
                {
                    {
                        k_ParameterEffectorAction,
                        Enum.GetName(typeof(EffectorAction), effectorAction)
                    },
                    { k_ParameterEffectorNames, effectorNames }
                };

                SendAnalyticsCustomData(k_EventEffectorUsed, dict);
            }

            CacheOrRunEvent(EffectorUsed);
        }

        public static void SendTimelineAction(TimelineAction timelineAction)
        {
            void TimelineAction()
            {
                SendAnalyticsCustomData(Enum.GetName(typeof(TimelineAction), timelineAction), null);
            }

            CacheOrRunEvent(TimelineAction);
        }
        
        public static void SendTimelineSetPlaybackSpeed(float playbackSpeed)
        {
            void TimelineSetPlaybackSpeed()
            {
                var dict = new Dictionary<string, object>
                {
                    {
                        k_ParameterTimelineSetPlaybackSpeed, playbackSpeed
                    }
                };

                SendAnalyticsCustomData(k_EventTimelineSetPlaybackSpeed, dict);
            }

            CacheOrRunEvent(TimelineSetPlaybackSpeed);
        }

        public static void SendTimelineKeyAction(TimelineKeyAction timelineKeyAction, int keyIndex)
        {
            void TimelineKeyAction()
            {
                var dict = new Dictionary<string, object>
                {
                    {
                        k_ParameterTimelineKeyIndex, keyIndex
                    }
                };
                
                SendAnalyticsCustomData(Enum.GetName(typeof(TimelineKeyAction), timelineKeyAction), dict);
            }

            CacheOrRunEvent(TimelineKeyAction);
        }
        
        public static void SendSetCurrentFrameAction(int frameIndex)
        {
            void SetCurrentFrameAction()
            {
                var dict = new Dictionary<string, object>
                {
                    {
                        k_ParameterFrameIndex, frameIndex
                    }
                };
                
                SendAnalyticsCustomData(k_EventSetCurrentFrame, dict);
            }

            CacheOrRunEvent(SetCurrentFrameAction);
        }
        
        public static void SendKeyAction(string keyActionName, int keyIndex)
        {
            void KeyAction()
            {
                var dict = new Dictionary<string, object>
                {
                    {k_ParameterKeyActionName, keyActionName},
                    {k_ParameterTimelineKeyIndex, keyIndex}
                };
                
                SendAnalyticsCustomData(k_EventKeyAction, dict);
            }

            CacheOrRunEvent(KeyAction);
        }
        
        private static void SendAnalyticsCustomData(string eventName, IDictionary<string, object> eventParams)
        {

        }
       
        private static string GetDictionaryString(IDictionary<string, object> eventParams)
        {
            var sb = new StringBuilder();

            if (eventParams != null)
            {
                foreach (var kvp in eventParams)
                {
                    sb.Append($"{kvp.Key}: {kvp.Value}");
                    sb.AppendLine(); // Add a new line after each key-value pair
                }
            }

            return sb.ToString();
        }

        [Conditional("LOG_ANALYTICS_EVENTS")]
        private static void LogAnalyticsEvent(string eventName, IDictionary<string, object> eventParams)
        {
            Debug.Log($"ANALYTICS - event: {eventName}, dict: {GetDictionaryString(eventParams)}");
        }

        public enum ActionOfInterest
        {
            Save,
            Export,
            ResetPose,
            CopyPose,
            PastePose,
            FrameCameraOnEntitySelection,
            FrameCameraAll,
            AddKey// TODO: pressing 'f' does nothing when no entity is selected ATM; add when it does
        }

        public enum EffectorAction
        {
            Select,
            Deselect,
            Translate,
            Rotate,
            AdjustTolerance
        }
        
        public enum TimelineAction
        {
            Play,
            Pause,
            LoopEnable,
            LoopDisable
        }
        
        public enum TimelineKeyAction
        {
            GoToPreviousKey,
            GoToNextKey,
            AddKey,
            SelectKey,
            InsertKey
        }
    }
}
