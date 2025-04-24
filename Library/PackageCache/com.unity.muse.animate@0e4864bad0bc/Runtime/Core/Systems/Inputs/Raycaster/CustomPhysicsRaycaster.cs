using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace Unity.Muse.Animate
{
    [RequireComponent(typeof(Camera))]
    class CustomPhysicsRaycaster : BaseRaycaster
    {
        struct RayAttempt
        {
            public Ray Ray;
            public Color Color;
        }
        
        protected const int kNoEventMaskSet = -1;
        protected Camera m_EventCamera;
        
        List<RayAttempt> m_RayAttempts = new List<RayAttempt>();

        [SerializeField]
        protected LayerMask m_EventMask = kNoEventMaskSet;

        /// <summary>
        /// The max number of intersections allowed. 0 = allocating version anything else is non alloc.
        /// </summary>
        [SerializeField]
        protected int m_MaxRayIntersections = 0;
        protected int m_LastMaxRayIntersections = 0;

        RaycastHit[] m_Hits;
        List<GameObject> m_InfiniteObjects = new ();

        protected CustomPhysicsRaycaster()
        {
            
        }

        public override Camera eventCamera
        {
            get
            {
                if (m_EventCamera == null)
                    m_EventCamera = GetComponent<Camera>();

                if (m_EventCamera == null)
                    return Camera.main;

                return m_EventCamera ;
            }
        }

        /// <summary>
        /// Depth used to determine the order of event processing.
        /// </summary>
        public virtual int depth
        {
            get { return (eventCamera != null) ? (int)eventCamera.depth : 0xFFFFFF; }
        }

        /// <summary>
        /// Event mask used to determine which objects will receive events.
        /// </summary>
        public int finalEventMask
        {
            get { return (eventCamera != null) ? eventCamera.cullingMask & m_EventMask : kNoEventMaskSet; }
        }

        /// <summary>
        /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
        /// </summary>
        public LayerMask eventMask
        {
            get { return m_EventMask; }
            set { m_EventMask = value; }
        }

        /// <summary>
        /// Max number of ray intersection allowed to be found.
        /// </summary>
        /// <remarks>
        /// A value of zero will represent using the allocating version of the raycast function where as any other value will use the non allocating version.
        /// </remarks>
        public int maxRayIntersections
        {
            get { return m_MaxRayIntersections; }
            set { m_MaxRayIntersections = value; }
        }

        /// <summary>
        /// Returns a ray going from camera through the event position and the distance between the near and far clipping planes along that ray.
        /// </summary>
        /// <param name="eventData">The pointer event for which we will cast a ray.</param>
        /// <param name="ray">The ray to use.</param>
        /// <param name="eventDisplayIndex">The display index used.</param>
        /// <param name="distanceToClipPlane">The distance between the near and far clipping planes along the ray.</param>
        /// <returns>True if the operation was successful. false if it was not possible to compute, such as the eventPosition being outside of the view.</returns>
        protected bool ComputeRayAndDistance(PointerEventData eventData, ref Ray ray, ref int eventDisplayIndex, ref float distanceToClipPlane, bool debug = false)
        {
            if (eventCamera == null)
                return false;
            
            var eventPosition = RelativeMouseAtScaled(eventData.position);
            
            if (eventPosition != Vector3.zero)
            {
                // We support multiple display and display identification based on event position.
                eventDisplayIndex = (int)eventPosition.z;

                // Discard events that are not part of this display so the user does not interact with multiple displays at once.
                if (eventDisplayIndex != eventCamera.targetDisplay)
                    return false;
            }
            else
            {
                // The multiple display system is not supported on all platforms, when it is not supported the returned position
                // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
                eventPosition = eventData.position;
                
                Log($"Position based on eventData: {eventPosition}");
            }

            eventCamera.forceIntoRenderTexture = true;
            
            // Cull ray casts that are outside of the view rect. (case 636595)
            if (!eventCamera.pixelRect.Contains(eventPosition))
            {
                Log($"Event position {eventPosition} is outside of eventCamera.pixelRect: {eventCamera.pixelRect}");
                return false;
            }
            

            ray = eventCamera.ScreenPointToRay(eventPosition);
                Log($"ScreenToViewportPoint: "+eventCamera.ScreenToViewportPoint(eventPosition));
                
            // compensate far plane distance - see MouseEvents.cs
            float projectionDirection = ray.direction.z;
            distanceToClipPlane = Mathf.Approximately(0.0f, projectionDirection)
                ? Mathf.Infinity
                : Mathf.Abs((eventCamera.farClipPlane - eventCamera.nearClipPlane) / projectionDirection);
            return true;
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            Raycast(eventData, resultAppendList, false);
        }

        public void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList, bool debug)
        {
            var ray = new Ray();
            var displayIndex = 0;
            var distanceToClipPlane = 0f;
            
            if (!ComputeRayAndDistance(eventData, ref ray, ref displayIndex, ref distanceToClipPlane, debug))
            {
                LogRayAttempt(ray, Color.magenta);
                return;
            }

            // Handle infinite objects
            foreach (var infiniteObject in m_InfiniteObjects)
            {
                if (!CheckValidityAndGetSorting(infiniteObject, eventData.position, default, out var sortingOrder, out var sortingDepth))
                    continue;

                var result = new RaycastResult
                {
                    gameObject = infiniteObject,
                    module = this,
                    distance = 0f,
                    worldPosition = Vector3.zero,
                    worldNormal = Vector3.zero,
                    screenPosition = eventData.position,
                    displayIndex = displayIndex,
                    index = resultAppendList.Count,
                    sortingLayer = 0,
                    sortingOrder = sortingOrder,
                    depth = sortingDepth
                };

                resultAppendList.Add(result);
            }

            var hitCount = 0;

            if (m_MaxRayIntersections == 0)
            {
                Debug.LogError("Using allocating raycast, please define max rays intersections instead");
                return;
            }
            else
            {
                if (m_LastMaxRayIntersections != m_MaxRayIntersections)
                {
                    m_Hits = new RaycastHit[m_MaxRayIntersections];
                    m_LastMaxRayIntersections = m_MaxRayIntersections;
                }

                hitCount = RaycastNonAlloc(ray, m_Hits, distanceToClipPlane, finalEventMask);
            }

            if (hitCount != 0)
            {
                if (hitCount > 1)
                    System.Array.Sort(m_Hits, 0, hitCount, RaycastHitComparer.instance);

                for (int b = 0, bmax = hitCount; b < bmax; ++b)
                {
                    var raycastHit = m_Hits[b];

                    if (!CheckValidityAndGetSorting(raycastHit.collider.gameObject, eventData.position, raycastHit, out var sortingOrder, out var sortingDepth))
                        continue;

                    var result = new RaycastResult
                    {
                        gameObject = raycastHit.collider.gameObject,
                        module = this,
                        distance = raycastHit.distance,
                        worldPosition = raycastHit.point,
                        worldNormal = raycastHit.normal,
                        screenPosition = eventData.position,
                        displayIndex = displayIndex,
                        index = resultAppendList.Count,
                        sortingLayer = 0,
                        sortingOrder = sortingOrder,
                        depth = sortingDepth
                    };

                    resultAppendList.Add(result);
                    LogRayAttempt(ray, Color.green);
                }
            }
            else
            {
                LogRayAttempt(ray, Color.red);
            }
        }
        
        public int RaycastNonAlloc(
            Ray ray,
            RaycastHit[] results,
            float maxDistance,
            int layerMask)
        {
            var physicsScene = PhysicsSceneExtensions.GetPhysicsScene(gameObject.scene);
            return physicsScene.Raycast(ray.origin, ray.direction, results, maxDistance, layerMask);
        }
        
        bool CheckValidityAndGetSorting(GameObject go, Vector2 screenPosition, RaycastHit hit, out int sortingOrder, out int depth)
        {
            sortingOrder = 0;
            depth = 0;

            var raycastValidationHandler = GetPhysicsRaycastHandlerInHierarchy(go);
            if (raycastValidationHandler != null)
            {
                if (!raycastValidationHandler.ValidateRaycastHit(screenPosition, hit))
                    return false;

                sortingOrder = raycastValidationHandler.GetPhysicsRaycastSortingOrder(screenPosition, hit);
                depth = raycastValidationHandler.GetPhysicsRaycastDepth(screenPosition, hit);
            }

            return true;
        }

        class RaycastHitComparer : IComparer<RaycastHit>
        {
            public static RaycastHitComparer instance = new RaycastHitComparer();
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }

        /// <summary>
        /// A version of Display.RelativeMouseAt that scales the position when the main display has a different rendering resolution to the system resolution.
        /// By default, the mouse position is relative to the main render area, we need to adjust this so it is relative to the system resolution
        /// in order to correctly determine the position on other displays.
        /// </summary>
        /// <returns></returns>
        static Vector3 RelativeMouseAtScaled(Vector2 position)
        {
            // If the main display is now the same resolution as the system then we need to scale the mouse position. (case 1141732)
            if (Display.main.renderingWidth != Display.main.systemWidth || Display.main.renderingHeight != Display.main.systemHeight)
            {
                // Calculate any padding that may be added when the rendering apsect ratio does not match the system aspect ratio.
                int widthPlusPadding = Screen.fullScreen ? Display.main.renderingWidth : (int)(Display.main.renderingHeight * (Display.main.systemWidth / (float)Display.main.systemHeight));

                // Calculate the padding on each side of the screen.
                int padding = Screen.fullScreen ? 0 : (int)((widthPlusPadding - Display.main.renderingWidth) * 0.5f);
                int widthPlusRightPadding = widthPlusPadding - padding;

                // If we are not inside of the main display then we must adjust the mouse position so it is scaled by
                // the main display and adjusted for any padding that may have been added due to different aspect ratios.
                if ((position.y < 0 || position.y > Display.main.renderingHeight ||
                     position.x < 0 || position.x > widthPlusRightPadding))
                {
                    if (!Screen.fullScreen)
                    {
                        // When in windowed mode, the window will be centered with the 0,0 coordinate at the top left, we need to adjust so it is relative to the screen instead.
                        position.x -= (Display.main.renderingWidth - Display.main.systemWidth) * 0.5f;
                        position.y -= (Display.main.renderingHeight - Display.main.systemHeight) * 0.5f;
                    }
                    else
                    {
                        // Scale the mouse position
                        position.x += padding;

                        float xScale = Display.main.systemWidth / (float)widthPlusPadding;
                        float yScale = Display.main.systemHeight / (float)Display.main.renderingHeight;
                        position.x *= xScale;
                        position.y *= yScale;
                    }

                    return Display.RelativeMouseAt(position);
                }
                else
                {
                    // We are using the main display.
                    return new Vector3(position.x, position.y, 0);
                }
            }

            return Display.RelativeMouseAt(position);
        }

        static bool IsActivePhysicsRaycastHandler(Component component)
        {
            var valid = component is IPhysicsRaycastHandler;
            if (!valid)
                return false;

            var behaviour = component as Behaviour;
            if (behaviour != null)
                return behaviour.isActiveAndEnabled;
            return true;
        }

        static IPhysicsRaycastHandler GetPhysicsRaycastHandler(GameObject go)
        {
            if (go == null || !go.activeInHierarchy)
                return null;

            IPhysicsRaycastHandler res = null;

            var components = ListPool<Component>.Get();
            go.GetComponents(components);

            var componentsCount = components.Count;
            for (var i = 0; i < componentsCount; i++)
            {
                if (!IsActivePhysicsRaycastHandler(components[i]))
                    continue;

                res = (IPhysicsRaycastHandler)components[i];
            }
            ListPool<Component>.Release(components);

            return res;
        }

        static IPhysicsRaycastHandler GetPhysicsRaycastHandlerInHierarchy(GameObject root)
        {
            if (root == null)
                return null;

            var t = root.transform;
            while (t != null)
            {
                var res = GetPhysicsRaycastHandler(t.gameObject);
                if (res != null)
                    return res;

                t = t.parent;
            }

            return null;
        }

        public void RegisterInfiniteObject(GameObject go)
        {
            Assert.IsFalse(m_InfiniteObjects.Contains(go), "Infinite object already registered");
            m_InfiniteObjects.Add(go);
        }

        public void UnregisterInfiniteObject(GameObject go)
        {
            Assert.IsTrue(m_InfiniteObjects.Contains(go), "Infinite object not registered");
            m_InfiniteObjects.Remove(go);
        }

        void LogRayAttempt(Ray ray, Color color)
        {
            if (!ApplicationConstants.DebugRaycastEvents)
                return;
            
            m_RayAttempts.Add(new RayAttempt(){Ray = ray, Color = color});
            
            if (m_RayAttempts.Count > 100)
            {
                m_RayAttempts.RemoveAt(0);
            }
        }

        void OnDrawGizmos()
        {
            if (!ApplicationConstants.DebugRaycastEvents)
                return;
            
            foreach (var attempt in m_RayAttempts)
            {
                Gizmos.color = attempt.Color;
                Gizmos.DrawRay(attempt.Ray);
            }
        }
        
        // [Section] Debugging
        
        void Log(string msg)
        {
            if (!ApplicationConstants.DebugRaycastEvents)
                return;

            Debug.Log(GetType().Name + " -> " + msg);
        }
        
        
    }
}
