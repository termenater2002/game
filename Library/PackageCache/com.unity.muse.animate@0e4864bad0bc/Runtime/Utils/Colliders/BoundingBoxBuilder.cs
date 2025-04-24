using UnityEngine;

namespace Unity.Muse.Animate
{
    class BoundingBoxBuilder
    {
        public int NumPoints => m_NumPoints;
        public Vector3 Center => (m_Min + m_Max) / 2;
        public Vector3 Size => (m_Max - m_Min);
        public Vector3 Min => m_Min;
        public Vector3 Max => m_Max;

        int m_NumPoints;
        Vector3 m_Min;
        Vector3 m_Max;

        public BoundingBoxBuilder()
        {
            m_NumPoints = 0;
        }

        public void AddPoint(Vector3 position)
        {
            if (m_NumPoints == 0)
            {
                m_Min = position;
                m_Max = position;
            }
            else
            {
                if (position.x < m_Min.x)
                    m_Min.x = position.x;
                if (position.y < m_Min.y)
                    m_Min.y = position.y;
                if (position.z < m_Min.z)
                    m_Min.z = position.z;

                if (position.x > m_Max.x)
                    m_Max.x = position.x;
                if (position.y > m_Max.y)
                    m_Max.y = position.y;
                if (position.z > m_Max.z)
                    m_Max.z = position.z;
            }

            m_NumPoints++;
        }
    }
}
