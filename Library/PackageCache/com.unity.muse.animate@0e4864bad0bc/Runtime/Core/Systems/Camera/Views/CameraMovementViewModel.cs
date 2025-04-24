using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && UNITY_INPUTSYSTEM_PRESENT
    using UnityEngine.InputSystem;
#endif

namespace Unity.Muse.Animate
{
    class CameraMovementViewModel
    {
        enum MovementType
        {
            None,
            LookAround,
            Orbit,
            Pan,
            Dolly
        }

        public Vector3 Pivot => m_CameraMovementModel.Pivot;
#if ENABLE_INPUT_SYSTEM && UNITY_INPUTSYSTEM_PRESENT
    //new system
    public bool ShouldCameraCaptureInput =>
            Keyboard.current[Key.LeftAlt].isPressed;
#else
    //old system
    public bool ShouldCameraCaptureInput =>
            Input.GetKey(KeyCode.LeftAlt);
#endif
        

        public delegate void ClickedWithoutDragging(CameraMovementViewModel model, PointerEventData eventData);
        public event ClickedWithoutDragging OnClickedWithoutDragging;

        CameraMovementModel m_CameraMovementModel;
        KeyboardWalkMovement m_KeyboardWalkMovement;

        MovementType m_MovementType;
        float m_PrevCenterDistance;
        bool m_DidADrag;

        public float DollySpeed => 0.02f;
        public float PanSpeed => 0.005f;
        public float OrbitSpeed => 0.25f;
        public float LookAroundSpeed => 0.2f;

        public CameraMovementViewModel(CameraMovementModel cameraMovementModel)
        {
            m_CameraMovementModel = cameraMovementModel;
            m_KeyboardWalkMovement = new KeyboardWalkMovement();
            m_MovementType = MovementType.None;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_KeyboardWalkMovement.Reset();
            m_MovementType = GetMovementType(eventData);
            m_PrevCenterDistance = GetMovementFromCursorDelta(ScreenUtils.DistanceToScreenCenter(m_CameraMovementModel.Camera, eventData.position)).magnitude;

            if (m_MovementType != MovementType.None)
                eventData.Use();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_MovementType = MovementType.None;
            eventData.Use();
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_DidADrag = true;
            
            switch (m_MovementType)
            {
                case MovementType.Orbit:
                    Orbit(eventData);
                    break;

                case MovementType.Pan:
                    Pan(eventData);
                    break;

                case MovementType.LookAround:
                    LookAround(eventData);
                    break;

                case MovementType.Dolly:
                    Dolly(eventData);
                    break;
            }

            eventData.Use();
        }

        MovementType GetMovementType(PointerEventData eventData)
        {
            // Observed Unity Behavior (Windows):
            // Left = Orbit
            // Alt + Left = Orbit
            // Alt + Ctrl + Left = Pan
            // Middle = Pan
            // Middle + Ctrl = Pan
            // Alt + Middle = Pan
            // Alt + Ctrl + Middle = Pan
            // Right = LookAround
            // Right + Ctrl = LookAround
            // Alt + Right = Dolly
            // Alt + Ctrl + Right = Dolly

            // Middle click always Pan
            if (eventData.button == PointerEventData.InputButton.Middle)
                return MovementType.Pan;

            // Right click LookAround or Dolly when Alt  pressed
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (InputUtils.IsAlt())
                    return MovementType.Dolly;

                return MovementType.LookAround;
            }

            if (InputUtils.IsControl())
                return MovementType.Pan;

            // Left drag orbit when alt
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (InputUtils.IsAlt())
                    return MovementType.Orbit;
            }

            return MovementType.None;
        }

        void Orbit(PointerEventData eventData)
        {
            m_CameraMovementModel.Orbit(GetMovementFromCursorDelta(eventData.delta, OrbitSpeed));
        }

        void Pan(PointerEventData eventData)
        {
            m_CameraMovementModel.Pan(-GetMovementFromCursorDelta(eventData.delta, PanSpeed));
        }

        void LookAround(PointerEventData eventData)
        {
            m_CameraMovementModel.LookAround(GetMovementFromCursorDelta(eventData.delta, LookAroundSpeed));
        }

        void Dolly(PointerEventData eventData)
        {
            var newDistance2DNormalized = GetMovementFromCursorDelta(ScreenUtils.DistanceToScreenCenter(m_CameraMovementModel.Camera, eventData.position));
            var newDistance = newDistance2DNormalized.magnitude;
            var dollyAmount = newDistance - m_PrevCenterDistance;
            m_PrevCenterDistance = newDistance;
            m_CameraMovementModel.Dolly(dollyAmount * DollySpeed);
        }

        static Vector2 GetMovementFromCursorDelta(Vector2 delta, float power = 1f)
        {
            return CameraMovementUtils.ViewRatio * delta * power;
        }

        public void OnUpdate(float deltaTime)
        {
            Log($"OnUpdate(deltaTime: {deltaTime}, movement type:{m_MovementType})");
            if (m_MovementType != MovementType.LookAround)
                return;
            m_KeyboardWalkMovement.Update(deltaTime);
            if (m_KeyboardWalkMovement.Movement.sqrMagnitude > 1e-3f)
            {
                var onPlane = Input.GetKey(KeyCode.Space);
                m_CameraMovementModel.Walk(m_KeyboardWalkMovement.Movement, onPlane);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_DidADrag)
            {
                Log("OnPointerClick() -> Ignored click, a drag just ended.");
                return;
            }
            
            Log("OnPointerClick()");
            
            OnClickedWithoutDragging?.Invoke(this, eventData);
            eventData.Use();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Log("OnPointerDown()");
            m_DidADrag = false;
        }

        public void OnPointerEnter(PointerEventData eventData) { }

        public void OnPointerExit(PointerEventData eventData) { }
        
        void Log(string msg)
        {
            if (!ApplicationConstants.DebugCameraMovement)
                return;
            
            Debug.Log(GetType().Name+" -> "+msg);
        }

    }
}
