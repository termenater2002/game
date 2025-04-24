using System;
using System.Collections.Generic;
using System.Linq;
using StyleTrainer.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.StyleTrainer.Debug;
using UnityEngine;

namespace Unity.Muse.StyleTrainer
{
    [Serializable]
    class StyleData : Artifact<StyleData, StyleData>
    {
        [SerializeField]
        bool m_IsFallback;

        [SerializeReference]
        ImageArtifact m_Thumbnail;
        [SerializeField]
        bool m_Visible = true;
        [SerializeField]
        public string selectedCheckPointGUID = Utilities.emptyGUID;
        [SerializeField]
        public string m_FavoriteCheckPointGUID = Utilities.emptyGUID;
        [SerializeField]
        public string parentID = Utilities.emptyGUID;
        [SerializeField]
        string m_ProjectID = Utilities.emptyGUID;
        [SerializeField]
        int m_TrainingSteps;
        [SerializeField]
        List<CheckPointData> m_CheckPoints = new();

        [SerializeField]
        List<string> m_SampleOutputPrompts = new();
        [SerializeField]
        List<TrainingSetData> m_TrainingSetData = new();
        [SerializeField]
        string m_Name;
        [SerializeField]
        string m_Description;

        // Arbitrary. Server limits to 256.
        public const int maxNameLength = 150;
        public const int maxDescriptionLength = 256;
        public const int maxPromptLength = 256;
#pragma warning disable 414
        CheckPointStatusCheckTask m_CheckPointStatusCheckTask;
#pragma warning restore 414

        public string projectID => m_ProjectID;
        public const int maxTrainingSetImages = 15;

        /// <summary>
        /// Returns true if the style is a built-in fallback style.
        /// </summary>
        public bool isFallback => m_IsFallback;

        public override void OnDispose()
        {
            m_Thumbnail?.OnDispose();
            for (var i = 0; i < m_CheckPoints?.Count; ++i)
                m_CheckPoints[i]?.OnDispose();
            base.OnDispose();
        }

        public override void Init()
        {
            base.Init();
            for (var i = 0; i < m_CheckPoints?.Count; ++i)
                m_CheckPoints[i]?.Init();
        }

        public bool InTrainingState()
        {
            for (var i = 0; i < m_CheckPoints.Count; ++i)
                if (m_CheckPoints[i].state == EState.Training)
                    return true;

            return false;
        }

        public string favoriteCheckPoint
        {
            get => m_FavoriteCheckPointGUID;
            set
            {
                if (m_FavoriteCheckPointGUID != value)
                {
                    m_FavoriteCheckPointGUID = value;
                    DataChanged(this);
                }
            }
        }

        public StyleData(EState state, string guid, string projectId)
            : base(state)
        {
            title = StringConstants.newVersion;
            description = string.Empty;
            this.guid = guid;
            parentID = Utilities.emptyGUID;
            m_ProjectID = projectId;
            m_TrainingSteps = StyleTrainerConfig.config.trainingSteps;
        }

        public int SelectedCheckPointIndex()
        {
            var index = 0;
            for (; index < m_CheckPoints.Count; ++index)
                if (m_CheckPoints[index].guid == selectedCheckPointGUID)
                    return index;

            return 0;
        }

        public static StyleData CreateNewStyle(string projectID)
        {
            var styleData = new StyleData(EState.New, Utilities.emptyGUID,  projectID);
            styleData.title = "New Style";
            styleData.guid = string.Empty;
            styleData.m_Thumbnail = new ImageArtifact(EState.New);
            for(int i = 0; i < StyleTrainerConfig.config.minSampleSetSize; ++i)
                styleData.m_SampleOutputPrompts.Add("");
            return styleData;
        }

        public CheckPointData GetFavouriteOrLatestCheckPoint()
        {
            if (Utilities.ValidStringGUID(favoriteCheckPoint))
                for (var i = 0; i < m_CheckPoints.Count; ++i)
                    if (m_CheckPoints[i].guid == favoriteCheckPoint && m_CheckPoints[i].state == EState.Loaded)
                        return m_CheckPoints[i];

            for (var i = m_CheckPoints.Count - 1; i >= 0; --i)
                if (m_CheckPoints[i].state == EState.Loaded)
                    return m_CheckPoints[i];

            return null;
        }

        public CheckPointData GetSelectedCheckPoint()
        {
            var index = SelectedCheckPointIndex();
            if (index >= m_CheckPoints?.Count || index < 0)
                return null;
            return m_CheckPoints[SelectedCheckPointIndex()];
        }

