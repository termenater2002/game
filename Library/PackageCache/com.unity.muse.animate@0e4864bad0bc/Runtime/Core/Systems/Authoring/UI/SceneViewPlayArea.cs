using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{

#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class SceneViewPlayArea : Image
    {
        RenderTexture m_RenderTexture;
        AuthorContext m_AuthorContext;
        PointerEventData m_PointerEventData;

        GameObject m_PointerTarget = null;
        GameObject m_PointerDragTarget = null;

        PointerEventData.InputButton m_PointerDragButton;

        List<RaycastResult> m_RaycastResults = new();
        List<RaycastResult> m_RaycastResultsFiltered = new();

        SceneViewManipulator SceneViewManipulator { get; }

        RenderTexture TargetRenderTexture
        {
            get => m_RenderTexture;
            set
            {
                if (m_RenderTexture == value)
                    return;

                if (m_RenderTexture)
                {
                    m_AuthorContext?.CameraContext.CameraModel.ClearPermanentRenderTextureTarget();
                    m_RenderTexture.Release();
                    UnityObject.Destroy(m_RenderTexture);
                }

                m_RenderTexture = value;
                image = m_RenderTexture;
            }
        }

        public SceneViewPlayArea()
        {
            m_PointerEventData = new PointerEventData(EventSystem.current);
            SceneViewManipulator = new SceneViewManipulator();

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            this.AddManipulator(SceneViewManipulator);
            SceneViewManipulator.target = this;

            SceneViewManipulator.OnPointerEnter += OnPointerEnter;
            SceneViewManipulator.OnPointerMove += OnPointerMove;
            SceneViewManipulator.OnPointerLeave += OnPointerLeave;
            SceneViewManipulator.OnPointerDown += OnPointerDown;
            SceneViewManipulator.OnPointerUp += OnPointerUp;
            SceneViewManipulator.OnClick += OnClicked;
            SceneViewManipulator.OnContextClick += OnContextClicked;
            SceneViewManipulator.OnWheel += OnWheel;
            SceneViewManipulator.OnBeginDrag += OnDragBegin;
            SceneViewManipulator.OnDrag += OnDrag;
            SceneViewManipulator.OnEndDrag += OnDragEnd;
            SceneViewManipulator.OnKeyHold += OnKeyHold;
            SceneViewManipulator.OnKeyDown += OnKeyDown;
            SceneViewManipulator.OnKeyUp += OnKeyUp;

            UpdateTextureDimensions();
            Update();
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            this.RemoveManipulator(SceneViewManipulator);
            SceneViewManipulator.target = null;

            SceneViewManipulator.OnPointerEnter -= OnPointerEnter;
            SceneViewManipulator.OnPointerMove -= OnPointerMove;
            SceneViewManipulator.OnPointerLeave -= OnPointerLeave;
            SceneViewManipulator.OnPointerDown -= OnPointerDown;
            SceneViewManipulator.OnPointerUp -= OnPointerUp;
            SceneViewManipulator.OnClick -= OnClicked;
            SceneViewManipulator.OnContextClick -= OnContextClicked;
            SceneViewManipulator.OnWheel -= OnWheel;
            SceneViewManipulator.OnBeginDrag -= OnDragBegin;
            SceneViewManipulator.OnDrag -= OnDrag;
            SceneViewManipulator.OnEndDrag -= OnDragEnd;
            SceneViewManipulator.OnKeyHold -= OnKeyHold;
            SceneViewManipulator.OnKeyDown -= OnKeyDown;
            SceneViewManipulator.OnKeyUp -= OnKeyUp;
        }

        void OnWheel(WheelEvent obj)
        {
            m_AuthorContext.CameraContext.CameraMovementModel.DollyScroll(-obj.delta.y);
        }

        public void SetContext(AuthorContext context)
        {
            Clear();
            m_AuthorContext = context;
            
            if (m_AuthorContext != null)
            {
                RegisterToCamera();
                Update();
            }
            else
            {
                ClearContext();
            }
        }

        public void ClearContext()
        {
            if (m_AuthorContext == null)
                return;

            Clear();
        }

        public void Update()
        {
            SceneViewManipulator.Update();
        }

        public void LateUpdate()
        {

        }

        public void Render()
        {
            if (m_AuthorContext == null)
                return;

            UpdateTextureDimensions();
            m_AuthorContext.CameraContext.CameraModel.RenderToTexture(TargetRenderTexture, true);
        }

        static RenderTexture CreateDummyTexture() => new(8, 8, 0) { hideFlags = HideFlags.HideAndDontSave };

        void UpdateTextureDimensions()
        {
            var dpi = Vector2.one * Mathf.Max(Unity.AppUI.Core.Platform.scaleFactor, 1f);

            if (m_AuthorContext == null)
            {
                TargetRenderTexture = CreateDummyTexture();
                return;
            }

            dpi.x = Mathf.Max(dpi.x, m_AuthorContext.Camera.RenderScaling.x, Unity.AppUI.Core.Platform.scaleFactor);
            dpi.y = Mathf.Max(dpi.y, m_AuthorContext.Camera.RenderScaling.y, Unity.AppUI.Core.Platform.scaleFactor);

            var w = (int)(m_AuthorContext.Camera.ViewportSize.x * dpi.x);
            var h = (int)(m_AuthorContext.Camera.ViewportSize.y * dpi.y);

            // Prevent < 0 dimensions
            if (w <= 0 || h <= 0)
            {
                TargetRenderTexture = CreateDummyTexture();
                return;
            }

            var needToUpdate = !TargetRenderTexture
                || TargetRenderTexture.width != w
                || TargetRenderTexture.height != h;

            if (needToUpdate)
            {
                TargetRenderTexture = m_AuthorContext != null
                    ? new RenderTexture(w, h, 24)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    }
                    : CreateDummyTexture();
            }
        }

        void OnCameraViewportRenderScalingChanged(CameraModel model, Vector2 scaling)
        {
            UpdateTextureDimensions();
        }

        void OnCameraViewportSizeChanged(CameraModel model, Vector2 size)
        {
            UpdateTextureDimensions();
        }

        void OnPointerMove(SceneViewManipulator manipulator)
        {
            UpdatePointerEvent();
            UpdateTarget();
        }

        void OnPointerLeave(SceneViewManipulator manipulator)
        {
            if (ApplicationConstants.DebugViewportPointerEvents)
                Log($"OnPointerLeave()");

            UpdatePointerEvent();

            //ClearTarget();
        }

        void OnPointerEnter(SceneViewManipulator manipulator)
        {
            if (ApplicationConstants.DebugViewportPointerEvents)
                Log($"OnPointerEnter()");

            UpdatePointerEvent();
        }

        void OnPointerDown(SceneViewManipulator manipulator)
        {
            if (ApplicationConstants.DebugViewportPointerEvents)
                Log($"OnPointerDown()");

            UpdatePointerEvent();

            if (m_PointerTarget == null)
                return;

            m_PointerEventData.pointerPressRaycast = m_PointerEventData.pointerCurrentRaycast;
            m_PointerEventData.pointerPress = m_PointerTarget;

            PointerEvent<IPointerDownHandler>(m_PointerTarget);
        }

        void OnPointerUp(SceneViewManipulator manipulator)
        {
            if (ApplicationConstants.DebugViewportPointerEvents)
                Log($"OnPointerUp()");

            UpdatePointerEvent();

            if (m_PointerTarget == null)
                return;

            PointerEvent<IPointerUpHandler>(m_PointerTarget);
        }

        void OnClicked(SceneViewManipulator manipulator)
        {
            UpdatePointerEvent();
            if (ApplicationConstants.DebugViewportPointerEvents)
                Log($"OnClicked()");

            if (m_PointerTarget == null)
                return;

            PointerEvent<IPointerClickHandler>(m_PointerTarget);
        }

        void OnContextClicked(SceneViewManipulator manipulator)
        {
            UpdatePointerEvent();
            if (ApplicationConstants.DebugViewportPointerEvents)
                Log($"OnContextClicked()");

            if (m_PointerTarget == null)
                return;

            PointerEvent<IPointerClickHandler>(m_PointerTarget);
        }

        void OnCameraContextMenuOpen(CameraModel cameraModel)
        {
            SceneViewManipulator.ReleasePointer();
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (m_AuthorContext == null)
                return;

            m_AuthorContext.Camera.ViewportPosition = new Vector2((int)worldBound.x, (int)worldBound.y);
            m_AuthorContext.Camera.ViewportSize = new Vector2((int)worldBound.size.x, (int)worldBound.size.y);
        }

        void OnDragBegin(SceneViewManipulator manipulator)
        {
            if (ApplicationConstants.DebugViewportPointerEvents)
                Log($"OnDragBegin()");

            UpdatePointerEvent();

            // Remember the drag button
            m_PointerDragButton = (PointerEventData.InputButton)SceneViewManipulator.LastDownButton;

            // Overwrite the button in the PointerEventData
            m_PointerEventData.button = m_PointerDragButton;

            // Note: Overwrite the position of the PointerEventData in order to
            // select the drag target from where the drag actually started
            // (where was the mouse pressed down initially)
            m_PointerEventData.position = ViewportPixelToCameraScreenPosition(SceneViewManipulator.LastDownLocalPosition);

            // Pick the drag target with a raycast
            m_PointerDragTarget = GetViewportTargetWithComponent<IDragHandler>(m_PointerEventData, out var resultDraggable);

            if (m_PointerDragTarget == null)
                return;

            // Save the drag target in the PointerEventData
            m_PointerEventData.pointerDrag = m_PointerDragTarget;

            // Dispatch the event to the target
            PointerEvent<IBeginDragHandler>(m_PointerDragTarget);
        }

        void OnDrag(SceneViewManipulator manipulator)
        {
            UpdatePointerEvent();
            m_PointerEventData.button = m_PointerDragButton;

            if (ApplicationConstants.DebugViewportPointerEvents)
                Log($"OnDrag({SceneViewManipulator.DeltaPosition})");

            if (m_PointerDragTarget == null)
                return;

            PointerEvent<IDragHandler>(m_PointerDragTarget);
        }

        void OnDragEnd(SceneViewManipulator manipulator)
        {
            if (ApplicationConstants.DebugViewportPointerEvents)
                Log($"OnDragEnd()");

            UpdatePointerEvent();
            m_PointerEventData.button = m_PointerDragButton;

            // If the pointer was dragging a target, send an end of drag event
            if (m_PointerDragTarget == null)
                return;

            PointerEvent<IEndDragHandler>(m_PointerDragTarget);
        }

        void RegisterToCamera()
        {
            Assert.IsNotNull(m_AuthorContext, "Could not register to camera, m_AuthorContext is null.");
            m_AuthorContext.CameraContext.CameraModel.OnViewportRenderScalingChanged += OnCameraViewportRenderScalingChanged;
            m_AuthorContext.CameraContext.CameraModel.OnViewportSizeChanged += OnCameraViewportSizeChanged;
            m_AuthorContext.CameraContext.CameraModel.OnContextMenu += OnCameraContextMenuOpen;

            m_AuthorContext.Camera.ViewportSize = new Vector2((int)worldBound.size.x, (int)worldBound.size.y);
            m_AuthorContext.Camera.ViewportPosition = new Vector2((int)worldBound.x, (int)worldBound.y);
        }

        void UnregisterFromCamera()
        {
            if (m_AuthorContext == null)
                return;

            m_AuthorContext.CameraContext.CameraModel.OnViewportRenderScalingChanged -= OnCameraViewportRenderScalingChanged;
            m_AuthorContext.CameraContext.CameraModel.OnViewportSizeChanged -= OnCameraViewportSizeChanged;
            m_AuthorContext.CameraContext.CameraModel.OnContextMenu -= OnCameraContextMenuOpen;
        }

        void UpdatePointerEvent()
        {
            if (m_AuthorContext == null || !m_AuthorContext.Camera.Target) return;

            m_PointerEventData ??= new PointerEventData(EventSystem.current);
            m_PointerEventData.Reset();
            m_PointerEventData.pointerId = SceneViewManipulator.PointerId;
            m_PointerEventData.dragging = SceneViewManipulator.IsDragging;
            m_PointerEventData.button = (PointerEventData.InputButton)SceneViewManipulator.LastDownButton;
            m_PointerEventData.delta = new Vector2(SceneViewManipulator.DeltaPosition.x, -SceneViewManipulator.DeltaPosition.y);
            var position = ViewportPixelToCameraScreenPosition(SceneViewManipulator.LocalPosition);
            m_PointerEventData.position = position;
            m_PointerEventData.clickCount = SceneViewManipulator.ClickCount;

            if (m_AuthorContext == null)
                return;

            var viewportRatio = m_AuthorContext.Camera.Target.ScreenToViewportPoint(position);
            var viewportPixel = new Vector2(viewportRatio.x * m_AuthorContext.Camera.ViewportSize.x, viewportRatio.y * m_AuthorContext.Camera.ViewportSize.y);

            m_AuthorContext.Camera.ViewportCursor = viewportPixel;
        }

        void UpdateTarget()
        {
            if (SceneViewManipulator.IsDragging)
                return;

            var target = GetViewportTarget(m_PointerEventData, out var result);

            m_PointerEventData.pointerCurrentRaycast = result;

            if (m_PointerTarget != target)
            {
                ClearTarget();
                m_PointerTarget = target;

                if (m_PointerTarget != null)
                {
                    m_PointerEventData.pointerCurrentRaycast = result;
                    PointerEvent<IPointerEnterHandler>(m_PointerTarget);
                }
            }

            if (m_PointerTarget != null)
            {
                PointerEvent<IPointerMoveHandler>(m_PointerTarget);
            }
        }

        void PointerEvent<T>(GameObject target) where T : IEventSystemHandler
        {
            var suffix = target.name;
            m_PointerEventData.Reset();

            while (true)
            {
                var components = target.GetComponents<T>();

                if (components.Length > 0)
                {
                    foreach (var handler in components)
                    {
                        if (ApplicationConstants.DebugViewportPointerEvents)
                            Log($"PointerEvent<{typeof(T).Name}>() -> {suffix}.{handler.GetType().Name}");

                        if (!m_PointerEventData.used)
                            SendEventToHandler(handler);
                    }
                }
                else
                {
                    if (target.transform.parent != null)
                    {
                        target = target.transform.parent.gameObject;
                        suffix = suffix + "." + target.name;
                        continue;
                    }
                }

                break;
            }
        }

        static T[] FindHandlersInTargetOrParents<T>(GameObject target) where T : IEventSystemHandler
        {
            var result = Array.Empty<T>();

            while (true)
            {
                var components = target.GetComponents<T>();

                if (components.Length > 0)
                {
                    result = components;
                }
                else
                {
                    if (target.transform.parent != null)
                    {
                        target = target.transform.parent.gameObject;
                        continue;
                    }
                }

                break;
            }

            return result;
        }

        void SendEventToHandler<T>(T handler) where T : IEventSystemHandler
        {
            var t = typeof(T);

            if (t == typeof(IPointerClickHandler))
            {
                ((IPointerClickHandler)handler).OnPointerClick(m_PointerEventData);
            }
            else if (t == typeof(IPointerDownHandler))
            {
                ((IPointerDownHandler)handler).OnPointerDown(m_PointerEventData);
            }
            else if (t == typeof(IPointerUpHandler))
            {
                ((IPointerUpHandler)handler).OnPointerUp(m_PointerEventData);
            }
            else if (t == typeof(IPointerEnterHandler))
            {
                ((IPointerEnterHandler)handler).OnPointerEnter(m_PointerEventData);
            }
            else if (t == typeof(IPointerExitHandler))
            {
                ((IPointerExitHandler)handler).OnPointerExit(m_PointerEventData);
            }
            else if (t == typeof(IPointerMoveHandler))
            {
                ((IPointerMoveHandler)handler).OnPointerMove(m_PointerEventData);
            }
            else if (t == typeof(IBeginDragHandler))
            {
                ((IBeginDragHandler)handler).OnBeginDrag(m_PointerEventData);
            }
            else if (t == typeof(IEndDragHandler))
            {
                ((IEndDragHandler)handler).OnEndDrag(m_PointerEventData);
            }
            else if (t == typeof(IDragHandler))
            {
                ((IDragHandler)handler).OnDrag(m_PointerEventData);
            }
        }

        static bool SetTargetToParent<T>(ref GameObject target, ref string suffix) where T : IEventSystemHandler
        {
            if (target.transform.parent != null)
            {
                target = target.transform.parent.gameObject;
                suffix = suffix + "." + target.name;
                return true;
            }

            return false;
        }

        void OnKeyDown(KeyPressEvent keyEvent)
        {
            if (ApplicationConstants.DebugViewportKeyboardEvents)
                Log($"OnKeyDown({keyEvent.KeyCode})");

            Application.Instance.ApplicationFlow.SendKeyDownEvent(keyEvent);
        }

        void OnKeyUp(KeyPressEvent keyEvent)
        {
            if (ApplicationConstants.DebugViewportKeyboardEvents)
                Log($"OnKeyUp({keyEvent.KeyCode})");

            Application.Instance.ApplicationFlow.SendKeyUpEvent(keyEvent);
        }

        void OnKeyHold(KeyPressEvent keyEvent)
        {
            if (ApplicationConstants.DebugViewportKeyboardEvents)
                Log($"OnKeyHold({keyEvent.KeyCode})");

            Application.Instance.ApplicationFlow.SendKeyHoldEvent(keyEvent);
        }

        void ClearTarget()
        {
            if (m_PointerTarget != null)
            {
                PointerEvent<IPointerExitHandler>(m_PointerTarget);
            }

            m_PointerTarget = null;
        }

        Vector3 ViewportPixelToCameraScreenPosition(Vector3 localPosition)
        {
            if (m_AuthorContext == null)
            {
                return localPosition;
            }

            var camera = m_AuthorContext.Camera.Target;
            var viewportSize = new Vector3(m_AuthorContext.Camera.ViewportSize.x, m_AuthorContext.Camera.ViewportSize.y, 1f);
            var viewportPosition = new Vector3(localPosition.x / viewportSize.x, localPosition.y / viewportSize.y, 0f);
            var screenSize = camera.ViewportToScreenPoint(Vector3.one);
            var screenPosition = new Vector3(viewportPosition.x * screenSize.x, screenSize.y - (viewportPosition.y * screenSize.y), 0f);

            return screenPosition;
        }

        public new void Clear()
        {
            base.Clear();
            TargetRenderTexture = null;
            UnregisterFromCamera();
            m_AuthorContext = null;
        }

        GameObject GetViewportTarget(PointerEventData pointerEventData, out RaycastResult raycastResult)
        {
            if (m_AuthorContext == null)
            {
                raycastResult = m_EmptyResult;
                return null;
            }

            GameObject target = null;

            m_RaycastResults.Clear();
            var raycaster = m_AuthorContext.CameraContext.CameraModel.Target.GetComponent<CustomPhysicsRaycaster>();

            raycaster.Raycast(pointerEventData, m_RaycastResults, true);
            m_RaycastResults.Sort(s_RaycastComparer);

            if (m_RaycastResults.Count > 0)
            {
                var result = m_RaycastResults[0];
                raycastResult = result;
                target = result.gameObject;

                if (ApplicationConstants.DebugViewportPointerEvents)
                    Log($"GetViewportTarget() -> {pointerEventData.position} - {target.name}");
            }
            else
            {
                raycastResult = m_EmptyResult;

                if (ApplicationConstants.DebugViewportPointerEvents)
                    Log($"GetViewportTarget() -> {pointerEventData.position} - No target hit");
            }

            return target;
        }

        GameObject GetViewportTargetWithComponent<T>(PointerEventData pointerEventData, out RaycastResult raycastResult) where T : IEventSystemHandler
        {
            if (m_AuthorContext == null)
            {
                raycastResult = m_EmptyResult;
                return null;
            }

            GameObject target = null;

            m_RaycastResults.Clear();
            m_RaycastResultsFiltered.Clear();

            var raycaster = m_AuthorContext.CameraContext.CameraModel.Target.GetComponent<CustomPhysicsRaycaster>();

            raycaster.Raycast(pointerEventData, m_RaycastResults, true);

            if (m_RaycastResults.Count > 0)
            {
                m_RaycastResults.Sort(s_RaycastComparer);

                for (int i = 0; i < m_RaycastResults.Count; i++)
                {
                    var result = m_RaycastResults[i];
                    var handlers = FindHandlersInTargetOrParents<T>(result.gameObject);

                    if (handlers.Length > 0)
                    {
                        m_RaycastResultsFiltered.Add(result);
                    }
                }

                if (m_RaycastResultsFiltered.Count > 0)
                {
                    raycastResult = m_RaycastResultsFiltered[0];
                    target = m_RaycastResultsFiltered[0].gameObject;
                }
                else
                {
                    raycastResult = m_EmptyResult;

                    if (ApplicationConstants.DebugViewportPointerEvents)
                        Log($"GetViewportTargetWithComponent<{typeof(T)}>() -> {pointerEventData.position} - No target had the correct component.");
                }

                if (ApplicationConstants.DebugViewportPointerEvents)
                    Log($"GetViewportTargetWithComponent<{typeof(T)}>() -> {pointerEventData.position} - {target.name}");
            }
            else
            {
                raycastResult = m_EmptyResult;

                if (ApplicationConstants.DebugViewportPointerEvents)
                    Log($"GetViewportTargetWithComponent<{typeof(T)}>() -> {pointerEventData.position} - No target hit");
            }

            return target;
        }

        void Log(string msg)
        {
            Debug.Log(GetType().Name + " -> " + msg);
        }

        // [Section] Raycasting

        static readonly Comparison<RaycastResult> s_RaycastComparer = RaycastComparer;
        RaycastResult m_EmptyResult = new();

        private static int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
        {
            if (lhs.module != rhs.module)
            {
                var lhsEventCamera = lhs.module.eventCamera;
                var rhsEventCamera = rhs.module.eventCamera;
                if (lhsEventCamera != null && rhsEventCamera != null && lhsEventCamera.depth != rhsEventCamera.depth)
                {
                    // need to reverse the standard compareTo
                    if (lhsEventCamera.depth < rhsEventCamera.depth)
                        return 1;
                    if (lhsEventCamera.depth == rhsEventCamera.depth)
                        return 0;

                    return -1;
                }

                if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
                    return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);

                if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
                    return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
            }

            // Renderer sorting
            if (lhs.sortingLayer != rhs.sortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
                var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
                return rid.CompareTo(lid);
            }

            if (lhs.sortingOrder != rhs.sortingOrder)
                return rhs.sortingOrder.CompareTo(lhs.sortingOrder);

            // comparing depth only makes sense if the two raycast results have the same root canvas (case 912396)
            if (lhs.depth != rhs.depth && lhs.module.rootRaycaster == rhs.module.rootRaycaster)
                return rhs.depth.CompareTo(lhs.depth);

            if (lhs.distance != rhs.distance)
                return lhs.distance.CompareTo(rhs.distance);

#if PACKAGE_PHYSICS2D
			// Sorting group
            if (lhs.sortingGroupID != SortingGroup.invalidSortingGroupID && rhs.sortingGroupID != SortingGroup.invalidSortingGroupID)
            {
                if (lhs.sortingGroupID != rhs.sortingGroupID)
                    return lhs.sortingGroupID.CompareTo(rhs.sortingGroupID);
                if (lhs.sortingGroupOrder != rhs.sortingGroupOrder)
                    return rhs.sortingGroupOrder.CompareTo(lhs.sortingGroupOrder);
            }
#endif

            return lhs.index.CompareTo(rhs.index);
        }

        


#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<SceneViewPlayArea, UxmlTraits> { }
#endif
    }
}
