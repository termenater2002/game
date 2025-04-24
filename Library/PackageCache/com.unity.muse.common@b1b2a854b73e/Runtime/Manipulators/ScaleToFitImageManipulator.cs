using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class ScaleToFitImageManipulator : Manipulator
    {
        Image image => target as Image;
        Texture texture => image.image;
        float m_AspectRatio;

        protected override void RegisterCallbacksOnTarget()
        {
            m_AspectRatio = texture.height / (float) texture.width;
            target.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            target.style.height = evt.newRect.width * m_AspectRatio;
        }
    }
}
