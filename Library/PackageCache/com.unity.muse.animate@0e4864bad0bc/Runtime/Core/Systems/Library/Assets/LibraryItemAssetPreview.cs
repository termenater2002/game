using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Asset holding the Preview information of a LibraryItemAsset.
    /// </summary>
    [Serializable]
    class LibraryItemAssetPreview : ScriptableObject, ISerializationCallbackReceiver
    {
        public event Action<ThumbnailModel> OnThumbnailChanged;

        [SerializeField]
        public ThumbnailModel Thumbnail;

        [SerializeReference]
        public Texture2D Texture;

        public void OnEnable()
        {
            Thumbnail?.SetTexture(Texture, true);
        }

        internal void SetupNewAsset<TModel>(TModel item) where TModel : LibraryItemModel
        {
            name = "Preview";
            
            // TODO: Learn about and use Unity.Muse.Common.TextureUtils.Create() instead.
            // https://jira.unity3d.com/browse/MUSEANIM-324
            
            // Init the texture
            Texture = new Texture2D(256, 256)
            {
                name = "Thumbnail"
            };

            // Set all pixels to transparent
            var pixels = new Color[Texture.width * Texture.height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear; 
            }
            
            Texture.SetPixels(pixels);
            Texture.Apply();
            
#if UNITY_EDITOR
            Texture.hideFlags = HideFlags.HideInHierarchy;
#endif
            // Init the ThumbnailModel, binding it to the created Texture
            Thumbnail = new ThumbnailModel(Texture);
            
            SubscribeToModels();
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            SubscribeToModels();
        }

        public void Save()
        {
            Log("Save()");
            SaveThumbnail();
        }

        public void SaveThumbnail()
        {
            Log("SaveThumbnail()");
#if UNITY_EDITOR

            // Make sure the texture is valid before copying it over
            ThumbnailModel.ValidateTexture(ref Texture, Thumbnail.Shape[0], Thumbnail.Shape[1]);
            PlatformUtils.CopyTexture(Thumbnail.Texture, Texture);
            UnityEditor.EditorUtility.SetDirty(Texture);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(Texture);
#endif
        }

        void SubscribeToModels()
        {
            Thumbnail.OnChanged += OnThumbnailChangedInternal;
        }

        void OnThumbnailChangedInternal()
        {
            SaveThumbnail();
            OnThumbnailChanged?.Invoke(Thumbnail);
        }

        // [Section] Debugging

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        void Log(string msg)
        {
            DevLogger.LogInfo($"{GetType().Name} -> {msg}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        void LogError(string msg)
        {
            DevLogger.LogError($"{GetType().Name} -> {msg}");
        }

        
    }
}
