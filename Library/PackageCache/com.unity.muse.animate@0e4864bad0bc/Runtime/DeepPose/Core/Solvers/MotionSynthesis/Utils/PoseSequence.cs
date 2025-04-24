using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    class PoseSequence
    {
        public IEnumerable<PoseFrame> Frames => m_Frames;
        public int[] ContactJointIndices => m_ContactJointIndices;
        public int JointsCount => m_JointsCount;
        public int ContactsCount => m_ContactJointIndices.Length;
        public int RaycastsCount => m_RaycastJointIndices.Length;
        public int Length => m_Frames.Count;

        List<PoseFrame> m_Frames = new ();
        int m_JointsCount;
        int[] m_ContactJointIndices;
        int[] m_RaycastJointIndices;

        public PoseSequence(int jointCounts, int[] contactJointIndices = null, int[] raycastJointIndices = null)
        {
            m_JointsCount = jointCounts;

            if (contactJointIndices != null)
            {
                m_ContactJointIndices = new int[contactJointIndices.Length];
                contactJointIndices.CopyTo(m_ContactJointIndices, 0);
            }
            else
            {
                m_ContactJointIndices = Array.Empty<int>();
            }
            
            if (raycastJointIndices != null)
            {
                m_RaycastJointIndices = new int[raycastJointIndices.Length];
                raycastJointIndices.CopyTo(m_RaycastJointIndices, 0);
            }
            else
            {
                m_RaycastJointIndices = Array.Empty<int>();
            }
        }

        public PoseSequence Clone(int startFrame = 0, int endFrame = -1)
        {
            if (endFrame < 0)
                endFrame += m_Frames.Count;

            var sequence = new PoseSequence(m_JointsCount, m_ContactJointIndices, m_RaycastJointIndices);
            for (var i = startFrame; i <= endFrame; i++)
            {
                var frame = m_Frames[i];
                var newFrame = frame.Clone();
                sequence.AddFrame(newFrame);
            }
            return sequence;
        }

        public void Clear()
        {
            m_Frames.Clear();
        }

        public PoseFrame AddFrame()
        {
            var frame = new PoseFrame(m_JointsCount, m_ContactJointIndices.Length, m_RaycastJointIndices.Length);
            m_Frames.Add(frame);
            return frame;
        }

        public void AddFrame(PoseFrame frame)
        {
            Assert.AreEqual(JointsCount, frame.JointsCount, "Frame joints count must match sequence joints count");
            Assert.AreEqual(ContactsCount, frame.ContactsCount, "Frame contacts count must match sequence contacts count");
            Assert.AreEqual(RaycastsCount, frame.RaycastsCount, "Raycasts count must match sequence raycast count");

            m_Frames.Add(frame);
        }

        public void RemoveFrame(int idx)
        {
            m_Frames.RemoveAt(idx);
        }

        public void RemoveLastFrame()
        {
            m_Frames.RemoveAt(m_Frames.Count - 1);
        }

        public void Resize(int length)
        {
            var currentLength = Length;

            if (currentLength == length)
                return;

            if (currentLength < length)
            {
                var framesToAdd = length - currentLength;
                for (var i = 0; i < framesToAdd; i++)
                {
                    AddFrame();
                }

                return;
            }

            var framesToRemove = currentLength - length;
            for (var i = 0; i < framesToRemove; i++)
            {
                RemoveLastFrame();
            }
        }

        public PoseFrame GetFrame(int idx)
        {
            return m_Frames[idx];
        }
    }
}
