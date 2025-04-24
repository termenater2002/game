using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.StyleModelEvents;
using Unity.Muse.StyleTrainer.Events.StyleTrainerMainUIEvents;

namespace Unity.Muse.StyleTrainer
{
    class ValidateTrainingParametersTask
    {
        readonly StyleData m_StyleData;
        readonly EventBus m_EventBus;
        Action<bool> m_OnDoneCallback;
        int m_TrainingImagesLoaded = 0;

        public ValidateTrainingParametersTask(StyleData styleData, EventBus eventBus)
        {
            m_StyleData = styleData;
            m_EventBus = eventBus;
        }

        public void Execute(Action<bool> onDoneCallback)
        {
            m_OnDoneCallback = onDoneCallback;
            var showDialogEvent = new ShowDialogEvent
            {
                title = "Error",
                description = "Cannot generate style",
                semantic = AlertSemantic.Error
            };

            // Validate style name and description
            if (!HasValidNameAndDescription(showDialogEvent, true)) return;

            // Validate sample output
            if (IsPracticePromptsCountBelowMinSize(showDialogEvent, true)) return;
            if (IsPracticePromptsCountExceedingMaxSize(showDialogEvent, true)) return;

            // Validate training set
            if (IsTrainingSetImagesCountBelowMinSize(showDialogEvent, true)) return;
            if (IsTrainingSetImagesCountExceedingMaxSize(showDialogEvent, true)) return;

            if (HasEmptyPrompts(showDialogEvent, true)) return;
            if (HasDuplicatePrompts(showDialogEvent, true)) return;

            //validate training are all unique
            m_TrainingImagesLoaded = 0;
            for (var i = 0; i < m_StyleData.trainingSetData[0]?.Count; ++i) m_StyleData.trainingSetData[0][i].imageArtifact.GetArtifact(_ => ValidateTrainingSetImages(), true);
        }

        internal bool HasEmptyPrompts(ShowDialogEvent showDialogEvent, bool showDialog)
        {
            // check if any of the samples are empty
            for (var i = 0; i < m_StyleData.sampleOutputPrompts?.Count; ++i)
            {
                var prompt1 = m_StyleData.sampleOutputPrompts[i];
                if (string.IsNullOrWhiteSpace(prompt1))
                {
                    if (showDialog)
                    {
                        showDialogEvent.description = $"Practice prompts cannot have empty prompts.";
                        showDialogEvent.confirmAction = () =>
                        {
                            m_EventBus.SendEvent(new RequestChangeTabEvent { tabIndex = StyleModelInfoEditor.sampleOutputTab });
                        };
                        m_EventBus.SendEvent(showDialogEvent);
                        m_OnDoneCallback.Invoke(false);
                    }
                    return true;
                }
            }

            return false;
        }

        internal bool HasDuplicatePrompts(ShowDialogEvent showDialogEvent, bool showDialog)
        {
            var duplicatedItem = new List<int>();

            for (var i = 0; i < m_StyleData.sampleOutputPrompts?.Count; ++i)
            {
                var prompt1 = m_StyleData.sampleOutputPrompts[i];

                duplicatedItem.Clear();
                for (var j = i + 1; j < m_StyleData.sampleOutputPrompts?.Count; ++j)
                    if (prompt1 == m_StyleData.sampleOutputPrompts[j])
                        duplicatedItem.Add(j);

                if (duplicatedItem.Count > 0)
                {
                    duplicatedItem.Add(i);
                    if (showDialog)
                    {
                        showDialogEvent.description = "One of the practice prompts is a duplicate. Please review your practice prompts.";
                        showDialogEvent.confirmAction = () =>
                        {
                            m_EventBus.SendEvent(new RequestChangeTabEvent
                            {
                                tabIndex = StyleModelInfoEditor.sampleOutputTab,
                                highlightIndices = duplicatedItem.AsReadOnly()
                            });
                        };
                        m_EventBus.SendEvent(showDialogEvent);
                        m_OnDoneCallback.Invoke(false);
                    }

                    return true;
                }
            }

            return false;
        }

        bool IsPracticePromptsCountBelowMinSize(ShowDialogEvent showDialogEvent, bool showDialog)
        {
            var config = StyleTrainerConfig.config;

            if (m_StyleData.sampleOutputPrompts?.Count < config.minSampleSetSize)
            {
                if (showDialog)
                {
                    showDialogEvent.description = $"Practice prompts must have at least {config.minSampleSetSize} prompts";
                    showDialogEvent.confirmAction = () =>
                    {
                        m_EventBus.SendEvent(new RequestChangeTabEvent { tabIndex = StyleModelInfoEditor.sampleOutputTab });
                    };
                    m_EventBus.SendEvent(showDialogEvent);
                    m_OnDoneCallback.Invoke(false);
                }

                return true;
            }

            return false;
        }

