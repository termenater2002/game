using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    [Serializable]
    class Skeleton : ISerializationCallbackReceiver
    {
        /// <summary>
        /// Represents a joint in a skeleton
        /// </summary>
        public interface IJoint
        {
            /// <summary>
            /// Name of the joint
            /// </summary>
            string Name { get; }

            /// <summary>
            /// Index of the joint
            /// </summary>
            int Index { get; }

            /// <summary>
            /// Local offset of the joint
            /// </summary>
            Vector3 Offset { get; }

            /// <summary>
            /// Parent of the joint
            /// </summary>
            IJoint Parent { get; }

            /// <summary>
            /// Children of the joint
            /// </summary>
            IEnumerable<IJoint> Children { get; }
        }

        // Note: private as using this directly might break cache
        [Serializable]
        class Joint : IJoint, IEquatable<Joint>, ISerializationCallbackReceiver
        {
            public string Name => m_Name;
            public int Index => m_Index;
            public Vector3 Offset => m_Offset;
            public IJoint Parent => m_Parent;
            public IEnumerable<IJoint> Children => m_Children;

            [SerializeField]
            string m_Name;

            [SerializeField]
            int m_Index;

            [SerializeField]
            Vector3 m_Offset;

            [NonSerialized]
            Joint m_Parent;

            [NonSerialized]
            List<Joint> m_Children = new List<Joint>();

            /// <summary>
            /// Creates a new joint
            /// </summary>
            /// <param name="name">Name of the joint</param>
            /// <param name="index">Index of the joint</param>
            /// <param name="offset">Local offset of the joint</param>
            public Joint(string name, int index, Vector3 offset)
            {
                Assert.IsFalse(String.IsNullOrEmpty(name), "Joint name cannot be null or empty");
                Assert.IsTrue(index >= 0, "Joint index must be positive");

                m_Name = name;
                m_Index = index;
                m_Offset = offset;
            }

            public void AddChild(Joint child)
            {
                Assert.IsFalse(m_Children.Contains(child), "Cannot add the same joint twice");
                child.m_Parent = this;
                m_Children.Add(child);
            }

            public void RemoveChild(Joint child)
            {
                Assert.IsTrue(m_Children.Contains(child), "Trying to remove a joint that is not a child");
                child.m_Parent = null;
                m_Children.Remove(child);
            }

            public bool Equals(Joint other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return m_Name == other.m_Name && m_Index == other.m_Index && m_Offset.Equals(other.m_Offset);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != GetType())
                    return false;
                return Equals((Joint)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (m_Name != null ? m_Name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ m_Index;
                    hashCode = (hashCode * 397) ^ m_Offset.GetHashCode();
                    return hashCode;
                }
            }

            public void OnBeforeSerialize() { }

            public void OnAfterDeserialize()
            {
                if (m_Children == null)
                    m_Children = new List<Joint>();
                else
                    m_Children.Clear();
            }
        }

        /// <summary>
        /// Root joints of the skeleton
        /// </summary>
        public IEnumerable<IJoint> RootJoints => m_RootJoints;

        /// <summary>
        /// Total number of joints in the skeleton
        /// </summary>
        public int Count => m_NameToJoint.Count;

        /// <summary>
        /// Returns all joints in the skeleton
        /// </summary>
        public IEnumerable<IJoint> AllJoints => m_AllJoints;

        public List<int> JointsParentIndexes => m_JointParent;

        [NonSerialized]
        List<Joint> m_RootJoints = new List<Joint>();

        [NonSerialized]
        Dictionary<string, Joint> m_NameToJoint = new Dictionary<string, Joint>();

        [NonSerialized]
        Dictionary<int, Joint> m_IndexToJoint = new Dictionary<int, Joint>();

        [SerializeField]
        List<Joint> m_AllJoints = new List<Joint>();

        // For serialization purpose only
        [SerializeField]
        List<int> m_JointParent = new List<int>();

        /// <summary>
        /// Removes all joints from this skeleton
        /// </summary>
        public void Clear()
        {
            m_RootJoints.Clear();
            m_NameToJoint.Clear();
            m_IndexToJoint.Clear();
            m_AllJoints.Clear();
            m_JointParent.Clear();
        }

        /// <summary>
        /// Add a joint to the skeleton
        /// Joints must be unique
        /// </summary>
        /// <param name="name">Name of the joint</param>
        /// <param name="index">Index of the joint</param>
        /// <param name="offset">Local offset of the joint</param>
        /// <param name="parent">The parent of this joint or null of this is a root joint</param>
        public IJoint AddJoint(string name, int index, Vector3 offset, IJoint parent = null)
        {
            Assert.IsFalse(m_NameToJoint.ContainsKey(name), "Joint names must be unique");
            Assert.IsFalse(m_IndexToJoint.ContainsKey(index), "Joint indexes must be unique");

            var joint = new Joint(name, index, offset);

            if (parent == null)
            {
                m_RootJoints.Add(joint);
            }
            else
            {
                var internalParent = parent as Joint;
                Assert.IsNotNull(internalParent, "Unknown parent joint type");
                internalParent.AddChild(joint);
            }

            m_NameToJoint[joint.Name] = joint;
            m_IndexToJoint[joint.Index] = joint;
            m_AllJoints.Add(joint);

            return joint;
        }

        /// <summary>
        /// Add a copied joint to the skeleton (used when making a copy of an other skeleton).
        /// Note: Since the skeleton joints are not in order of hierarchy, we copy them in a first pass,
        /// then set their parent in a second pass.
        /// </summary>
        /// <param name="name">Name of the joint</param>
        /// <param name="index">Index of the joint</param>
        /// <param name="offset">Local offset of the joint</param>
        IJoint AddCopiedJoint(string name, int index, Vector3 offset)
        {
            var joint = new Joint(name, index, offset);
            m_AllJoints.Add(joint);
            return joint;
        }

        /// <summary>
        /// Removes a joint from the skeleton
        /// </summary>
        /// <param name="joint">The joint to remove</param>
        public void RemoveJoint(IJoint joint)
        {
            if (joint == null)
                return;

            var internalJoint = joint as Joint;
            Assert.IsNotNull(internalJoint, "Unknown joint type");

            if (joint.Parent == null)
            {
                Assert.IsTrue(m_RootJoints.Contains(internalJoint), "Trying to remove a joint that is not part of this skeleton");
                m_RootJoints.Remove(internalJoint);
            }
            else
            {
                var internalParent = joint.Parent as Joint;
                Assert.IsNotNull(internalParent, "Unknown parent joint type");

                internalParent.RemoveChild(internalJoint);
            }

            m_NameToJoint.Remove(joint.Name);
            m_IndexToJoint.Remove(joint.Index);
            m_AllJoints.Remove(internalJoint);
        }

        /// <summary>
        /// Find a joint by index
        /// </summary>
        /// <param name="index">The index of the joint to find</param>
        /// <returns>The joint with corresponding index, null if none found</returns>
        public IJoint FindJoint(int index)
        {
            return m_IndexToJoint.TryGetValue(index, out var joint) ? joint : null;
        }

        /// <summary>
        /// Find a joint by name
        /// </summary>
        /// <param name="name">The name of the joint to find</param>
        /// <returns>The joint with corresponding name, null if none found</returns>
        public IJoint FindJoint(string name)
        {
            return m_NameToJoint.TryGetValue(name, out var joint) ? joint : null;
        }

        public void OnBeforeSerialize()
        {
            // Build flat hierarchy
            m_JointParent.Clear();
            foreach (var joint in m_AllJoints)
            {
                if (joint.Parent == null)
                    m_JointParent.Add(-1);
                else
                    m_JointParent.Add(joint.Parent.Index);
            }
        }

        public void OnAfterDeserialize()
        {
            m_RootJoints.Clear();
            m_NameToJoint.Clear();
            m_IndexToJoint.Clear();

            // Build look-up
            foreach (var joint in m_AllJoints)
            {
                m_NameToJoint[joint.Name] = joint;
                m_IndexToJoint[joint.Index] = joint;
            }

            // Build hierarchy
            for (var i = 0; i < m_AllJoints.Count; i++)
            {
                var joint = m_AllJoints[i];
                var parentIdx = m_JointParent[i];

                if (parentIdx >= 0)
                {
                    var parentJoint = m_IndexToJoint[parentIdx];
                    parentJoint.AddChild(joint);
                }
                else
                {
                    m_RootJoints.Add(joint);
                }
            }
        }

        /// <summary>
        /// Copy the joint data from an other <see cref="Skeleton"/> instance into this one.
        /// </summary>
        /// <param name="skeleton">The <see cref="Skeleton"/> instance to copy joints from.</param>
        public void CopyFrom(Skeleton skeleton)
        {
            m_AllJoints = new List<Joint>();
            m_IndexToJoint.Clear();
            m_NameToJoint.Clear();
            m_RootJoints.Clear();
            m_JointParent.Clear();

            // Copy the hierarchy from the skeleton
            foreach (var index in skeleton.JointsParentIndexes)
            {
                m_JointParent.Add(index);
            }

            // Copy the joints first
            foreach (var joint in skeleton.AllJoints)
            {
                AddCopiedJoint(joint.Name, joint.Index, joint.Offset);
            }

            OnAfterDeserialize();
        }
    }
}
