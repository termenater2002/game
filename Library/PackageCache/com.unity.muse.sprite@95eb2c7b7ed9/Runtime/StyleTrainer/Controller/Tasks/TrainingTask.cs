using System.Linq;
using StyleTrainer.Backend;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.StyleTrainer.Debug;
using Unity.Muse.StyleTrainer.Events.CheckPointModelEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelEvents;
using Unity.Muse.StyleTrainer.Events.StyleTrainerMainUIEvents;
using Unity.Muse.StyleTrainer.Events.TrainingControllerEvents;
using UnityEngine;

namespace Unity.Muse.StyleTrainer
{
    class TrainingTask
    {
        readonly StyleData m_StyleData;
        readonly string m_ProjectID;
        readonly EventBus m_EventBus;
        ETrainingState m_TrainingState;

        public TrainingTask(StyleData styleData, string projectID, EventBus eventBus)
        {
            m_StyleData = styleData;
            m_ProjectID = projectID;
            m_EventBus = eventBus;
        }

        public void Execute()
        {
            m_TrainingState = ETrainingState.CreateStyle;
            m_StyleData.state = EState.Training;
            m_EventBus.SendEvent(new StyleTrainingEvent
            {
                state = EState.Training,
                styleData = m_StyleData,
                trainingState = m_TrainingState
            });
            if (!Utilities.ValidStringGUID(m_StyleData.guid))
            {
                // style not created. create style.
                var request = new CreateStyleRequest
                {
                    name = m_StyleData.title,
                    desc = m_StyleData.description,
                    asset_id = m_ProjectID,
                    prompts = m_StyleData.sampleOutputPrompts.ToArray(),
                    parent_id = m_StyleData.parentID
                };
                var createStyle = new CreateStyleRestCall(ServerConfig.serverConfig, request);
                createStyle.RegisterOnSuccess(OnCreateStyleSuccess);
                createStyle.RegisterOnFailure(OnCreateStyleFailure);
                createStyle.SendRequest();
            }
            else
            {
                if (!Utilities.ValidStringGUID(m_StyleData.trainingSetData[0].guid))
                    CreateTrainingSet();
                else
                    CreateCheckPoint();
            }
        }

        void OnCreateStyleSuccess(CreateStyleRestCall arg1, CreateStyleResponse arg2)
        {
            if (arg2.success)
            {
                m_StyleData.guid = arg2.guid;

                // Style created. Now create training set.
                CreateTrainingSet();
            }
            else
            {
                StyleTrainerDebug.LogError($"CreateStyleRestCall: Request call success but response failed. {arg2.success}");
                CreateStyleFailed(arg2.error);
            }
        }

        void CreateStyleFailed(string failReason)
        {

            m_EventBus.SendEvent(new ShowDialogEvent
            {
                title = "Training Failed",
                description = $"Failed to create new style. Please try again. {failReason}",
                semantic = AlertSemantic.Error
            });

            //check point create fail. So we just reset it back so that it can be trained again
            m_StyleData.state = EState.New;
            m_EventBus.SendEvent(new StyleTrainingEvent
            {
                state = EState.Error,
                styleData = m_StyleData,
                trainingState = m_TrainingState
            });
            m_EventBus.SendEvent(new GenerateButtonStateUpdateEvent
            {
                state = true
            });
        }

        void OnCreateStyleFailure(CreateStyleRestCall obj)
        {
            if (obj.retriesFailed)
            {
                StyleTrainerDebug.LogError($"CreateStyleRestCall: Failed to create style. {obj.requestError} {obj.errorMessage}");
                CreateStyleFailed(obj.requestError);
            }
        }

