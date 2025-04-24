using UnityEngine;

namespace Unity.DeepPose.Core
{
    class PoseComparison
    {
        public int JointsCount => m_LocalRotations.Length;

        public float[] GlobalPositions => m_GlobalPositions;
        public float[] LocalRotations => m_LocalRotations;
        public float[] GlobalRotations => m_GlobalRotations;

        float[] m_GlobalPositions;
        float[] m_LocalRotations;
        float[] m_GlobalRotations;

        public PoseComparison(int jointsCount)
        {
            m_GlobalPositions = new float[jointsCount];
            m_LocalRotations = new float[jointsCount];
            m_GlobalRotations = new float[jointsCount];
        }

        public void Update(PoseFrame frame1, PoseFrame frame2)
        {
            for (var i = 0; i < frame1.GlobalPositions.Length; i++)
            {
                m_LocalRotations[i] = Quaternion.Angle(frame1.LocalRotations[i], frame2.LocalRotations[i]);
                m_GlobalRotations[i] = Quaternion.Angle(frame1.GlobalRotations[i], frame2.GlobalRotations[i]);
                m_GlobalPositions[i] = (frame1.GlobalPositions[i] - frame2.GlobalPositions[i]).magnitude;
            }
        }
    }
}
