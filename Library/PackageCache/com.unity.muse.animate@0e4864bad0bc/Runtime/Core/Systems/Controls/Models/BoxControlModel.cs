using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class BoxControlModel
    {
        [SerializeField]
        BoxControlData m_Data;

        public Vector3 LocalNormal => m_Data.LocalNormal;
        public Vector3 WorldNormal => m_Data.Transform.Rotation * m_Data.LocalNormal;
        public Vector3 LocalFirstAxis => m_Data.LocalFirstAxis;
        public Vector3 WorldFirstAxis => m_Data.Transform.Rotation * m_Data.LocalFirstAxis;
        public Vector3 LocalSecondAxis => m_Data.LocalSecondAxis;
        public Vector3 WorldSecondAxis => m_Data.Transform.Rotation * m_Data.LocalSecondAxis;
        public Vector3 WorldThirdAxis => m_Data.Transform.Rotation * m_Data.LocalThirdAxis;
        
        public Color FirstAxisBaseColor
        {
            get => m_Data.FirstAxisColor.Base;
            set => m_Data.FirstAxisColor.Base = value;
        }

        public Color FirstAxisHighlightedColor
        {
            get => m_Data.FirstAxisColor.Highlighted;
            set => m_Data.FirstAxisColor.Highlighted = value;
        }

        public Color FirstAxisInteractionColor
        {
            get => m_Data.FirstAxisColor.Interacting;
            set => m_Data.FirstAxisColor.Interacting = value;
        }

        public Color SecondAxisBaseColor
        {
            get => m_Data.SecondAxisColor.Base;
            set => m_Data.SecondAxisColor.Base = value;
        }

        public Color SecondAxisHighlightedColor
        {
            get => m_Data.SecondAxisColor.Highlighted;
            set => m_Data.SecondAxisColor.Highlighted = value;
        }

        public Color SecondAxisInteractionColor
        {
            get => m_Data.SecondAxisColor.Interacting;
            set => m_Data.SecondAxisColor.Interacting = value;
        }

        public Color NormalBaseColor
        {
            get => m_Data.NormalColor.Base;
            set => m_Data.NormalColor.Base = value;
        }

        public Color NormalHighlightedColor
        {
            get => m_Data.NormalColor.Highlighted;
            set => m_Data.NormalColor.Highlighted = value;
        }

        public Color NormalInteractionColor
        {
            get => m_Data.NormalColor.Interacting;
            set => m_Data.NormalColor.Interacting = value;
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
        
        public Bounds LocalBounds
        {
            get => m_Data.LocalBounds;
            set => m_Data.LocalBounds = value;
        }
        
        public delegate void TransformChanged(BoxControlModel ControlModel, Vector3 newPosition, Quaternion newRotation);
        public event TransformChanged OnTransformChanged;

        public delegate void ColorChanged(BoxControlModel ControlModel);
        public event ColorChanged OnColorChanged;

        public BoxControlModel(Vector3 firstLocalAxis, Vector3 secondLocalAxis, Vector3 thirdLocalAxis, Color firstAxisColor, Color secondAxisColor, Color thirdAxisColor, Color normalColor) :
            this(firstAxisColor, secondAxisColor, thirdAxisColor, normalColor)
        {
            m_Data.LocalFirstAxis = firstLocalAxis.normalized;
            m_Data.LocalSecondAxis = secondLocalAxis.normalized;
            m_Data.LocalThirdAxis = thirdLocalAxis.normalized;
            m_Data.LocalNormal = Vector3.Cross(m_Data.LocalFirstAxis, m_Data.LocalSecondAxis);
        }

        public BoxControlModel(Color firstAxisColor, Color secondAxisColor, Color thirdAxisColor, Color normalColor)
        {
            m_Data.FirstAxisColor = new ControlColorModel(firstAxisColor);
            m_Data.SecondAxisColor = new ControlColorModel(secondAxisColor);
            m_Data.ThirdAxisColor = new ControlColorModel(thirdAxisColor);
            m_Data.NormalColor = new ControlColorModel(normalColor);
            m_Data.Transform = new RigidTransformModel(Vector3.zero, Quaternion.identity);
            RegisterEvents();
        }

        [JsonConstructor]
        public BoxControlModel(BoxControlData m_Data)
        {
            this.m_Data = m_Data;
        }

        public void ForceTransformChanged()
        {
            OnTransformChanged?.Invoke(this, Position, Rotation);
        }

        public void SetColorScheme(Color firstAxisColor, Color secondAxisColor, Color normalColor)
        {
            m_Data.FirstAxisColor.SetColorScheme(firstAxisColor);
            m_Data.SecondAxisColor.SetColorScheme(secondAxisColor);
            m_Data.NormalColor.SetColorScheme(normalColor);
        }

        public void TranslateAlongPlane(Vector2 inPlaneOffset)
        {
            var worldOffset = inPlaneOffset.x * WorldFirstAxis + inPlaneOffset.y * WorldSecondAxis;
            Position += worldOffset;
        }

        public void TranslateAlongPlane(Vector3 worldOffset)
        {
            var projected = worldOffset - Vector3.Dot(worldOffset, WorldNormal) * WorldNormal;
            Position += projected;
        }

        void RegisterEvents()
        {
            m_Data.FirstAxisColor.OnChanged += model => OnColorChanged?.Invoke(this);
            m_Data.SecondAxisColor.OnChanged += model => OnColorChanged?.Invoke(this);
            m_Data.NormalColor.OnChanged += model => OnColorChanged?.Invoke(this);

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
