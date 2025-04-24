using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Tools
{
    internal class DoodlePadManipulator : Manipulator
    {
        DoodlePad m_DoodlePad;

        const int k_BrushSizeStep = 2;

        Vector2Int m_Size;

        public event Action onDoodleUpdate;
        public event Action<byte[]> onValueChanged;

        public DoodleModifierState currentState => m_DoodlePad?.modifierState ?? DoodleModifierState.None;

        public DoodlePadManipulator(Vector2Int size, float opacity = 1.0f, DoodleModifierState state = DoodleModifierState.Brush)
        {
            m_Size = size;

            m_DoodlePad = new DoodlePad(opacity);
            m_DoodlePad.SetBrushSize(20);
            m_DoodlePad.SetDoodleSize(m_Size);
            switch (state)
            {
                case DoodleModifierState.Brush:
                    m_DoodlePad.SetBrush();
                    break;
                case DoodleModifierState.Erase:
                    m_DoodlePad.SetEraser();
                    break;
                case DoodleModifierState.BucketFill:
                    m_DoodlePad.SetBucketFill();
                    break;
            }
        }

        public bool isClear
        {
            get => m_DoodlePad.isClear;
            set => m_DoodlePad.isClear = value;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.Add(m_DoodlePad);
            m_DoodlePad.StretchToParentSize();
            m_DoodlePad.onModifierStateChanged += state => onModifierStateChanged?.Invoke(state);
            m_DoodlePad.RegisterValueChangedCallback(evt =>  onValueChanged?.Invoke(evt.newValue));
            m_DoodlePad.onDoodleStart += onDoodleUpdate;
            m_DoodlePad.onDoodleUpdate += onDoodleUpdate;
            m_DoodlePad.onDoodleEnd += onDoodleUpdate;
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            if (m_DoodlePad == null)
                return;

            m_DoodlePad.Dispose();
            target.Remove(m_DoodlePad);
            m_DoodlePad = null;
        }

        public void SetBrushSize(float size)
        {
            m_DoodlePad.SetBrushSize(size);
        }

        public float GetBrushSize() => m_DoodlePad.brushRadius;

        public void IncreaseBrushSize()
        {
            if(m_DoodlePad == null)
                return;

            var size = (int)m_DoodlePad.brushRadius + k_BrushSizeStep;
            if (size > 100)
                size = 100;
            m_DoodlePad.SetBrushSize(size);
        }

        public void DecreaseBrushSize()
        {
            if(m_DoodlePad == null)
                return;

            var size = (int)m_DoodlePad.brushRadius - k_BrushSizeStep;
            if (size < 0)
                size = 1;
            m_DoodlePad.SetBrushSize(size);
        }

        public void SetValueWithoutNotify(byte[] newValue)
        {
            m_DoodlePad?.SetValueWithoutNotify(newValue);
        }

        public event Action<DoodleModifierState> onModifierStateChanged;

        public void ToggleBrush()
        {
            if(m_DoodlePad?.modifierState != DoodleModifierState.Brush)
                m_DoodlePad?.SetBrush();
            else
                m_DoodlePad?.SetNone();
        }

        public void SetBrush()
        {
            if(m_DoodlePad?.modifierState != DoodleModifierState.Brush)
                m_DoodlePad?.SetBrush();
        }

        public void ToggleEraser()
        {
            if(m_DoodlePad?.modifierState != DoodleModifierState.Erase)
                m_DoodlePad?.SetEraser();
            else
                m_DoodlePad?.SetNone();
        }

        public void SetEraser()
        {
            if(m_DoodlePad?.modifierState != DoodleModifierState.Erase)
                m_DoodlePad?.SetEraser();
        }

        public void SetNone()
        {
            m_DoodlePad?.SetNone();
        }

        public void ClearDoodle()
        {
            if (!m_DoodlePad.isClear)
                m_DoodlePad?.SetDoodle(null);
        }

        public void Resize(Vector2Int newSize)
        {
            m_DoodlePad?.SetDoodleSize(newSize);
        }
    }
}
