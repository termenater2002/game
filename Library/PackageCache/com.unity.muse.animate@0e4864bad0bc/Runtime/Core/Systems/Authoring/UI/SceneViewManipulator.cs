using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class SceneViewManipulator : PointerManipulator
    {
        public event Action<SceneViewManipulator> OnDrag;
        public event Action<SceneViewManipulator> OnBeginDrag;
        public event Action<SceneViewManipulator> OnEndDrag;
        public event Action<SceneViewManipulator> OnCancelDrag;
        public event Action<SceneViewManipulator> OnClick;
        public event Action<SceneViewManipulator> OnContextClick;
        public event Action<SceneViewManipulator> OnPointerMove;
        public event Action<SceneViewManipulator> OnPointerLeave;
        public event Action<SceneViewManipulator> OnPointerEnter;
        public event Action<SceneViewManipulator> OnPointerDown;
        public event Action<SceneViewManipulator> OnPointerUp;
        public event Action<KeyPressEvent> OnKeyUp;
        public event Action<KeyPressEvent> OnKeyDown;
        public event Action<KeyPressEvent> OnKeyHold;
        public event Action<WheelEvent> OnWheel;
        
        public Vector2 LastPosition { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 LocalPosition { get; set; }
        public Vector2 DeltaPosition { get; set; }
        public Vector2 LastDownPosition { get; set; }
        public Vector2 LastDownLocalPosition { get; set; }
        public DateTime LastDownTime { get; set; }
        
        bool HasMoved;
        Vector3 LastLocalPosition { get; set; }
        Vector3 DragStartLocalPosition { get; set; }
        Vector2 DragStartPosition { get; set; }
        public bool IsDragging { get; private set; }

        public bool IsPointerDown { get; set; }

        public bool IsPointerInside { get; set; }
        public int LastDownButton { get; set; }
        public int ClickCount { get; set; }

        PointerEventData m_PointerEventData;
        public int PointerId = -1;
        Dragger m_Draggable;

        protected override void RegisterCallbacksOnTarget()
        {
            InputUtils.ClearKeys();

            // Pointer Events
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnterInternal);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeftInternal);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMoveInternal);
            target.RegisterCallback<PointerDownEvent>(OnPointerDownInternal);
            target.RegisterCallback<PointerUpEvent>(OnPointerUpInternal);

            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            target.RegisterCallback<PointerCaptureEvent>(OnPointerCapture);

            // Mouse Events
            target.RegisterCallback<MouseMoveEvent>(OnMouseMoveInternal);

            // Click and Mouse Wheel
            target.RegisterCallback<WheelEvent>(OnWheelInternal);

            // Keyboard Events
            InputUtils.OnKeyDown += OnInputKeyDownInternal;
            InputUtils.OnKeyUp += OnInputKeyUpInternal;
            target.RegisterCallback<KeyDownEvent>(OnKeyDownInternal);
            target.RegisterCallback<KeyUpEvent>(OnKeyUpInternal);

#if !UNITY_2023_1_OR_NEWER
            target.RegisterCallback<MouseDownEvent>(OnMouseDownInternal);
#endif
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            // Pointer Events
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnterInternal);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeftInternal);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMoveInternal);
            target.UnregisterCallback<PointerDownEvent>(OnPointerDownInternal);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUpInternal);

            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            target.UnregisterCallback<PointerCaptureEvent>(OnPointerCapture);

            // Mouse Events
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMoveInternal);

            // Click and Mouse Wheel
            target.UnregisterCallback<WheelEvent>(OnWheelInternal);

            // Keyboard Events
            InputUtils.OnKeyDown -= OnInputKeyDownInternal;
            InputUtils.OnKeyUp -= OnInputKeyUpInternal;
            target.UnregisterCallback<KeyDownEvent>(OnKeyDownInternal);
            target.UnregisterCallback<KeyUpEvent>(OnKeyUpInternal);
            
#if !UNITY_2023_1_OR_NEWER
            target.UnregisterCallback<MouseDownEvent>(OnMouseDownInternal);
