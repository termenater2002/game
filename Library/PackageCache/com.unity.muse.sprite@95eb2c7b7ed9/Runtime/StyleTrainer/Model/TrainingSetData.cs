using System;
using System.Collections.Generic;
using StyleTrainer.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.StyleTrainer.Debug;
using UnityEngine;

namespace Unity.Muse.StyleTrainer
{
    [Serializable]
    class TrainingSetData : Artifact<IList<TrainingData>, TrainingSetData>
    {
        [SerializeField]
        List<TrainingData> m_TrainingSet = new();
        [SerializeField]
        string m_ProjectID;
        GetTrainingSetRestCall m_GetTrainingSetRestCall;

        public TrainingSetData(EState state, string guid, string projectID)
            : base(state)
        {
            this.guid = guid;
            m_ProjectID = projectID;
        }

        public override void OnDispose()
        {
            for (var i = 0; i < m_TrainingSet?.Count; ++i)
                m_TrainingSet[i]?.OnDispose();
            m_GetTrainingSetRestCall?.Dispose();
            m_GetTrainingSetRestCall = null;
            base.OnDispose();
        }

        public override void GetArtifact(Action<IList<TrainingData>> onDoneCallback, bool useCache)
        {
            OnArtifactLoaded += onDoneCallback;
            if (state == EState.Initial)
            {
                state = EState.Loading;
                LoadTrainingSet();
            }
            else if (state != EState.Loading)
            {
                ArtifactLoaded(m_TrainingSet);
            }
        }

        void LoadTrainingSet()
        {
            if (m_GetTrainingSetRestCall?.restCallState != QuarkRestCall.EState.InProgress)
            {
                for (var i = 0; i < m_TrainingSet?.Count; ++i)
                {
                    m_TrainingSet[i].Delete();
                    m_TrainingSet[i].OnDispose();
                }

                ClearTrainingSet();
                var getTrainingSetRequest = new GetTrainingSetRequest
                {
                    guid = m_ProjectID,
                    training_set_guid = guid
                };

                m_GetTrainingSetRestCall ??= new GetTrainingSetRestCall(ServerConfig.serverConfig, getTrainingSetRequest);
                m_GetTrainingSetRestCall.RegisterOnSuccess(OnGetTrainingSetSuccess);
                m_GetTrainingSetRestCall.RegisterOnFailure(OnGetTrainingSetFailure);
                m_GetTrainingSetRestCall.SendRequest();
            }
        }

        void ClearTrainingSet()
        {
            for (int i = 0; i < m_TrainingSet.Count; ++i)
            {
                m_TrainingSet[i].OnDispose();
            }

            m_TrainingSet.Clear();
        }

        void OnGetTrainingSetSuccess(GetTrainingSetRestCall arg1, GetTrainingSetResponse arg2)
        {
            if (arg2.success)
            {
                for (var i = 0; i < m_TrainingSet?.Count; ++i)
                {
                    m_TrainingSet[i].Delete();
                    m_TrainingSet[i].OnDispose();
                }

                ClearTrainingSet();
                for (var i = 0; i < arg2.training_image_guids.Length; ++i) m_TrainingSet.Add(new TrainingData(EState.Initial, arg2.training_image_guids[i]));
                state = EState.Loaded;
                ArtifactLoaded(m_TrainingSet);
            }
            else
            {
                state = EState.Error;
                StyleTrainerDebug.Log($"OnGetTrainingSetSuccess but not successful {arg1.errorMessage}");
            }
        }

        void OnGetTrainingSetFailure(GetTrainingSetRestCall obj)
        {
            state = EState.Error;
            StyleTrainerDebug.Log($"OnGetTrainingSetFailure {obj.request.training_set_guid} {obj.errorMessage}");
        }

        public TrainingData this[int i] => m_TrainingSet[i];

        public TrainingSetData DuplicateNew(Action<TrainingSetData> duplicateDone)
        {
            var duplicate = new TrainingSetData(EState.New, Utilities.emptyGUID, m_ProjectID);
            duplicate.m_TrainingSet = new List<TrainingData>();
            for (var i = 0; i < m_TrainingSet.Count; ++i) m_TrainingSet[i].DuplicateNew(x => OnTrainingSetDuplicate(x, duplicate, duplicateDone));
            return duplicate;
        }

        void OnTrainingSetDuplicate(TrainingData obj, TrainingSetData newTrainingSet, Action<TrainingSetData> duplicateDone)
        {
            newTrainingSet.Add(obj);
            if (newTrainingSet.Count == m_TrainingSet.Count)
                duplicateDone?.Invoke(newTrainingSet);
        }

        public int Count => m_TrainingSet.Count;

        public void RemoveAt(int i)
        {
            m_TrainingSet.RemoveAt(i);
        }

        public void Add(TrainingData trainingData)
        {
            m_TrainingSet.Add(trainingData);
        }

        public void Delete()
        {
            for (var i = 0; i < m_TrainingSet?.Count; ++i)
                m_TrainingSet[i]?.Delete();
        }
    }
}