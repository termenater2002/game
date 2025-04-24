using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.Sprite.Artifacts;
using Unity.Muse.Sprite.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.Sprite.Data;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;
using TextField = Unity.Muse.AppUI.UI.TextField;

namespace Unity.Muse.Sprite.Operators
{
    [Preserve]
    [Serializable]
    class SessionOperator : IOperator
    {
        public string OperatorName => "Unity.Muse.Sprite.Operators.SessionOperator";
        /// <summary>
        /// Human-readable label for the operator.
        /// </summary>
        public string Label => "Session";

        [SerializeField]
        OperatorData m_OperatorData;
        public bool Enabled()
        {
            return (ServerConfig.serverConfig.debugMode & ServerConfig.EDebugMode.SessionDebug) > 0 || m_OperatorData.enabled;
        }

        public SessionOperator()
        {
            m_OperatorData = new OperatorData(OperatorName, "Unity.Muse.Sprite","0.0.1",  new []
                {string.Empty}, false);
        }

        public void Enable(bool enable)
        {
            m_OperatorData.enabled = enable;
        }

        public bool Hidden { get; set; }

        public VisualElement GetCanvasView()
        {
            Debug.Log("SessionOperator.GetCanvasView()");
            return new VisualElement();
        }

        TextField m_SessionText;
        Model m_Model;

        List<string> m_Jobs = new List<string>();
        Dictionary<string, Artifact> m_JobIdMapping = new Dictionary<string, Artifact>();
        public VisualElement GetOperatorView(Model model)
        {
            m_Model = model;
            var sessionData = m_Model.GetData<SessionData>();
            m_OperatorData.settings[0] = sessionData.activeSession;
            var m_UI = new ExVisualElement
            {
                passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows | ExVisualElement.Passes.BackgroundColor
            };
            m_UI.AddToClassList("muse-node");
            m_UI.name = "prompt-node";

            var text = new Text();
            text.text = Label;
            text.AddToClassList("muse-node__title");
            text.AddToClassList("bottom-gap");
            m_UI.Add(text);

            m_SessionText = new TextField(m_OperatorData.settings[0]);
            m_SessionText.AddToClassList("bottom-gap");
            m_UI.Add(m_SessionText);

            var sessionButton = new Button();
            sessionButton.name = "generate-button";
            sessionButton.title = "Load Session";
            sessionButton.AddToClassList("muse-theme");
            sessionButton.AddToClassList("muse-node__button");
            sessionButton.variant = ButtonVariant.Accent;
            sessionButton.clicked += SetSession;
            m_UI.Add(sessionButton);
            if(!string.IsNullOrEmpty(m_OperatorData.settings[0]))
                LoadSession();
            return m_UI;
        }

        void SetSession()
        {
            m_OperatorData.settings[0] = m_SessionText.value;
            //m_OperatorData.enabled = false;
            var sessionData = m_Model.GetData<SessionData>();
            sessionData.activeSession = m_SessionText.value;
            m_Model.UpdateOperators(this);
            LoadSession();
        }
        static ServerConfig serverConfig => ServerConfig.serverConfig;

        void LoadSession()
        {
            if (m_Model != null)
            {
                var sessionData = m_Model.GetData<SessionData>();
                if (sessionData.activeSession == sessionData.sessionLoaded)
                    return;
                sessionData.sessionLoaded = sessionData.activeSession;
                var assets = m_Model.AssetsData?.Where(x => x is SpriteMuseArtifact).ToArray();
                if (assets != null)
                {
                    foreach(var s in assets)
                        m_Model.RemoveAssets(s);
                }

                m_JobIdMapping.Clear();
                m_Jobs = new List<string>();
                var getSpriteGeneratorJobs = new GetSpriteGeneratorJobListRestCall(serverConfig, new ServerRequest<EmptyPayload>(), GetSessionID());
                getSpriteGeneratorJobs.RegisterOnSuccess(GetSpriteGeneratorJobListSuccess);
                getSpriteGeneratorJobs.RegisterOnFailure(GetSpriteGeneratorJobListFailed);
                getSpriteGeneratorJobs.SendRequest();

                var getSpriteRefinerJobs = new GetSpriteRefinerJobListRestCall(serverConfig, new ServerRequest<EmptyPayload>(), GetSessionID());
                getSpriteRefinerJobs.RegisterOnSuccess(GetSpriteRefinerJobListSuccess);
                getSpriteRefinerJobs.RegisterOnFailure(GetSpriteRefinerJobListFailed);
                getSpriteRefinerJobs.SendRequest();
            }
        }

        void GetSpriteRefinerJobListFailed(GetSpriteRefinerJobListRestCall obj)
        {
            Debug.Log($"GetSpriteRefinerJobListFailed: {GetSessionID()}");
        }

