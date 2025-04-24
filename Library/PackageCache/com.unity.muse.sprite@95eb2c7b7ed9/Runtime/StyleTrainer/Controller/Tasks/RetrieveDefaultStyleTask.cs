using System;
using StyleTrainer.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Debug;
using Unity.Muse.StyleTrainer.Events.StyleTrainerProjectEvents;

namespace Unity.Muse.StyleTrainer
{
    class RetrieveDefaultStyleTask
    {
        EventBus m_EventBus;
        StyleTrainerData m_DefaultProject;
        Action m_OnDone;
        public void Execute(StyleTrainerData mainProject, Action onDone)
        {
            m_DefaultProject = mainProject;
            m_OnDone = onDone;
            var getDefaultProjectRequest = new GetDefaultStyleProjectRequest();
            var getDefaultProjectRestCall = new GetDefaultStyleProjectRestCall(ServerConfig.serverConfig, getDefaultProjectRequest);
            getDefaultProjectRestCall.RegisterOnFailure(OnFailure);
            getDefaultProjectRestCall.RegisterOnSuccess(OnSuccess);
            getDefaultProjectRestCall.SendRequest();
        }

        void OnSuccess(GetDefaultStyleProjectRestCall arg1, GetDefaultStyleProjectResponse arg2)
        {
            if (arg2.success)
            {
                var projectId = arg2.guid;
                StyleTrainerDebug.Log($"Default style project id {projectId}");
                m_DefaultProject.guid = projectId;
                m_DefaultProject.state = EState.Initial;
                m_DefaultProject.GetArtifact(GetDefaultProjectDone, false);
            }
            else
            {
                m_DefaultProject.guid = Utilities.emptyGUID;
                m_DefaultProject.state = EState.Error;
                StyleTrainerDebug.LogWarning($"GetDefaultStyleProjectRestCall: call success but response is not success");
                m_OnDone.Invoke();
            }
        }

        void GetDefaultProjectDone(StyleTrainerData obj)
        {
            StyleTrainerDebug.Log("Default style project loaded");
            m_OnDone.Invoke();
        }

        void OnFailure(GetDefaultStyleProjectRestCall obj)
        {
            if (obj.retriesFailed)
            {
                m_DefaultProject.guid = Utilities.emptyGUID;
                m_DefaultProject.state = EState.Error;
                StyleTrainerDebug.Log($"Unable to load default style trainer project {obj.responseCode}:{obj.errorMessage}");
                m_OnDone.Invoke();
            }
        }
    }
}
