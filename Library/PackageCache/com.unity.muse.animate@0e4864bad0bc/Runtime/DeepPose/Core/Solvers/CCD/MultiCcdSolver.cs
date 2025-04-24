using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    class MultiCcdSolver
    {
        int[] m_ParentJointId;
        bool[] m_JointIsEffector;
        int[] m_JointBranch;
        int[] m_ActiveChildEffectorsCount;
        int[] m_ChildrenCount;
        Vector3[] m_TargetsOffsets;

        Quaternion[] m_JointsBranchOriginalRotations;
        Quaternion[] m_JointsOriginalRotations;

        List<int> m_SortedBranchIndexes;

        public void Initialize(in MultiCcdData data)
        {
            m_TargetsOffsets = new Vector3[data.Positions.Count];
            m_ParentJointId = new int[data.Joints.Count];
            m_JointIsEffector = new bool[data.Joints.Count];
            m_JointBranch = new int[data.Joints.Count];
            m_ActiveChildEffectorsCount = new int[data.Joints.Count];
            m_ChildrenCount = new int[data.Joints.Count];

            RefreshHierarchy(in data);
            RefreshOffsets(in data);
            SortBranches(in data);

            m_JointsBranchOriginalRotations = new Quaternion[data.Joints.Count];
            m_JointsOriginalRotations = new Quaternion[data.Joints.Count];
        }

        public void Solve(in MultiCcdData data)
        {
            // Only perform job if used
            if (data.Weight > 0f)
            {
                // Parse Joints
                if (data.Joints.Count > 0)
                {
                    // Refresh number of child effectors
                    UpdateJointEffectorCounts(in data);

                    // Keep the original joints rotations before solving
                    for (var i = 0; i < data.Joints.Count; i++)
                    {
                        var jointTransform = data.Joints[i];
                        var jointRotation = jointTransform.Transform.localRotation;
                        m_JointsOriginalRotations[i] = jointRotation;
                    }

                    // Save the original position of the armature root
                    var rootOriginalPosition = data.Joints[0].Transform.position;

                    if (data.Positions.Count > 0)
                    {
                        // Iterate and solve each branch to calculate the final rotations
                        for (var branchUnsortedIndex = 0; branchUnsortedIndex < data.Positions.Count; branchUnsortedIndex++)
                        {
                            var branchIndex = m_SortedBranchIndexes[branchUnsortedIndex];
                            var effector = data.Positions[branchIndex];
                            if (!effector.IsEnabled)
                                continue;

                            var branchWeight = effector.Weight;
                            var maintainOffset = effector.MaintainOriginalOffset;
                            var branchTolerance = effector.Tolerance;
                            var totalTolerance = branchTolerance + data.Tolerance;

                            // Recover the branch data needed to iterate & solve
                            var targetTransform = effector.Transform;
                            if (targetTransform == null)
                                continue;

                            var targetPosition = targetTransform.position;

                            if (maintainOffset)
                            {
                                targetPosition += m_TargetsOffsets[branchIndex];
                            }

                            var tipJointIndex = effector.Id;
                            var tipTransform = data.Joints[tipJointIndex].Transform;
                            if (tipTransform == null)
                                continue;

                            // Root branch
                            if (tipJointIndex == 0)
                            {
                                // Snap the tip bone position on its target
                                tipTransform.position = targetPosition;
                            }
                            else
                            {
                                // First check if already reached target
                                var tipToTarget = targetPosition - tipTransform.position;
                                var reachedTarget = tipToTarget.magnitude <= totalTolerance;

                                if (reachedTarget)
                                    continue;

                                // Save the branch pre-solve rotations
                                // Note: We keep the values of all the bones in order
                                // to blend the weight of each branches with the final pose.
                                for (var i = 0; i < data.Joints.Count; i++)
                                {
                                    var jointTransform = data.Joints[i].Transform;
                                    m_JointsBranchOriginalRotations[i] = jointTransform.localRotation;
                                }

                                // Save the branch pre-solve root position
                                var rootJointTransform = data.Joints[0].Transform;
                                var rootBranchOriginalPosition = rootJointTransform.position;

                                // Iterate & solve the branch
                                for (var k = 0; k < data.MaxIterations; k++)
                                {
                                    // Starting from the second joint (the first being the tip)
                                    // going back up to the root
                                    var jointIndex = GetParentJointIndexInChain(tipJointIndex);
                                    while (jointIndex >= 0)
                                    {
                                        // We don't solve joints that split into multiple chains
                                        if (m_ChildrenCount[jointIndex] > 1)
                                            break;

                                        // Skip stiff joints
                                        if (data.Joints[jointIndex].Stiff)
                                        {
                                            jointIndex = GetParentJointIndexInChain(jointIndex);
                                            continue;
                                        }

                                        var jointTransform = data.Joints[jointIndex].Transform;
                                        var jointPosition = jointTransform.position;
                                        var jointToTip = tipTransform.position - jointPosition;
                                        var jointToTarget = targetPosition - jointPosition;

                                        float linkCos;
                                        float linkSin;

                                        if (jointToTip.magnitude * jointToTarget.magnitude <= 0.001f)
                                        {
                                            // Prevent errors from a distance that would be too small
                                            linkCos = 1f;
                                            linkSin = 0f;
                                        }
                                        else
                                        {
                                            // Find the components using dot and cross product
                                            linkCos = Vector3.Dot(jointToTip, jointToTarget) / (jointToTip.magnitude * jointToTarget.magnitude);
                                            linkSin = Vector3.Cross(jointToTip, jointToTarget).magnitude / (jointToTip.magnitude * jointToTarget.magnitude);
                                        }

                                        // The axis of rotation is basically the
                                        // unit vector along the cross product
                                        var axis = Vector3.Cross(jointToTip, jointToTarget) / (jointToTip.magnitude * jointToTarget.magnitude);

                                        // Find the angle between r1 and r2 (and clamp values of cos to avoid errors)
                                        //linkTheta[pathIndex] = Mathf.Acos(Mathf.Max(-1, Mathf.Min(1, linkCos[pathIndex])));
                                        var linkTheta = Mathf.Acos(Mathf.Max(-1, Mathf.Min(1, linkCos)));

                                        // Invert angle if sin component is negative
                                        if (linkSin < 0.0f)
                                            linkTheta = -linkTheta;

                                        // Obtain an angle value between -pi and pi, and then convert to degrees
                                        linkTheta = GeometryUtils.WrapAngle(linkTheta) * Mathf.Rad2Deg;

                                        // Rotate the ith joint along the axis by theta degrees in the world space
                                        var wantedRotation = Quaternion.AngleAxis(linkTheta, axis);
                                        var previousRotation = jointTransform.rotation;

                                        // Apply the rotation to the joint transform
                                        jointTransform.rotation = wantedRotation * previousRotation;

                                        // If the joint is an effector with non-null weight, we stop the solve here
                                        if (m_JointIsEffector[jointIndex] && data.Positions[m_JointBranch[jointIndex]].Weight > 0f)
                                            break;

                                        // Check if reached the target
                                        reachedTarget = CcdUtils.CheckIfReachedDestination(targetPosition, tipTransform.position, totalTolerance);
                                        if (reachedTarget)
                                            break;

                                        jointIndex = GetParentJointIndexInChain(jointIndex);
                                    }

                                    if (reachedTarget)
                                        break;
                                }

                                // Re-Apply the solved branch final root position, taking the weight of the branch into consideration
                                var rootBranchTransform = data.Joints[0].Transform;
                                rootBranchTransform.position = Vector3.Lerp(rootBranchOriginalPosition, rootBranchTransform.position, branchWeight);

                                // Re-Apply the solved branch final rotations, taking the weight of the branch into consideration
                                for (var i = 0; i < data.Joints.Count; i++)
                                {
                                    var jointTransform = data.Joints[i].Transform;
                                    jointTransform.localRotation = Quaternion.Slerp(m_JointsBranchOriginalRotations[i], jointTransform.localRotation, branchWeight);
                                }
                            }
                        }

                        // Re-Apply the solved armature final root position, taking the weight of the job into consideration
                        var rootTransform = data.Joints[0].Transform;
                        rootTransform.position = Vector3.Lerp(rootOriginalPosition, rootTransform.position, data.Weight);

                        // Re-Apply the solved armature final rotations, but taking the weight of the branch into consideration
                        for (var i = 0; i < data.Joints.Count; i++)
                        {
                            var jointTransform = data.Joints[i].Transform;
                            jointTransform.localRotation = Quaternion.Slerp(m_JointsOriginalRotations[i], jointTransform.localRotation, data.Weight);
                        }
                    }
                }
            }
        }

        void RefreshOffsets(in MultiCcdData data)
        {
            // Calculate offsets (Effectors values)
            if (data.Positions.Count > 0)
            {
                for (var i = 0; i < data.Positions.Count; i++)
                {
                    var jointId = data.Positions[i].Id;
                    if (jointId < 0 || jointId >= data.Joints.Count)
                        continue;

                    var effectorTransform = data.Positions[i].Transform;
                    var jointTransform = data.Joints[jointId].Transform;
                    var offset = jointTransform.position - effectorTransform.position;
                    m_TargetsOffsets[i] = offset;
                }
            }
        }

        void RefreshHierarchy(in MultiCcdData data)
        {
            if (data.Joints.Count > 0)
            {
                // Initialize the initial amount of children of each joint of the armature
                for (var i = 0; i < data.Joints.Count; i++)
                {
                    m_ParentJointId[i] = 0;
                    m_JointIsEffector[i] = false;
                    m_JointBranch[i] = -1;
                }

                // If a joint is used as an effector, track it
                for (var i = 0; i < data.Positions.Count; i++)
                {
                    var jointIdx = data.Positions[i].Id;
                    if (jointIdx < 0 || jointIdx >= m_JointIsEffector.Length)
                        continue;

                    m_JointIsEffector[jointIdx] = true;
                    m_JointBranch[jointIdx] = i;
                }

                // Save the parent joint and the amount of children per joint
                // Skip the root of the armature
                // Note: The list of joints has to be already ordered by hierarchy.
                // (As done by "DeepPose/Setup Human Rigging" menu)
                // In the future we want to implement a unified way to input the skeleton-related information.
                for (var i = 1; i < data.Joints.Count; i++)
                {
                    // Recover the path (Includes the tip as well, order is root -> tip)
                    var tip = data.Joints[i];

                    if (tip.Transform.parent == null)
                    {
                        // Can happen if the "Pelvis" or "Hips" root joint
                        // is not at index 0 in the joints list.
                        Debug.LogError("The joint has no parent. Make sure your root joint is at the index 0 in your joints list.");
                    }
                    else
                    {
                        // Save the parent joint index
                        var parentJointIndex = GetJointIndex(in data, tip.Transform.parent);

                        //UnityEngine.Debug.Log("Parent joint index: "+parentJointIndex);
                        m_ParentJointId[i] = parentJointIndex;
                    }
                }
            }
        }

        void SortBranches(in MultiCcdData data)
        {
            var joints = data.Joints;
            m_SortedBranchIndexes = data.Positions
                .Select((item, index) => new { item, index })
                .OrderBy(p => (p.item.Id > 0 ? joints[p.item.Id].Transform : null).Depth())
                .Select(p => p.index).ToList();
        }

        int GetJointIndex(in MultiCcdData data, Transform t)
        {
            for (var i = 0; i < data.Joints.Count; i++)
            {
                if (data.Joints[i].Transform == t)
                {
                    return i;
                }
            }

            return -1;
        }

        int GetParentJointIndexInChain(int currentJointIndex)
        {
            if (currentJointIndex == 0)
                return -1;

            var parentIndex = m_ParentJointId[currentJointIndex];
            return parentIndex;
        }

        void UpdateJointEffectorCounts(in MultiCcdData data)
        {
            for (var i = 0; i < m_ActiveChildEffectorsCount.Length; i++)
            {
                m_ActiveChildEffectorsCount[i] = 0;
                m_ChildrenCount[i] = 0;
            }

            // For each joint, count how many child effector it has
            foreach (var effector in data.Positions)
            {
                if (!effector.IsEnabled)
                    continue;

                var parentIdx = GetParentJointIndexInChain(effector.Id);
                while (parentIdx >= 0)
                {
                    m_ActiveChildEffectorsCount[parentIdx]++;
                    parentIdx = GetParentJointIndexInChain(parentIdx);
                }
            }

            // For each joint, count how many children it has
            for (var jointIdx = 1; jointIdx < m_ChildrenCount.Length; jointIdx++)
            {
                var parentIdx = GetParentJointIndexInChain(jointIdx);
                if (parentIdx < 0)
                    continue;

                // Only consider effectors
                if (m_ActiveChildEffectorsCount[jointIdx] == 0 && !m_JointIsEffector[jointIdx])
                    continue;

                m_ChildrenCount[parentIdx]++;
            }
        }
    }
}
