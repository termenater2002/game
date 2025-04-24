using System;
using System.Collections.Generic;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Common.Tools
{
    /// <summary>
    /// Painter allows to paint textures with brush strokes and span-fill.
    /// </summary>
    internal class Painter : IDisposable
    {
        /// <summary>
        /// Brush radius in texture coordinates.
        /// </summary>
        public float brushRadius { get; set; } = 1;

        /// <summary>
        /// Returns true if the painter texture is clear.
        /// </summary>
        public bool isClear => m_Clear;

        /// <summary>
        /// The current size of the Painter.
        /// </summary>
        public Vector2Int size => m_Size;

        /// <summary>
        /// Sets the clear state.
        /// </summary>
        /// <param name="clear">Is clear.</param>
        /// <remarks>To clear the content of the texture, use InitializeWithData method.</remarks>
        public void SetClear(bool clear) => m_Clear = clear;

        /// <summary>
        /// Default paint color used for painting.
        /// </summary>
        public Color paintColor { get; set; } = Color.white;

        /// <summary>
        /// Default clear color used for erasing and clearing.
        /// </summary>
        public Color clearColor { get; set; } = Color.clear;

        /// <summary>
        /// Returns true if painter is correctly initialized;
        /// </summary>
        public bool isInitialized => m_Initialized;

        readonly PaintMaterial m_PaintMaterial;
        readonly TransformMaterial m_TransformMaterial;

        Texture2D m_Texture;
        RenderTexture m_RenderTexture;

        Vector2Int m_Size;

        bool m_Initialized;
        bool m_Clear;

        class PaintMaterial : IDisposable
        {
            static readonly int k_MainTexture = Shader.PropertyToID("_MainTex");
            static readonly int k_Color = Shader.PropertyToID("_Color");
            static readonly int k_Pos = Shader.PropertyToID("_Pos");
            static readonly int k_Radius = Shader.PropertyToID("_Radius");
            static readonly int k_AspectRatio = Shader.PropertyToID("_AspectRatio");

            public readonly Material material;

            public PaintMaterial()
            {
                material = new Material(ResourceManager.Load<Shader>(PackageResources.doodlePaintShader)) { hideFlags = HideFlags.HideAndDontSave };
            }

            public void SetMainTexture(UnityEngine.Texture texture) => material.SetTexture(k_MainTexture, texture);
            public void SetRadius(float radius) => material.SetFloat(k_Radius, radius);
            public void SetColor(Color color) => material.SetColor(k_Color, color);

            public void SetPositions(Vector2 startPosition, Vector2 endPosition) =>
                material.SetVector(k_Pos, new Vector4(startPosition.x, startPosition.y, endPosition.x, endPosition.y));

            public void SetAspectRatio(float aspectRatio) => material.SetFloat(k_AspectRatio, aspectRatio);

            public void Dispose() => material.SafeDestroy();
        }

        class TransformMaterial : IDisposable
        {
            static readonly int k_MainTexture = Shader.PropertyToID("_MainTex");
            static readonly int k_Offset = Shader.PropertyToID("_Offset");
            static readonly int k_Rotation = Shader.PropertyToID("_Rotation");
            static readonly int k_Scale = Shader.PropertyToID("_Scale");

            public readonly Material material;

            public TransformMaterial()
            {
                material = new Material(ResourceManager.Load<Shader>(PackageResources.doodleTransformShader)) { hideFlags = HideFlags.HideAndDontSave };
            }

            public void SetMainTexture(UnityEngine.Texture texture) => material.SetTexture(k_MainTexture, texture);
            public void SetOffset(Vector2 offset) => material.SetVector(k_Offset, offset);
            public void SetRotation(float rotation) => material.SetFloat(k_Rotation, rotation * Mathf.Deg2Rad);
            public void SetScale(Vector2 scale) => material.SetVector(k_Scale, scale);
            public void Dispose() => material.SafeDestroy();
        }

        /// <summary>
        /// Current render texture.
        /// </summary>
        public UnityEngine.Texture texture => m_RenderTexture;

        /// <summary>
        /// Creates a new instance of a Painter.
        /// </summary>
        /// <param name="size">Texture size.</param>
        /// <param name="imageData">Optional texture data.</param>
        /// <exception cref="Exception">Thrown if the size is smaller than 2x2.</exception>
        public Painter(Vector2Int size, byte[] imageData = null)
        {
            if (size.x < 2 || size.y < 2)
                throw new Exception($"Incorrect size {size}. Dimensions must be greater than 2x2.");

            m_Size = size;
            m_PaintMaterial = new PaintMaterial();
            m_TransformMaterial = new TransformMaterial();

            InitializeWithData(imageData);
        }

        /// <summary>
        /// Texture2D with paint data.
        /// </summary>
        /// <param name="updateData">Updates the current state of the texture before returning.</param>
        /// <returns>Instance of Texture2D with the paint data.</returns>
        public Texture2D GetTextureData(bool updateData = false)
        {
            if (updateData)
                UpdateTextureData();
            return m_Texture;
        }

        /// <summary>
        /// Initialize with texture data.
        /// </summary>
        /// <param name="imageData">Image data. If null, the painter will be cleared.</param>
        public void InitializeWithData(byte[] imageData)
        {
            if (m_Texture == null)
                m_Texture = new Texture2D(m_Size.x, m_Size.y) { hideFlags = HideFlags.HideAndDontSave };
            if (imageData != null && imageData.Length > 0)
            {
                m_Texture.LoadImage(imageData);
                m_Texture.Apply();
                m_Size.x = m_Texture.width;
                m_Size.y = m_Texture.height;

                m_Clear = false;
            }
            else
            {
                m_Texture.Reinitialize(m_Size.x, m_Size.y);
                var pixels = new Color32[m_Size.x * m_Size.y];
                for (var i = 0; i < pixels.Length; i++)
                    pixels[i] = clearColor;
                m_Texture.SetPixels32(pixels);
                m_Texture.Apply();

                m_Clear = true;
            }

            if (m_RenderTexture != null)
                m_RenderTexture.Release();

            m_RenderTexture = GetNewRenderTexture(m_Size.x, m_Size.y, clearColor);
            UpdateRenderTexture();

            m_Initialized = true;
        }

        /// <summary>
        /// Resize the painter.
        /// </summary>
        /// <param name="newSize">New size. Must be larger than 2x2.</param>
        public void Resize(Vector2Int newSize)
        {
            if (!isInitialized)
                return;

            if (m_Size == newSize)
                return;

            if (newSize.x < 2 || newSize.y < 2)
            {
                Debug.Log($"Incorrect size {newSize}");
                return;
            }

            m_Size = newSize;

            if (m_RenderTexture != null)
                m_RenderTexture.Release();

            m_RenderTexture = GetNewRenderTexture(m_Size.x, m_Size.y, clearColor);

            var activeRT = RenderTexture.active;
            RenderTexture.active = m_RenderTexture;
            Graphics.Blit(m_Texture, m_RenderTexture);
            if (m_Texture == null)
                m_Texture = new Texture2D(m_Size.x, m_Size.y) { hideFlags = HideFlags.HideAndDontSave };
            else
                m_Texture.Reinitialize(m_Size.x, m_Size.y);
            m_Texture.ReadPixels(new Rect(Vector2.zero, new Vector2(m_Size.x, m_Size.y)), 0, 0);
            m_Texture.Apply();

            RenderTexture.active = activeRT;

            InitializeWithData(m_Texture.EncodeToPNG());
        }

        /// <summary>
        /// Transforms the painted image by specified parameters.
        /// </summary>
        /// <param name="offset">Translation offset in pixels.</param>
        /// <param name="rotation">Rotation in angles.</param>
        /// <param name="scale">Scale vector.</param>
        public void Transform(Vector2 offset, float rotation, Vector2 scale)
        {
            if (!isInitialized)
                return;

            m_TransformMaterial.SetMainTexture(m_RenderTexture);
            m_TransformMaterial.SetOffset(offset);
            m_TransformMaterial.SetRotation(rotation);
            m_TransformMaterial.SetScale(scale);

            var activeRT = RenderTexture.active;

            RenderTexture.active = m_RenderTexture;
            var temporary = RenderTexture.GetTemporary(m_RenderTexture.descriptor);
            Graphics.Blit(m_RenderTexture, temporary, m_TransformMaterial.material);
            Graphics.Blit(temporary, m_RenderTexture);
            m_Texture.ReadPixels(new Rect(Vector2.zero, new Vector2(m_Size.x, m_Size.y)), 0, 0);
            m_Texture.Apply();
            RenderTexture.ReleaseTemporary(temporary);

            RenderTexture.active = activeRT;
        }

        /// <summary>
        /// Translates the painted image by offset.
        /// </summary>
        /// <param name="offset">Translation offset in pixels.</param>
        public void Translate(Vector2 offset) => Transform(offset, 0, Vector2.one);

        /// <summary>
        /// Rotates the painted image by an angle.
        /// </summary>
        /// <param name="rotation">Rotation in angles</param>
        public void Rotate(float rotation) => Transform(Vector2.zero, rotation, Vector2.one);

        /// <summary>
        /// Scales the painted image by a vector.
        /// </summary>
        /// <param name="scale">Scale vector.</param>
        public void Scale(Vector2 scale) => Transform(Vector2.zero, 0, scale);

        /// <summary>
        /// Paint stroke with a paintColor.
        /// </summary>
        /// <param name="startPosition">Stroke start position in texture coordinates.</param>
        /// <param name="endPosition">Stroke end position in texture coordinates.</param>
        public void Paint(Vector2 startPosition, Vector2 endPosition)
        {
            Paint(startPosition, endPosition, paintColor);
        }

        /// <summary>
        /// Paint stroke with a clearColor.
        /// </summary>
        /// <param name="startPosition">Stroke start position in texture coordinates.</param>
        /// <param name="endPosition">Stroke end position in texture coordinates.</param>
        public void Erase(Vector2 startPosition, Vector2 endPosition)
        {
            Paint(startPosition, endPosition, clearColor);
        }

        /// <summary>
        /// Paint stroke.
        /// </summary>
        /// <param name="startPosition">Stroke start position in texture coordinates.</param>
        /// <param name="endPosition">Stroke end position in texture coordinates.</param>
        /// <param name="color">Paint color.</param>
        public void Paint(Vector2 startPosition, Vector2 endPosition, Color color)
        {
            if (!isInitialized)
                return;

            m_Clear = false;

            var aspectRatio = m_Size.x / (float)m_Size.y;
            startPosition /= m_Size;
            startPosition.x *= aspectRatio;

            endPosition /= m_Size;
            endPosition.x *= aspectRatio;

            m_PaintMaterial.SetMainTexture(m_RenderTexture);
            m_PaintMaterial.SetRadius(brushRadius / Mathf.Min(m_Size.x, m_Size.y));
            m_PaintMaterial.SetColor(color);
            m_PaintMaterial.SetPositions(startPosition, endPosition);
            m_PaintMaterial.SetAspectRatio(aspectRatio);

            var activeRT = RenderTexture.active;

            RenderTexture.active = m_RenderTexture;
            var temporary = RenderTexture.GetTemporary(m_RenderTexture.descriptor);
            Graphics.Blit(m_RenderTexture, temporary);
            Graphics.Blit(temporary, m_RenderTexture, m_PaintMaterial.material);
            RenderTexture.ReleaseTemporary(temporary);

            RenderTexture.active = activeRT;
        }

        /// <summary>
        /// Perform a span-fill.
        /// </summary>
        /// <param name="seedPosition">Seed position in texture coordinates.</param>
        public void DoodleFill(Vector2 seedPosition)
        {
            if (!isInitialized)
                return;

            m_Clear = false;

            UpdateTextureData();

            var doodleCanvas = m_Texture.GetPixels();

            // Fetch the seed position.
            var seedX = (int)seedPosition.x;
            var seedY = (int)seedPosition.y;

            // Finish if seed point out-of-bounds or already filled.
            if (!(seedX >= 0 && seedX < m_Size.x && seedY >= 0 && seedY < m_Size.y) &&
                (doodleCanvas[seedY * m_Size.x + seedX] != paintColor))
                return;

            // Set-up the initial seed points.
            var fillSpans = new Stack<PaintFillSpan>(128);
            fillSpans.Push(new PaintFillSpan(x1: seedX, x2: seedX, y: seedY, dy: 1));
            fillSpans.Push(new PaintFillSpan(x1: seedX, x2: seedX, y: seedY - 1, dy: -1));

            // Iterate the spans.
            while (fillSpans.Count > 0)
            {
                // Fetch the span.
                var span = fillSpans.Pop();
                var x1 = span.x1;
                var x2 = span.x2;
                var y = span.y;
                var dy = span.dy;

                var x = x1;

                var canvasSpanOffset = y * m_Size.x;

                if ((x >= 0 && x < m_Size.x && y >= 0 && y < m_Size.y) &&
                    doodleCanvas[canvasSpanOffset + x] != paintColor)
                {
                    while (x > 0 && doodleCanvas[canvasSpanOffset + x - 1] != paintColor)
                    {
                        doodleCanvas[canvasSpanOffset + x - 1] = paintColor;
                        --x;
                    }
                }

                if (x < x1)
                {
                    fillSpans.Push(new PaintFillSpan(x1: x, x2: x1 - 1, y: y - dy, dy: -dy));
                }

                while (x1 <= x2)
                {
                    if (y >= 0 && y < m_Size.x)
                    {
                        while (x1 >= 0 && x1 < m_Size.x && doodleCanvas[canvasSpanOffset + x1] != paintColor)
                        {
                            doodleCanvas[canvasSpanOffset + x1] = paintColor;

                            ++x1;
                            fillSpans.Push(new PaintFillSpan(x1: x, x2: x1 - 1, y: y + dy, dy: dy));

                            if ((x1 - 1) > x2)
                            {
                                fillSpans.Push(new PaintFillSpan(x1: x2 + 1, x2: x1 - 1, y: y - dy, dy: -dy));
                            }
                        }
                    }

                    ++x1;
                    if (y >= 0 && y < m_Size.y)
                    {
                        while (x1 < x2 && !(x1 < m_Size.x && doodleCanvas[canvasSpanOffset + x1] != paintColor))
                        {
                            ++x1;
                        }
                    }

                    x = x1;
                }
            }

            // Update the textures.
            m_Texture.SetPixels(doodleCanvas);
            m_Texture.Apply();

            UpdateRenderTexture();
        }

        /// <summary>
        /// Updates the content of the Texture2D instance with the latest painter data.
        /// </summary>
        public void UpdateTextureData()
        {
            if (!isInitialized)
                return;

            if (m_Texture.width != m_Size.x || m_Texture.height != m_Size.y)
                m_Texture.Reinitialize(m_Size.x, m_Size.y);

            var activeRt = RenderTexture.active;
            RenderTexture.active = m_RenderTexture;
            m_Texture.ReadPixels(new Rect(0, 0, m_Size.x, m_Size.y), 0, 0);
            m_Texture.Apply();
            RenderTexture.active = activeRt;
        }

        void UpdateRenderTexture()
        {
            var restoreRT = RenderTexture.active;
            RenderTexture.active = m_RenderTexture;
            Graphics.Blit(m_Texture, m_RenderTexture);
            RenderTexture.active = restoreRT;
        }

        public void Dispose()
        {
            if (!isInitialized)
                return;

            m_PaintMaterial.Dispose();
            m_TransformMaterial.Dispose();
            m_Texture.SafeDestroy();
            m_RenderTexture.SafeDestroy();

            m_Initialized = false;
        }

        static RenderTexture GetNewRenderTexture(int width, int height, Color clearColor)
        {
            var newRenderTexture = new RenderTexture(width, height, 0)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };

            var active = RenderTexture.active;

            RenderTexture.active = newRenderTexture;
            GL.Clear(true, true, clearColor);

            RenderTexture.active = active;
            return newRenderTexture;
        }

        struct PaintFillSpan
        {
            public int x1, x2, y, dy;

            public PaintFillSpan(int x1, int x2, int y, int dy)
            {
                this.x1 = x1;
                this.x2 = x2;
                this.y = y;
                this.dy = dy;
            }
        }
    }
}
