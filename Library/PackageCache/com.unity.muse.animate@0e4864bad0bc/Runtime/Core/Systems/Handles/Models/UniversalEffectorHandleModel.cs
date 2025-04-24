using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class UniversalEffectorHandleModel
    {
        const float k_CircleRadius = 0.12f;
        const float k_CircleColliderRadiusPadding = 0.05f;

        [SerializeField]
        UniversalEffectorHandleData m_Data;

        public CircleControlModel CircleControlModel => m_Data.CircleControl;
        public BallControlModel ballControlModel => m_Data.BallControl;
        public RingControlModel FrontControlModel => m_Data.Front;

        public Vector3 CurrentPosition
        {
            get => m_Data.CurrentTransform.Position;
            set => m_Data.CurrentTransform.Position = value;
        }

        public Quaternion CurrentRotation
        {
            get => m_Data.CurrentTransform.Rotation;
            set => m_Data.CurrentTransform.Rotation = value;
        }

        public Vector3 InitialPosition
        {
            get => m_Data.InitialTransform.Position;
            set => m_Data.InitialTransform.Position = value;
        }

        public Quaternion InitialRotation
        {
            get => m_Data.InitialTransform.Rotation;
            set => m_Data.InitialTransform.Rotation = value;
        }

        public bool RotationEnabled
        {
            get => m_Data.RotationEnabled;
            set
            {
                if (m_Data.RotationEnabled == value)
                    return;

                m_Data.RotationEnabled = value;
                OnChanged?.Invoke(this);
            }
        }

        public delegate void Changed(UniversalEffectorHandleModel model);
        public event Changed OnChanged;

        public delegate void RotationChanged(UniversalEffectorHandleModel model, Quaternion newRotation);
        public event RotationChanged OnRotationChanged;

        public delegate void InitialTransformChanged(UniversalEffectorHandleModel model, Vector3 newPosition, Quaternion newRotation);
        public event InitialTransformChanged OnInitialTransformChanged;

        public UniversalEffectorHandleModel(Vector3 initialPosition, Quaternion initialRotation)
        {
            m_Data.InitialTransform = new RigidTransformModel(initialPosition, initialRotation);
            m_Data.CurrentTransform = new RigidTransformModel(initialPosition, initialRotation);

            m_Data.CircleControl = new CircleControlModel(HandleColors.ColorLine);
            m_Data.CircleControl.Radius = k_CircleRadius;
            m_Data.CircleControl.ColliderRadiusPadding = k_CircleColliderRadiusPadding;

            m_Data.BallControl = new BallControlModel(0.47f, HandleColors.ColorLine);
            m_Data.Front = new RingControlModel(Vector3.forward, 0.6f, false, HandleColors.ColorLine);

            m_Data.RotationEnabled = true;

            RegisterEvents();
        }

        [JsonConstructor]
        public UniversalEffectorHandleModel(UniversalEffectorHandleData m_Data)
        {
            this.m_Data = m_Data;
        }

        public void Reset(Vector3 position, Quaternion rotation)
        {
            m_Data.InitialTransform.Set(position, rotation);
            m_Data.CurrentTransform.Set(position, rotation);
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            RegisterEvents();
        }

        void RegisterEvents()
        {
            m_Data.CurrentTransform.OnPositionChanged += OnCurrentTransformPositionChanged;
            m_Data.CurrentTransform.OnRotationChanged += OnCurrentTransformRotationChanged;

            m_Data.InitialTransform.OnPositionChanged += OnInitialTransformPositionChanged;
            m_Data.InitialTransform.OnRotationChanged += OnInitialTransformRotationChanged;

            m_Data.CircleControl.OnPositionChanged += CircleControlPositionChanged;
            m_Data.BallControl.OnTransformChanged += BallControlTransformChanged;
            m_Data.Front.OnTransformChanged += OnRingTransformChanged;
        }

        void CircleControlPositionChanged(CircleControlModel ControlModel, Vector3 newPosition)
        {
            CurrentPosition = newPosition;
        }

        void BallControlTransformChanged(BallControlModel controlModel, Vector3 newPosition, Quaternion newRotation)
        {
            CurrentRotation = newRotation;
        }

        void OnRingTransformChanged(RingControlModel ControlModel, Vector3 newPosition, Quaternion newRotation)
        {
            CurrentRotation = newRotation;
        }

        void OnCurrentTransformPositionChanged(RigidTransformModel model, Vector3 newPosition)
        {
            m_Data.CircleControl.Position = newPosition;
            m_Data.BallControl.Position = newPosition;
            m_Data.Front.Position = newPosition;
        }

        void OnCurrentTransformRotationChanged(RigidTransformModel model, Quaternion newRotation)
        {
            m_Data.BallControl.Rotation = newRotation;
            m_Data.Front.Rotation = newRotation;

            OnRotationChanged?.Invoke(this, newRotation);
        }

        void OnInitialTransformPositionChanged(RigidTransformModel model, Vector3 newPosition)
        {
            OnInitialTransformChanged?.Invoke(this, m_Data.InitialTransform.Position, m_Data.InitialTransform.Rotation);
        }

        void OnInitialTransformRotationChanged(RigidTransformModel model, Quaternion newRotation)
        {
            OnInitialTransformChanged?.Invoke(this, m_Data.InitialTransform.Position, m_Data.InitialTransform.Rotation);
        }
    }
}
