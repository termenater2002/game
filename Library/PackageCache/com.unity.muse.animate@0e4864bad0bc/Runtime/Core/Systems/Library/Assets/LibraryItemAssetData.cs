using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds the data of a LibraryItemAsset.
    /// TODO: It is not the case now, but LibraryItemAssetData is later meant to be loaded only when needed, separately from LibraryItemPreview.
    /// TODO: It could also be worth splitting the "Input" data and the "Output" data, which could be used later on it's own.
    /// https://jira.unity3d.com/browse/MUSEANIM-280
    /// </summary>
    [Serializable]
    class LibraryItemAssetData : ScriptableObject, ISerializationCallbackReceiver
    {
        public event Action<LibraryItemAssetData, LibraryItemModel, LibraryItemModel.Property> OnModelChanged;
        
        [SerializeReference]
        public LibraryItemModel Model;

        [NonSerialized]
        bool m_IsSubscribedToModel;

        internal void SetupNewAsset<TModel>(TModel item) where TModel : LibraryItemModel
        {
            name = "Data";
            Model = item;
            SubscribeToModel();
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            // When we deserialize, it's possible that the Model serializedRef has changed,
            // so we need to re-subscribe to the new model.
            SubscribeToModel();
        }

        void SubscribeToModel()
        {
            if (Model != null)
            {
                Model.OnChanged -= OnModelChangedInternal;
                Model.OnChanged += OnModelChangedInternal;
                Log("SubscribeToModels() -> Subscribed to Model.");
            }
            else
            {
                Log("SubscribeToModels() -> Failed, Model is null.");
            }
        }

        void OnModelChangedInternal(LibraryItemModel.Property property)
        {
            OnModelChanged?.Invoke(this, Model, property);
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