        void GetSpriteRefinerJobListSuccess(GetSpriteRefinerJobListRestCall arg1, JobListResponse arg2)
        {
            m_Jobs.AddRange(arg2.jobIDs);

            foreach (var jobID in arg2.jobIDs)
            {
                var request = new ServerRequest<JobInfoRequest>();
                request.guid = jobID;
                request.data = new JobInfoRequest() { jobID = jobID, assetID = GetSessionID() };
                var getJob = new GetJobRestCall(serverConfig, request);
                getJob.RegisterOnSuccess(OnGetSpriteRefineJobSuccess);
                getJob.RegisterOnFailure(OnGetJobFailed);
                getJob.SendRequest();
            }
        }

        void GetSpriteGeneratorJobListFailed(GetSpriteGeneratorJobListRestCall obj)
        {
            Debug.Log($"GetSpriteGeneratorJobListFailed: {GetSessionID()}");
        }

        void GetSpriteGeneratorJobListSuccess(GetSpriteGeneratorJobListRestCall arg1, JobListResponse arg2)
        {
            m_Jobs.AddRange(arg2.jobIDs);

            foreach (var jobID in arg2.jobIDs)
            {
                var request = new ServerRequest<JobInfoRequest>();
                request.guid = jobID;
                request.data = new JobInfoRequest() { jobID = jobID, assetID = GetSessionID() };
                request.access_token = serverConfig.accessToken;
                var getJob = new GetJobRestCall(serverConfig, request);
                getJob.RegisterOnSuccess(OnGetSpriteGeneratorJobSuccess);
                getJob.RegisterOnFailure(OnGetJobFailed);
                getJob.SendRequest();
            }
        }

        void OnGetJobFailed(GetJobRestCall obj)
        {
            Debug.Log($"Failed to get job {obj.jobID}");
        }

        SpriteMuseArtifact LoadFromJobInfo(GetJobRestCall arg1, JobInfoResponse jir)
        {
            var artifact = new SpriteMuseArtifact();
            artifact.Guid = jir.guid;
            artifact.Seed = (uint)jir.request.settings.seed;
            m_JobIdMapping[arg1.jobID] = artifact;

            artifact.SetOperators(ModesFactory.GetMode(UIMode.UIMode.modeKey));
            var promptOperator = artifact.GetOperator<PromptOperator>();
            if (promptOperator != null)
            {
                var opData = promptOperator.GetOperatorData();
                opData.settings[0] = jir.request.prompt;
                opData.settings[1] = jir.request.settings.negative_prompt;
                promptOperator.SetOperatorData(opData);
            }

            var sgso = artifact.GetOperator<SpriteGeneratorSettingsOperator>();
            sgso.InitFromJobInfo(jir);

            return artifact;
        }

        void OnGetSpriteRefineJobSuccess(GetJobRestCall arg1, JobInfoResponse arg2)
        {
            var artifact = LoadFromJobInfo(arg1, arg2);
            var srmo = artifact.GetOperator<SpriteRefiningMaskOperator>();
            srmo.InitFromJobInfo(arg2);
            TryAddArtifacts();
        }

        void OnGetSpriteGeneratorJobSuccess(GetJobRestCall arg1, JobInfoResponse arg2)
        {
            var artifact = LoadFromJobInfo(arg1, arg2);
            var kio = artifact.GetOperator<KeyImageOperator>();
            kio.InitFromJobInfo(arg2);
            TryAddArtifacts();
        }

        void TryAddArtifacts()
        {
            if(m_Jobs.Count != m_JobIdMapping.Count)
                return;

            foreach (var job in m_Jobs)
            {
                var artifact = m_JobIdMapping[job];
                m_Model.AddAsset(artifact);
            }
        }

        public OperatorData GetOperatorData()
        {
            return m_OperatorData;
        }

        public void SetOperatorData(OperatorData data)
        {
            m_OperatorData.enabled = data.enabled;
            if (data.settings == null || data.settings.Length != m_OperatorData.settings.Length)
                return;
            m_OperatorData.settings = data.settings;
        }

        public IOperator Clone()
        {
            var result = new SessionOperator();
            var operatorData = new OperatorData();
            operatorData.FromJson(GetOperatorData().ToJson());
            result.SetOperatorData(operatorData);
            return result;
        }

        public void RegisterToEvents(Model model)
        {
            m_Model = model;

            m_OperatorData.settings[0] = m_Model.GetData<SessionData>().activeSession;
        }

        public void UnregisterFromEvents(Model model)
        {
            if(m_Model == model)
                m_Model = null;
        }

        public bool IsSavable()
        {
            return true;
        }

        public string GetSessionID()
        {
            Debug.Assert(!string.IsNullOrEmpty(m_OperatorData.settings[0]));

            return m_OperatorData.settings[0];
        }

        /// <summary>
        /// Get the settings view for this operator.
        /// </summary>
        /// <param name="model">Current Model</param>
        /// <param name="isCustomSection">This VisualElement will override the whole operator section used by the generation settings</param>
        /// <param name="dismissAction">Action to trigger on dismiss</param>
        /// <returns> UI for the operator. Set to Null if the operator should not be displayed in the settings view. Disable the returned VisualElement if you want it to be displayed but not usable.</returns>
        public VisualElement GetSettingsView(Model model, ref bool isCustomSection, Action dismissAction)
        {
            return null;
        }
    }
}