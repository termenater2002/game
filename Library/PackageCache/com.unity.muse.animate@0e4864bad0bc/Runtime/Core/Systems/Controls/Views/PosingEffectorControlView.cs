using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    class PosingEffectorControlView : ControlView,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ICancelHandler, IDeselectHandler
    {
        const float k_EffectorDotRadius = 0.5f;
        const float k_EffectorElasticDotRadius = 0.3f;
        const float k_LookAtDirectionLength = 0.2f;
        const float k_EffectorScale = 0.1f;

        PosingEffectorControlViewModel m_Model;

        DiscMesh m_DotShape;
        DiscMesh m_ElasticDiscShape;
        DiscMesh m_ToleranceShape;
        ConeMesh m_LookAtTip;

        LineRenderer m_LookAtTargetHorizontalShape;
        LineRenderer m_LookAtTargetVerticalShape;
        LineRenderer m_ElasticLineShape;
        LineRenderer m_LookAtLine;
        
        SphereCollider m_Collider;

        // [Section] Model Registration

        public void SetModel(PosingEffectorControlViewModel model)
        {
            base.SetModel(model);
            
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void RegisterModel()
        {
            QueueUpdateAll();

            if (m_Model == null)
                return;

            m_Model.OnEffectorChanged += OnEffectorChanged;
            m_Model.OnJointTransformChanged += OnJointTransformChanged;
        }

        void UnregisterModel()
        {
            m_DotShape.enabled = false;
            m_LookAtTargetHorizontalShape.enabled = false;
            m_LookAtTargetVerticalShape.enabled = false;
            m_ElasticLineShape.enabled = false;
            m_ElasticDiscShape.enabled = false;
            m_LookAtLine.enabled = false;
            m_LookAtTip.enabled = false;
            m_ToleranceShape.enabled = false;
            m_Collider.enabled = false;

            if (m_Model == null)
                return;

            m_Model.OnEffectorChanged -= OnEffectorChanged;
            m_Model.OnJointTransformChanged -= OnJointTransformChanged;
        }

        // [Section] View Model Events Handlers

        void OnJointTransformChanged()
        {
            QueueUpdate(UpdateFlags.Transform);
            QueueUpdate(UpdateFlags.Camera);
            QueueUpdate(UpdateFlags.Shape);
            QueueUpdate(UpdateFlags.Alpha);
            QueueUpdate(UpdateFlags.Color);
        }

        void OnEffectorChanged()
        {
            QueueUpdate(UpdateFlags.Transform);
            QueueUpdate(UpdateFlags.Camera);
            QueueUpdate(UpdateFlags.Shape);
            QueueUpdate(UpdateFlags.Alpha);
            QueueUpdate(UpdateFlags.Color);
            QueueUpdate(UpdateFlags.Visibility);
        }

        
        
        // [Section] HandleElementView Methods Overrides

        public override void CreateShapesAndColliders()
        {
            base.CreateShapesAndColliders();
            
            // Create the colliders
            m_Collider = gameObject.AddComponent<SphereCollider>();
            m_Collider.radius = 0.03f;
            m_Collider.enabled = false;

            // Create the shapes
            m_DotShape = HandlesUtils.CreateDisc(gameObject, "Position Disc");
            m_DotShape.RotationOffset = new Vector3(90f, 0f, 0f);
            m_DotShape.BorderSize = HandlesUtils.LineSize * (1 / k_EffectorScale);
            m_DotShape.BorderSpacing = HandlesUtils.LineSize * (1 / k_EffectorScale);
            m_DotShape.Radius = k_EffectorDotRadius;
            m_DotShape.Fill = true;

            m_LookAtTargetHorizontalShape = HandlesUtils.CreateLine(gameObject, "LookAt Target Line (Horizontal)");
            m_LookAtTargetVerticalShape = HandlesUtils.CreateLine(gameObject, "LookAt Target Line (Vertical)");

            m_ElasticLineShape = HandlesUtils.CreateLine(gameObject, "Elastic Line");
            m_ElasticDiscShape = HandlesUtils.CreateDisc(m_ElasticLineShape.gameObject, "Elastic Disc");
            m_ElasticDiscShape.RotationOffset = new Vector3(90f, 0f, 0f);
            m_ElasticDiscShape.Radius = k_EffectorElasticDotRadius;
            m_ElasticDiscShape.Fill = true;
            m_ElasticDiscShape.BorderSize = 0f;
            m_ElasticDiscShape.BorderSpacing = 0f;

            var lookAtEnd = k_LookAtDirectionLength * Vector3.forward;

            m_LookAtLine = HandlesUtils.CreateLine(gameObject, "LookAt Direction Line");
            m_LookAtLine.SetPositions(new[]
            {
                Vector3.zero,
                lookAtEnd
            });

            m_LookAtTip = HandlesUtils.CreateCone(m_LookAtLine.gameObject, "LookAt Direction Tip");
            m_LookAtTip.Radius = 0.02f;
            m_LookAtTip.Height = 0.04f;
            m_LookAtTip.transform.localPosition = lookAtEnd;
            m_LookAtTip.RotationOffset = new Vector3(90f, 0f, 0f);

            m_ToleranceShape = HandlesUtils.CreateDisc(gameObject, "Tolerance");
            m_ToleranceShape.BorderSize = HandlesUtils.LineSize;
            m_ToleranceShape.BorderSpacing = 0;
            m_ToleranceShape.Radius = k_EffectorDotRadius;
            m_ToleranceShape.Resolution = 32;
            m_ToleranceShape.Fill = false;
            m_ToleranceShape.RotationOffset = new Vector3(90f, 0f, 0f);
        }

        public override void Step(float delta)
        {
            base.Step(delta);
            
            // Note: Force an update on Shapes in case they are not enabled
            m_DotShape.Update();
            m_ToleranceShape.Update();
            m_ElasticDiscShape.Update();
            m_LookAtTip.Update();
        }

        protected override void UpdateVisibility()
        {
            base.UpdateVisibility();
            
            if (m_Model == null || !m_Model.IsVisible)
            {
                // Disable all shapes
                m_DotShape.enabled = false;
                m_LookAtTargetHorizontalShape.enabled = false;
                m_LookAtTargetVerticalShape.enabled = false;
                m_ElasticLineShape.enabled = false;
                m_ElasticDiscShape.enabled = false;
                m_LookAtLine.enabled = false;
                m_LookAtTip.enabled = false;
                m_ToleranceShape.enabled = false;

                // Disable collider
                m_Collider.enabled = false;
                return;
            }

            // Update the parts visibility
            // Effector Dot
            m_DotShape.enabled = m_Model.HandlesPosition || m_Model.HandlesRotation;

            // Line between effector dot and bone
            var displayElastic = m_Model.PositionEnabled || m_Model.HandlesLookAt;
            m_ElasticLineShape.enabled = displayElastic;
            m_ElasticDiscShape.enabled = displayElastic;

            // LookAt target cross
            m_LookAtTargetHorizontalShape.enabled = m_Model.HandlesLookAt;
            m_LookAtTargetVerticalShape.enabled = m_Model.HandlesLookAt;

            // Arrow showing LookAt orientation
            m_LookAtLine.enabled = m_Model.LookAtEnabled;
            m_LookAtTip.enabled = m_Model.LookAtEnabled;

            // Tolerance Radius
            m_ToleranceShape.enabled = m_Model.PositionEnabled && m_Model.PositionTolerance > 0f;

            // Enable the collider
            m_Collider.enabled = true;
        }

        protected override void UpdateTransform()
        {
            base.UpdateTransform();
            
            if (m_Model == null)
                return;

            transform.SetPositionAndRotation(m_Model.Position, m_Model.Rotation);
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            
            if (m_Model == null)
                return;

            // Elastic Disc and Line Scaling
            m_ElasticLineShape.widthMultiplier = HandlesUtils.LineSize * m_Model.JointSizeRatio;
            m_ElasticDiscShape.transform.localScale = Vector3.one * (k_EffectorScale * m_Model.JointSizeRatio);
            
            // Dot Scaling
            m_DotShape.transform.localScale = Vector3.one * (k_EffectorScale * m_Model.SizeRatio);

            // Look At Scaling
            m_LookAtLine.endWidth = HandlesUtils.LineSize * m_Model.SizeRatio;
            m_LookAtLine.startWidth = HandlesUtils.LineSize * m_Model.JointSizeRatio;

            m_LookAtTargetHorizontalShape.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            m_LookAtTargetVerticalShape.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            
            // Elastic
            if (m_Model.PositionEnabled || m_Model.HandlesLookAt)
            {
                m_ElasticLineShape.transform.position = m_Model.SourceJointPosition;

                m_ElasticLineShape.SetPositions(new[]
                {
                    Vector3.zero,
                    m_ElasticLineShape.transform.InverseTransformPoint(m_Model.Position)
                });
            }

            // Direction arrow
            if (m_Model.LookAtEnabled)
            {
                var lookAtDirectionTransform = m_LookAtLine.transform;
                lookAtDirectionTransform.position = m_Model.SourceJointPosition;
                lookAtDirectionTransform.rotation = m_Model.SourceJointRotation;
            }

            // Show / Hide Parts of the Dot
            if (m_Model.HandlesPosition)
            {
                m_DotShape.Fill = true;
            }
            else
            {
                m_DotShape.Fill = false;
            }

            if (m_Model.HandlesRotation)
            {
                m_DotShape.BorderSpacing = HandlesUtils.LineSize * (1 / k_EffectorScale);
                m_DotShape.BorderSize = HandlesUtils.LineSize * (1 / k_EffectorScale);
            }
            else
            {
                m_DotShape.BorderSpacing = 0f;
                m_DotShape.BorderSize = 0f;
            }

            // Tolerance Shape Radius
            if (m_Model.PositionTolerance > 0f)
            {
                m_ToleranceShape.Radius = m_Model.PositionTolerance * ApplicationConstants.ToleranceToRadiusFactor;
                m_ToleranceShape.BorderSize = HandlesUtils.LineSize * m_Model.SizeRatio;
                m_ToleranceShape.BorderSideRatio = 1f;
            }
        }

        protected override void UpdateCamera()
        {
            base.UpdateCamera();
            
            if (m_Model == null)
                return;
            
            var modelCamera = m_Model.Camera.Target;

            if (m_Model.Camera.ViewportSize.magnitude <= 0)
                return;
            
            var cameraForward = -modelCamera.transform.forward;

            m_DotShape.transform.forward = cameraForward;
            m_ToleranceShape.transform.forward = cameraForward;
            m_ElasticDiscShape.transform.forward = cameraForward;

            var size = 10f;
            var center = m_Model.Position;
            var top = ScreenUtils.CalculateWorldDistanceFromPixelDistance(m_Model.Camera, center, Vector3.up * size);
            var down = ScreenUtils.CalculateWorldDistanceFromPixelDistance(m_Model.Camera, center, Vector3.down * size);
            var left = ScreenUtils.CalculateWorldDistanceFromPixelDistance(m_Model.Camera, center, Vector3.left * size);
            var right = ScreenUtils.CalculateWorldDistanceFromPixelDistance(m_Model.Camera, center, Vector3.right * size);

            m_LookAtTargetHorizontalShape.SetPositions(new[]
            {
                m_LookAtTargetHorizontalShape.transform.InverseTransformVector(left),
                m_LookAtTargetHorizontalShape.transform.InverseTransformVector(right)
            });

            m_LookAtTargetVerticalShape.SetPositions(new[]
            {
                m_LookAtTargetVerticalShape.transform.InverseTransformVector(top),
                m_LookAtTargetVerticalShape.transform.InverseTransformVector(down)
            });
        }

        protected override void UpdateColor()
        {
            base.UpdateColor();

            if (m_Model == null)
                return;
            
            var distanceFromCursor = HandlesUtils.GetWorldToViewportPixelDistanceFromCursor(m_Model.Camera, m_Model.Position, false);
            distanceFromCursor = HandlesUtils.PixelsToNoots(distanceFromCursor);

            var positionColor = ComputeColor(m_Model.PositionColor, distanceFromCursor);
            m_DotShape.ColorFill = positionColor;
            
            var toleranceColor = ComputeColor(m_Model.ToleranceColor, distanceFromCursor);
            //m_ToleranceShape.ColorFill = toleranceColor;
            m_ToleranceShape.ColorBorder = toleranceColor;
            
            var rotationColor = ComputeColor(m_Model.RotationColor, distanceFromCursor);
            m_DotShape.ColorBorder = rotationColor;

            var lookAtColor = ComputeColor(m_Model.LookAtColor, distanceFromCursor);

            m_LookAtTargetHorizontalShape.startColor = lookAtColor;
            m_LookAtTargetHorizontalShape.endColor = lookAtColor;

            m_LookAtTargetVerticalShape.startColor = lookAtColor;
            m_LookAtTargetVerticalShape.endColor = lookAtColor;

            var elasticColor = m_Model.HandlesLookAt ? lookAtColor : positionColor;
            m_ElasticLineShape.startColor = elasticColor;
            m_ElasticLineShape.endColor = elasticColor;
            m_ElasticDiscShape.ColorFill = elasticColor;

            m_LookAtLine.startColor = lookAtColor;
            m_LookAtLine.endColor = lookAtColor;

            m_LookAtTip.Color = lookAtColor;
        }
        
        public override int GetPhysicsRaycastDepth(Vector2 screenPosition, RaycastHit hit)
        {
            // Note: for effectors we pick the closest to the cursor instead of by depth
            // TODO: exception if cursor is inside the circle (return default 0 depth)?
            var distanceToCursor = (screenPosition - GetScreenPosition()).magnitude;
            return -Mathf.FloorToInt(distanceToCursor);
        }
        
        
        
        Color ComputeColor(Color baseColor, float distanceFromCursor)
        {
            if (!m_Model.IsActive && !m_Model.IsSelected)
            {
                baseColor.a *= HandlesUtils.GetAlphaFromDistanceFromCursor(distanceFromCursor);
            }

            return baseColor;
        }

        // [Section] User Inputs Events Listeners
        
        public void OnPointerClick(PointerEventData eventData)
        {
            m_Model?.OnPointerClick(eventData);
        }

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
