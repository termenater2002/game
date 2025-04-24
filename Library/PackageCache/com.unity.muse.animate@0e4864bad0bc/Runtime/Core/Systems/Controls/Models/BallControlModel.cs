using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class BallControlModel
    {
        const float k_Epsilon = 1e-5f;

        [SerializeField]
        BallControlData m_Data;

        public float Radius
        {
            get => m_Data.Radius;
            set
            {
                if (m_Data.Radius.NearlyEquals(value, k_Epsilon))
                    return;

                m_Data.Radius = value;
                OnRadiusChanged?.Invoke(this, m_Data.Radius);
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

        public Quaternion Rotation
        {
            get => m_Data.Transform.Rotation;
            set => m_Data.Transform.Rotation = value;
        }

        public delegate void RadiusChanged(BallControlModel controlModel, float newRadius);
        public event RadiusChanged OnRadiusChanged;

        public delegate void TransformChanged(BallControlModel controlModel, Vector3 newPosition, Quaternion newRotation);
        public event TransformChanged OnTransformChanged;

        public delegate void ColorChanged(BallControlModel controlModel);
        public event ColorChanged OnColorChanged;

        public BallControlModel(float radius, Color baseColor)
        {
            m_Data.Color = new ControlColorModel(baseColor);
            m_Data.Transform = new RigidTransformModel(Vector3.zero, Quaternion.identity);
            m_Data.Radius = radius;

            RegisterEvents();
        }

        [JsonConstructor]
        public BallControlModel(BallControlData m_Data)
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
            m_Data.Transform.OnPositionChanged += (model, position) => OnTransformChanged?.Invoke(this, Position, Rotation);
            m_Data.Transform.OnRotationChanged += (model, position) => OnTransformChanged?.Invoke(this, Position, Rotation);
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            RegisterEvents();
        }
    }
}