        void CreateTrainingSetAfterImageLoaded(ImageArtifact artifact, string[] imageArtifacts, int index)
        {
            StyleTrainerDebug.Log("Creating training set artifact image loaded.");
            imageArtifacts[index] = artifact.Base64ImageNoAlpha();
            for (var i = 0; i < imageArtifacts.Length; ++i)
                if (!Utilities.ValidStringGUID(imageArtifacts[i]))
                    return;
            m_TrainingState = ETrainingState.CreateTrainingSet;
            m_EventBus.SendEvent(new StyleTrainingEvent
            {
                state = EState.Training,
                styleData = m_StyleData,
                trainingState = m_TrainingState
            });
            var createTrainingSetRequest = new CreateTrainingSetRequest
            {
                guid = m_StyleData.guid,
                asset_id = m_ProjectID,
                images = imageArtifacts
            };
            var createTrainingSet = new CreateTrainingSetRestCall(ServerConfig.serverConfig, createTrainingSetRequest);
            createTrainingSet.RegisterOnSuccess(OnCreateTrainingSetSuccess);
            createTrainingSet.RegisterOnFailure(OnCreateTrainingSetFailure);
            createTrainingSet.SendRequest();
        }

        void CreateTrainingSet()
        {
            StyleTrainerDebug.Log("Creating training set.");

            var imageArtifacts = new string[m_StyleData.trainingSetData[0].Count];
            for (var i = 0; i < imageArtifacts.Length; ++i)
            {
                var imageArtifact = m_StyleData.trainingSetData[0][i].imageArtifact;
                var index = i;
                m_StyleData.trainingSetData[0][i].imageArtifact.GetArtifact(_ =>
                    CreateTrainingSetAfterImageLoaded(imageArtifact, imageArtifacts, index), true);
            }
        }

        void OnCreateTrainingSetFailure(CreateTrainingSetRestCall obj)
        {
            if (obj.retriesFailed)
            {
                StyleTrainerDebug.LogError($"OnCreateTrainingSetFailure: Failed to create style. {obj.requestError} {obj.errorMessage}");
                ReportTrainingSetError(obj.requestError);
            }
        }

        void ReportTrainingSetError(string error)
        {
            m_EventBus.SendEvent(new ShowDialogEvent
            {
                title = "Training Failed",
                description = $"Failed to create training set. Please try again.\nError:{error}",
                semantic = AlertSemantic.Error
            });
            m_EventBus.SendEvent(new StyleTrainingEvent
            {
                state = EState.Error,
                styleData = m_StyleData,
                trainingState = m_TrainingState
            });
            m_EventBus.SendEvent(new GenerateButtonStateUpdateEvent
            {
                state = true
            });
        }

        void OnCreateTrainingSetSuccess(CreateTrainingSetRestCall arg1, CreateTrainingSetResponse arg2)
        {
            if (arg2.success)
            {
                m_StyleData.trainingSetData[0].guid = arg2.guid;
                // Marking this as loaded since it comes from training.
                m_StyleData.trainingSetData[0].state = EState.Initial;
                CreateCheckPoint();
            }
            else
            {
                StyleTrainerDebug.LogError($"OnCreateTrainingSetSuccess: Request call success but response failed. {arg2.success} {arg2.error}");
                ReportTrainingSetError(arg2.error);
            }
        }

        void CreateCheckPoint()
        {
            StyleTrainerDebug.Log("Creating checkpoint.");
            m_TrainingState = ETrainingState.CreateCheckPoint;
            m_EventBus.SendEvent(new StyleTrainingEvent
            {
                state = EState.Training,
                styleData = m_StyleData,
                trainingState = m_TrainingState
            });

            // Training set created. Now train style.
            var createCheckPointRequest = new CreateCheckPointV2Request
            {
                guid = m_StyleData.guid,
                asset_id = m_ProjectID,
                training_guid = m_StyleData.trainingSetData[0].guid,
                name = m_StyleData.title,
                description = m_StyleData.description,
                resume_guid = Utilities.emptyGUID,
                training_steps =  StyleTrainerConfig.config.trainingSteps
            };

            var createCheckPoint = new CreateCheckPointV2RestCall(ServerConfig.serverConfig, createCheckPointRequest);
            createCheckPoint.RegisterOnSuccess(OnCreateCheckPointSuccess);
            createCheckPoint.RegisterOnFailure(OnCreateCheckPointFailure);
            createCheckPoint.SendRequest();
        }

