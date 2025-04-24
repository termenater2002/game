using System;
using System.Collections.Generic;
using System.IO;
using Unity.Muse.Chat.UI.Components.ServerCompatibility;
using Unity.Muse.Common.Account;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Utils
{
    static class VisualElementExtensions
    {
        /// <summary>
        /// Helper method to retrieve and setup a button with a callback.
        /// </summary>
        /// <param name="root">The root element to search on</param>
        /// <param name="id">The name of the button element</param>
        /// <param name="callback">The callback to register when clicking the button</param>
        /// <returns>The button element</returns>
        /// <exception cref="InvalidDataException">if no element could be found</exception>
        public static Button SetupButton(this VisualElement root, string id, EventCallback<PointerUpEvent> callback)
        {
            var element = root.Q<Button>(id);
            if (element == null)
            {
                throw new InvalidDataException("No such Button: " + id);
            }

            element.RegisterCallback(callback);
            return element;
        }

        /// <summary>
        /// Helper method to retrieve and setup a button with a callback.
        /// </summary>
        /// <param name="root">The root element to search on</param>
        /// <param name="id">The name of the button element</param>
        /// <param name="callback">The callback to register when clicking the button</param>
        /// <returns>The button element</returns>
        /// <exception cref="InvalidDataException">if no element could be found</exception>
        public static Toggle SetupToggle(this VisualElement root, string id, EventCallback<PointerUpEvent> callback)
        {
            var element = root.Q<Toggle>(id);
            if (element == null)
            {
                throw new InvalidDataException("No such Toggle: " + id);
            }

            element.RegisterCallback(callback);
            return element;
        }

        /// <summary>
        /// Helper method to setup a dropdown field based on an enum
        /// </summary>
        /// <param name="root">the root that the element lives under</param>
        /// <param name="id">name of the element</param>
        /// <param name="enumDisplayResolver">resolve callback to get the enum names, if null it will resolve to the enum string</param>
        /// <param name="selectionType">selection type of the dropdown</param>
        /// <param name="defaultSelection">The default selected value</param>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <returns>The dropdown element</returns>
        /// <exception cref="InvalidDataException">if the element does not exist as a member of the root</exception>
        public static DropdownField SetupEnumDropdown<T>(this VisualElement root, string id, Func<T, string> enumDisplayResolver = null, T defaultSelection = default)
            where T: Enum
        {
            var element = root.Q<DropdownField>(id);
            if (element == null)
            {
                throw new InvalidDataException("No such Dropdown: " + id);
            }

            string defaultSelectionValue = defaultSelection.ToString();
            if (enumDisplayResolver != null)
            {
                defaultSelectionValue = enumDisplayResolver.Invoke(defaultSelection);
            }

            var choices = new List<string>();
            foreach (T value in EnumDef<T>.Values)
            {
                string displayValue = value.ToString();
                if (enumDisplayResolver != null)
                {
                    displayValue = enumDisplayResolver.Invoke(value);
                }

                choices.Add(displayValue);
            }

            element.choices = choices;
            element.value = defaultSelectionValue;

            return element;
        }

        /// <summary>
        /// Helper method to setup a dropdown field based on a list of strings
        /// </summary>
        /// <param name="root">the root that the element lives under</param>
        /// <param name="id">name of the element</param>
        /// <param name="values">The strings to choose from</param>
        /// <param name="defaultSelection">The index of the default value</param>
        /// <returns>The dropdown element</returns>
        /// <exception cref="InvalidDataException">if the element does not exist as a member of the root</exception>
        public static DropdownField SetupStringsDropdown(this VisualElement root, string id, List<string> values, int defaultSelection = 0)
        {
            var element = root.Q<DropdownField>(id);
            if (element == null)
            {
                throw new InvalidDataException("No such Dropdown: " + id);
            }

            var defaultSelectionValue = (defaultSelection >= 0 && defaultSelection < values.Count) ? values[defaultSelection]  : "";

            element.choices = values;
            element.value = defaultSelectionValue;

            return element;
        }

        /// <summary>
        /// Helper method to register an element as session tracked, which means it's disable state is based on the muse session status
        /// </summary>
        /// <param name="element">The element to track</param>
        public static void AddSessionAndCompatibilityStatusManipulators(this VisualElement element)
        {
            if (!Assistant.instance.SessionStatusTrackingEnabled)
            {
                return;
            }

            element.AddManipulator(new SessionStatusTracker());
            element.AddManipulator(new ServerCompatibilityTracker());
        }

        public static MuseChatImage SetupImage(this VisualElement root, string id, string defaultImageClass = null)
        {
            var image = root.Q<Image>(id);
            var result = new MuseChatImage(image);
            if (!string.IsNullOrEmpty(defaultImageClass))
            {
                result.SetIconClassName(defaultImageClass);
            }

            return result;
        }
    }
}
