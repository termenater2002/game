using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    static class ExportUtils
    {
        [Serializable]
        struct ClipboardPoseData
        {
            public HumanPoseData HumanPose;
            public bool IsValid => HumanPose.IsValid;
        }

        [Serializable]
        struct HumanPoseData
        {
            public Vector3 BodyPosition;
            public Quaternion BodyRotation;
            public float[] Muscles;
            public bool IsValid;

            public HumanPoseData(HumanPose pose)
            {
                BodyPosition = pose.bodyPosition;
                BodyRotation = pose.bodyRotation;
                Muscles = new float[pose.muscles.Length];
                pose.muscles.CopyTo(Muscles, 0);
                IsValid = true;
            }

            public void Apply(ref HumanPose pose)
            {
                Assert.IsTrue(IsValid, "Pose in not valid");

                pose.bodyPosition = BodyPosition;
                pose.bodyRotation = BodyRotation;

                pose.muscles = new float[Muscles.Length];
                Muscles.CopyTo(pose.muscles, 0);
            }

            public HumanPose ToHumanPose()
            {
                var humanPose = new HumanPose();
                Apply(ref humanPose);
                return humanPose;
            }
        }

        public static string GetHumanoidPoseAsJson(GameObject gameObject)
        {
            if (!gameObject.TryGetHumanoidAnimator(out var animator))
                return null;

            var poseData = animator.GetHumanoidData();
            var jsonData = JsonUtility.ToJson(poseData, true);
            return jsonData;
        }

        public static bool SetHumanoidPoseFromJson(GameObject gameObject, string jsonData)
        {
            if (!gameObject.TryGetHumanoidAnimator(out var animator))
                return false;

            if (string.IsNullOrEmpty(jsonData))
                return false;

            var poseData = JsonUtility.FromJson<ClipboardPoseData>(jsonData);
            if (!poseData.IsValid)
                return false;

            animator.ApplyHumanoidData(poseData);
            return true;
        }

        public static void CopyHumanoidPoseToClipboard(GameObject gameObject)
        {
            var jsonData = GetHumanoidPoseAsJson(gameObject);
            GUIUtility.systemCopyBuffer = jsonData;
        }

        public static void PasteHumanoidPoseFromClipboard(GameObject gameObject)
        {
            var jsonData = GUIUtility.systemCopyBuffer;
            SetHumanoidPoseFromJson(gameObject, jsonData);
        }

        static ClipboardPoseData GetHumanoidData(this Animator animator)
        {
            var humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            var humanPose = new HumanPose();
            humanPoseHandler.GetHumanPose(ref humanPose);
            humanPoseHandler.Dispose();

            var poseData = new ClipboardPoseData();
            poseData.HumanPose = new HumanPoseData(humanPose);

            return poseData;
        }

        static void ApplyHumanoidData(this Animator animator, ClipboardPoseData poseData)
        {
            var humanPose = poseData.HumanPose.ToHumanPose();

            var humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            humanPoseHandler.SetHumanPose(ref humanPose);
            humanPoseHandler.Dispose();
        }
    }
}
