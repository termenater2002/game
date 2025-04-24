using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class BakingNoticeViewModel
    {
        public delegate void Changed();
        public event Changed OnChanged;

        public bool IsVisible
        {
            get => m_IsVisible;
            private set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnChanged?.Invoke();
            }
        }

        public string Icon
        {
            get => m_Icon;
            private set
            {
                if (m_Icon.Equals(value))
                    return;

                m_Icon = value;
                OnChanged?.Invoke();
            }
        }

        public string MainLabel
        {
            get => m_MainLabel;
            private set
            {
                if (value.Equals(m_MainLabel))
                    return;

                m_MainLabel = value;
                OnChanged?.Invoke();
            }
        }

        bool m_IsVisible;
        string m_MainLabel = "Main Label";
        string m_Icon = "";
        
        readonly List<BakingLogic> m_BakingLogics = new ();
        readonly AuthoringModel m_AuthoringModel;

        public void Show(string label, string icon = "warning")
        {
            IsVisible = true;
            MainLabel = label;
            Icon = icon;
        }
        
        public void Hide()
        {
            IsVisible = false;
            MainLabel = "";
            Icon = "";
        }
    }
}
