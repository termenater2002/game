using System;
using Unity.DeepPose.Core;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct TutorialTrackData
    {
        public int Index;
        public string TrackId;
        public string Label;
        public string Title;
        public SerializableDictionary<string, TutorialTrackStepData> Steps;

        public int NbSteps => Steps.Count;
        
        public bool TryGetStep(string id, out TutorialTrackStepData trackStep)
        {
            Steps ??= new SerializableDictionary<string, TutorialTrackStepData>();
            return Steps.TryGetValue(id, out trackStep);
        }

        public void AddStep(TutorialTrackStepData stepData)
        {
            Steps ??= new SerializableDictionary<string, TutorialTrackStepData>();
            
            if (Steps.ContainsKey(stepData.StepId))
                throw new Exception($"Could not add step. A step with the same id ({stepData.StepId}) already exists in this track ({TrackId}).");
            
            stepData.Index = NbSteps;
            Steps.Add(stepData.StepId, stepData);
        }
    }
}
