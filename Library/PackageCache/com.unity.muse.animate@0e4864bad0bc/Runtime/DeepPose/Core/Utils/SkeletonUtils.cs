using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Unity.DeepPose.Core
{
    static class SkeletonUtils
    {
        public static int[] GetEndJointIndices(this Skeleton skeleton)
        {
            return skeleton.AllJoints.Where(x => !x.Children.Any()).Select(x => x.Index).ToArray();
        }

        public static Dictionary<Skeleton.IJoint, Transform> FindTransforms(this Skeleton skeleton, Transform rootTransform)
        {
            var jointTransforms = new Dictionary<Skeleton.IJoint, Transform>();
            if (skeleton == null)
                return jointTransforms;

            // Register all transforms under that root by name
            var allTransforms = rootTransform.GetComponentsInChildren<Transform>();
            var nameToTransform = new Dictionary<string, Transform>();
            foreach (var t in allTransforms)
            {
                nameToTransform[BonifyJointName(t.name)] = t;
            }

            // For each joint, find a transform with matching name
            foreach (var joint in skeleton.AllJoints)
            {
                var jointName = BonifyJointName(joint.Name);
                if (nameToTransform.TryGetValue(jointName, out var jointTransform))
                    jointTransforms[joint] = jointTransform;
            }

            return jointTransforms;
        }

        public static bool IsValidNonNullHumanAvatar(Avatar avatar)
        {
            return avatar != null && avatar.IsValidHumanAvatar();
        }

        static bool IsValidHumanAvatar(this Avatar avatar)
        {
            return avatar.isValid && avatar.isHuman;
        }

        public static float GetHumanScale(this Avatar avatar)
        {
            Assert.IsTrue(IsValidNonNullHumanAvatar(avatar), "This is not a valid human avatar.");

            // Very ugly trick...
            var go = new GameObject();
            var animator = go.AddComponent<Animator>();

            animator.avatar = avatar;
            var humanScale = animator.humanScale;

#if UNITY_EDITOR
            Object.DestroyImmediate(go);
#else
        GameObject.Destroy(go);
#endif

            return humanScale;
        }

        public static Transform FindRootTransform(GameObject gameObject)
        {
            var animator = gameObject.GetComponentInParent<Animator>();
            if (animator != null)
                return animator.transform;

            return gameObject.transform;
        }

        public static List<Transform> GetAllChildren(this Transform root, bool includeRoot = true)
        {
            var res = new List<Transform>();
            GetAllChildrenRecursive(root, res, includeRoot);
            return res;
        }

        static void GetAllChildrenRecursive(Transform root, List<Transform> transforms, bool includeRoot = true)
        {
            if (root == null)
                return;

            if (includeRoot)
                transforms.Add(root);

            foreach (Transform child in root)
            {
                GetAllChildrenRecursive(child, transforms, true);
            }
        }

        public static Dictionary<string, string> AvatarToRigNames(this Avatar avatar)
        {
            Assert.IsTrue(IsValidNonNullHumanAvatar(avatar), "Must be a valid human avatar");

            var names = new Dictionary<string, string>();

            foreach (var joint in avatar.humanDescription.human)
            {
                names[joint.humanName] = joint.boneName;
            }

            return names;
        }

        public static Dictionary<string, string> RigToAvatarNames(this Avatar avatar)
        {
            Assert.IsTrue(IsValidNonNullHumanAvatar(avatar), "Must be a valid human avatar");

            var names = new Dictionary<string, string>();

            foreach (var joint in avatar.humanDescription.human)
            {
                names[joint.boneName] = joint.humanName;
            }

            return names;
        }

        public static Dictionary<string, Transform> GetAvatarJointTransforms(this Animator animator)
        {
            Assert.IsTrue(IsValidNonNullHumanAvatar(animator.avatar));

            var transforms = new Dictionary<string, Transform>();

            var allBonesNames = Enum.GetNames(typeof(HumanBodyBones));
            foreach (var boneName in allBonesNames)
            {
                if (!Enum.TryParse<HumanBodyBones>(boneName, out var bone))
                    continue;

                if (bone == HumanBodyBones.LastBone)
                    continue;

                transforms[boneName] = animator.GetBoneTransform(bone);
            }

            return transforms;
        }

        public static Transform FindRecursive(this Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;

                var found = FindRecursive(child, childName);
                if (found != null)
                    return found;
            }

            return null;
        }

        public static int Depth(this Transform transform)
        {
            if (transform == null)
                return -1;

            var depth = 0;
            var parent = transform.parent;
            while (parent != null)
            {
                depth++;
                parent = parent.parent;
            }

            return depth;
        }

        public static string BonifyJointName(string name)
        {
            name = name.ToLower();
            name = name.Replace(".l", "left");
            name = name.Replace(".r", "right");
            name = name.Replace("_l", "left");
            name = name.Replace("_r", "right");
            name = name.Replace("pelvis", "hips");
            name = name.Replace("_", "");

            return name;
        }

        public static GameObject Instantiate(this Skeleton skeleton, string rootName = "Skeleton")
        {
            var rootObject = new GameObject(rootName);
            var rootTransform = rootObject.transform;

            foreach (var joint in skeleton.RootJoints)
            {
                var jointObject = joint.Instantiate();
                jointObject.transform.SetParent(rootTransform, false);
            }

            return rootObject;
        }

        static GameObject Instantiate(this Skeleton.IJoint joint)
        {
            var jointObject = new GameObject(joint.Name);
            var jointTransform = jointObject.transform;
            jointTransform.localPosition = joint.Offset;

            foreach (var child in joint.Children)
            {
                var childObject = child.Instantiate();
                childObject.transform.SetParent(jointTransform, false);
            }

            return jointObject;
        }

        public static bool HaveSameJointNames(this Skeleton skeleton1, Skeleton skeleton2)
        {
            if (skeleton1 == null || skeleton2 == null)
                return false;

            if (skeleton1.Count != skeleton2.Count)
                return false;

            foreach (var joint1 in skeleton1.AllJoints)
            {
                var joint2 = skeleton2.FindJoint(joint1.Index);
                if (joint2 == null)
                    return false;

                if (joint1.Name != joint2.Name)
                    return false;
            }

            return true;
        }

        public static bool IsParentOf(this Skeleton.IJoint parent, Skeleton.IJoint child)
        {
            if (parent == null || child == null)
                return false;

            while (child != null)
            {
                if (child.Index == parent.Index)
                    return true;

                child = child.Parent;
            }

            return false;
        }
    }
}
