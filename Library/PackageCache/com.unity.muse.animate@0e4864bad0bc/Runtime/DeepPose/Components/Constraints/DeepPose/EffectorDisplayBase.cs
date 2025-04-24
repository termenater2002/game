using System;
using System.Collections.Generic;
using Unity.DeepPose.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.DeepPose.Components
{
    [ExecuteAlways]
    abstract class EffectorDisplayBase : MonoBehaviour
    {
        public Color Color;
        public float Size = 0.05f;
        public float SecondarySize = 0.05f;
        public bool AutoSnap = true;
        public bool FollowPosition;
        public bool FollowRotation;
        public float ToleranceRadiusFactor = 3f;
        public float LookAtAlphaFactor = 0.5f;

        protected virtual bool IsValid { get; }
        protected virtual IEnumerable<Effector> Positions { get; }
        protected virtual IEnumerable<Effector> Rotations { get; }
        protected virtual IEnumerable<Effector> LookAts { get; }
        protected virtual IList<Transform> JointTransforms { get; }
        protected virtual Vector3 GetLookAtDirection(int jointId) { return Vector3.forward; }

        void LateUpdate()
        {
            if (!IsValid)
                return;

            if (IsVisible())
            {
#if UNITY_EDITOR
                SceneVisibilityManager.instance.Show(gameObject, true);
#endif
            }
            else
            {
#if UNITY_EDITOR
                SceneVisibilityManager.instance.Hide(gameObject, true);
#endif
                if (AutoSnap)
                    SnapEffector();
            }

            if (FollowPosition && !IsPositional())
            {
                foreach (var effector in Rotations)
                {
                    if (effector.transform == transform && TryGetTargetJointTransform(effector.id, out var jointTransform) && jointTransform != null)
                        transform.position = jointTransform.position;
                }
            }

            if (FollowRotation && !IsRotational())
            {
                foreach (var effector in Positions)
                {
                    if (effector.transform == transform && TryGetTargetJointTransform(effector.id, out var jointTransform) && jointTransform != null)
                        transform.rotation = jointTransform.rotation;
                }
            }
        }

        bool IsPositional()
        {
            foreach (var effector in Positions)
            {
                if (effector.weight == 0f)
                    continue;

                if (effector.transform == transform && TryGetTargetJointTransform(effector.id, out var jointTransform) && jointTransform != null)
                    return true;
            }

            return false;
        }

        bool IsRotational()
        {
            foreach (var effector in Rotations)
            {
                if (effector.weight == 0f)
                    continue;

                if (effector.transform == transform && TryGetTargetJointTransform(effector.id, out var jointTransform) && jointTransform != null)
                    return true;
            }

            return false;
        }

        void SnapEffector()
        {
            if (!IsValid)
                return;

            foreach (var effector in Positions)
            {
                if (effector.transform == transform && TryGetTargetJointTransform(effector.id, out var jointTransform) && jointTransform != null)
                    transform.position = jointTransform.position;
            }

            foreach (var effector in Rotations)
            {
                if (effector.transform == transform && TryGetTargetJointTransform(effector.id, out var jointTransform) && jointTransform != null)
                    transform.rotation = jointTransform.rotation;
            }

            foreach (var effector in LookAts)
            {
                if (effector.transform == transform && TryGetTargetJointTransform(effector.id, out var jointTransform) && jointTransform != null)
                {
                    var pos = jointTransform.position;
                    var direction = (jointTransform.rotation * transform.localRotation) * GetLookAtDirection(effector.id);
                    transform.position = pos + 1f * direction;
                }
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!IsVisible())
                return;

            foreach (var effector in LookAts)
            {
                if (effector.transform == transform && effector.weight > 0f && TryGetTargetJointTransform(effector.id, out var jointTransform) && jointTransform != null)
                {
                    Gizmos.color = new Color(Color.r, Color.g, Color.b, Color.a * LookAtAlphaFactor);
                    Gizmos.DrawLine(jointTransform.position, transform.position);
                    Handles.color = Color;

                    if (TryGetSourceJointTransform(effector.id, out var sourceJointTransform))
                    {
                        var offsetRotation = Quaternion.FromToRotation(Vector3.forward, GetLookAtDirection(effector.id));
                        Handles.ArrowHandleCap(0, jointTransform.position, sourceJointTransform.rotation * (transform.localRotation * offsetRotation), 0.2f, EventType.Repaint);
                    }

                    Gizmos.DrawSphere(transform.position, Size);
                }
            }

            foreach (var effector in Positions)
            {
                if (effector.transform == transform && effector.weight > 0f)
                {
                    Gizmos.color = Color;
                    if (effector.tolerance > 0f)
                        Gizmos.DrawWireSphere(transform.position, ToleranceRadiusFactor * effector.tolerance);

                    Gizmos.DrawSphere(transform.position, Size);
                }
            }

            foreach (var effector in Rotations)
            {
                if (effector.transform == transform && effector.weight > 0f)
                {
                    var prevMatrix = Gizmos.matrix;
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.color = Color;
                    Gizmos.DrawCube(Vector3.zero, new Vector3(SecondarySize, 0.05f * SecondarySize, SecondarySize));
                    Gizmos.matrix = prevMatrix;
                }
            }
        }
#endif

        protected virtual bool IsVisible()
        {
            if (!IsValid)
                return false;

            foreach (var effector in Positions)
            {
                if (effector.transform == transform && effector.weight > 0f)
                    return true;
            }

            foreach (var effector in Rotations)
            {
                if (effector.transform == transform && effector.weight > 0f)
                    return true;
            }

            foreach (var effector in LookAts)
            {
                if (effector.transform == transform && effector.weight > 0f)
                    return true;
            }

            return false;
        }

        bool TryGetSourceJointTransform(int id, out Transform t)
        {
            if (id >= 0 && id < JointTransforms.Count)
            {
                t = JointTransforms[id];
                return true;
            }

            t = null;
            return false;
        }

        bool TryGetTargetJointTransform(int id, out Transform t)
        {
            if (id >= 0 && id < JointTransforms.Count)
            {
                t = JointTransforms[id];
                return true;
            }

            t = null;
            return false;
        }
    }
}
