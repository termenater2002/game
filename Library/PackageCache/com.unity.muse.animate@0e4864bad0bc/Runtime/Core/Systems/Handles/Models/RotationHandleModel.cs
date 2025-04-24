using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class RotationHandleModel
    {
        [SerializeField]
        RotationHandleData m_Data;

        public BallControlModel Ball => m_Data.BallControl;
        public RingControlModel Front => m_Data.Front;
        public RingControlModel RingX => m_Data.AxisX;
        public RingControlModel RingY => m_Data.AxisY;
        public RingControlModel RingZ => m_Data.AxisZ;

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

        public delegate void RotationChanged(RotationHandleModel model, Quaternion newRotation);
        public event RotationChanged OnRotationChanged;

        public delegate void InitialTransformChanged(RotationHandleModel model, Vector3 newPosition, Quaternion newRotation);
        public event InitialTransformChanged OnInitialTransformChanged;

        public RotationHandleModel(Vector3 initialPosition, Quaternion initialRotation)
        {
            m_Data.InitialTransform = new RigidTransformModel(initialPosition, initialRotation);
            m_Data.CurrentTransform = new RigidTransformModel(initialPosition, initialRotation);

            m_Data.BallControl = new BallControlModel(0.47f, HandleColors.ColorLine);
            m_Data.Front = new RingControlModel(Vector3.forward, 0.6f, false, HandleColors.ColorLine);
            m_Data.AxisX = new RingControlModel(Vector3.right, 0.5f, true, HandleColors.ColorXLine);
            m_Data.AxisY = new RingControlModel(Vector3.up, 0.5f, true, HandleColors.ColorYLine);
            m_Data.AxisZ = new RingControlModel(Vector3.forward, 0.5f, true, HandleColors.ColorZLine);

            RegisterEvents();
        }

        [JsonConstructor]
        public RotationHandleModel(RotationHandleData m_Data)
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

            m_Data.BallControl.OnTransformChanged += BallControlTransformChanged;
            m_Data.Front.OnTransformChanged += OnRingTransformChanged;
            m_Data.AxisX.OnTransformChanged += OnRingTransformChanged;
            m_Data.AxisY.OnTransformChanged += OnRingTransformChanged;
            m_Data.AxisZ.OnTransformChanged += OnRingTransformChanged;
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
            m_Data.BallControl.Position = newPosition;
            m_Data.Front.Position = newPosition;
            m_Data.AxisX.Position = newPosition;
            m_Data.AxisY.Position = newPosition;
            m_Data.AxisZ.Position = newPosition;
        }

        void OnCurrentTransformRotationChanged(RigidTransformModel model, Quaternion newRotation)
        {
            m_Data.BallControl.Rotation = newRotation;
            m_Data.Front.Rotation = newRotation;
            m_Data.AxisX.Rotation = newRotation;
            m_Data.AxisY.Rotation = newRotation;
            m_Data.AxisZ.Rotation = newRotation;

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