        bool IsPracticePromptsCountExceedingMaxSize(ShowDialogEvent showDialogEvent, bool showDialog)
        {
            var config = StyleTrainerConfig.config;

            if (m_StyleData.sampleOutputPrompts?.Count > config.maxSampleSetSize)
            {
                if (showDialog)
                {
                    showDialogEvent.description = $"Practice prompts must have at most {config.maxSampleSetSize} prompts";
                    showDialogEvent.confirmAction = () =>
                    {
                        m_EventBus.SendEvent(new RequestChangeTabEvent { tabIndex = StyleModelInfoEditor.sampleOutputTab });
                    };
                    m_EventBus.SendEvent(showDialogEvent);
                    m_OnDoneCallback.Invoke(false);
                }

                return true;
            }

            return false;
        }

        internal bool IsTrainingSetImagesCountBelowMinSize(ShowDialogEvent showDialogEvent, bool showDialog)
        {
            var config = StyleTrainerConfig.config;

            if(m_StyleData.trainingSetData?.Count < 1 || m_StyleData.trainingSetData[0]?.Count < config.minTrainingSetSize)
            {
                if (showDialog)
                {
                    showDialogEvent.description = $"Training set must have at least {config.minTrainingSetSize} samples";
                    showDialogEvent.confirmAction = () =>
                    {
                        m_EventBus.SendEvent(new RequestChangeTabEvent { tabIndex = StyleModelInfoEditor.trainingSetTab });
                    };
                    m_EventBus.SendEvent(showDialogEvent);
                    m_OnDoneCallback.Invoke(false);
                }

                return true;
            }

            return false;
        }

        bool IsTrainingSetImagesCountExceedingMaxSize(ShowDialogEvent showDialogEvent, bool showDialog)
        {
            var config = StyleTrainerConfig.config;

            if(m_StyleData.trainingSetData?.Count < 1 || m_StyleData.trainingSetData[0]?.Count > config.maxTrainingSetSize)
            {
                if (showDialog)
                {
                    showDialogEvent.description = $"Training set must have at most {config.maxTrainingSetSize} samples";
                    showDialogEvent.confirmAction = () =>
                    {
                        m_EventBus.SendEvent(new RequestChangeTabEvent { tabIndex = StyleModelInfoEditor.trainingSetTab });
                    };
                    m_EventBus.SendEvent(showDialogEvent);
                    m_OnDoneCallback.Invoke(false);
                }

                return true;
            }

            return false;
        }

        internal bool HasValidNameAndDescription(ShowDialogEvent showDialogEvent, bool showDialog)
        {
            if (string.IsNullOrWhiteSpace(m_StyleData.title) || string.IsNullOrWhiteSpace(m_StyleData.description))
            {
                if (showDialog)
                {
                    showDialogEvent.description = $"Style name and description cannot be empty.";
                    m_EventBus.SendEvent(showDialogEvent);
                    m_OnDoneCallback.Invoke(false);
                }

                return false;
            }

            return true;
        }

        void ValidateTrainingSetImages()
        {
            ++m_TrainingImagesLoaded;
            var trainingSetData = m_StyleData.trainingSetData[0];
            if (trainingSetData == null || m_TrainingImagesLoaded < trainingSetData.Count)
                return;

            var duplicatedItem = new List<int>();
            var showDialogEvent = new ShowDialogEvent
            {
                title = "Error",
                description = "Cannot generate style",
                semantic = AlertSemantic.Error
            };

            for (var i = 0; i < trainingSetData.Count; ++i)
            {
                var data = trainingSetData[i].imageArtifact.GetRawData();
                duplicatedItem.Clear();
                for (var j = i + 1; j < trainingSetData.Count; ++j)
                {
                    var data1 = trainingSetData[j].imageArtifact.GetRawData();
                    if (data?.Length == data1?.Length &&
                        Utilities.ByteArraysEqual(data, data1))
                        duplicatedItem.Add(j);
                }

                if (duplicatedItem.Count > 0)
                {
                    duplicatedItem.Add(i);
                    showDialogEvent.description = "One of the training samples is a duplicate. Please review your training set.";
                    showDialogEvent.confirmAction = () =>
                    {
                        m_EventBus.SendEvent(new RequestChangeTabEvent
                        {
                            tabIndex = StyleModelInfoEditor.trainingSetTab,
                            highlightIndices = duplicatedItem.AsReadOnly()
                        });
                    };
                    m_EventBus.SendEvent(showDialogEvent);
                    m_OnDoneCallback.Invoke(false);
                    return;
                }
            }

            // Any more validation should be done here next
            m_OnDoneCallback.Invoke(true);
        }
    }
}