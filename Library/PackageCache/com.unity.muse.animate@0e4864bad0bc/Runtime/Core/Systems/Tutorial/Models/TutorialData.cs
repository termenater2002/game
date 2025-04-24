using System;
using Unity.DeepPose.Core;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct TutorialData
    {
        public SerializableDictionary<string, TutorialTrackData> Tracks;
        public string CurrentTrackId;
        public string CurrentStepId;

        public int NbTracks => Tracks.Count;

        public void AddTrack(string trackId, string title, string label)
        {
            Tracks ??= new SerializableDictionary<string, TutorialTrackData>();
            
            if (Tracks.ContainsKey(trackId))
                throw new Exception($"Could not add track, track ({trackId}) already exists.");

            Tracks[trackId] = new TutorialTrackData()
            {
                Index = NbTracks,
                TrackId = trackId,
                Title = title,
                Label = label,
            };
        }

        public void AddStep(string trackId, TutorialTrackStepData stepData)
        {
            if (!Tracks.TryGetValue(trackId, out var track))
                throw new Exception($"Cannot add step, track ({trackId}) does not exist.");
            
            track.AddStep(stepData);
            Tracks[trackId] = track;
        }
    }
}
