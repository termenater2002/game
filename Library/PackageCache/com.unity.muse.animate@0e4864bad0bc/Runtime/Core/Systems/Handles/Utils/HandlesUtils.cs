using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Muse.Animate
{
    static class HandlesUtils
    {
        public const float ThicknessRegular = 0.15f;
        public const float ThicknessBold = 0.4f;
        public const float ThicknessRegularPixels = 1f;
        public const float ThicknessBoldPixels = 3f;

        public const float LineSize = 2f * NootsPerPixels;
        public const float PixelsPerNoots = 100f;
        public const float NootsPerPixels = 1f / PixelsPerNoots;

        /// <summary>
        /// Creates a new GameObject and adds the specified type of HandleElementView component to it.
        /// </summary>
        /// <param name="createdObjectName">The name of the new GameObject created.</param>
        /// <param name="parent">The Transform the new GameObject will be parented to.</param>
        /// <param name="priority">The PhysicsRaycastDepth the new HandleElementView component will have.</param>
        /// <typeparam name="T">The type of HandleElementView component to attach to the new GameObject instance.</typeparam>
        /// <returns>The newly created HandleElementView component.</returns>
        public static T CreateElement<T>(string createdObjectName, Transform parent, int priority = 0) where T : ControlView
        {
            var createdObject = Locator.Get<IRootObjectSpawner<GameObject>>().CreateGameObject(createdObjectName);
            createdObject.transform.SetParent(parent, false);
            createdObject.layer = ApplicationLayers.LayerHandles;

            var handleElement = createdObject.AddComponent<T>();
            handleElement.PhysicsRaycastDepth = priority;
            handleElement.Initialize();
            return handleElement;
        }

        /// <summary>
        /// Returns the distance from a point in world space to the pointer position on the camera's output viewport in pixel space.
        /// </summary>
        /// <param name="camera">The camera rendering the output viewport.</param>
        /// <param name="point">The point in world space.</param>
        /// <returns></returns>
        public static float GetWorldToViewportPixelDistanceFromCursor(CameraModel camera, Vector3 point, bool debug)
        {
            var viewportRatio = camera.WorldToViewportPoint(point);
            var viewportPixel = new Vector2(viewportRatio.x * camera.ViewportSize.x, viewportRatio.y * camera.ViewportSize.y);
            var diff = new Vector2(camera.ViewportCursor.x, camera.ViewportCursor.y) - viewportPixel;
            var dist = diff.magnitude;

            if (debug)
                Debug.Log($"camera.ViewportCursor: {camera.ViewportCursor}, viewportRatio: {viewportRatio}, viewportPixel: {viewportPixel}, dist: {dist}");

            return diff.magnitude;
        }

        public static float GetAlphaFromDistanceFromCursor(float distance)
        {
            return Mathf.Max(0f, Mathf.Min(1f, 1f - (distance - 2f) / 4f));
        }

        /// <summary>
        /// Returns the scale required for an object to stay at the same size when viewed from the specified camera.
        /// </summary>
        /// <param name="camera">The camera viewing the position.</param>
        /// <param name="position">The position in world space of the object viewed from the camera.</param>
        /// <returns>A scaling ratio which can be applied to a Transform for it to remain the same size on screen.</returns>
        public static float ComputeSizeRatio(CameraModel camera, Vector3 position)
        {
            // Note:
            // AVOID camera.WorldToScreenPoint,
            // it does not return the coordinates relative to the renderTexture we use for the viewport,
            // instead, it uses the current user's screen size, which is different.

            var compare = position + camera.Target.transform.right;
            var compareV = camera.Target.WorldToViewportPoint(compare);
            var compareScreen = camera.ViewportSize * compareV;

            var positionV = camera.Target.WorldToViewportPoint(position);
            var positionScreen = camera.ViewportSize * positionV;

            var unitSizeOnScreen = (compareScreen - positionScreen).magnitude;
            var sizeRatio = (PixelsPerNoots / unitSizeOnScreen);
            return sizeRatio;
        }

        public static float ComputeCameraFacingRatio(CameraModel camera, Vector3 worldForward, Vector3 position)
        {
            // Note:
            // AVOID camera.WorldToScreenPoint,
            // it does not return the coordinates relative to the renderTexture we use for the viewport,
            // instead, it uses the current user's screen size, which is different.

            var positionV = camera.Target.WorldToViewportPoint(position);
            var ray = camera.Target.ViewportPointToRay(positionV);
            var facingRatio = Vector3.Angle(ray.direction, worldForward) / 180f;

            if (facingRatio < 0.5)
            {
                facingRatio = 1f - facingRatio;
            }

            facingRatio = (facingRatio - 0.5f) * 2f;
            return facingRatio;
        }

        public static LineRenderer CreateLine(GameObject container, string childName = null, bool enabled = false)
        {
            var shapeContainer = childName != null ? CreateChild(childName, container.transform) : container;
            var line = shapeContainer.AddComponent<LineRenderer>();
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.alignment = LineAlignment.View;
            line.loop = false;
            line.material = GetNewLineMaterial(true);
            line.widthMultiplier = LineSize;
            line.useWorldSpace = false;
            line.enabled = enabled;
            return line;
        }

        public static QuadMesh CreateQuad(GameObject container, string childName = null, bool enabled = false)
        {
            var shapeContainer = childName != null ? CreateChild(childName, container.transform) : container;
            var line = shapeContainer.AddComponent<QuadMesh>();
            line.enabled = enabled;
            return line;
        }

        public static ConeMesh CreateCone(GameObject container, string childName = null, bool enabled = false)
        {
            var shapeContainer = childName != null ? CreateChild(childName, container.transform) : container;
            var meshComponent = shapeContainer.AddComponent<ConeMesh>();
            meshComponent.enabled = enabled;
            return meshComponent;
        }

        public static DiscMesh CreateDisc(GameObject container, string childName = null, bool enabled = false)
        {
            var shapeContainer = childName != null ? CreateChild(childName, container.transform) : container;
            var meshComponent = shapeContainer.AddComponent<DiscMesh>();
            meshComponent.enabled = enabled;
            return meshComponent;
        }

        static GameObject CreateChild(string objectName, Transform parent)
        {
            var createdObject = new GameObject(objectName);

            createdObject.transform.SetParent(parent, false);
            createdObject.layer = ApplicationLayers.LayerHandles;

            return createdObject;
        }

        static Material GetNewHandlesMaterial()
        {
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                return new Material(Application.Instance.ApplicationConfiguration.HandlesConfiguration.HandlesMaterialHDRP);
            }
            if (RenderPipelineUtils.IsUsingUrp())
            {
                return new Material(Application.Instance.ApplicationConfiguration.HandlesConfiguration.HandlesMaterialURP);
            }
            return new Material(Application.Instance.ApplicationConfiguration.HandlesConfiguration.HandlesMaterialBRP);
        }

        public static Material GetNewMeshMaterial(bool transparent = true)
        {
            return GetNewHandlesMaterial();
        }

        public static Material GetNewLineMaterial(bool transparent = true)
        {
            return GetNewHandlesMaterial();
        }

        public static Material GetNewOutlinerMaterial()
        {
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                return new Material(Application.Instance.ApplicationConfiguration.HandlesConfiguration.OutlinerMaterialHDRP);
            }
            if (RenderPipelineUtils.IsUsingUrp())
            {
                return new Material(Application.Instance.ApplicationConfiguration.HandlesConfiguration.OutlinerMaterialURP);
            }
            return new Material(Application.Instance.ApplicationConfiguration.HandlesConfiguration.OutlinerMaterialBRP);
        }

        /// <summary>
        /// Temporary fix used because some settings from Shader Graph don't seem to properly load when
        /// instantiating materials with it. (Transparency doesn't work)
        /// </summary>
        /// <param name="material">The material to "fix"</param>
        /// <param name="isTransparent">If the material is a transparent surface</param>
        public static Material SetMaterialTransparent(Material material, bool isTransparent)
        {
            // No Shadows
            material.SetShaderPassEnabled("SHADOWCASTER", false);

            if (RenderPipelineUtils.IsUsingHdrp())
            {
                material.EnableKeyword("_ALPHABLEND_ON");
                material.SetFloat("_SurfaceType", isTransparent ? 1 : 0);
                material.SetFloat("_TransparentSortPriority", isTransparent ? 1 : 0);
                material.SetFloat("_SrcBlend", isTransparent ? 1 : 0);
                material.SetFloat("_AlphaSrcBlend", isTransparent ? 1 : 0);
                material.SetFloat("_DstBlend", isTransparent ? 0 : 1);
                material.SetFloat("_AlphaDstBlend", isTransparent ? 0 : 1);
                material.SetFloat("_ZWrite", isTransparent ? 1 : 1);
                material.SetFloat("_TransparentZWrite", isTransparent ? 1 : 1);
                material.renderQueue = isTransparent ? 3000 : 2000;
            }
            else
            {
                material.SetFloat("_Surface", isTransparent ? 1 : 0);
                material.renderQueue = isTransparent ? 3000 : 2000;
                material.SetFloat("_DstBlend", isTransparent ? 10 : 0);
                material.SetFloat("_SrcBlend", isTransparent ? 5 : 1);
                material.SetFloat("_ZWrite", isTransparent ? 0 : 1);
            }
            return material;
        }

        public static void RotateVertices(Vector3 eulerRotation, ref Vector3[] vertices)
        {
            var quaternion = Quaternion.Euler(eulerRotation);

            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = quaternion * vertices[i];
            }
        }

        public static int GetCircleVertexIndex(int step, int resolution)
        {
            resolution = Mathf.Max(3, resolution);

            var answer = step;

            while (answer >= resolution)
            {
                answer -= resolution;
            }

            return answer;
        }

        public static Vector3 CalculateNormal(int startAtTriangleIndex, ref Vector3[] vertices, ref int[] triangles)
        {
            return CalculateNormal(vertices[startAtTriangleIndex], vertices[triangles[startAtTriangleIndex + 1]], vertices[triangles[startAtTriangleIndex + 2]]);
        }

        public static Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 side1 = b - a;
            Vector3 side2 = c - a;
            return Vector3.Cross(side1, side2).normalized;
        }

        public static Vector3 GetCircleXZPoint(float radius, int t, int resolution)
        {
            var angle = 2f * Mathf.PI * t / resolution;
            var dx = Mathf.Cos(angle) * radius;
            var dz = Mathf.Sin(angle) * radius;
            return new Vector3(dx, 0f, dz);
        }

        public static float PixelsToNoots(float pixelDistance)
        {
            return pixelDistance * NootsPerPixels;
        }
    }
}
