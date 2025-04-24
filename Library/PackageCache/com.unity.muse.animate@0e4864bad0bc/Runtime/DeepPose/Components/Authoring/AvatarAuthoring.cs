using System;
using System.Collections.Generic;
using Unity.DeepPose.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    class AvatarAuthoring : MonoBehaviour
    {
        public Animator[] Sources;
        public Animator[] Targets;
        public Animator TPose;

        private void Update()
        {
            if (!CheckRequirements())
                return;

            var importAvatar = BuildAvatar();
            ApplyPoses(importAvatar);
        }

        bool CheckRequirements()
        {
            if (TPose == null)
                return false;

            if (Sources == null || Targets == null)
                return false;

            if (Sources.Length != Targets.Length)
                return false;

            for (var i = 0; i < Sources.Length; i++)
            {
                if (Sources[i] == null)
                    return false;

                if (Targets[i] == null)
                    return false;
            }

            return true;
        }

        Avatar BuildAvatar()
        {
            var sourceAnimator = TPose;
            var sourceDescription = sourceAnimator.avatar.humanDescription;

            var skeletonBones = new List<SkeletonBone>();
            for (var i = 0; i < sourceDescription.skeleton.Length; i++)
            {
                var oldSkeletonBone = sourceDescription.skeleton[i];
                var boneTransform = sourceAnimator.transform.FindRecursive(oldSkeletonBone.name);

                if (boneTransform == null)
                {
                    if (i == 0)
                        boneTransform = sourceAnimator.transform;
                    else
                        throw new NullReferenceException($"Could not find bone: {oldSkeletonBone.name}");
                }

                // Add skeleton bone
                var newSkeletonBone = new SkeletonBone
                {
                    name = oldSkeletonBone.name,
                    position = boneTransform.localPosition,
                    rotation = boneTransform.localRotation,
                    scale = boneTransform.localScale
                };
                skeletonBones.Add(newSkeletonBone);
            }

            var humanDescription = new HumanDescription
            {
                armStretch = sourceDescription.armStretch,
                feetSpacing = sourceDescription.feetSpacing,
                hasTranslationDoF = sourceDescription.hasTranslationDoF,
                legStretch = sourceDescription.legStretch,
                lowerArmTwist = sourceDescription.lowerArmTwist,
                lowerLegTwist = sourceDescription.lowerLegTwist,
                upperArmTwist = sourceDescription.upperArmTwist,
                upperLegTwist = sourceDescription.upperLegTwist,
                skeleton = skeletonBones.ToArray(),
                human = sourceDescription.human
            };

            var avatar = AvatarBuilder.BuildHumanAvatar(sourceAnimator.gameObject, humanDescription);
            Assert.IsTrue(avatar.isValid, "Avatar is not valid");

            return avatar;
        }

        void ApplyPoses(Avatar importAvatar)
        {
            for (var i = 0; i < Sources.Length; i++)
            {
                var sourceAnimator = Sources[i];
                var targetAnimator = Targets[i];

                var humanPose = new HumanPose();

                var sourcePoseHandler = new HumanPoseHandler(importAvatar, sourceAnimator.transform);
                var targetPoseHandler = new HumanPoseHandler(targetAnimator.avatar, targetAnimator.transform);

                sourcePoseHandler.GetHumanPose(ref humanPose);
                targetPoseHandler.SetHumanPose(ref humanPose);

                sourcePoseHandler.Dispose();
                targetPoseHandler.Dispose();
            }
        }
    }
}