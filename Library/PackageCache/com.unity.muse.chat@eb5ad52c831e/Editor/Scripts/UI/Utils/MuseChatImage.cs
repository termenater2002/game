using System;
using System.Collections.Generic;
using Unity.Muse.Common.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Utils
{
    class MuseChatImage
    {
        static readonly IDictionary<string, Type> k_TypeLookupCache = new Dictionary<string, Type>();

        string m_CurrentIconClass;
        Image m_Image;

        public MuseChatImage(Image image)
        {
            Debug.Assert(image != null, "Image cannot be null");

            m_Image = image;
        }

        public void SetTooltip(string tooltip)
        {
            m_Image.tooltip = tooltip;
        }

        public void SetPickingMode(PickingMode mode)
        {
            m_Image.pickingMode = mode;
        }

        public void SetTexture(Texture2D texture)
        {
            if (texture != null)
            {
                m_Image.image = texture;

                if (!string.IsNullOrEmpty(m_CurrentIconClass))
                {
                    // Unset the class icon, texture overrides
                    m_Image.RemoveFromClassList(m_CurrentIconClass);
                }

                return;
            }

            // Remove the texture and restore the class icon if set
            m_Image.image = null;
            SetIconClassName(m_CurrentIconClass, true);
        }

        public bool IsIconClassSet(string className)
        {
            return m_CurrentIconClass == className;
        }

        public void SetIconClassName(string className, bool force = false)
        {
            string fullClassName = MuseChatConstants.IconStylePrefix + className;
            if (m_CurrentIconClass == fullClassName && !force)
            {
                return;
            }

            if (!string.IsNullOrEmpty(m_CurrentIconClass))
            {
                m_Image.RemoveFromClassList(m_CurrentIconClass);
            }

            m_CurrentIconClass = fullClassName;
            if (string.IsNullOrEmpty(fullClassName))
            {
                return;
            }

            m_Image.AddToClassList(fullClassName);
        }

        public void SetIconByTypeString(string typeString)
        {
            if (string.IsNullOrEmpty(typeString))
            {
                m_Image.image = null;
                return;
            }

            var type = FindIconTargetType(typeString);
            if (type == null)
            {
                m_Image.image = null;
                return;
            }

            m_Image.image = EditorGUIUtility.ObjectContent(null, type).image as Texture2D;
        }

        public void SetDisplay(bool isVisible)
        {
            m_Image.SetDisplay(isVisible);
        }

        Type FindIconTargetType(string typeString)
        {
            if (k_TypeLookupCache.TryGetValue(typeString, out Type result))
            {
                return result;
            }

            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                foreach (Type type in assemblies[i].GetTypes())
                {
                    if (type.FullName == typeString)
                    {
                        k_TypeLookupCache.Add(typeString, type);
                        return type;
                    }
                }
            }

            return null;
        }
    }
}
