using System;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Debug;
using Unity.Muse.StyleTrainer.Events.StyleTrainerMainUIEvents;
using Unity.Muse.StyleTrainer.Events.TrainingControllerEvents;

namespace Unity.Muse.StyleTrainer
{
    class StyleTrainDataUpgrader
    {
        StyleTrainerData m_StyleTrainerData;
        EventBus m_EventBus;

        public StyleTrainDataUpgrader(StyleTrainerData styleTrainerData, EventBus eventBus)
        {
            m_StyleTrainerData = styleTrainerData;
            m_EventBus = eventBus;
        }

        (int UnityVersion, int TrainerVersion) ParseVersion(string version)
        {
            var unityVersion = version.Substring(0, 5);
            var trainerVersin = version.Substring(6);
            return (int.Parse(unityVersion), int.Parse(trainerVersin));
        }

        public void Execute(Action onUpgradeDone)
        {
            if (StyleTrainerData.k_Version == "202231001")
            {
                // First GA release version. Any version lower than this will have it's data cleared
                var currentVersion = ParseVersion(StyleTrainerData.k_Version);
                var dataVersion = ParseVersion(m_StyleTrainerData.version);
                if (dataVersion.UnityVersion == currentVersion.UnityVersion && dataVersion.TrainerVersion < currentVersion.TrainerVersion)
                {
                    m_EventBus.SendEvent(new ShowDialogEvent()
                    {
                        title = "Style Trainer Data Outdated",
                        description = "The Style Trainer project was created during beta and is no longer usable. To continue using Style Trainer, a new project will be created.",
                        cancelAction = OnCancel,
                        confirmAction = () => OnConfirm(onUpgradeDone)
                    });
                }
                else
                {
                    onUpgradeDone?.Invoke();
                }
            }
        }

        void OnConfirm(Action onUpgradeDone)
        {
            m_StyleTrainerData.UpdateVersion();
            m_StyleTrainerData.ClearStyles();
            onUpgradeDone?.Invoke();
        }

        void OnCancel()
        {
            m_EventBus.SendEvent(new SystemEvents()
            {
                state = SystemEvents.ESystemState.CloseWindow
            });
        }
    }
}