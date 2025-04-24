using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Utils
{
    internal static class VisualElementExtensions
    {
        public static void ApplyTemplate(this VisualElement element, VisualTreeAsset uxmlTemplate)
        {
            element.ApplyTemplate(uxmlTemplate.Instantiate());
        }

        public static void ApplyTemplate(this VisualElement element, TemplateContainer template)
        {
            const string rootVisualElement = "__root__";

            if (element == null || template == null)
            {
                var nullArgName = element == null ? nameof(element) : nameof(template);
                Debug.LogError($"Argument \"{nullArgName}\" cannot be null.");
                return;
            }

            var root = template.contentContainer.Q<VisualElement>(rootVisualElement);
            if (root == null)
            {
                Debug.LogError($"Element \"{rootVisualElement}\" can't be found in the content container.");
                return;
            }

            // send properties from root of template to the element
            foreach (var style in root.GetClasses())
            {
                element.AddToClassList(style);
            }

            for(var index = 0; index < template.contentContainer.styleSheets.count; index++)
            {
                element.styleSheets.Add(template.contentContainer.styleSheets[index]);
            }

            foreach (var child in Enumerable.Range(0, root.childCount).Select(root.ElementAt).ToList())
            {
                root.Remove(child);
                element.hierarchy.Add(child);
            }
        }

        public static void ApplyTemplate(this VisualElement element, string resourcePath)
        {
            element.ApplyTemplate(ResourceManager.Load<VisualTreeAsset>(resourcePath));
        }

        /// <summary>
        /// Sets the display of an element with better performance than <c>display:none</c>.
        ///
        /// The reason is setting <c>display:none</c> style results in a worse performance than adding/removing an element.
        /// </summary>
        public static void SetDisplay(this VisualElement element, VisualElement parent, bool display)
        {
            if (display)
            {
                if (parent != null && element.parent != parent)
                    parent.Add(element);
            }
            else
                element.RemoveFromHierarchy();
        }

        public static void SetDisplay(this VisualElement element, bool display)
        {
            element.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static float GetPixelWidth(this VisualElement element)
        {
            return element.resolvedStyle.width * Unity.AppUI.Core.Platform.scaleFactor;
        }

        public static float GetPixelHeight(this VisualElement element)
        {
            return element.resolvedStyle.height * Unity.AppUI.Core.Platform.scaleFactor;
        }
    }
}
