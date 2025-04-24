using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// This represents a fully baked frame for multiple entities
    /// </summary>
    [Serializable]
    struct BakedFrameData
    {
        [SerializeField]
        public List<EntityPose> Entities;
    }
    
    /// <summary>
    /// This represents a fully baked frame for a single entity
    /// </summary>
    [Serializable]
    struct EntityPose
    {
        [SerializeField]
        public EntityID ID;
        
        [SerializeField]
        public BakedArmaturePoseModel Model;
        
        public EntityPose(EntityID id, BakedArmaturePoseModel model)
        {
            ID = id;
            Model = model;
        }
    }
}
