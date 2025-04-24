using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Utils;

namespace Unity.Muse.Common
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class LoadableImage : Image
    {
        const string k_ClassStatusElement = "li-element";
        const string k_ResolutionClassName = "li-resolution-chip";

        internal readonly GenericLoader GenericLoader;
        readonly Chip m_ResolutionChip;
        readonly VisualElement m_ResolutionChipContainer;

        internal GenericLoader.State LoadingState => GenericLoader.LoadingState;

        public LoadableImage()
            : this(true) { }

        internal enum ImageDisplay
        {
            Image,
            BackgroundImage
        }

        protected LoadableImage(bool autoLoading = true)
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.loadableImageStyleSheet));
            AddToClassList(k_ClassStatusElement);

            GenericLoader = new GenericLoader(autoLoading ? GenericLoader.State.Loading : GenericLoader.State.None)
            {
                style =
                {
                    position = Position.Absolute,
                    width = Length.Percent(100),
                    height = Length.Percent(100)
                }
            };
            GenericLoader.SetDisplay(this, autoLoading);

            m_ResolutionChip = new Chip
            {
                variant = Chip.Variant.Filled,
                label = "2K"
            };

            m_ResolutionChipContainer = new VisualElement
            {
                style =
                {
                   flexDirection = FlexDirection.ColumnReverse,
                   position = Position.Absolute,
                   width = Length.Percent(100),
                   height = Length.Percent(100)
                },
                pickingMode = PickingMode.Ignore
            };

            m_ResolutionChip.AddToClassList(k_ResolutionClassName);

            m_ResolutionChipContainer.Add(m_ResolutionChip);
        }

        protected void OnLoaded(Texture texture, ImageDisplay imageDisplay = ImageDisplay.Image)
        {
            if (imageDisplay == ImageDisplay.BackgroundImage)
            {
                style.backgroundImage = texture as Texture2D;
            }
            else
            {
                image = texture;
            }

            GenericLoader.SetState(GenericLoader.State.None);
            GenericLoader.SetDisplay(this, false);

            UpdateResolutionChip(texture);
        }

        public void OnError(string error)
        {
            GenericLoader.SetState(GenericLoader.State.Error, error);
            GenericLoader.SetDisplay(this, true);
        }

        protected void OnLoading()
        {
            image = null;
            style.backgroundImage = null;
            GenericLoader.SetState(GenericLoader.State.Loading);
            GenericLoader.SetDisplay(this, true);
        }

        void UpdateResolutionChip(Texture texture)
        {

            if (texture)
            {
                m_ResolutionChip.label = texture.width switch
                {
                    2048 => "2K",
                    4096 => "4K",
                    8192 => "8K",
                    _ => string.Empty
                };
            }
            else
            {
                m_ResolutionChip.label = string.Empty;
            }

            m_ResolutionChipContainer.SetDisplay(this, !string.IsNullOrEmpty(m_ResolutionChip.label));
        }
    }
}
