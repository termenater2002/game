using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CameraData
    {
        public Camera Target;
        public Vector2 ViewportSize;
        public Vector2 ViewportCursor;
        public Vector2 ViewportPosition;
        public Vector2 RenderScaling = Vector2.one;
        public RigidTransformModel RigidTransformModel;
        public bool IsDraggingControl;
    }
}
