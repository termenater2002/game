using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class TranslationHandleModel
    {
        const float k_CircleRadius = 0.12f;
        const float k_CircleColliderRadiusPadding = 0.05f;

        [SerializeField]
        TranslationHandleData m_Data;

        public CircleControlModel CircleControlModel => m_Data.CircleControl;
        public AxisControlModel AxisControlXModel => m_Data.AxisControlX;
        public AxisControlModel AxisControlYModel => m_Data.AxisControlY;
        public AxisControlModel AxisControlZModel => m_Data.AxisControlZ;
        public PlaneControlModel PlaneControlXYModel => m_Data.PlaneControlXY;
        public PlaneControlModel PlaneControlXZModel => m_Data.PlaneControlXZ;
        public PlaneControlModel PlaneControlYZModel => m_Data.PlaneControlYZ;

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

        public delegate void PositionChanged(TranslationHandleModel model, Vector3 newPosition);
        public event PositionChanged OnPositionChanged;

        public delegate void InitialTransformChanged(TranslationHandleModel model, Vector3 newPosition, Quaternion newRotation);
        public event InitialTransformChanged OnInitialTransformChanged;

        public TranslationHandleModel(Vector3 initialPosition, Quaternion initialRotation)
        {
            m_Data.InitialTransform = new RigidTransformModel(initialPosition, initialRotation);
            m_Data.CurrentTransform = new RigidTransformModel(initialPosition, initialRotation);

            m_Data.CircleControl = new CircleControlModel(HandleColors.ColorLine);
            m_Data.CircleControl.Radius = k_CircleRadius;
            m_Data.CircleControl.ColliderRadiusPadding = k_CircleColliderRadiusPadding;

            m_Data.AxisControlX = new AxisControlModel(Vector3.right, HandleColors.ColorXLine);
            m_Data.AxisControlY = new AxisControlModel(Vector3.up, HandleColors.ColorYLine);
            m_Data.AxisControlZ = new AxisControlModel(Vector3.forward, HandleColors.ColorZLine);

            m_Data.PlaneControlXY = new PlaneControlModel(m_Data.AxisControlX.LocalAxis, m_Data.AxisControlY.LocalAxis,
                HandleColors.ColorXLine, HandleColors.ColorYLine, HandleColors.ColorZLine);
            m_Data.PlaneControlXZ = new PlaneControlModel(m_Data.AxisControlZ.LocalAxis, m_Data.AxisControlX.LocalAxis,
                HandleColors.ColorZLine, HandleColors.ColorXLine, HandleColors.ColorYLine);
            m_Data.PlaneControlYZ = new PlaneControlModel(m_Data.AxisControlY.LocalAxis, m_Data.AxisControlZ.LocalAxis,
                HandleColors.ColorYLine, HandleColors.ColorZLine, HandleColors.ColorXLine);

            RegisterEvents();
        }

        [JsonConstructor]
        public TranslationHandleModel(TranslationHandleData m_Data)
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

            m_Data.AxisControlX.OnTransformChanged += OnAxisControlTransformChanged;
            m_Data.AxisControlY.OnTransformChanged += OnAxisControlTransformChanged;
            m_Data.AxisControlZ.OnTransformChanged += OnAxisControlTransformChanged;

            m_Data.PlaneControlXY.OnTransformChanged += PlaneControlTransformChanged;
            m_Data.PlaneControlXZ.OnTransformChanged += PlaneControlTransformChanged;
            m_Data.PlaneControlYZ.OnTransformChanged += PlaneControlTransformChanged;
        }

        void PlaneControlTransformChanged(PlaneControlModel controlModel, Vector3 newPosition, Quaternion newRotation)
        {
            CurrentPosition = newPosition;
        }

        void OnAxisControlTransformChanged(AxisControlModel ControlModel, Vector3 newPosition, Quaternion newRotation)
        {
            CurrentPosition = newPosition;
        }

        void CircleControlPositionChanged(CircleControlModel ControlModel, Vector3 newPosition)
        {
            CurrentPosition = newPosition;
        }

        void OnCurrentTransformPositionChanged(RigidTransformModel model, Vector3 newPosition)
        {
            m_Data.CircleControl.Position = newPosition;

            m_Data.AxisControlX.Position = newPosition;
            m_Data.AxisControlY.Position = newPosition;
            m_Data.AxisControlZ.Position = newPosition;

            m_Data.PlaneControlXY.Position = newPosition;
            m_Data.PlaneControlXZ.Position = newPosition;
            m_Data.PlaneControlYZ.Position = newPosition;
            
            OnPositionChanged?.Invoke(this, newPosition);
        }

        void OnCurrentTransformRotationChanged(RigidTransformModel model, Quaternion newRotation)
        {
            m_Data.AxisControlX.Rotation = newRotation;
            m_Data.AxisControlY.Rotation = newRotation;
            m_Data.AxisControlZ.Rotation = newRotation;

            m_Data.PlaneControlXY.Rotation = newRotation;
            m_Data.PlaneControlXZ.Rotation = newRotation;
            m_Data.PlaneControlYZ.Rotation = newRotation;
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
