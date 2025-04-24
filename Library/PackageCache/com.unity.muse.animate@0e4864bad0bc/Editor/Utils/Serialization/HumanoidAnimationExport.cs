using System.Linq;
using Unity.DeepPose.Core.Editor;
using Unity.Muse.Animate.Usd;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate.Editor
{
    static class HumanoidAnimationExport
    {
        public static AnimationClip Export(ExportData exportData)
        {
            var actor = exportData.ActorsData.FirstOrDefault();
            Assert.IsNotNull(actor);
                        
            if (!actor.PosingArmature.TryGetComponent<Animator>(out var animator))
                return null;

            var clip = new AnimationClip();

            // Create a human poser handler to get the human pose from the avatar humanoid
            using var humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            var humanPose = new HumanPose();
            humanPoseHandler.GetHumanPose(ref humanPose);

            // There is only one root position curve since all other joints are only modified from a rotation perspective
            // The positions of each joint are the skeleton joint offset + rotations.
            var rootPositionCurve = AnimationClipUtils.PositionCurve.New();
            var rootRotationCurve = AnimationClipUtils.RotationCurve.New();
            var animationCurves = new AnimationCurve[humanPose.muscles.Length];
            for (var i = 0; i < animationCurves.Length; i++)
            {
                animationCurves[i] = new AnimationCurve();
            }

            for (var i = 0; i < exportData.BackedTimeline.FramesCount; i++)
            {
                var time = i / ApplicationConstants.FramesPerSecond;
                var frameModel = exportData.BackedTimeline.GetFrame(i);
                if (!frameModel.TryGetModel(actor.ActorModel.EntityID, out var pose))
                    continue;

                // Read the frame from the armature data and apply it to the transforms of the GameObject.
                // Then get the human pose which uses those transforms to get the pose of the humanoid.
                pose.ApplyTo(actor.PosingArmature.ArmatureMappingData);
                humanPoseHandler.GetHumanPose(ref humanPose);

                // Apply the root position and rotation to the curves
                rootPositionCurve.AddPositionKey(time, humanPose.bodyPosition);
                rootRotationCurve.AddRotationKey(time, humanPose.bodyRotation);

                // For every muscle, add a key
                for (var j = 0; j < humanPose.muscles.Length; j++)
                {
                    var muscleValue = humanPose.muscles[j];
                    animationCurves[j].AddKey(time, muscleValue);
                }
            }

            // Now that all the curves are filled, we can set them to the clip
            AnimationClipUtils.SetHumanoidCurves(clip, rootPositionCurve, rootRotationCurve, animationCurves);

            return clip;
        }
    }
}
