using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class AxisControlModel
    {
        [SerializeField]
        AxisControlData m_Data;

        public Vector3 LocalAxis
        {
            get => m_Data.LocalAxis;
            set
            {
                var valueNormalized = value.normalized;
                if (m_Data.LocalAxis == valueNormalized)
                    return;

                m_Data.LocalAxis = valueNormalized;
                OnAxisChanged?.Invoke(this, m_Data.LocalAxis);
            }
        }

        public Vector3 WorldAxis
        {
            get => m_Data.Transform.Rotation * m_Data.LocalAxis;
            set
            {
                var localAxis = Quaternion.Inverse(m_Data.Transform.Rotation) * value;
                LocalAxis = localAxis;
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

        public delegate void TransformChanged(AxisControlModel ControlModel, Vector3 newPosition, Quaternion newRotation);
        public event TransformChanged OnTransformChanged;

        public delegate void ColorChanged(AxisControlModel ControlModel);
        public event ColorChanged OnColorChanged;

        public delegate void AxisChanged(AxisControlModel ControlModel, Vector3 newLocalAxis);
        public event AxisChanged OnAxisChanged;

        public AxisControlModel(Vector3 localAxis, Color baseColor) :
            this(baseColor)
        {
            m_Data.LocalAxis = localAxis.normalized;
        }

        public AxisControlModel(Color baseColor)
        {
            m_Data.Color = new ControlColorModel(baseColor);
            m_Data.Transform = new RigidTransformModel(Vector3.zero, Quaternion.identity);

            RegisterEvents();
        }

        [JsonConstructor]
        public AxisControlModel(AxisControlData m_Data)
        {
            this.m_Data = m_Data;
        }

        public void SetColorScheme(Color baseColor)
        {
            m_Data.Color.SetColorScheme(baseColor);
        }

        public void TranslateAlongAxis(float distanceAlongAxis)
        {
            var offset = distanceAlongAxis * WorldAxis;
            Position += offset;
        }

        public void TranslateAlongAxis(Vector3 worldOffset)
        {
            var distanceAlongAxis = Vector3.Dot(WorldAxis, worldOffset);
            TranslateAlongAxis(distanceAlongAxis);
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
