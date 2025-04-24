using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Tools
{
    internal enum DoodleModifierState
    {
        None,
        Brush,
        Erase,
        BucketFill
    }

    internal class DoodleCursorOverlay : VisualElement
    {
        const float k_LineWidth = 1.0f;
        const float k_SegmentLength = 10.0f;
        readonly Color k_LineColor = Color.white;

        public DoodleCursorOverlay()
        {
#if UNITY_EDITOR
            k_LineColor = UnityEditor.EditorGUIUtility.isProSkin ? Color.white : Color.black;
#endif
            pickingMode = PickingMode.Ignore;
            generateVisualContent += GenerateVisualContent;
        }

        void GenerateVisualContent(MeshGenerationContext context)
        {
            var width = contentRect.width;
            var height = contentRect.height;
            var painter = context.painter2D;
            painter.lineWidth = k_LineWidth;
            painter.lineCap = LineCap.Butt;

            painter.strokeColor = k_LineColor;

            var radius = Mathf.Max(width, height) * 0.5f;
            var circumference = 2 * Mathf.PI * radius;
            var segmentCount = (int)(circumference / k_SegmentLength);
            var segmentAngle = 360f / segmentCount;
            var dashedPercentage = 0.7f;
            var currentAngle = 0f;
            for (var i = 0; i < segmentCount; i++)
            {
                painter.BeginPath();
                painter.Arc(new Vector2(width * 0.5f, height * 0.5f), width * 0.5f, currentAngle, currentAngle + segmentAngle * dashedPercentage);
                painter.Stroke();
                currentAngle += segmentAngle;
            }
        }
    }

    internal class DoodlePad : VisualElement, IValidatableElement<byte[]>, ISizeableElement, IDisposable
    {
        public const string baseStyleName = "doodle-pad";
        public const string doodleCanvasStyleName = baseStyleName + "-canvas";
        public const string cursorStyleName = baseStyleName + "-cursor";

        public Action onDoodleStart;
        public Action onDoodleUpdate;
        public Action onDoodleEnd;

        public Action<DoodleModifierState> onModifierStateChanged;

        Image m_Image;

        DoodleModifierState m_StartingState = DoodleModifierState.None;
        DoodleModifierState m_ModifierState;

        public DoodleModifierState modifierState
        {
            get => m_ModifierState;
            set
            {
                m_ModifierState = value;
                UpdateDoodleCursorStyle();

                onModifierStateChanged?.Invoke(m_ModifierState);
            }
        }

        float m_BrushRadius;
        public float brushRadius => m_BrushRadius;

        int m_DoodleWidth = 512;
        int m_DoodleHeight = 512;

        Vector2 m_CurrentDoodlePosition;
        Vector2 m_LastDoodlePosition;

        DoodleCursorOverlay m_DoodleCursorOverlay;
        bool m_IsPainting;

        Painter m_Painter;

        public bool isClear
        {
            get => m_Painter.isClear;
            set => m_Painter.SetClear(value);
        }

        public DoodlePad()
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(Muse.Common.PackageResources.doodlePadStyleSheet));
            AddToClassList(baseStyleName);

            pickingMode = PickingMode.Ignore;

            m_Painter = new Painter(new Vector2Int(m_DoodleWidth, m_DoodleHeight));

            m_Image = new Image { image = m_Painter.texture };
            m_Image.style.flexGrow = 1;
            m_Image.style.overflow = Overflow.Hidden;
            m_Image.AddToClassList(doodleCanvasStyleName);
            m_Image.RegisterCallback<PointerDownEvent>(OnDoodleStart);
            m_Image.RegisterCallback<PointerUpEvent>(OnDoodleStop);
            m_Image.RegisterCallback<PointerLeaveEvent>(OnMousePointerLeave);
            m_Image.RegisterCallback<PointerCancelEvent>(OnMousePointerCancel);
            m_Image.RegisterCallback<PointerMoveEvent>(OnDoodleMove);
            m_DoodleCursorOverlay = new DoodleCursorOverlay
            {
                style = { display = DisplayStyle.None }
            };
            m_DoodleCursorOverlay.AddToClassList(cursorStyleName);
            m_Image.Add(m_DoodleCursorOverlay);
            Add(m_Image);
        }

        public DoodlePad(float opacity)
            : this()
        {
            m_Image.style.opacity = Mathf.Clamp01(opacity);
        }

        public void SetNone() => modifierState = DoodleModifierState.None;
        public void SetBrush() => modifierState = DoodleModifierState.Brush;
        public void SetEraser() => modifierState = DoodleModifierState.Erase;
        public void SetBucketFill() => modifierState = DoodleModifierState.BucketFill;

        public void SetBrushSize(float newBrushRadius)
        {
            m_BrushRadius = newBrushRadius;
            m_Painter.brushRadius = m_BrushRadius;
            UpdateDoodleCursorStyle();
        }

        public void SetBrushColor(Color color)
        {
            m_Painter.paintColor = color;
        }

        public void SetDoodle(byte[] doodle)
        {
            SetValueWithoutNotify(doodle);

            SendValueChangedEvent();
        }

        void InitializeWithData(byte[] doodle)
        {
            m_Painter.InitializeWithData(doodle);

            m_DoodleWidth = m_Painter.size.x;
            m_DoodleHeight = m_Painter.size.y;

            m_Image.image = m_Painter.texture;
            m_Image.MarkDirtyRepaint();
        }

        void OnMousePointerLeave(PointerLeaveEvent evt)
        {
            m_DoodleCursorOverlay.style.display = DisplayStyle.None;
        }

        void OnMousePointerCancel(PointerCancelEvent evt)
        {
            m_DoodleCursorOverlay.style.display = DisplayStyle.None;
        }

        void OnDoodleStart(PointerDownEvent evt)
        {
            if (m_ModifierState == DoodleModifierState.None)
                return;

            if (evt.altKey)
                return;

            m_StartingState = modifierState;
            m_WasActionKeyPressed = evt.actionKey;
            if (m_WasActionKeyPressed)
            {
                SwitchBrush();
            }

            m_LastDoodlePosition = m_CurrentDoodlePosition = evt.localPosition;

            var currentPosition = GetPosition(m_CurrentDoodlePosition);

            switch (m_ModifierState)
            {
                case DoodleModifierState.Brush:
                case DoodleModifierState.Erase:
                    if (evt.button == 0)
                    {
                        m_IsPainting = true;
                        MouseCaptureController.CaptureMouse(m_Image);
                        if (m_ModifierState == DoodleModifierState.Brush)
                            m_Painter.Paint(currentPosition, currentPosition);
                        else if (m_ModifierState == DoodleModifierState.Erase)
                            m_Painter.Erase(currentPosition, currentPosition);
                        onDoodleStart?.Invoke();
                    }
                    else if (evt.button == 1)
                    {
                        m_Painter.DoodleFill(currentPosition);
                        onDoodleUpdate?.Invoke();
                        SendValueChangedEvent();
                    }

                    break;
                case DoodleModifierState.BucketFill:
                    m_Painter.DoodleFill(currentPosition);
                    onDoodleUpdate?.Invoke();
                    SendValueChangedEvent();
                    break;
            }

            m_Image.MarkDirtyRepaint();

            evt.StopPropagation();
        }

        void OnDoodleStop(PointerUpEvent evt)
        {
            if (m_ModifierState == DoodleModifierState.None || m_StartingState == DoodleModifierState.None)
                return;

            if (evt.button != 0)
                return;

            m_IsPainting = false;
            modifierState = m_StartingState;
            m_StartingState = DoodleModifierState.None;

            m_Painter.UpdateTextureData();
            schedule.Execute(SendValueChangedEvent);

            MouseCaptureController.ReleaseMouse(m_Image);
            onDoodleEnd?.Invoke();

            evt.StopPropagation();
        }

        void SendValueChangedEvent()
        {
            using var evt = ChangeEvent<byte[]>.GetPooled(null, m_Painter.GetTextureData().EncodeToPNG());
            evt.target = this;
            SendEvent(evt);
        }

        bool m_WasActionKeyPressed;

        void OnDoodleMove(PointerMoveEvent evt)
        {
            if (m_ModifierState == DoodleModifierState.None)
                return;

            m_CurrentDoodlePosition = evt.localPosition;
            UpdateDoodleCursorStyle();
            if (!m_IsPainting)
                return;

            // If control key on windows or command key on mac is pressed, switch between brush and erase
            if (m_WasActionKeyPressed != evt.actionKey)
            {
                SwitchBrush();
            }

            m_WasActionKeyPressed = evt.actionKey;

            var currentPosition = (Vector3)GetPosition(m_CurrentDoodlePosition);
            var previousPosition = (Vector3)GetPosition(m_LastDoodlePosition);

            if (m_ModifierState == DoodleModifierState.Brush)
                m_Painter.Paint(previousPosition, currentPosition);
            else if (m_ModifierState == DoodleModifierState.Erase)
                m_Painter.Erase(previousPosition, currentPosition);

            m_LastDoodlePosition = m_CurrentDoodlePosition;

            onDoodleUpdate?.Invoke();

            evt.StopPropagation();
        }

        void SwitchBrush()
        {
            if (modifierState == DoodleModifierState.Brush)
            {
                modifierState = DoodleModifierState.Erase;
            }
            else if (modifierState == DoodleModifierState.Erase)
            {
                modifierState = DoodleModifierState.Brush;
            }
        }

        float GetBrushSize(float brushSize)
        {
            var size = m_Image.contentRect.size;
            var parentAspectRatio = size.x / size.y;
            var imageAspectRatio = (float)m_DoodleWidth / m_DoodleHeight;
            if (imageAspectRatio > parentAspectRatio)
            {
                // width match
                brushSize *= size.x / m_DoodleWidth;
            }
            else
            {
                // height match
                brushSize *= size.y / m_DoodleHeight;
            }

            return brushSize;
        }

        Vector2 GetPosition(Vector3 evtLocalPosition)
        {
            var size = m_Image.contentRect.size;
            var pos = evtLocalPosition;
            pos.y = size.y - pos.y;

            var parentAspectRatio = size.x / size.y;
            var imageAspectRatio = (float)m_DoodleWidth / m_DoodleHeight;
            var realImageWidth = size.x;
            var realImageHeight = size.y;
            if (imageAspectRatio > parentAspectRatio)
            {
                // width match
                realImageHeight = size.x / imageAspectRatio;
                pos.y -= (size.y - realImageHeight) * 0.5f;
            }
            else
            {
                // height match
                realImageWidth = size.y * imageAspectRatio;
                pos.x -= (size.x - realImageWidth) * 0.5f;
            }

            pos.x *= m_DoodleWidth / realImageWidth;
            pos.y *= m_DoodleHeight / realImageHeight;

            return pos;
        }

        void UpdateDoodleCursorStyle()
        {
            var isVisible = m_ModifierState != DoodleModifierState.None;
            var currentPos = (Vector3)GetPosition(m_CurrentDoodlePosition);
            var withinDoodlingArea = currentPos.x >= 0 && currentPos.x < m_DoodleWidth && currentPos.y >= 0 && currentPos.y < m_DoodleHeight;
            if (withinDoodlingArea)
            {
                var doodleImageRadius = GetBrushSize(m_BrushRadius);

                m_DoodleCursorOverlay.style.position = Position.Absolute;
                m_DoodleCursorOverlay.style.top = m_CurrentDoodlePosition.y - doodleImageRadius + m_Image.resolvedStyle.paddingTop;
                m_DoodleCursorOverlay.style.left = m_CurrentDoodlePosition.x - doodleImageRadius;
                m_DoodleCursorOverlay.style.width = m_DoodleCursorOverlay.style.height = doodleImageRadius * 2;
                m_DoodleCursorOverlay.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else
                m_DoodleCursorOverlay.style.display = DisplayStyle.None;
            m_DoodleCursorOverlay.MarkDirtyRepaint();
        }

        public void SetValueWithoutNotify(byte[] newValue)
        {
            InitializeWithData(newValue);
        }

        public byte[] value
        {
            get => m_Painter.GetTextureData().EncodeToPNG();
            set => SetDoodle(value);
        }

        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }

        public Func<byte[], bool> validateValue { get; set; }
        public Size size { get; set; }

        public void SetDoodleSize(Vector2Int newSize)
        {
            m_DoodleWidth = newSize.x;
            m_DoodleHeight = newSize.y;

            m_Painter.Resize(newSize);

            m_Image.image = m_Painter.texture;
            m_Image.MarkDirtyRepaint();
        }

        public void Dispose()
        {
            m_Painter?.Dispose();
        }
    }
}