        public string styleTitle
        {
            get
            {
                if(string.IsNullOrEmpty(m_Name))
                    return GetFavouriteOrLatestCheckPoint()?.name ?? "Style has no name. Something is wrong";
                return m_Name;
            }
        }

        public string styleDescription
        {
            get
            {
                if(string.IsNullOrEmpty(m_Description))
                    return GetFavouriteOrLatestCheckPoint()?.description ?? "Style has no versions. Something is wrong";
                return m_Description;
            }
        }

        public string styleDescriptionWithCheckPointDetails
        {
            get
            {
                if(string.IsNullOrEmpty(m_Description))
                    return GetFavouriteOrLatestCheckPoint()?.description ?? "Style has no versions. Something is wrong";
                var s = GetCheckPointDescription(GetFavouriteOrLatestCheckPoint());
                return $"({s})\n{m_Description}";
            }
        }

        public string GetCheckPointDescription(CheckPointData checkPoint)
        {
            if (checkPoint == null)
                return "";
            var index = m_CheckPoints.FindIndex(x => x.guid == checkPoint.guid);
            return $"Version {index+1}, Trained with {checkPoint.trainingSteps} steps";
        }

        public string title
        {
            get
            {
                if(string.IsNullOrEmpty(m_Name))
                    return GetSelectedCheckPoint()?.name ?? "Style has no name. Something is wrong";
                return m_Name;
            }
            set
            {
                m_Name = value.Substring(0, Math.Min(value.Length, maxNameLength));
                DataChanged(this);
            }
        }

        public int trainingSteps
        {
            get => m_TrainingSteps;
            private set => m_TrainingSteps = value;
        }

        public string description
        {
            get
            {
               if(string.IsNullOrEmpty(m_Description))
                   return GetSelectedCheckPoint()?.description ?? "";
               return m_Description;
            }
            set
            {
                m_Description = value.Substring(0, Math.Min(value.Length, maxDescriptionLength));
                DataChanged(this);
            }
        }

        public bool visible
        {
            get => m_Visible;
            set
            {
                if (m_Visible != value)
                {
                    m_Visible = value;
                    DataChanged(this);
                }
            }
        }

        public ImageArtifact thumbnail => m_Thumbnail;

        public override void GetArtifact(Action<StyleData> onDoneCallback, bool useCache)
        {
            OnArtifactLoaded += onDoneCallback;
            if (state != EState.New)
            {
                if (state != EState.Loading)
                {
                    if (state != EState.Loaded && state != EState.Training)
                    {
                        state = EState.Loading;

                        LoadArtifact();
                    }
                    else
                    {
                        // ensure all checkpoints are loaded
                        for (var i = 0; i < m_CheckPoints?.Count; ++i)
                        {
                            m_CheckPoints[i].OnStateChanged += OnCheckPointStateChanged;
                            m_CheckPoints[i].GetArtifact(_ => { }, useCache);
                        }

                        if(m_CheckPoints?.Count > 0)
                            OnCheckPointStateChanged(null);
                        else
                            ArtifactLoaded(this);
                    }
                }
            }
            else
            {
                ArtifactLoaded(this);
            }
        }

        public IReadOnlyList<CheckPointData> checkPoints => m_CheckPoints;

        void LoadArtifact()
        {
            var getStyleRequest = new GetStyleRequest
            {
                style_guid = guid,
                guid = m_ProjectID
            };
            var getStyleRestCall = new GetStyleRestCall(ServerConfig.serverConfig, getStyleRequest);
            getStyleRestCall.RegisterOnSuccess(OnGetStyleSuccess);
            getStyleRestCall.RegisterOnFailure(OnGetStyleFailure);
            getStyleRestCall.SendRequest();
        }

        void OnGetStyleFailure(GetStyleRestCall obj)
        {
            if (obj.retriesFailed)
            {
                state = EState.Error;
                StyleTrainerDebug.Log($"Failed to load style {guid} {obj.errorMessage}");
                DataChanged(this);
                ArtifactLoaded(this);
            }
        }

        void ClearCheckPoints()
        {
            for (int i = 0; i < m_CheckPoints.Count; ++i)
            {
                m_CheckPoints[i]?.OnDispose();
            }
            m_CheckPoints.Clear();
        }

        void ClearTrainingData()
        {
            for(int i = 0; i < m_TrainingSetData.Count; ++i)
            {
                m_TrainingSetData[i]?.OnDispose();
            }
            m_TrainingSetData.Clear();
        }

