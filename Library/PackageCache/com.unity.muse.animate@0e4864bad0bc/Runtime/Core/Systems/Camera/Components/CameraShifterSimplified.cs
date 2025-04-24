using UnityEngine;

namespace Unity.Muse.Animate
{
    [ExecuteInEditMode]
    class CameraShifterSimplified : MonoBehaviour
    {
        [SerializeField]
        Vector2 Translate = Vector2.zero;

        void Update()
        {
            SetCameraMatrix();
        }

        void SetCameraMatrix()
        {
            var cam = GetComponent<Camera>();
            if (cam == null)
                return;
        
            cam.ResetProjectionMatrix();
            cam.projectionMatrix = Matrix4x4.Translate(Translate) * cam.projectionMatrix;
        }
    }
}