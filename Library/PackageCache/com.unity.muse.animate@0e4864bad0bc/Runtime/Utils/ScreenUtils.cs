using UnityEngine;

namespace Unity.Muse.Animate
{
    static class ScreenUtils
    {
        public static Vector3 ScreenToWorld(CameraModel cam, Vector2 screenPosition, Vector3 parallelPlaneOrigin)
        {
            var handlePositionOnScreen = cam.Target.WorldToScreenPoint(parallelPlaneOrigin);
            var screenPositionWithDepth = new Vector3(screenPosition.x, screenPosition.y, handlePositionOnScreen.z);
            var worldPosition = cam.Target.ScreenToWorldPoint(screenPositionWithDepth);
            return worldPosition;
        }

        public static Vector3 ScreenToAxisPoint(CameraModel cam, Vector3 screenPoint, Vector3 axisPoint, Vector3 axisDirection)
        {
            var cursorRay = cam.Target.ScreenPointToRay(screenPoint);
            var cursorOrigin = cursorRay.origin;
            var cursorDirection = cursorRay.direction;
            MathUtils.ClosestPointsOnTwoLines(cursorOrigin, cursorDirection, axisPoint, axisDirection, out var closestCursorPoint, out var closestPointOnAxis);
            return closestPointOnAxis;
        }

        public static Vector3 ScreenToAxisDistance(CameraModel cam, Vector3 screenPoint, Vector3 axisPoint, Vector3 axisDirection)
        {
            var cursorRay = cam.Target.ScreenPointToRay(screenPoint);
            var cursorOrigin = cursorRay.origin;
            var cursorDirection = cursorRay.direction;
            MathUtils.ClosestPointsOnTwoLines(cursorOrigin, cursorDirection, axisPoint, axisDirection, out var closestCursorPoint, out var closestPointOnAxis);
            return closestCursorPoint-closestPointOnAxis;
        }

        public static Vector3 ScreenToPlanePoint(CameraModel cam, Vector3 screenPoint, Transform transform, Vector3 localPlaneNormal)
        {
            return ScreenToPlanePoint(cam, screenPoint, transform.rotation * localPlaneNormal, transform.position);
        }

        public static Vector3 ScreenToPlanePoint(CameraModel cam, Vector3 screenPoint, Vector3 worldPlaneNormal, Vector3 worldPlanePosition)
        {
            var plane = new Plane(worldPlaneNormal, worldPlanePosition);
            var ray = cam.Target.ScreenPointToRay(screenPoint);
            plane.Raycast(ray, out var enter);
            return ray.origin + ray.direction * enter;
        }

        public static Vector3 ScreenToPlaneDistanceFromCenter(CameraModel cam, Vector3 screenPoint, Transform transform, Vector3 localPlaneNormal)
        {
            return ScreenToPlaneDistanceFromCenter(cam, screenPoint, transform.rotation * localPlaneNormal, transform.position);
        }

        public static Vector3 ScreenToPlaneDistanceFromCenter(CameraModel cam, Vector3 screenPoint, Vector3 worldPlaneNormal, Vector3 worldPlanePosition)
        {
            var worldScreenPointPosition = ScreenToPlanePoint(cam, screenPoint, worldPlaneNormal, worldPlanePosition);
            return worldScreenPointPosition - worldPlanePosition;
        }
        
        public static Vector2 DistanceToScreenCenter(CameraModel cam, Vector2 screenPosition)
        {
            var distance = screenPosition - ScreenCenter(cam);
            return distance;
        }

        public static Vector2 ScreenCenter(CameraModel cam)
        {
            return cam.ViewportSize * 0.5f;
        }

        public static Vector3 CalculateWorldDistanceFromPixelDistance(CameraModel camera, Vector3 worldPosition, Vector2 pixelDistance)
        {
            var viewportSize = camera.ViewportSize;
            if (viewportSize.magnitude <= 0)
                return Vector3.one;
            
            var center = worldPosition;
            var centerVp = camera.Target.WorldToViewportPoint(center);
            var centerScreen = new Vector3(centerVp.x * camera.ViewportSize.x, (float)centerVp.y * camera.ViewportSize.y, centerVp.z);
            var targetVp = new Vector3((centerScreen.x + pixelDistance.x)/camera.ViewportSize.x, (centerScreen.y+pixelDistance.y)/camera.ViewportSize.y, centerScreen.z);
            var targetDist = camera.Target.ViewportToWorldPoint(targetVp) - center;
            return targetDist;
        }
    }
}