        void OnGetStyleSuccess(GetStyleRestCall arg1, GetStyleResponse arg2)
        {
            if (arg2.success && !disposing)
            {
                var oldCheckPoints = m_CheckPoints.ToArray();
                ClearCheckPoints();
                title = arg2.name;
                description = arg2.desc;
                favoriteCheckPoint = SetStyleStateRestCall.activeState;
                visible = arg2.state == "active";
                //Load Prompts
                m_SampleOutputPrompts.Clear();
                for(int i = 0; i < arg2.prompts?.Length; ++i)
                {
                    m_SampleOutputPrompts.Add(arg2.prompts[i]);
                }

                ClearTrainingData();
                for (int i = 0; i < arg2.trainingsetIDs?.Length; ++i)
                {
                    int j = 0;
                    for (; j < m_TrainingSetData.Count; ++j)
                    {
                        if(m_TrainingSetData[j].guid == arg2.trainingsetIDs[i])
                            break;
                    }

                    if(j >= m_TrainingSetData.Count)
                        m_TrainingSetData.Add(new TrainingSetData(EState.Initial, arg2.trainingsetIDs[i], m_ProjectID));
                }

                if (arg2.checkpointIDs?.Length <= 0)
                {
                    StyleLoadSuccessNoCheckPoint(arg2.name, arg2.desc, arg2.prompts);
                }
                else
                {
                    for (var i = 0; i < arg2.checkpointIDs?.Length; ++i)
                    {
                        var checkPoint = new CheckPointData(EState.Initial, arg2.checkpointIDs[i], m_ProjectID);
                        var oldCheckPoint = Array.Find(oldCheckPoints, cp => cp.guid == checkPoint.guid);
                        if (oldCheckPoint != null)
                            checkPoint.trainingSteps = oldCheckPoint.trainingSteps;
                        m_CheckPoints.Add(checkPoint);
                        checkPoint.OnStateChanged += OnCheckPointStateChanged;

                        //checkPoint.GetArtifact(_ => { }, false);
                    }
                    for(int i = 0; i < m_CheckPoints.Count; ++i)
                    {
                        m_CheckPoints[i].GetArtifact(_ => { }, false);
                    }
                    // if(m_CheckPointStatusCheckTask == null)
                    //     m_CheckPointStatusCheckTask = new CheckPointStatusCheckTask(m_ProjectID);
                    // if(m_CheckPoints.Count > 0)
                    //     m_CheckPointStatusCheckTask.AddCheckPoint(m_CheckPoints, OnCheckPointStatusCheckDone);
                }
            }
            else
            {
                if (!disposing)
                    StyleTrainerDebug.LogWarning($"GetStyleResponse {guid} failed success={arg2.success} error={arg2.error}. Init to initial state");
                StyleLoadSuccessNoCheckPoint("New Style", "Set a description here to help you remember what this style is for.", null);
            }
        }

        void OnCheckPointStatusCheckDone(CheckPointData arg1, string arg2, bool arg3)
        {
            arg1.SetCheckPointStatus(arg2, arg3);
        }

        void StyleLoadSuccessNoCheckPoint(string checkpointName, string checkPointDescription, string[] prompts)
        {
            state = EState.New;
            ClearCheckPoints();
            ArtifactLoaded(this);
        }

        void OnCheckPointStateChanged(CheckPointData obj)
        {
            var hasTraining = false;
            bool hasLoading = false;
            for (var i = 0; i < m_CheckPoints.Count; ++i)
            {
                if (m_CheckPoints[i].state == EState.Initial || m_CheckPoints[i].state == EState.Loading)
                {
                    m_CheckPoints[i].OnStateChanged -= OnCheckPointStateChanged;
                    m_CheckPoints[i].OnStateChanged += OnCheckPointStateChanged;
                    m_CheckPoints[i].GetArtifact(_ => { }, false);
                    hasLoading = true;
                }
                else
                {
                    m_CheckPoints[i].OnStateChanged -= OnCheckPointStateChanged;
                    hasTraining |= m_CheckPoints[i].state == EState.Training;
                }
            }

            state = hasTraining ? EState.Training : EState.Loaded;
            if (!hasLoading)
            {
                ArtifactLoaded(this);
                DataChanged(this);
            }
        }


        public void AddCheckPoint(CheckPointData checkPoint)
        {
            checkPoint.versionName = $"Version {m_CheckPoints.Count + 1}";
            m_CheckPoints.Add(checkPoint);
            if (checkPoint.guid == Utilities.emptyGUID)
                checkPoint.OnGUIDChanged += OnCheckPointGUIDChanged;
            DataChanged(this);
        }

