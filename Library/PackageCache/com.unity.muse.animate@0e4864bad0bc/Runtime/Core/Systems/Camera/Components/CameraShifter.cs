using UnityEngine;

namespace Unity.Muse.Animate
{
    [ExecuteInEditMode]
    class CameraShifter : MonoBehaviour
    {
        [SerializeField]
        private Vector2 offsetAll = Vector2.zero;
    
        [SerializeField]
        private Camera camcam;

        //Where you want the vanishing point to be instead of the centre.
        [SerializeField]
        private Vector2 offsetVanishingPoint = Vector2.zero;

        /// <summary>
        /// Centre the camera should rotate z axis around, in case it shouldn't really be the centre
        /// </summary>
        [SerializeField]
        private Vector2 rotationCentre = Vector2.zero;

        //presumed distance of camera from subject, used to compensate 
        [SerializeField]
        private float presumedDistance = 10f;

        //scales the z to compensate for things looking too tall / long
        [SerializeField]
        private float zscale = 1f;

        void Update()
        {
            SetCameraMatrix();
        }

        public void SetCameraMatrix()
        {
            if (camcam != null)
            {
                camcam.ResetProjectionMatrix();
            
                var offsetVanishingPointB = offsetVanishingPoint + offsetAll;
                var rotationCentreB = rotationCentre + offsetAll;

                var fov = camcam.fieldOfView; //fov seems to be calculated based on vertical
                var aspect = camcam.aspect; //post offset needs to account for aspect ratio

                var combinedOffset = offsetVanishingPointB - rotationCentreB; //combine offset and rotationCentre

                //convert fov angle into slope and multiply by presumedDistance to get a compensating offset
                var slope = Mathf.Tan(Mathf.Deg2Rad * fov / 2f);
                var preOffset = new Vector2(combinedOffset.x * aspect, combinedOffset.y) * -slope * presumedDistance;

                var projectionMatrixBefore = camcam.projectionMatrix; //get starting point from camera

                //scales Z relative to the presumed distance and offsets X & Y according to preOffset
                var preTransform = Matrix4x4.Translate(Vector3.forward * -presumedDistance) * Matrix4x4.Scale(new Vector3(1, 1, zscale)) * Matrix4x4.Translate(new Vector3(preOffset.x, preOffset.y, presumedDistance));

                //shifts vanishing point
                var postTransform = Matrix4x4.Translate(offsetVanishingPointB);
            
                //applies everything to get the new projectionMatrix
                var projectionMatrixAfter = postTransform * projectionMatrixBefore * preTransform;

                //camcam.projectionMatrix = projectionMatrixAfter;
                camcam.projectionMatrix = projectionMatrixAfter;
            }
        }
    }
}