        void OnCreateCheckPointFailure(CreateCheckPointV2RestCall obj)
        {
            StyleTrainerDebug.LogError($"OnCreateCheckPointFailure: Failed to create style. {obj.requestError}");
            if (obj.restCallState == QuarkRestCall.EState.Error)
            {
                m_EventBus.SendEvent(new ShowDialogEvent
                {
                    title = "Training Failed",
                    description = $"Failed to create version. Please try again. {obj.requestError}",
                    semantic = AlertSemantic.Error
                });
                m_StyleData.state = EState.Initial;
                m_EventBus.SendEvent(new StyleTrainingEvent
                {
                    state = EState.Error,
                    styleData = m_StyleData,
                    trainingState = m_TrainingState
                });
                m_EventBus.SendEvent(new GenerateButtonStateUpdateEvent
                {
                    state = true
                });
            }
        }

        void OnCreateCheckPointSuccess(CreateCheckPointV2RestCall arg1, CreateCheckPointV2Response arg2)
        {
            if (arg2.success)
            {
                m_TrainingState = ETrainingState.CheckPointTraining;
                m_EventBus.SendEvent(new StyleTrainingEvent
                {
                    state = EState.Training,
                    styleData = m_StyleData,
                    trainingState = m_TrainingState
                });
                StyleTrainerDebug.Log("Loading checkpoint");

                for (int i = 0; i < arg2.guids.Length; ++i)
                {
                    var checkPoint = new CheckPointData(EState.Initial, arg2.guids[i], m_StyleData.projectID);
                    m_StyleData.selectedCheckPointGUID = arg2.guids[i];
                    m_StyleData.AddCheckPoint(checkPoint);
                    StyleTrainerDebug.Log($"CheckPointLoaded: {checkPoint.guid}");
                    //checkPoint.OnDataChanged += OnCheckPointDataChange;
                    checkPoint.GetArtifact(CheckPointLoaded, false);
                }

                m_EventBus.SendEvent(new CheckPointSourceDataChangedEvent
                {
                    styleData = m_StyleData
                });
            }
            else
            {
                StyleTrainerDebug.LogError($"OnCreateCheckPointSuccess: Request call success but response failed. {arg2.success} {arg2.error}");
            }
        }

        // void OnCheckPointDataChange(CheckPointData obj)
        // {
        //     if (obj.state == EState.Loaded || obj.state == EState.Error)
        //         obj.OnDataChanged -= OnCheckPointDataChange;
        //
        //     m_EventBus.SendEvent(new CheckPointDataChangedEvent
        //     {
        //         styleData =  m_StyleData,
        //         checkPointData = obj
        //     });
        // }

        void CheckPointLoaded(CheckPointData obj)
        {
            int i = 0;
            for (; i < m_StyleData.checkPoints.Count; ++i)
            {
                if(m_StyleData.checkPoints[i].state == EState.Loading || m_StyleData.checkPoints[i].state == EState.Training)
                    break;
                if (m_StyleData.checkPoints[i].state == EState.Initial)
                {
                    m_StyleData.checkPoints[i].GetArtifact(CheckPointLoaded, false);
                    break;
                }
            }

            if (i >= m_StyleData.checkPoints.Count)
            {
                m_StyleData.state = EState.Loaded;
                m_TrainingState = ETrainingState.TrainingDone;
                StyleTrainerDebug.Log($"CheckPointLoaded for style: {m_StyleData.guid}");
                // m_EventBus.SendEvent(new CheckPointSourceDataChangedEvent
                // {
                //     styleData = m_StyleData
                // });
                // m_EventBus.SendEvent(new GenerateButtonStateUpdateEvent
                // {
                //     state = true
                // });
                //
                // m_EventBus.SendEvent(new DuplicateButtonStateUpdateEvent
                // {
                //     state = m_StyleData.checkPoints?.Count > 0
                // });

                m_EventBus.SendEvent(new StyleTrainingEvent
                {
                    state = EState.Loaded,
                    styleData = m_StyleData,
                    trainingState = m_TrainingState
                });
            }

            m_EventBus.SendEvent(new SystemEvents
            {
                state = SystemEvents.ESystemState.RequestSave
            });
        }
    }
}