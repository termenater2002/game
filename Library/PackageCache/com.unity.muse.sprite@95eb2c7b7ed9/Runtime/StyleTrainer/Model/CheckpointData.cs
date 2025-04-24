using System;
using System.Collections.Generic;
using System.Linq;
using StyleTrainer.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.StyleTrainer.Debug;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Muse.StyleTrainer
{
    [Serializable]
    class CheckPointData : Artifact<CheckPointData, CheckPointData>
    {
        [SerializeField]
        string m_ProjectID = Utilities.emptyGUID;
        [FormerlySerializedAs("name")]
        [SerializeField]
        string m_Name = StringConstants.newVersion;
        [FormerlySerializedAs("description")]
        [SerializeField]
        string m_Description = StringConstants.newVersion;
        public string parent_id = Utilities.emptyGUID;

        [FormerlySerializedAs("trainingSetData")]
        [SerializeField]
        TrainingSetData m_TrainingSetData;

        [FormerlySerializedAs("validationImagesData")]
        [SerializeField]
        List<SampleOutputData> m_ValidationImagesData = new();
        [SerializeField]
        string m_FavoriteSampleOutputDataGuid;

        [SerializeField]
        int m_TrainingSteps;
        GetCheckPointRestCall m_GetCheckPointRestCall;
        GetCheckPointStatusRestCall m_GetCheckPointStatusRestCall;

        public CheckPointData(EState state, string guid, string projectID)
            : base(state)
        {
            m_ProjectID = projectID;
            this.guid = guid;
            m_TrainingSetData = new TrainingSetData(state, Utilities.emptyGUID, projectID);
            m_TrainingSteps = 0;
        }

        public override void Init()
        {
            base.Init();
            if (state != EState.New && state != EState.Loaded)
                state = EState.Initial;
        }

        public void SetName(string newName)
        {
            m_Name = newName.Substring(0, Math.Min(newName.Length, StyleData.maxNameLength));
        }

        public void SetDescription(string newDesp)
        {
            m_Description = newDesp.Substring(0, Math.Min(newDesp.Length, StyleData.maxDescriptionLength));
        }

        public string name => m_Name;
        public string description => m_Description;

        public string versionName { get; set; }

        public IReadOnlyList<SampleOutputData> validationImageData => m_ValidationImagesData;

        public string favoriteSampleOutputDataGuid
        {
            get => m_FavoriteSampleOutputDataGuid;
            set => m_FavoriteSampleOutputDataGuid = value;
        }

        public SampleOutputData GetFavoriteSampleOutputData()
        {
            if(string.IsNullOrEmpty(favoriteSampleOutputDataGuid))
            {
                favoriteSampleOutputDataGuid = validationImageData[0].guid;
            }

            var favorite = validationImageData?.FirstOrDefault(x => x.guid == favoriteSampleOutputDataGuid);

            return favorite;
        }

        public TrainingSetData trainingSetData => m_TrainingSetData;

        public int trainingSteps
        {
            get => m_TrainingSteps;
            set => m_TrainingSteps = value;
        }

        public override void OnDispose()
        {
            m_GetCheckPointRestCall?.Dispose();
            m_GetCheckPointRestCall = null;
            m_GetCheckPointStatusRestCall?.Dispose();
            m_GetCheckPointStatusRestCall = null;
            m_TrainingSetData?.OnDispose();
            for (var i = 0; i < m_ValidationImagesData?.Count; ++i)
                m_ValidationImagesData[i]?.OnDispose();
            base.OnDispose();
        }

        public void SetMockData(TrainingSetData trainingSetData, List<SampleOutputData> data, int trainingSteps)
        {
            UnityEngine.Debug.LogWarning("THIS SHOULD BE ONLY CALLED FROM MOCK");
            m_TrainingSetData = trainingSetData;
            m_ValidationImagesData = data;
            m_TrainingSteps = trainingSteps;
        }

        public override void GetArtifact(Action<CheckPointData> onDoneCallback, bool useCache)
        {
            if (Utilities.ValidStringGUID(m_ProjectID) && Utilities.ValidStringGUID(guid))
            {
                OnArtifactLoaded += onDoneCallback;
                if (state == EState.Initial)
                {
                    state = EState.Loading;
                    if (m_GetCheckPointRestCall == null)
                    {
                        var checkPointRequest = new GetCheckPointRequest
                        {
                            checkpoint_guid = guid,
                            guid = m_ProjectID
                        };
                        m_GetCheckPointRestCall = new GetCheckPointRestCall(ServerConfig.serverConfig, checkPointRequest);
                        m_GetCheckPointRestCall.RegisterOnSuccess(OnGetCheckPointSuccess);
                        m_GetCheckPointRestCall.RegisterOnFailure(OnGetCheckPointFailure);
                    }

                    if (m_GetCheckPointStatusRestCall == null)
                    {
                        var request = new GetCheckPointStatusRequest
                        {
                            guid = m_ProjectID,
                            guids = new[] { guid }
                        };
                        m_GetCheckPointStatusRestCall = new GetCheckPointStatusRestCall(ServerConfig.serverConfig, request);
                        m_GetCheckPointStatusRestCall.RegisterOnSuccess(OnGetCheckPointStatusSuccess);
                        m_GetCheckPointStatusRestCall.RegisterOnFailure(OnGetCheckPointStatusFailure);
                    }

                    LoadCheckPoint();
                }
                else if (state != EState.Loading && state != EState.Training)
                {
                    ArtifactLoaded(this);
                }
            }
            else if (state != EState.New && state != EState.Training)
            {
                state = EState.Loaded;
                StyleTrainerDebug.Log($"Check point data incomplete. Unable to load. guid:{guid} asset_id:{m_ProjectID}");
                onDoneCallback?.Invoke(this);
            }
        }

        void LoadCheckPoint()
        {
            if (!disposing)
            {
                m_GetCheckPointRestCall?.SendRequest();
            }
        }

        void OnGetCheckPointFailure(GetCheckPointRestCall obj)
        {
            if (obj.retriesFailed)
            {
                StyleTrainerDebug.LogError($"OnGetCheckPointFailure: Failed to create style. {obj.requestError} {obj.errorMessage}");
                state = EState.Error;
                ArtifactLoaded(this);
            }
        }

        void OnGetCheckPointSuccess(GetCheckPointRestCall arg1, GetCheckPointResponse arg2)
        {
            if (arg2.success)
            {
                StyleTrainerDebug.Log($"Loading checkpoint status {arg2.status} {arg2.checkpointID}");
                ProcessGetCheckPointResponse(arg2);
            }
            else
            {
                StyleTrainerDebug.LogError($"OnGetCheckPointSuccess: Request call success but response failed. {arg2.success}");
                state = EState.Error;
                ArtifactLoaded(this);
            }
        }

        void ClearValidationImageData()
        {
            for (int i = 0; i < m_ValidationImagesData.Count; ++i)
            {
                m_ValidationImagesData[i].OnDispose();
            }
            m_ValidationImagesData.Clear();
        }

        void ProcessGetCheckPointResponse(GetCheckPointResponse getCheckPointResponse)
        {
            bool sendDataChanged = false;
            m_Name = getCheckPointResponse.name;
            m_Description = getCheckPointResponse.description;
            sendDataChanged = m_TrainingSteps != getCheckPointResponse.train_steps;
            m_TrainingSteps = getCheckPointResponse.train_steps;

            if (m_TrainingSetData == null)
            {
                m_TrainingSetData = new TrainingSetData(EState.Initial, getCheckPointResponse.trainingsetID, m_ProjectID);
            }
            else if (m_TrainingSetData.guid != getCheckPointResponse.trainingsetID)
            {
                m_TrainingSetData.guid = getCheckPointResponse.trainingsetID;
                m_TrainingSetData.state = EState.Initial;
            }

            if (m_ValidationImagesData.Count != getCheckPointResponse.validation_image_prompts.Length)
            {
                ClearValidationImageData();
                for (var i = 0; i < getCheckPointResponse.validation_image_prompts.Length; i++)
                    m_ValidationImagesData.Add(new SampleOutputData(EState.Initial, getCheckPointResponse.validation_image_prompts[i]));
                sendDataChanged = true;
            }

            if (getCheckPointResponse.status == GetCheckPointResponse.Status.done)
            {
                if (getCheckPointResponse.validation_image_guids?.Length != getCheckPointResponse.validation_image_prompts?.Length)
                {
                    StyleTrainerDebug.Log($"Waiting for validation image {getCheckPointResponse.validation_image_guids?.Length} {getCheckPointResponse.validation_image_prompts?.Length}");
                    ScheduleCallback(LoadCheckPoint);
                }
                else
                {
                    state = EState.Loaded;
                    StoreSampleOutput(getCheckPointResponse);
                    ArtifactLoaded(this);
                }
            }
            else if (getCheckPointResponse.status == GetCheckPointResponse.Status.working)
            {
                if (state != EState.Training)
                {
                    state = EState.Training;
                    sendDataChanged = true;
                }

                ScheduleCallback(GetCheckPointStatus);
            }
            else if (getCheckPointResponse.status == GetCheckPointResponse.Status.failed)
            {
                state = EState.Error;
                StyleTrainerDebug.LogError($"Checkpoint training failed: assetid:{getCheckPointResponse.asset_id} styleid:{getCheckPointResponse.styleID} checkpointid:{getCheckPointResponse.checkpointID} error:{getCheckPointResponse.error}");
                StoreSampleOutput(getCheckPointResponse);
                ArtifactLoaded(this);
            }
            if(sendDataChanged)
            {
                DataChanged(this);
            }
        }

        void GetCheckPointStatus()
        {
            if (!disposing)
                m_GetCheckPointStatusRestCall?.SendRequest();
        }

        void OnGetCheckPointStatusFailure(GetCheckPointStatusRestCall obj)
        {
            if (obj.retriesFailed)
            {
                StyleTrainerDebug.LogError($"OnGetCheckPointStatusFailure: Failed to get style status. {obj.requestError} {obj.errorMessage}");
                state = EState.Error;
                ArtifactLoaded(this);
            }
        }

        EState ConvertCheckPointServerStateResponse(string stateString)
        {
            switch (stateString)
            {
                case GetCheckPointResponse.Status.failed:
                    return EState.Error;
                case GetCheckPointResponse.Status.done:
                    return EState.Loading;
                case GetCheckPointResponse.Status.working:
                    return EState.Training;
            }

            return EState.Initial;
        }

        public bool UpdateCheckPointStatus(string status, bool requestSuccess)
        {
            if (requestSuccess)
            {
                var newState = ConvertCheckPointServerStateResponse(status);
                state = newState;
                StyleTrainerDebug.Log($"Loading checkpoint status {guid} {status}");
                if (status == GetCheckPointResponse.Status.working)
                    return true;
            }

            ScheduleCallback(LoadCheckPoint);
            return false;
        }

        void OnGetCheckPointStatusSuccess(GetCheckPointStatusRestCall arg1, GetCheckPointStatusResponse arg2)
        {
            if (arg2.success)
            {
                for (var i = 0; i < arg2.results.Length; ++i)
                {
                    if (arg2.results[i].guid == guid)
                    {
                        var newState = ConvertCheckPointServerStateResponse(arg2.results[i].status);
                        state = newState;

                        StyleTrainerDebug.Log($"Loading checkpoint status {arg2.results[i].guid} {arg2.results[i].status}");
                        if (arg2.results[i].status != "working")
                            ScheduleCallback(LoadCheckPoint);
                        else
                            ScheduleCallback(GetCheckPointStatus);
                        break;
                    }
                }
            }
            else
            {
                StyleTrainerDebug.LogError($"OnGetCheckPointStatusSuccess: Request call success but response failed. {arg2.error} {guid}");
                ScheduleCallback(LoadCheckPoint);
            }
        }

        void ScheduleCallback(Action callback)
        {
            if (!disposing)
                callback();

            //Scheduler.ScheduleCallback(k_GetCheckPointStatusRetryPollRate, callback);
        }

        void StoreSampleOutput(GetCheckPointResponse p0)
        {
            for (var i = 0; i < p0.validation_image_prompts.Length && i < p0.validation_image_guids.Length; ++i)
            {
                var serverPrompt = p0.validation_image_prompts[i];
                var serverGUID = p0.validation_image_guids[i];

                if (Utilities.ValidStringGUID(serverGUID))
                {
                    // check if this guid is already assigned
                    int j;
                    for (j = 0; j < m_ValidationImagesData.Count; ++j)
                        if (m_ValidationImagesData[j].guid == serverGUID)
                            break;

                    if (j >= m_ValidationImagesData.Count)
                    {
                        // not found. Assign to a prompt
                        for (j = 0; j < m_ValidationImagesData.Count; ++j)
                            if (!Utilities.ValidStringGUID(m_ValidationImagesData[j].guid) &&
                                m_ValidationImagesData[j].prompt == serverPrompt)
                            {
                                m_ValidationImagesData[j].guid = serverGUID;
                                break;
                            }

                        if (j >= m_ValidationImagesData.Count) StyleTrainerDebug.LogWarning($"Prompt on server not found in local data. prompt:{serverPrompt} guid:{serverGUID}");
                    }
                }
            }
        }

        public void DuplicateNew(Action<CheckPointData> onDuplicateDone)
        {
            m_TrainingSetData.DuplicateNew(x => DuplicateCheckPoint(x, onDuplicateDone));
        }

        void DuplicateCheckPoint(TrainingSetData trainingSetData, Action<CheckPointData> onDuplicateDone)
        {
            var checkPoint = new CheckPointData(EState.New, Utilities.emptyGUID, m_ProjectID)
            {
                m_Name = m_Name,
                m_Description = m_Description,
                parent_id = guid,
                m_TrainingSetData = trainingSetData,
                m_ValidationImagesData = DuplicateNewValidationData()
            };

            onDuplicateDone?.Invoke(checkPoint);
        }

        List<SampleOutputData> DuplicateNewValidationData()
        {
            var list = new List<SampleOutputData>();
            for (var i = 0; i < m_ValidationImagesData.Count; ++i) list.Add(m_ValidationImagesData[i].Duplicate());

            return list;
        }

        public void Delete()
        {
            m_TrainingSetData?.Delete();
            for (var i = 0; i < m_ValidationImagesData?.Count; ++i) m_ValidationImagesData[i]?.Delete();
        }

        public void SetCheckPointStatus(string status, bool arg3)
        {
            var newState = ConvertCheckPointServerStateResponse(status);
            state = arg3 ? newState : EState.Initial;

            StyleTrainerDebug.Log($"Loading checkpoint status {guid} {status}");
            if (status != GetCheckPointResponse.Status.working)
                ScheduleCallback(LoadCheckPoint);
        }
    }
}