using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// This represents a user-authored key
    /// </summary>
    [Serializable]
    struct KeyData
    {
        /// <summary>
        /// There are multiple type of keys
        /// </summary>
        public enum KeyType
        {
            /// <summary>
            /// A key that contains no specific information, just a point in time
            /// </summary>
            Empty = 0,
            /// <summary>
            /// A key that contains a fully specified pose
            /// </summary>
            FullPose = 1,
            /// <summary>
            /// A key that loops a previous key, with a translation and rotation offsets
            /// </summary>
            Loop = 2
        }

        public static KeyType[] AllKeyTypes = (KeyType[])Enum.GetValues(typeof(KeyType));
        
        [SerializeField]
        public KeyType Type;
        [SerializeField]
        public List<KeyEntity> Entities;
        [SerializeField]
        public LoopKeyModel LoopKey;
        [SerializeField]
        public ThumbnailModel Thumbnail;
    }

    [Serializable]
    struct KeyEntity
    {
        [SerializeField]
        public EntityID ID;

        [SerializeField]
        public EntityKeyModel Model;

        public KeyEntity(EntityID id, EntityKeyModel model)
        {
            ID = id;
            Model = model;
        }
    }
}
