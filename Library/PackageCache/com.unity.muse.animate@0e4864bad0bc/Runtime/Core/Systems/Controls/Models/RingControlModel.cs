using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class RingControlModel
    {
        const float k_Epsilon = 1e-5f;

        [SerializeField]
        RingControlModelData m_Data;

        public bool HideBack => m_Data.HideBack;

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

        public Vector3 LocalAxis
        {
            get => m_Data.LocalAxis;
            set
            {
                var valueNormalized = value.normalized;
                if (m_Data.LocalAxis.NearlyEquals(valueNormalized, k_Epsilon))
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

        public Quaternion LocalRingRotation
        {
            get => Quaternion.FromToRotation(Vector3.forward, LocalAxis);
        }

        public Quaternion WorldRingRotation
        {
            get => Rotation * LocalRingRotation;
        }

        public delegate void RadiusChanged(RingControlModel model, float newRadius);
        public event RadiusChanged OnRadiusChanged;

        public delegate void TransformChanged(RingControlModel model, Vector3 newPosition, Quaternion newRotation);
        public event TransformChanged OnTransformChanged;

        public delegate void ColorChanged(RingControlModel model);
        public event ColorChanged OnColorChanged;

        public delegate void AxisChanged(RingControlModel model, Vector3 newLocalAxis);
        public event AxisChanged OnAxisChanged;

        public RingControlModel(Vector3 localAxis, float radius, bool hideBack, Color baseColor)
        {
            m_Data.Color = new ControlColorModel(baseColor);
            m_Data.Transform = new RigidTransformModel(Vector3.zero, Quaternion.identity);
            m_Data.LocalAxis = localAxis.normalized;
            m_Data.Radius = radius;
            m_Data.HideBack = hideBack;

            RegisterEvents();
        }

        [JsonConstructor]
        public RingControlModel(RingControlModelData m_Data)
        {
            this.m_Data = m_Data;
        }

        public void SetColorScheme(Color baseColor)
        {
            m_Data.Color.SetColorScheme(baseColor);
        }
        
        public void SetTransform(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
            
            ForceTransformChange();
        }
        
        public void RotateAroundAxis(float angleRad)
        {
            var localOffset = Quaternion.AngleAxis(Mathf.Rad2Deg * angleRad, LocalAxis);
            Rotation = Rotation * localOffset;
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

        public void ForceTransformChange()
        {
            OnTransformChanged?.Invoke(this, Position, Rotation);
        }
    }
}
