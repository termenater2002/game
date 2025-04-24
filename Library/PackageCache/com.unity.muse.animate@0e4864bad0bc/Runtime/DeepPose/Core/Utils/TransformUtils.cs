using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class TransformUtils
    {
        [Serializable]
        public struct PoseData
        {
            public Vector3 LocalPosition;
            public Quaternion LocalRotation;

            public PoseData(Vector3 localPosition, Quaternion localRotation)
            {
                LocalPosition = localPosition;
                LocalRotation = localRotation;
            }
        }

        public static void CapturePose(this Transform transform, Dictionary<Transform, PoseData> storage)
        {
            storage.Clear();

            if (transform == null)
                return;

            CapturePoseInternal(transform, storage);
        }

        public static void RestorePose(this Transform transform, Dictionary<Transform, PoseData> storage)
        {
            RestorePoseInternal(transform, storage);
        }

        static void CapturePoseInternal(Transform transform, Dictionary<Transform, PoseData> storage)
        {
            storage[transform] = new PoseData(transform.localPosition, transform.localRotation);
            foreach (Transform child in transform)
            {
                CapturePoseInternal(child, storage);
            }
        }

        static void RestorePoseInternal(Transform transform, Dictionary<Transform, PoseData> storage)
        {
            if (storage.TryGetValue(transform, out var poseData))
            {
                transform.localRotation = poseData.LocalRotation;
                transform.localPosition = poseData.LocalPosition;
            }

            foreach (Transform child in transform)
            {
                RestorePoseInternal(child, storage);
            }
        }

        public static string GetRelativePath(Transform child, Transform parent)
        {
            var relativePath = "";
            var currentTransform = child;
            var isFirst = true;
            while (currentTransform != null && currentTransform != parent)
            {
                if (isFirst)
                    isFirst = false;
                else
                    relativePath = string.IsNullOrEmpty(relativePath) ? currentTransform.name : $"{currentTransform.name}/{relativePath}";
                currentTransform = currentTransform.parent;
            }

            return relativePath;
        }
    }
}