#endif
        }

        public void Update()
        {
            HasMoved = false;
            UpdateKeyHolds();
        }

        // [Section] Keyboard Callbacks

        void UpdateKeyHolds()
        {
            InputUtils.UpdateKeyHolds(OnKeyHold);
        }

        void OnInputKeyDownInternal(KeyCode keyCode)
        {
            Log($"OnInputKeyDownInternal({keyCode})");
            
            if (keyCode == KeyCode.None)
                return;
            
            var ev = InputUtils.GetKeyPressEvent(keyCode);
            OnKeyDown?.Invoke(ev);
            ev.Release();
        }

        void OnInputKeyUpInternal(KeyCode keyCode)
        {
            Log($"OnKeyUpInternal({keyCode})");
            
            if (keyCode == KeyCode.None)
                return;

            var ev = InputUtils.GetKeyPressEvent(keyCode);
            OnKeyUp?.Invoke(ev);
            ev.Release();
        }
        
        void OnKeyDownInternal(KeyDownEvent keyEvent)
        {
            Log($"OnKeyDownInternal({keyEvent.keyCode})");
            
            if (keyEvent.keyCode == KeyCode.None)
                return;
            
            InputUtils.KeyDown(keyEvent.keyCode);
        }

        void OnKeyUpInternal(KeyUpEvent keyEvent)
        {
            Log($"OnKeyUpInternal({keyEvent.keyCode})");
            
            if (keyEvent.keyCode == KeyCode.None)
                return;
            
            InputUtils.KeyUp(keyEvent.keyCode);
        }
        
        

        // [Section] Event Processing

        /// <summary>
        /// This method processes the down event sent to the target Element.
        /// </summary>
        /// <param name="evt"> The event to process.</param>
        /// <param name="localPosition"> The local position of the pointer.</param>
        /// <param name="pointerId"> The pointer id of the pointer.</param>
        /// <param name="buttonId"> The button id of the pointer.</param>
        void ProcessDownEvent(EventBase evt, Vector2 localPosition,  int pointerId, int buttonId)
        {
            IsPointerDown = true;
            PointerId = pointerId;
            LastDownButton = buttonId;
            CapturePointer(pointerId);
            DeltaPosition = Vector2.zero;
            
            // Update Local and Global Positions
            LastLocalPosition = LastPosition;
            LocalPosition = localPosition;
            LastPosition = Position;
            Position = (evt is PointerDownEvent e) ? e.position : ((MouseDownEvent)evt).mousePosition;
            
            // Remember where and when the mouse was pressed down
            LastDownPosition = Position;
            LastDownLocalPosition = LocalPosition;
            LastDownTime = DateTime.Now;
            
            HasMoved = false;

            OnPointerDown?.Invoke(this);
        }


/// <summary>
        /// This method processes the up event sent to the target Element.
        /// </summary>
        /// <param name="evt"> The event to process.</param>
        /// <param name="localPosition"> The local position of the pointer.</param>
        /// <param name="pointerId"> The pointer id of the pointer.</param>
        void ProcessUpEvent(EventBase evt, Vector2 localPosition, int pointerId)
        {
            IsPointerDown = false;
            DeltaPosition = Vector2.zero;
            LocalPosition = localPosition;
            Position = (evt is PointerUpEvent e) ? e.position : ((MouseUpEvent)evt).mousePosition;
            
            OnPointerUp?.Invoke(this);
            
            if (IsDragging)
            {
                EndDrag();
            }
            else
            {
                var timeElapsed = DateTime.Now - LastDownTime;

                if (timeElapsed.TotalMilliseconds < 400)
                {
                    Click();
                }
            }
        }

        void Click()
        {
            if (LastDownButton == 0)
            {
                OnClick?.Invoke(this);
            }
            else if (LastDownButton == 1)
            {
                OnContextClick?.Invoke(this);
            }
        }

