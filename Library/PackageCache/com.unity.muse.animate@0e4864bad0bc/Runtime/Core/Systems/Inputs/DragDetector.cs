using System;
using UnityEngine;
using UnityEngine.UIElements;

class DragDetector : PointerManipulator
{
    /// <summary>
    /// Event that is triggered when a drag operation starts.
    /// </summary>
    public event Action<VisualElement> OnDragStart;

    const float k_DistanceToStartDrag = 5;

    bool m_Enabled;
    int m_LastPointerId = -1;
    Vector3 m_StartPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="DragDetector"/> class.
    /// </summary>
    /// <param name="target">The target <see cref="VisualElement"/> to apply the drag and drop manipulator to.</param>
    public DragDetector()
    {
        m_Enabled = false;
    }

    public DragDetector(VisualElement target)
    {
        this.target = target;
        m_Enabled = false;
    }

    public void SetTarget(VisualElement target)
    {
        this.target = target;
    }

    /// <summary>
    /// Registers the necessary callbacks on the target element.
    /// </summary>
    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    /// <summary>
    /// Unregisters the callbacks from the target element.
    /// </summary>
    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    /// <summary>
    /// Event handler for the pointer down event.
    /// </summary>
    /// <param name="evt">The pointer down event.</param>
    void PointerDownHandler(PointerDownEvent evt)
    {
        if (m_LastPointerId == -1 && evt.button == (int)MouseButton.LeftMouse)
        {
            m_StartPosition = evt.position;
            m_LastPointerId = evt.pointerId;
            target.CapturePointer(m_LastPointerId);
            target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        }
    }


    void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (m_LastPointerId != -1 && target.HasPointerCapture(m_LastPointerId) && !m_Enabled)
        {
            if ((evt.position - m_StartPosition).magnitude > k_DistanceToStartDrag)
            {
                m_Enabled = true;
                OnDragStart?.Invoke(target);
                HandleDragged(m_LastPointerId);
            }
        }
    }
    
    void PointerUpHandler(PointerUpEvent evt)
    {
        target.ReleasePointer(evt.pointerId);
        m_LastPointerId = -1;
    }

    /// <summary>
    /// Handles the end of a drag operation.
    /// </summary>
    /// <param name="pointerId">The pointer ID of the drag operation.</param>
    void HandleDragged(int pointerId)
    {
        if (m_Enabled && target.HasPointerCapture(pointerId) && pointerId == m_LastPointerId)
        {
            target.ReleasePointer(pointerId);
            m_LastPointerId = -1;
            m_Enabled = false;
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        }
    }
}
