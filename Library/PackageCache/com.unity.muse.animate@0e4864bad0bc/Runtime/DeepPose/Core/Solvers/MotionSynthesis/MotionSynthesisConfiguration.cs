using System;
using Unity.Sentis;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// Encapsulates models and their specific parameters.
    /// </summary>
    abstract class MotionSynthesisConfiguration : ScriptableObject
    {
        [Serializable]
        public struct JointDefinition
        {
            public string Name;
            public bool Positional;
            public bool Rotational;
            public bool LookAt;
        }

        public bool PredictsFootContacts;
        public int[] ContactJoints;

        /// <summary>The model.</summary>
        public ModelAsset Model;

        /// <summary>The type of worker used for Barracuda models.</summary>
        public BackendType BarracudaBackend = BackendType.CPU;

        /// <summary>Contains the skeleton used by the model.</summary>
        public Skeleton Skeleton;

        /// <summary>Title used to describe this model.</summary>
        public string Title = "Unnamed";

        /// <summary>If true, ortho6D uses matrix rows instead of columns (should be true when using pytorch3d).</summary>
        public bool TransposeOrtho6D;

        /// <summary>If true, the model uses raycasts distances as additional inputs on the joints specified in RaycastJoints.</summary>
        public bool UseRaycasts;

        /// <summary>Joints used to get raycast inputs</summary>
        public int[] RaycastJoints;

        /// <summary>Raycast distance clamping value (min= -RaycastClampValue, max = RaycastClampValue).</summary>
        public float RaycastClampValue;

        public bool HasTolerance;

        public float MaxPosNoiseStd;

        public float MaxRotNoiseStd;

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

        public abstract void ReadMetaData();
    }
}
