using System;
using System.Collections.Generic;
using Unity.Sentis;
using Unity.DeepPose.ModelBackend;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// Encapsulates models and their specific parameters.
    /// </summary>
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "DeepPoseConfig", menuName = "Muse Animate Dev/Posing/Posing Configuration")]
#endif
    class DeepPoseConfiguration : ScriptableObject
    {
        /// <summary>
        /// The model
        /// </summary>
        public ModelAsset Model;

        /// <summary>
        /// The type of worker used for Barracuda models
        /// </summary>
        public BackendType BarracudaBackend = BackendType.CPU;

        /// <summary>
        /// Avatar used for retargeting
        /// </summary>
        public Avatar Avatar;

        /// <summary>
        /// If true, the model is Additive, otherwise it is not
        /// If the model is Additive, it needs to get the output joints as extra inputs
        /// </summary>
        public bool Additive;

        /// <summary>
        /// If true, look-at can be any local axis, otherwise it is always the forward axis
        /// </summary>
        public bool GeneralizedLookAt = true;

        /// <summary>
        /// If true, ortho6D uses matrix rows instead of columns (should be true when using pytorch3d)
        /// </summary>
        public bool TransposeOrtho6D;

        /// <summary>
        /// Contains the skeleton used by the model
        /// </summary>
        public Skeleton Skeleton;

        /// <summary>
        /// Indexes of joints that can be used as positional effectors
        /// </summary>
        public List<int> JointsWithPosition = new List<int>();

        /// <summary>
        /// Indexes of joints that can be used as rotational effectors
        /// </summary>
        public List<int> JointsWithRotation = new List<int>();

        /// <summary>
        /// Indexes of joints that can be used as look-at effectors
        /// </summary>
        public List<int> JointsWithLookAt = new List<int>();

        public bool UseRaycast = false;
        public float MaxRayDistance = 0f;

        public bool IsValid()
        {
            if (Model == null)
                return false;

            if (Skeleton == null)
                return false;

            if (Skeleton.Count == 0)
                return false;

            return true;
        }

        public void ReadMetaData()
        {
            if (Model == null)
                return;

            var modelDefinition = new ModelDefinition(Model);
            if (modelDefinition.TryReadMetaData(out DeepPoseModelMetadata metadata))
            {
                Additive = metadata.model.Contains("Additive");
                GeneralizedLookAt = metadata.model_params.generalized_lookat;
                TransposeOrtho6D = metadata.model_params.transpose_ortho6d;
                Skeleton = metadata.GetSkeleton();
                UseRaycast = metadata.model_params.use_raycast;
                MaxRayDistance = metadata.model_params.max_ray_distance;

                UpdateEffectorList(metadata.model_params.pos_effectors, JointsWithPosition);
                UpdateEffectorList(metadata.model_params.rot_effectors, JointsWithRotation);
                UpdateEffectorList(metadata.model_params.lookat_effectors, JointsWithLookAt);
            }
            else
            {
                Skeleton = null;
            }
        }

        void UpdateEffectorList(IEnumerable<string> effectorNames, ICollection<int> effectorIndexes)
        {
            effectorIndexes.Clear();
            foreach (var effector in effectorNames)
            {
                var joint = Skeleton.FindJoint(effector);
                if (joint == null)
                {
                    Debug.LogWarning($"Effector not found: {effector}");
                    continue;
                }

                effectorIndexes.Add(joint.Index);
            }
        }
    }

    static class DeepPoseConfigurationUtils
    {
        public static bool HaveSameJointNames(this DeepPoseConfiguration config1, DeepPoseConfiguration config2)
        {
            if (config1 == null || config2 == null)
                return false;

            return config1.Skeleton.HaveSameJointNames(config2.Skeleton);
        }
    }
}
