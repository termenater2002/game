using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct LoopKeyData
    {
        [SerializeField]
        public int StartFrame;
        [SerializeField]
        public int NumBakingLoopbacks;
        [SerializeField]
        public List<LoopKeyEntity> Entities;
    }
    
    [Serializable]
    struct LoopKeyEntity
    {
        [SerializeField]
        public EntityID EntityID;
        [SerializeField]
        public RigidTransformModel RigidTransformModel;

        public LoopKeyEntity(EntityID entityID, RigidTransformModel rigidTransformModel)
        {
            EntityID = entityID;
            RigidTransformModel = rigidTransformModel;
        }
    }
}
