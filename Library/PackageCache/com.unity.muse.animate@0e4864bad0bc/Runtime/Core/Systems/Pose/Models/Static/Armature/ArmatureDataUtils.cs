using System.Collections.Generic;
using Unity.DeepPose.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    static class ArmatureDataUtils
    {
        public static bool HaveSameJointNames(this in ArmatureData armature1, in ArmatureData armature2)
        {
            if (!armature1.IsValid || !armature2.IsValid)
                return false;

            if (armature1.NumJoints != armature2.NumJoints)
                return false;

            for (var i = 0; i < armature1.NumJoints; i++)
            {
                var joint1 = armature1.Joints[i];
                var joint2 = armature2.Joints[i];

                if (joint1.Name != joint2.Name)
                    return false;
            }

            return true;
        }

        public static void FindJoints(this in ArmatureData armature, Transform rootTransform, Transform[] result)
        {
            Assert.AreEqual(armature.NumJoints, result.Length);

            // For each joint, find a transform with matching name
            for (var i = 0; i < armature.NumJoints; i++)
            {
                var path = armature.Joints[i].Path;
                var jointTransform = rootTransform.Find(path);

                result[i] = jointTransform;
            }
        }

        public static ArmatureData CreateArmature(Skeleton skeleton)
        {
            Assert.IsNotNull(skeleton);

            var armature = new ArmatureData(skeleton.Count);
            armature.FromSkeleton(skeleton);
            return armature;
        }

        public static void FromSkeleton(this ref ArmatureData armature, Skeleton skeleton)
        {
            Assert.IsNotNull(skeleton);

            for (var i = 0; i < skeleton.Count; i++)
            {
                var skeletonJoint = skeleton.FindJoint(i);
                var jointIndex = skeletonJoint.Index;
                armature.Joints[jointIndex] = JointFromSkeletonJoint(skeletonJoint);
            }
        }

        static ArmatureData.Joint JointFromSkeletonJoint(Skeleton.IJoint joint)
        {
            var jointName = joint.Name;
            var jointPath = GetJointPathRecursive(joint);
            var parentJoint = joint.Parent;
            var parentIndex = parentJoint?.Index ?? -1;

            return new ArmatureData.Joint
            {
                Name = jointName,
                Path = jointPath,
                ParentIndex = parentIndex
            };
        }

        static string GetJointPathRecursive(Skeleton.IJoint joint, string suffix = "")
        {
            if (joint == null)
                return suffix;

            var path = string.IsNullOrEmpty(suffix) ? joint.Name : $"{joint.Name}/{suffix}";
            return GetJointPathRecursive(joint.Parent, path);
        }

        public static void FromHierarchy(this ref ArmatureData armature, Transform rootTransform)
        {
            using var tmpList = TempList<ArmatureData.Joint>.Allocate();

            foreach (Transform t in rootTransform)
            {
                AddHierarchyRecursive("", t, tmpList.List, -1);
            }

            armature.Joints = new ArmatureData.Joint[tmpList.Count];
            tmpList.List.CopyTo(armature.Joints, 0);
        }

        static void AddHierarchyRecursive(string path, Transform transform, List<ArmatureData.Joint> outJointLists, int parentIndex)
        {
            var jointPath = string.IsNullOrEmpty(path) ? transform.name : $"{path}/{transform.name}";
            var joint = new ArmatureData.Joint
            {
                Name = transform.name,
                Path = jointPath,
                ParentIndex = parentIndex
            };
            outJointLists.Add(joint);

            parentIndex = outJointLists.Count - 1;
            foreach (Transform t in transform)
            {
                AddHierarchyRecursive(jointPath, t, outJointLists, parentIndex);
            }
        }
    }
}
