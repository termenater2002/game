using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    internal class PaintingManipulator : Manipulator
    {
        public PaintingElement paintingElement { get; private set; }
        Model m_Model;
        public bool Seamless { get; private set; }
        public bool WrapAround { get; private set; }

        public PaintingManipulator(bool seamless, bool wrapAround = false)
        {
            SetMaskSeamless(seamless);
            WrapAround = wrapAround;
        }

        public void SetRadius(float radius)
        {
            if(paintingElement != null)
                paintingElement.PaintRadius = radius;
        }

        public float GetRadius()
        {
            return paintingElement?.PaintRadius ?? 5.0f;
        }

        public void SetEraserMode(bool erase)
        {
            if(paintingElement != null)
                paintingElement.EraseMode = erase;
        }

        public void ClearPainting()
        {
            if(paintingElement != null)
                paintingElement.ClearPainting();
        }
        public void SetMaskSeamless(bool value)
        {
            Seamless = value;
            paintingElement?.SetMaskSeamless(Seamless);
        }
        protected override void RegisterCallbacksOnTarget()
        {
            Texture baseTexture = null;
            if (target is Image imageTarget)
            {
                baseTexture = imageTarget.image;
            }
            else
            {
                baseTexture = target.style.backgroundImage.value.texture;
            }

            paintingElement = new PaintingElement() { WrapAround = WrapAround };
            paintingElement.SetMaskSeamless(Seamless);

            paintingElement.SetModel(m_Model);
            target.Add(paintingElement);
            paintingElement.InitializeImage(baseTexture, target);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            if (paintingElement == null)
                return;

            paintingElement.Dispose();
            target.Remove(paintingElement);
            paintingElement = null;
        }

        public RenderTexture GetTexture()
        {
            return paintingElement?.Export();
        }
        public void SetModel(Model model)
        {
            m_Model = model;
        }
    }
}
