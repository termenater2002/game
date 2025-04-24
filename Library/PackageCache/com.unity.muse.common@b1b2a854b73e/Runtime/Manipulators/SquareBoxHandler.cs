
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    internal class SquareBoxHandler : Manipulator
    {
        VisualElement m_GeometryChangedTarget;
        public SquareBoxHandler(VisualElement geometryChangedTarget)
        {
            m_GeometryChangedTarget = geometryChangedTarget;
        }
        protected override void RegisterCallbacksOnTarget()
        {
            m_GeometryChangedTarget.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            target.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            OnGeometryChanged(null);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            m_GeometryChangedTarget.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            target.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var resolvedDelta = target.worldBound.width - target.worldBound.height;

            target.style.height = target.resolvedStyle.width;
            target.style.paddingLeft =resolvedDelta / 2;
            target.style.paddingRight = resolvedDelta / 2;
        }
    }
}
