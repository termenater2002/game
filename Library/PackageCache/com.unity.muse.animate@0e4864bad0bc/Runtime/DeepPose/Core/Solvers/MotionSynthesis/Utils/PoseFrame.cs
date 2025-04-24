using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    class PoseFrame
    {
        public Vector3[] GlobalPositions => m_GlobalPositions;
        public Quaternion[] LocalRotations => m_LocalRotations;
        public Quaternion[] GlobalRotations => m_GlobalRotations;
        public float[] Contacts => m_Contacts;
        public float[] RaycastDistances => m_Raycasts;
        public int JointsCount => m_GlobalPositions.Length;
        public int ContactsCount => m_Contacts.Length;
        public int RaycastsCount => m_Raycasts.Length;

        Vector3[] m_GlobalPositions;
        Quaternion[] m_LocalRotations;
        Quaternion[] m_GlobalRotations;
        float[] m_Contacts;
        float[] m_Raycasts;

        public PoseFrame(int jointsCount, int contactsCount, int raycastsCount)
        {
            m_GlobalPositions = new Vector3[jointsCount];
            m_LocalRotations = new Quaternion[jointsCount];
            m_GlobalRotations = new Quaternion[jointsCount];
            m_Contacts = new float[contactsCount];
            m_Raycasts = new float[raycastsCount];
        }

        public PoseFrame Clone()
        {
            var frame = new PoseFrame(JointsCount, ContactsCount, RaycastsCount);
            CopyTo(frame);
            return frame;
        }

        public void CopyTo(PoseFrame other)
        {
            Assert.AreEqual(m_GlobalPositions.Length, other.m_GlobalPositions.Length, "Frames must have the same number of global positions");
            Assert.AreEqual(m_LocalRotations.Length, other.m_LocalRotations.Length, "Frames must have the same number of local rotations");
            Assert.AreEqual(m_GlobalRotations.Length, other.m_GlobalRotations.Length, "Frames must have the same number of global rotations");
            Assert.AreEqual(m_Contacts.Length, other.m_Contacts.Length, "Frames must have the same number of contacts");
            Assert.AreEqual(m_Raycasts.Length, other.m_Raycasts.Length, "Frames must have the same number of raycasts");

            m_GlobalPositions.CopyTo(other.m_GlobalPositions, 0);
            m_LocalRotations.CopyTo(other.m_LocalRotations, 0);
            m_GlobalRotations.CopyTo(other.m_GlobalRotations, 0);
            m_Contacts.CopyTo(other.m_Contacts, 0);
            m_Raycasts.CopyTo(other.m_Raycasts, 0);
        }
    }
}