/// <summary>
        /// This method processes the move event sent to the target Element.
        /// </summary>
        /// <param name="evt"> The event to process.</param>
        /// <param name="localPosition"> The local position of the pointer.</param>
        void ProcessMoveEvent(EventBase evt, Vector2 localPosition)
        {
            LastLocalPosition = LocalPosition;
            LocalPosition = localPosition;
            LastPosition = Position;
            Position = (evt is PointerMoveEvent e) ? e.position : ((MouseMoveEvent)evt).mousePosition;
            
            if (!HasMoved)
            {
                DeltaPosition = Position - LastPosition;
                
                if (IsPointerDown)
                {
                    Drag();
                }
            }

            HasMoved = true;
            OnPointerMove?.Invoke(this);
        }
        

        // [Section] Mouse Callbacks

        void OnMouseMoveInternal(MouseMoveEvent evt)
        {
            //Log("OnMouseMoveInternal");
            ProcessMoveEvent(evt, evt.localMousePosition);
            evt.StopPropagation();
        }

        void OnMouseDownInternal(MouseDownEvent evt)
        {
            if (IsPointerInside)
            {
                if (!target.HasMouseCapture())
                    target.CaptureMouse();
            }

            evt.StopPropagation();
        }

        // [Section] Pointer Callbacks

        public void ReleasePointer()
        {
            if (PointerId != -1)
            {
                if (target.HasPointerCapture(PointerId))
                {
                    target.ReleasePointer(PointerId);
                }
            }
        }

        void CapturePointer(int pointerId)
        {
            if (!target.HasPointerCapture(pointerId))
            {
                target.CapturePointer(pointerId);
            }
            
#if !UNITY_2023_1_OR_NEWER
            if (pointerId == UnityEngine.UIElements.PointerId.mousePointerId)
            {
                if (!target.HasMouseCapture())
                    target.CaptureMouse();
            }
#endif
        }

        /// <summary>
        /// Cancels the drag.
        /// </summary>
        public void Cancel()
        {
            Log("Cancel");
            ReleasePointer();
            IsDragging = false;
            IsPointerDown = false;
            PointerId = -1;
            OnCancelDrag?.Invoke(this);
        }

        void OnPointerCancel(PointerCancelEvent evt)
        {
            Log("OnPointerCancel");
            if (evt.pointerId == PointerId)
                Cancel();
        }

        void OnPointerCapture(PointerCaptureEvent evt)
        {
            Log("OnPointerCapture");
        }

        void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            Log("OnPointerCaptureOut");
            if (evt.pointerId == PointerId)
                Cancel();
        }

        void OnPointerDownInternal(PointerDownEvent evt)
        {
            ProcessDownEvent(evt, evt.localPosition, evt.pointerId, evt.button);
            evt.StopPropagation();
        }

        void OnPointerMoveInternal(PointerMoveEvent evt)
        {
            //Log("OnPointerMoveInternal");
            ClickCount = evt.clickCount;
            ProcessMoveEvent(evt, evt.localPosition);
            evt.StopPropagation();
        }

        void OnPointerUpInternal(PointerUpEvent evt)
        {
            ClickCount = evt.clickCount;
            ProcessUpEvent(evt, LocalPosition, evt.pointerId);
            evt.StopPropagation();
        }

        void OnPointerLeftInternal(PointerLeaveEvent evt)
        {
            IsPointerInside = false;
            
            if (!IsDragging)
            {
                ReleasePointer();
            }
            
            OnPointerLeave?.Invoke(this);
        }

        void OnPointerEnterInternal(PointerEnterEvent evt)
        {
            IsPointerInside = true;
            OnPointerEnter?.Invoke(this);
        }

        void OnClickInternal(ClickEvent evt)
        {
            ClickCount = evt.clickCount;
            OnClick?.Invoke(this);
        }
        
        void OnContextClickInternal(ContextClickEvent evt)
        {
            ClickCount = evt.clickCount;
            OnContextClick?.Invoke(this);
        }

        void OnWheelInternal(WheelEvent evt)
        {
            OnWheel?.Invoke(evt);
        }

        // [Section] Drag Methods

        void BeginDrag()
        {
            Assert.IsFalse(IsDragging, "Cannot begin a new drag, IsDragging is already True.");
            IsDragging = true;
            DragStartLocalPosition = LastLocalPosition;
            DragStartPosition = LastPosition;
            
            OnBeginDrag?.Invoke(this);
        }

        void Drag()
        {
            if (!IsDragging)
                BeginDrag();

            OnDrag?.Invoke(this);
        }

        void EndDrag()
        {
            IsDragging = false;
            ReleasePointer();
            OnEndDrag?.Invoke(this);
        }

        // [Section] Debugging

        void Log(string msg)
        {
            if (!ApplicationConstants.DebugViewportManipulatorEvents)
                return;

            Debug.Log(GetType().Name + " -> " + msg);
        }

        
    }
}
