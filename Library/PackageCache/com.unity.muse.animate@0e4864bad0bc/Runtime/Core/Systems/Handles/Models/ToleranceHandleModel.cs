using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class ToleranceHandleModel
    {
        [SerializeField]
        ToleranceHandleData m_Data;

        public RadiusControlModel RadiusControlModel => m_Data.RadiusControl;

        public float Tolerance
        {
            get => m_Data.RadiusControl.Radius / ApplicationConstants.ToleranceToRadiusFactor;
            set => m_Data.RadiusControl.Radius = value * ApplicationConstants.ToleranceToRadiusFactor;
        }

        public Vector3 Position
        {
            get => m_Data.Transform.Position;
            set => m_Data.Transform.Position = value;
        }

        public delegate void ToleranceChanged(ToleranceHandleModel model, float newTolerance);
        public event ToleranceChanged OnToleranceChanged;

        public ToleranceHandleModel()
        {
            m_Data.Transform = new RigidTransformModel(Vector3.zero, Quaternion.identity);
            m_Data.RadiusControl = new RadiusControlModel(HandleColors.ColorLine);
            m_Data.RadiusControl.MinDisplayRadiusRelative = 0.05f;
            m_Data.RadiusControl.MaxRadius = 0.5f;
            m_Data.RadiusControl.SnapToZeroBelowMinRadius = true;

            RegisterEvents();
        }

        [JsonConstructor]
        public ToleranceHandleModel(ToleranceHandleData m_Data)
        {
            this.m_Data = m_Data;
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            RegisterEvents();
        }

        void RegisterEvents()
        {
            m_Data.RadiusControl.OnRadiusChanged += RadiusControlChanged;

            m_Data.Transform.OnPositionChanged += OnTransformPositionChanged;
        }

        void OnTransformPositionChanged(RigidTransformModel model, Vector3 newPosition)
        {
            m_Data.RadiusControl.Position = newPosition;
        }

        void RadiusControlChanged(RadiusControlModel ControlModel, float newRadius)
        {
            OnToleranceChanged?.Invoke(this, Tolerance);
        }
    }
}
