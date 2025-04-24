using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class RingControlView : ControlView, IBeginDragHandler, IEndDragHandler, IDragHandler, ICancelHandler, IDeselectHandler
    {
        RingControlViewModel m_Model;
        GameObject m_RingObject;
        CapsuleCollider[] m_SegmentColliders;
        LineRenderer m_FrontLine;
        LineRenderer m_BackLine;
        LineRenderer m_DragLineShape;

        const int k_NbColliderSegments = 32;
        const int k_NbLineSegments = 64;
        const float k_ColliderPipeRadius = 0.032f;
        const float k_DepthCutoffPoint = -0.02f;
        const float k_SegmentAngleSpan = 2 * Mathf.PI / k_NbColliderSegments;
        const float k_SegmentAngleSpanHalf = k_SegmentAngleSpan / 2f;

        float ColliderRadius => k_ColliderPipeRadius * m_Model.ThicknessRatio;
        
        public void SetModel(RingControlViewModel model)
        {
            base.SetModel(model);
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            UpdateAll();

            m_Model.OnLocalAxisChanged += OnLocalAxisChanged;
            m_Model.OnColorSchemeChanged += OnColorSchemeChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            for (var i = 0; i < m_SegmentColliders.Length; i++)
            {
                var capsuleCollider = m_SegmentColliders[i];
                capsuleCollider.enabled = false;
            }

            m_FrontLine.enabled = false;
            m_BackLine.enabled = false;
            m_DragLineShape.enabled = false;

            m_Model.OnLocalAxisChanged -= OnLocalAxisChanged;
            m_Model.OnColorSchemeChanged -= OnColorSchemeChanged;
        }

        // [Section] HandleElementView Methods Overrides

        public override void Step(float delta)
        {
            base.Step(delta);
            
            if (m_Model.IsDragging)
            {
                var dragLineTransform = m_DragLineShape.transform;
                m_DragLineShape.SetPosition(0, dragLineTransform.InverseTransformPoint(m_Model.DragStartPointerPosition));
                m_DragLineShape.SetPosition(1, dragLineTransform.InverseTransformPoint(m_Model.DragPreviousAxisPosition));
            }
        }

        public override void CreateShapesAndColliders()
        {
            base.CreateShapesAndColliders();
            
            m_SegmentColliders = new CapsuleCollider[k_NbColliderSegments];
            m_RingObject = CreateChild("Ring");
            
            var frontLineObject = CreateChild("FrontDisc", m_RingObject.transform);
            var backLineObject = CreateChild("BackDisc", m_RingObject.transform);
            
            m_FrontLine = HandlesUtils.CreateLine(frontLineObject, "Line");
            m_BackLine = HandlesUtils.CreateLine(backLineObject, "Line");
            
            var positionsFront = new Vector3[k_NbLineSegments];
            var positionsBack = new Vector3[k_NbLineSegments];

            for (var i = 0; i < k_NbLineSegments; i++)
            {
                positionsFront[i] = Vector3.zero;
                positionsBack[i] = Vector3.zero;
            }

            m_FrontLine.positionCount = k_NbLineSegments;
            m_BackLine.positionCount = k_NbLineSegments;

            m_FrontLine.SetPositions(positionsFront);
            m_BackLine.SetPositions(positionsBack);
            
            var dragLineObject = CreateChild("DragLine", transform);
            m_DragLineShape = HandlesUtils.CreateLine(dragLineObject, "Line");
            m_DragLineShape.SetPositions(new[] { Vector3.zero, Vector3.zero });

            for (var i = 0; i < k_NbColliderSegments; i++)
            {
                CreateSegmentCollider(i);
            }
        }

        void CreateSegmentCollider(int segmentIndex)
        {
            // Create the segment's gameObject
            var segmentObject = CreateChild("Segment", m_RingObject.transform);
            var segmentTransform = segmentObject.transform;
            segmentTransform.localRotation = Quaternion.AngleAxis(Mathf.Rad2Deg * (segmentIndex + 0.5f) * k_SegmentAngleSpan, Vector3.forward);

            // Create the segment's collider
            var capsuleCollider = CreateCollider<CapsuleCollider>(segmentObject);
            capsuleCollider.radius = k_ColliderPipeRadius;
            capsuleCollider.enabled = false;

            m_SegmentColliders[segmentIndex] = capsuleCollider;
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            
            // Set the orientation based on the LocalRingRotation of the model
            m_RingObject.transform.localRotation = m_Model.LocalRingRotation;
            
            // Set the scale of things
            transform.localScale = Vector3.one * m_Model.SizeRatio;
            m_FrontLine.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            m_BackLine.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            m_DragLineShape.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            
            // Update the colliders
            var pos1 = m_Model.GetRingLocalPosition(k_SegmentAngleSpanHalf);
            var pos2 = m_Model.GetRingLocalPosition(k_SegmentAngleSpanHalf + k_SegmentAngleSpan);
            var segmentLength = (pos1 - pos2).magnitude + 2f * ColliderRadius;

            for (var i = 0; i < k_NbColliderSegments; i++)
            {
                var capsuleCollider = m_SegmentColliders[i];
                capsuleCollider.height = segmentLength;
                var angleRad = i * k_SegmentAngleSpan;
                capsuleCollider.transform.localPosition = m_Model.GetRingLocalPosition(angleRad + k_SegmentAngleSpanHalf);
                capsuleCollider.radius = ColliderRadius;
            }
        }

        void ComputeBackAndFrontSegmentIndices(out int firstIndex, out int lastIndex, out bool isBackSegment)
        {
            var modelCamera = m_Model.Camera;
            var cameraTransform = modelCamera.Target.transform;

            var centerDepth = cameraTransform.GetRelativeDepth(m_RingObject.transform.position);

            // For optimization we only store the first and last index of the segment
            firstIndex = -1;
            lastIndex = -1;
            isBackSegment = true;

            for (var i = 0; i < m_SegmentColliders.Length; i++)
            {
                var segmentCollider = m_SegmentColliders[i];
                var segmentPosition = segmentCollider.transform.position;
                var segmentDepth = cameraTransform.GetRelativeDepth(segmentPosition);
                var relativeDepth = centerDepth - segmentDepth;

                var isBack = relativeDepth < k_DepthCutoffPoint;

                // If we start in the "back" segment, indices will represent the front segment
                if (isBack && i == 0)
                {
                    isBackSegment = false;
                }

                if (isBackSegment)
                {
                    if (isBack)
                    {
                        if (firstIndex < 0)
                        {
                            firstIndex = i;
                        }

                        lastIndex = i;
                    }
                }
                else
                {
                    if (!isBack)
                    {
                        if (firstIndex < 0)
                        {
                            firstIndex = i;
                        }

                        lastIndex = i;
                    }
                }
            }
        }

        protected override void UpdateColor()
        {
            var colorLine = m_Model.Color;

            m_BackLine.startColor = colorLine;
            m_BackLine.endColor = colorLine;
            m_FrontLine.startColor = colorLine;
            m_FrontLine.endColor = colorLine;
            m_DragLineShape.startColor = colorLine;
            m_DragLineShape.endColor = colorLine;
            
            ResetUpdate(UpdateFlags.Color);
        }

        protected override void UpdateTransform()
        {
            transform.SetPositionAndRotation(m_Model.Position, m_Model.Rotation);

            ResetUpdate(UpdateFlags.Transform);
        }

        protected override void UpdateCamera()
        {
            base.UpdateCamera();
            
            ComputeBackAndFrontSegmentIndices(out var firstIndex, out var lastIndex, out var isBackSegment);
            UpdateEnabledColliders(firstIndex, lastIndex, isBackSegment);

            var firstAngleRad = firstIndex * k_SegmentAngleSpan;
            var lastAngleRad = lastIndex * k_SegmentAngleSpan;

            if (isBackSegment)
            {
                UpdateLineGeometry(m_BackLine, firstAngleRad, lastAngleRad);
                UpdateLineGeometry(m_FrontLine, lastAngleRad, firstAngleRad + 2 * Mathf.PI);
            }
            else
            {
                UpdateLineGeometry(m_FrontLine, firstAngleRad, lastAngleRad);
                UpdateLineGeometry(m_BackLine, lastAngleRad, firstAngleRad + 2 * Mathf.PI);
            }
        }

        void UpdateEnabledColliders(int firstIndex, int lastIndex, bool isBackSegment)
        {
            if (m_Model.ShowRingBack)
                return;

            for (var i = 0; i < k_NbColliderSegments; i++)
            {
                var capsuleCollider = m_SegmentColliders[i];

                if (i >= firstIndex && i < lastIndex)
                {
                    capsuleCollider.enabled = !isBackSegment;
                }
                else
                {
                    capsuleCollider.enabled = isBackSegment;
                }
            }
        }

        void UpdateLineGeometry(LineRenderer line, float firstAngleRad, float lastAngleRad)
        {
            for (var i = 0; i < k_NbLineSegments; i++)
            {
                var t = i / (float)(k_NbLineSegments - 1);
                var angleRad = Mathf.Lerp(firstAngleRad, lastAngleRad, t);
                var position = m_Model.GetRingLocalPosition(angleRad);
                line.SetPosition(i, position);
            }
        }

        protected override void UpdateVisibility()
        {
            base.UpdateVisibility();
            
            var isDragging = m_Model.IsDragging;
            var colliderEnabled = m_Model.IsVisible;
            
            for (var i = 0; i < m_SegmentColliders.Length; i++)
            {
                var capsuleCollider = m_SegmentColliders[i];
                capsuleCollider.enabled = colliderEnabled;
            }

            m_FrontLine.enabled = m_Model.IsVisible || isDragging;
            m_BackLine.enabled = (m_Model.IsVisible && m_Model.ShowRingBack) || isDragging;
            m_DragLineShape.enabled = isDragging && m_Model.DragMode == RingControlViewModel.DragModeType.Rotate;
        }

        // [Section] Video Model Events Handlers

        void OnColorSchemeChanged()
        {
            QueueUpdate(UpdateFlags.Color);
        }

        void OnLocalAxisChanged(Vector3 newAxis)
        {
            QueueUpdate(UpdateFlags.Shape);
        }

        // [Section] User Inputs Events Handlers

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_Model?.OnViewBegunDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_Model?.OnViewEndedDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_Model?.OnViewDragged(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            m_Model?.OnViewCancelled(eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_Model?.OnViewDeselected(eventData);
        }

        
    }
}