        void OnCheckPointGUIDChanged(CheckPointData obj)
        {
            if (selectedCheckPointGUID == Utilities.emptyGUID) selectedCheckPointGUID = obj.guid;

            obj.OnGUIDChanged -= OnCheckPointGUIDChanged;
        }

        void OnThumbnailChanged()
        {
            ArtifactLoaded(this);
            DataChanged(this);
        }

        public IReadOnlyList<TrainingSetData> trainingSetData
        {
            get
            {
                if (m_TrainingSetData.Count == 0)
                {
                    for (int i = 0; i < m_CheckPoints?.Count; ++i)
                    {
                        m_TrainingSetData.Add(m_CheckPoints[i].trainingSetData);
                    }
                    // still empty means no checkpoints data
                    if(m_TrainingSetData.Count == 0)
                        m_TrainingSetData.Add(new TrainingSetData(EState.New, Utilities.emptyGUID, m_ProjectID));
                }
                return m_TrainingSetData;
            }
        }

        public void AddSampleOutput(SampleOutputData sampleOutput)
        {
            m_SampleOutputPrompts.Add(sampleOutput.prompt);
            DataChanged(this);
        }

        public IReadOnlyList<string> sampleOutputPrompts
        {
            get
            {
                if (m_SampleOutputPrompts.Count == 0 && GetSelectedCheckPoint()?.validationImageData?.Count > 0)
                {
                    for (var i = 0; i < GetSelectedCheckPoint().validationImageData.Count; ++i)
                        m_SampleOutputPrompts.Add(GetSelectedCheckPoint().validationImageData[i].prompt);
                }

                return m_SampleOutputPrompts;
            }
        }
        public void AddTrainingData(TrainingData trainingData)
        {
            if(m_TrainingSetData.Count == 0)
            {
                m_TrainingSetData.Add(new TrainingSetData(EState.New, Utilities.emptyGUID, m_ProjectID));
            }

            m_TrainingSetData[0].Add(trainingData);
            DataChanged(this);
        }

        public void RemoveTrainingData(int i)
        {
            if (m_TrainingSetData.Count > 0 && m_TrainingSetData[0].Count > i)
            {
                var e = m_TrainingSetData[0][i];
                e.Delete();
                e.OnDispose();
                m_TrainingSetData[0].RemoveAt(i);
                DataChanged(this);
            }
        }

        public void RemoveTrainingData(IEnumerable<int> indices)
        {
            var descendingIndices = indices.OrderByDescending(x => x).ToArray();

            foreach (var i in descendingIndices)
            {
                if (m_TrainingSetData.Count > 0 && m_TrainingSetData[0].Count > i)
                {
                    var e = m_TrainingSetData[0][i];
                    e.Delete();
                    e.OnDispose();
                    m_TrainingSetData[0].RemoveAt(i);
                    DataChanged(this);
                }
            }
        }

        public void RemoveSampleOutputPrompt(int i)
        {
            m_SampleOutputPrompts.RemoveAt(i);
            DataChanged(this);
        }

        public void RemoveCheckPointAt(int index)
        {
            if (m_CheckPoints.Count > index && index >= 0)
            {
                var checkPoint = m_CheckPoints[index];
                checkPoint.Delete();
                m_CheckPoints.RemoveAt(index);
                if (selectedCheckPointGUID == checkPoint.guid) selectedCheckPointGUID = checkPoint.parent_id;
                checkPoint.OnDispose();
            }
        }

        public void Delete()
        {
            for (int i = 0; i < m_TrainingSetData?.Count; ++i)
            {
                m_TrainingSetData[i]?.Delete();
            }
            for (var i = 0; i < m_CheckPoints?.Count; ++i)
                m_CheckPoints[i]?.Delete();
        }

        public bool HasTraining()
        {
            if (state == EState.Training)
                return true;

            for (var i = 0; i < m_CheckPoints?.Count; ++i)
            {
                if (m_CheckPoints[i]?.state == EState.Training)
                    return true;
            }

            return false;
        }

        public string UpdateSamplePrompt(int i, string s)
        {
            if (m_SampleOutputPrompts.Count > i)
            {
                m_SampleOutputPrompts[i] = s.Substring(0, Math.Min(s.Length, maxPromptLength));
                DataChanged(this);
                return m_SampleOutputPrompts[i];
            }

            return "";
        }
    }
}
