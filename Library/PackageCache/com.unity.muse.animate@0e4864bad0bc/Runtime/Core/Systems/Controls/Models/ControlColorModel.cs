using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class ControlColorModel
    {
        [SerializeField]
        ControlColorData m_Data;

        public delegate void Changed(ControlColorModel model);
        public event Changed OnChanged;

        public Color Base
        {
            get => m_Data.Color;
            set
            {
                if (value == m_Data.Color)
                    return;

                m_Data.Color = value;
                OnChanged?.Invoke(this);
            }
        }

        public Color Highlighted
        {
            get => m_Data.Highlighted;
            set
            {
                if (value == m_Data.Highlighted)
                    return;

                m_Data.Highlighted = value;
                OnChanged?.Invoke(this);
            }
        }

        public Color Interacting
        {
            get => m_Data.Interacting;
            set
            {
                if (value == m_Data.Interacting)
                    return;

                m_Data.Interacting = value;
                OnChanged?.Invoke(this);
            }
        }

        public ControlColorModel(Color baseColor)
        {
            SetColorScheme(baseColor);
        }

        [JsonConstructor]
        public ControlColorModel(ControlColorData m_Data)
        {
            this.m_Data = m_Data;
        }

        public void SetColorScheme(Color baseColor)
        {
            Base = baseColor;
            Highlighted = ColorUtils.AddAlpha(ColorUtils.AddRGB(baseColor, 0.2f), 0.2f);
            Interacting = ColorUtils.AddAlpha(ColorUtils.AddRGB(baseColor, 0.4f), 0.4f);
        }
    }
}
