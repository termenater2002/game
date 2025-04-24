using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    internal class NullableColorField: VisualElement 
    {
        private readonly ColorField m_ColorField;
        private readonly IconButton m_ClearButton;
        
        public event Action OnClear;

        public Color? value
        {
            set
            {
                m_ColorField.value = value ?? new Color(0f,0f,0f,0f); 
                m_ClearButton.SetEnabled(value != null);
            }
        }

        internal NullableColorField() 
        {
            AddToClassList("appui-row");

            m_ColorField = new ColorField();

            var colorFieldContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1
                }
            };
            
            colorFieldContainer.AddToClassList("right-gap");

            colorFieldContainer.Add(m_ColorField);
            hierarchy.Add(colorFieldContainer);

            m_ClearButton = new IconButton("x");
            m_ClearButton.clicked += () => Clear();
            m_ClearButton.SetEnabled(false);
            hierarchy.Add(m_ClearButton);
            
            Add(new VisualElement());
        }

        public void Clear(bool notify = true)
        {
            value = null; 
            if(notify)
                OnClear?.Invoke();
        }
        
        public Pressable clickable
        {
            get => m_ColorField.clickable;
            set => m_ColorField.clickable = value;
        }
    }
}