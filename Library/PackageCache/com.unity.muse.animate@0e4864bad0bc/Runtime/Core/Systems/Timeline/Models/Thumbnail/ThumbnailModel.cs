using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    [Serializable]
    class ThumbnailModel : ICopyable<ThumbnailModel>, ISerializationCallbackReceiver
    {
        public static Texture2D BlendingTexture { get; set; }
        public int[] Shape = { 256, 256 };

        public Texture2D Texture
        {
            get => m_Texture;
        }

        public Vector3 Position
        {
            get => m_Position;
            set
            {
                m_Position = value;
                OnChanged?.Invoke();
            }
        }

        public Quaternion Rotation
        {
            get => m_Rotation;
            set
            {
                m_Rotation = value;
                OnChanged?.Invoke();
            }
        }

        public float Time
        {
            get => m_Time;
            set
            {
                m_Time = value;
                OnChanged?.Invoke();
            }
        }

        [SerializeField]
        Texture2D m_Texture;

        [SerializeField]
        Vector3 m_Position;

        [SerializeField]
        Quaternion m_Rotation;

        [SerializeField]
        float m_Time;

        public event Action OnChanged;

        public ThumbnailModel(Texture2D texture)
        {
            Shape = new[] { 256, 256 };
            ValidateTexture(Shape[0], Shape[1]);
            SetTexture(texture, true);
        }

        public ThumbnailModel()
        {
            Shape = new[] { 256, 256 };
        }

        public ThumbnailModel(ThumbnailModel source)
        {
            source.CopyTo(this);
        }

        void OnEnable()
        {
            ValidateTexture();
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {

        }

        public void CopyTo(ThumbnailModel other)
        {
            // Make sure both textures have the same amount of dimensions (2D,3D,etc)
            Assert.AreEqual(Shape.Length, other.Shape.Length);
            Shape.CopyTo(other.Shape, 0);
            other.Position = Position;
            other.Rotation = Rotation;
            other.SetTexture(Texture, true);
            other.OnChanged?.Invoke();
        }

        public ThumbnailModel Clone()
        {
            return new ThumbnailModel(this);
        }

        public void Read(RenderTexture renderTexture)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = renderTexture;

            ValidateTexture(renderTexture.width, renderTexture.height);

            Texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, true);
            Texture.Apply();

            Shape[0] = renderTexture.width;
            Shape[1] = renderTexture.height;

            RenderTexture.active = previous;

            OnChanged?.Invoke();
        }

        public void Read(Texture2D texture)
        {
            ValidateTexture(texture.width, texture.height);
            PlatformUtils.CopyTexture(texture, Texture);

            Shape[0] = texture.width;
            Shape[1] = texture.height;

            OnChanged?.Invoke();
        }

        public void Blend(RenderTexture blend, float opacity = 0.05f)
        {
            var original = Texture;
            ValidateBlendingTexture(blend.width, blend.height);

            var previous = RenderTexture.active;

            RenderTexture.active = blend;

            BlendingTexture.ReadPixels(new Rect(0, 0, blend.width, blend.height), 0, 0);
            BlendingTexture.Apply(true);

            for (var y = 0; y < original.height; y++)
            {
                for (var x = 0; x < original.width; x++)
                {
                    original.SetPixel(
                        x, y,
                        ColorUtils.MergeBlend(
                            original.GetPixel(x, y),
                            BlendingTexture.GetPixel(x, y),
                            opacity)
                    );
                }
            }

            original.Apply();
            RenderTexture.active = previous;
        }

        /// <summary>
        /// Compares the Shape width and height with the current Texture's dimensions.
        /// If the dimensions don't match, the Texture is resized to match the specified width and height.
        /// </summary>
        void ValidateTexture()
        {
            ValidateTexture(Shape[0], Shape[1]);
        }

        /// <summary>
        /// Compares the specified width and height with the current Texture's dimensions.
        /// If the dimensions don't match, the Texture is resized to match the specified width and height.
        /// </summary>
        /// <param name="width">The desired width, in pixels.</param>
        /// <param name="height">The desired height, in pixels.</param>
        void ValidateTexture(int width, int height)
        {
            ValidateTexture(ref m_Texture, width, height);
        }

        /// <summary>
        /// Compares the specified width and height with the current Texture's dimensions.
        /// If the dimensions don't match, the Texture is resized to match the specified width and height.
        /// </summary>
        /// <param name="texture">A reference to the texture to validate</param>
        /// <param name="width">The desired width, in pixels.</param>
        /// <param name="height">The desired height, in pixels.</param>
        public static void ValidateTexture(ref Texture2D texture, int width, int height)
        {
            if (texture == null || texture.width != width || texture.height != height)
            {
                if (texture != null)
                {
                    GameObjectUtils.Destroy(texture);
                }

                texture = new Texture2D(width, height);

                // Setting this flags means that Unity will not clean this up when the scene changes, which
                // is useful when we're in the editor

                if (!UnityEngine.Application.isPlaying)
                {
                    texture.hideFlags = HideFlags.DontSaveInEditor;
                }
            }
        }

        public static void ValidateBlendingTexture(int width, int height)
        {
            if (BlendingTexture == null || BlendingTexture.width != width || BlendingTexture.height != height)
            {
                GameObjectUtils.Destroy(BlendingTexture);
                BlendingTexture = new Texture2D(width, height);

                if (!UnityEngine.Application.isPlaying)
                {
                    BlendingTexture.hideFlags = HideFlags.DontSaveInEditor;
                }
            }
        }

        public void SetTexture(Texture2D value, bool silent = false)
        {
            // Get rid of the current texture only if the next value is null
            if (value == null)
            {
                if (m_Texture != null)
                {
                    GameObjectUtils.Destroy(m_Texture);
                    m_Texture = null;
                }
            }
            else
            {
                // This will set m_Texture again if it was removed earlier
                ValidateTexture(Shape[0], Shape[1]);
#if UNITY_EDITOR

                //UnityEditor.EditorUtility.CopySerialized(value, m_Texture);
                PlatformUtils.CopyTexture(value, Texture);
#endif
            }

            if(!silent)
                OnChanged?.Invoke();
        }

        public void SetPosition(Vector3 value, bool silent = false)
        {
            if (m_Position.Equals(value))
                return;

            m_Position = value;

            if(!silent)
                OnChanged?.Invoke();
        }

        public void SetRotation(Quaternion value, bool silent = false)
        {
            if (m_Rotation.Equals(value))
                return;

            m_Rotation = value;

            if(!silent)
                OnChanged?.Invoke();
        }
    }
}
