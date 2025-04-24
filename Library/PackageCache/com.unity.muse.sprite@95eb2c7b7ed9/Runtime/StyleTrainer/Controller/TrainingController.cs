using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Debug;
using Unity.Muse.StyleTrainer.Events.CheckPointModelEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelEvents;
using Unity.Muse.StyleTrainer.Events.StyleTrainerMainUIEvents;
using Unity.Muse.StyleTrainer.Events.TrainingControllerEvents;
using UnityEngine;

namespace Unity.Muse.StyleTrainer
{
    class TrainingController: IDisposable
    {
        readonly EventBus m_EventBus;
        StyleData m_StyleData;
        StyleTrainerData m_ProjectData;
        string m_ContextGUID;

        public TrainingController(EventBus eventBus)
        {
            m_EventBus = eventBus;
            m_EventBus.RegisterEvent<GenerateButtonClickEvent>(OnGenerateClicked);
            m_EventBus.RegisterEvent<StyleTrainingEvent>(OnStyleTrainingEvent);
        }

        void OnStyleTrainingEvent(StyleTrainingEvent arg0)
        {
            if (arg0.state == EState.Error)
            {
                switch (arg0.trainingState)
                {
                    case ETrainingState.CreateTrainingSet:
                    case ETrainingState.CreateStyle:
                    case ETrainingState.CreateCheckPoint:
                        if (Utilities.ValidStringGUID(arg0.styleData.guid))
                            arg0.styleData.state = EState.Initial;
                        else
                            arg0.styleData.state = EState.New;
                        break;
                }
            }
        }

        void OnGenerateClicked(GenerateButtonClickEvent arg0)
        {
            if (m_StyleData.state == EState.New)
            {
                GenerateStyle();
            }
            else
            {
                StyleTrainerDebug.Log($"Generate button clicked when not in new state. {m_StyleData.state}");
            }
        }

        void GenerateStyle()
        {
            // validate if there is already training going on. If so, we don't allow training.
            if (m_ProjectData.HasTraining())
            {
                var showDialogEvent = new ShowDialogEvent
                {
                    title = "Error",
                    description = "Only 1 style can be trained at a time. Please wait for the current training to finish.",
                    semantic = AlertSemantic.Error
                };
                m_EventBus.SendEvent(showDialogEvent);
                return;
            }
            var validateTask = new ValidateTrainingParametersTask(m_StyleData, m_EventBus);

            // Validate if can train
            m_EventBus.SendEvent(new ShowLoadingScreenEvent
            {
                description = "Validating Training Parameters...",
                show = true
            });
            validateTask.Execute(canTrain =>
            {
                m_EventBus.SendEvent(new ShowLoadingScreenEvent
                {
                    show = false
                });
                if (!canTrain)
                {
                    m_EventBus.SendEvent(new StyleTrainingEvent
                    {
                        state = EState.Error,
                        styleData = m_StyleData,
                        trainingState = ETrainingState.Validation
                    });
                    return;
                }

                StyleTrainerDebug.Log("Generate Style");
                m_EventBus.SendEvent(new GenerateButtonStateUpdateEvent
                {
                    state = false
                });

                m_EventBus.SendEvent(new DuplicateButtonStateUpdateEvent
                {
                    state = false
                });

                var trainingTask = new TrainingTask(m_StyleData, m_ContextGUID, m_EventBus);
                trainingTask.Execute();
                m_EventBus.SendEvent(new RequestChangeTabEvent { tabIndex = StyleModelInfoEditor.sampleOutputTab });
            });
        }

        public void SetStyleData(StyleTrainerData projectData, StyleData styleData)
        {
            m_ContextGUID = projectData?.guid;
            m_StyleData = styleData;
            m_ProjectData = projectData;
        }

        public void Dispose()
        {
            m_EventBus.UnregisterEvent<GenerateButtonClickEvent>(OnGenerateClicked);
        }
    }
}