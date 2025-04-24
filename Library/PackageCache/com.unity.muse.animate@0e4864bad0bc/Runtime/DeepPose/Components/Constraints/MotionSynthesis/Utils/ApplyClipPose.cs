using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Unity.DeepPose.Components
{
    [ExecuteAlways]
    class ApplyClipPose : MonoBehaviour
    {
        public AnimationClip Clip
        {
            get => m_Clip;
            set
            {
                m_Clip = value;
                var currentFrame = Frame;
                Frame = currentFrame;
            }
        }

        public int Frame
        {
            get => m_Frame;
            set
            {
                m_Frame = Mathf.Clamp(value, 0, FrameCount);
                Apply();
            }
        }

        public Animator Target
        {
            get => m_Target;
            set
            {
                if (m_Target == value)
                    return;

                m_Target = value;
                CapturePose();
                Apply();
            }
        }

        public int FrameCount
        {
            get
            {
                if (m_Clip == null)
                    return 1;

                return Mathf.RoundToInt(m_Clip.frameRate * m_Clip.length);
            }
        }

        [SerializeField]
        Animator m_Target;

        [SerializeField]
        AnimationClip m_Clip;

        [SerializeField]
        int m_Frame;

        [SerializeField]
        bool m_ApplyInEditor = false;

        [SerializeField]
        bool m_ApplyRootMotion = true;

        struct PoseData
        {
            public Quaternion LocalRotation;
            public Vector3 LocalPosition;
        }

        private Dictionary<Transform, PoseData> m_PoseData = new();

        private void Awake()
        {
            if (m_Target == null)
                Target = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            CapturePose();
        }

        private void OnDisable()
        {
            RestorePose();
        }

        void Apply()
        {
            if (!enabled)
                return;

            if (m_Target == null || m_Clip == null)
                return;

            if (!m_ApplyInEditor && !Application.isPlaying)
                return;

            m_Target.applyRootMotion = m_ApplyRootMotion;

            var playableGraph = PlayableGraph.Create();
            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", m_Target);
            var clipPlayable = AnimationClipPlayable.Create(playableGraph, m_Clip);
            playableOutput.SetSourcePlayable(clipPlayable);

            var time = (float)m_Frame / m_Clip.frameRate;
            clipPlayable.SetTime(time);
            playableGraph.Evaluate();
        }

        public void CapturePose()
        {
            if (m_Target == null)
                return;

            m_PoseData.Clear();
            CapturePoseRecursive(m_Target.transform);
        }

        void CapturePoseRecursive(Transform parent)
        {
            var poseData = new PoseData();
            poseData.LocalPosition = parent.localPosition;
            poseData.LocalRotation = parent.localRotation;
            m_PoseData[parent] = poseData;

            foreach (Transform child in parent)
            {
                CapturePoseRecursive(child);
            }
        }

        public void RestorePose()
        {
            if (m_Target == null)
                return;

            RestorePoseRecursive(m_Target.transform);
        }

        void RestorePoseRecursive(Transform parent)
        {
            if (m_PoseData.TryGetValue(parent, out var poseData))
            {
                parent.localPosition = poseData.LocalPosition;
                parent.localRotation = poseData.LocalRotation;
            }

            foreach (Transform child in parent)
            {
                RestorePoseRecursive(child);
            }
        }
    }
}