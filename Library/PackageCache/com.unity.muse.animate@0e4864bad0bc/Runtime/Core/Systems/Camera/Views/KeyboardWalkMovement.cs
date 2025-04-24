using UnityEngine;

namespace Unity.Muse.Animate
{
    class KeyboardWalkMovement
    {
        const float k_MainSpeed = 4.0f;
        const float k_ShiftAdd = 10.0f;
        const float k_MaxShift = 20.0f;

        Vector3 m_Movement;
        float m_TotalRun;

        public Vector3 Movement => m_Movement;

        public KeyboardWalkMovement()
        {
            Reset();
        }

        public void Update(float deltaTime)
        {
            m_Movement = Vector3.zero;

            if (InputUtils.IsKeyHeld(KeyCode.W))
            {
                m_Movement += Vector3.forward;
            }

            if (InputUtils.IsKeyHeld(KeyCode.S))
            {
                m_Movement += Vector3.back;
            }

            if (InputUtils.IsKeyHeld(KeyCode.A))
            {
                m_Movement += Vector3.left;
            }

            if (InputUtils.IsKeyHeld(KeyCode.D))
            {
                m_Movement += Vector3.right;
            }

            if (InputUtils.IsKeyHeld(KeyCode.Q))
            {
                m_Movement += Vector3.down;
            }

            if (InputUtils.IsKeyHeld(KeyCode.E))
            {
                m_Movement += Vector3.up;
            }

            if (m_Movement.sqrMagnitude == 0)
                return;

            if (InputUtils.IsShift())
            {
                m_TotalRun += deltaTime;
                m_Movement *= m_TotalRun * k_ShiftAdd;
                m_Movement.x = Mathf.Clamp(m_Movement.x, -k_MaxShift, k_MaxShift);
                m_Movement.y = Mathf.Clamp(m_Movement.y, -k_MaxShift, k_MaxShift);
                m_Movement.z = Mathf.Clamp(m_Movement.z, -k_MaxShift, k_MaxShift);
            }
            else
            {
                m_TotalRun = Mathf.Clamp(m_TotalRun / (1 + 10 * deltaTime), 1f, 1000f);
                m_Movement *= k_MainSpeed;
            }
        }

        public void Reset()
        {
            m_TotalRun = 1f;
            m_Movement = Vector3.zero;
        }
    }
}
