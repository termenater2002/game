using System.Linq;
using StyleTrainer.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Debug;
using Unity.Muse.StyleTrainer.Events.SampleOutputModelEvents;
using Unity.Muse.StyleTrainer.Events.StyleTrainerProjectEvents;
using Unity.Muse.StyleTrainer.Events.TrainingControllerEvents;
using Unity.Muse.StyleTrainer.Events.TrainingSetModelEvents;
using UnityEngine;

namespace Unity.Muse.StyleTrainer
{
    class LoadStyleProjectEvent : BaseEvent<LoadStyleProjectEvent> { }

    class LoadingTask
    {
        StyleTrainerData m_Project;
        EventBus m_EventBus;
        EState m_State;
        public LoadingTask(StyleTrainerData styleData, EventBus eventBus)
        {
            m_State = EState.Initial;
            m_Project = styleData;
            m_EventBus = eventBus;
        }

        public void Execute()
        {
            if (m_Project == null || !Utilities.ValidStringGUID(m_Project.guid))
            {
                StyleTrainerDebug.LogWarning("Unable to load project. Missing Project GUID");
                return;
            }

            if (m_State != EState.Loading)
            {
                m_State = EState.Loading;
                m_Project.GetArtifact(OnLoadStyleProjectDone, false);
            }
        }

        void OnLoadStyleProjectDone(StyleTrainerData project)
        {
            m_State = EState.Loaded;
            m_EventBus.SendEvent(new SystemEvents
            {
                state = SystemEvents.ESystemState.RequestSave
            });

            m_EventBus.SendEvent(new StyleModelSourceChangedEvent
            {
                styleModels = m_Project.styles
            }, true);

            for (int i = 0; i < project.styles?.Count; ++i)
            {
                var obj = project.styles[i];
                if (obj.state == EState.Loaded)
                {
                    m_EventBus.SendEvent(new TrainingSetDataSourceChangedEvent
                    {
                        styleData = obj,
                        trainingSetData = obj.trainingSetData
                    });
                    m_EventBus.SendEvent(new SampleOutputDataSourceChangedEvent
                    {
                        styleData = obj,
                        sampleOutput = obj.sampleOutputPrompts
                    });
                }
            }
        }
    }
}