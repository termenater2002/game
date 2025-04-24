using System;
using UnityEngine;
using UnityEngine.UIElements;

#pragma warning disable 0067

namespace Unity.Muse.Common
{
    [Serializable]
    internal class GenerateButtonData : IModelData
    {
        public event Action OnModified;
        public event Action OnSaveRequested;

        string m_Tooltip;
        bool m_IsEnabled;
        bool m_IsCoolingDown;

        public bool isEnabled => m_IsEnabled && !m_IsCoolingDown;
        public string tooltip => m_IsCoolingDown ? TextContent.generateButtonCooldownTooltip : m_Tooltip;

        public void SetGenerateButtonData(bool isButtonEnabled, string buttonTooltip = null)
        {
            m_IsEnabled = isButtonEnabled;
            m_Tooltip = buttonTooltip;

            OnModified?.Invoke();
        }

        public void SetCooldown(VisualElement target, float cooldownSeconds)
        {
            if (target == null || m_IsCoolingDown)
                return;

            m_IsCoolingDown = true;
            OnModified?.Invoke();

            target.schedule.Execute(() =>
            {
                m_IsCoolingDown = false;
                OnModified?.Invoke();
            }).ExecuteLater((long)(cooldownSeconds * 1000));
        }
    }
}