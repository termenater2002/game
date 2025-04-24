using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents a scene serialized data
    /// </summary>
    [Serializable]
    struct StageData
    {
        /// <summary>
        /// Increase if data format changed in a way where old data can't be loaded anymore to prevent trying to use
        /// it.
        /// </summary>
        public const int DataVersion = 1;
        public int Version;
        public string Name;
        public int NextEntityId;
        
        [SerializeField]
        public List<ActorModel> Actors;
        
        [SerializeField]
        public List<PropModel> Props;
        
        [SerializeField]
        public List<CameraCoordinatesModel> CameraViewpoints;
    }
}
