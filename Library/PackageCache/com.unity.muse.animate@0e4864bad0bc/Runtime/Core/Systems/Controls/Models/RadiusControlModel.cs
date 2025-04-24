using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class RadiusControlModel
    {
        const float k_Epsilon = 1e-5f;

        [SerializeField]
        RadiusControlData m_Data;

        public float MinDisplayRadiusRelative
        {
            get => m_Data.MinDisplayRadiusRelative;
            set
            {
                if (m_Data.MinDisplayRadiusRelative.NearlyEquals(value, k_Epsilon))
                    return;

                m_Data.MinDisplayRadiusRelative = value;
            }
        }

        public float Radius
        {
            get => m_Data.Radius;
            set
            {
                var correctedRadius = Mathf.Min(value, MaxRadius);

                if (m_Data.Radius.NearlyEquals(correctedRadius, k_Epsilon))
                    return;

                m_Data.Radius = correctedRadius;
                OnRadiusChanged?.Invoke(this, m_Data.Radius);
            }
        }

        public bool SnapToZeroBelowMinRadius
        {
            get => m_Data.SnapToZeroBelowMinRadius;
            set
            {
                if (m_Data.SnapToZeroBelowMinRadius == value)
                    return;

                m_Data.SnapToZeroBelowMinRadius = value;
            }
        }

        public float MaxRadius
        {
            get => m_Data.MaxRadius;
            set
            {
                if (m_Data.MaxRadius.NearlyEquals(value, k_Epsilon))
                    return;

                m_Data.MaxRadius = value;
                if (m_Data.Radius > m_Data.MaxRadius)
                    Radius = m_Data.MaxRadius;
            }
        }

        public Color BaseColor
        {
            get => m_Data.Color.Base;
            set => m_Data.Color.Base = value;
        }

        public Color HighlightedColor
        {
            get => m_Data.Color.Highlighted;
            set => m_Data.Color.Highlighted = value;
        }

        public Color InteractionColor
        {
            get => m_Data.Color.Interacting;
            set => m_Data.Color.Interacting = value;
        }

        public Vector3 Position
        {
            get => m_Data.Transform.Position;
            set => m_Data.Transform.Position = value;
        }

        public delegate void PositionChanged(RadiusControlModel ControlModel, Vector3 newPosition);
        public event PositionChanged OnPositionChanged;

        public delegate void ColorChanged(RadiusControlModel ControlModel);
        public event ColorChanged OnColorChanged;

        public delegate void RadiusChanged(RadiusControlModel ControlModel, float newRadius);
        public event RadiusChanged OnRadiusChanged;

        public RadiusControlModel(Color baseColor)
        {
            m_Data.Color = new ControlColorModel(baseColor);
            m_Data.Transform = new RigidTransformModel(Vector3.zero, Quaternion.identity);
            m_Data.MinDisplayRadiusRelative = 0.05f;
            m_Data.Radius = 0.1f;
            m_Data.MaxRadius = 10f;
            m_Data.SnapToZeroBelowMinRadius = true;

            RegisterEvents();
        }

        [JsonConstructor]
        public RadiusControlModel(RadiusControlData m_Data)
        {
            this.m_Data = m_Data;
        }

        public void SetColorScheme(Color baseColor)
        {
            m_Data.Color.SetColorScheme(baseColor);
        }

        void RegisterEvents()
        {
            m_Data.Color.OnChanged += model => OnColorChanged?.Invoke(this);
            m_Data.Transform.OnPositionChanged += (model, position) => OnPositionChanged?.Invoke(this, Position);
            m_Data.Transform.OnRotationChanged += (model, position) => OnPositionChanged?.Invoke(this, Position);
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            RegisterEvents();
        }
    }
}
