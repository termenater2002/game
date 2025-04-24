using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CircleControlModel
    {
        [SerializeField]
        CircleControlData m_Data;

        public float Radius
        {
            get => m_Data.Radius;
            set
            {
                if (m_Data.Radius == value)
                    return;

                m_Data.Radius = value;
                OnShapeChanged?.Invoke(this);
            }
        }

        public float ColliderRadiusPadding
        {
            get => m_Data.ColliderRadiusPadding;
            set
            {
                if (m_Data.ColliderRadiusPadding == value)
                    return;

                m_Data.ColliderRadiusPadding = value;
                OnShapeChanged?.Invoke(this);
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

        public delegate void PositionChanged(CircleControlModel ControlModel, Vector3 newPosition);
        public event PositionChanged OnPositionChanged;

        public delegate void ColorChanged(CircleControlModel ControlModel);
        public event ColorChanged OnColorChanged;

        public delegate void ShapeChanged(CircleControlModel ControlModel);
        public event ShapeChanged OnShapeChanged;

        public CircleControlModel(Color baseColor)
        {
            m_Data.Color = new ControlColorModel(baseColor);
            m_Data.Transform = new RigidTransformModel(Vector3.zero, Quaternion.identity);
            m_Data.Radius = 0.1f;

            RegisterEvents();
        }

        [JsonConstructor]
        public CircleControlModel(CircleControlData m_Data)
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
