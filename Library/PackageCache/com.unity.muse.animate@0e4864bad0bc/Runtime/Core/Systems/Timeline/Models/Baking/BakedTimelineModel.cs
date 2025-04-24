using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    [Serializable]
    class BakedTimelineModel : ICopyable<BakedTimelineModel>, ISerializationCallbackReceiver
    {
        [SerializeField]
        BakedTimelineData m_Data;

        public delegate void Changed(BakedTimelineModel model);

        public event Changed OnChanged;

        public int FramesCount
        {
            get => m_Data.Frames.Count;
            set
            {
                while (FramesCount > value)
                {
                    RemoveFrame(FramesCount - 1, false);
                }

                while (FramesCount < value)
                {
                    AddFrame();
                }
            }
        }

        public bool IsValid
        {
            get
            {
                if (m_Data.Frames == null)
                    return false;

                foreach (var frame in m_Data.Frames)
                {
                    if (frame == null || !frame.IsValid)
                        return false;
                }

                return true;
            }
        }

        public BakedTimelineModel()
        {
            m_Data.Frames = new List<BakedFrameModel>();
            RegisterEvents();
        }

        public void OnBeforeSerialize()
        {
            //DevLogger.LogInfo("BakedTimelineModel -> OnBeforeSerialize()");
        }

        public void OnAfterDeserialize()
        {
            //DevLogger.LogInfo("BakedTimelineModel -> OnAfterDeserialize()");
        }

        public void Clear(bool silent = false)
        {
            while (FramesCount > 0)
            {
                RemoveFrame(FramesCount - 1, silent);
            }
        }

        public BakedFrameModel AddFrame(bool silent = false)
        {
            var frame = m_Data.Frames.Count == 0 ? new BakedFrameModel() : new BakedFrameModel(m_Data.Frames[0]);
            RegisterFrame(frame);

            m_Data.Frames.Add(frame);
            
            if (!silent)
                OnChanged?.Invoke(this);
            
            return frame;
        }

        public BakedFrameModel GetFrame(int frameIndex)
        {
            if (frameIndex < 0 || frameIndex >= FramesCount)
                AssertUtils.Fail($"Invalid frame index: {frameIndex}");

            return m_Data.Frames[frameIndex];
        }

        /// <summary>
        /// Get a range of frames
        /// </summary>
        /// <param name="fromIndex">The index of the first frame to get</param>
        /// <param name="toIndex">The index of the last frame to get, inclusive</param>
        /// <param name="frames">A list where to store the retrieved frames</param>
        public void GetFrames(int fromIndex, int toIndex, List<BakedFrameModel> frames)
        {
            for (var frameIndex = fromIndex; frameIndex <= toIndex; frameIndex++)
            {
                var frame = GetFrame(frameIndex);
                frames.Add(frame);
            }
        }

        public void RemoveFrame(int frameIndex, bool silent)
        {
            if (frameIndex < 0 || frameIndex >= FramesCount)
                AssertUtils.Fail($"Invalid frame index: {frameIndex}");

            var frame = m_Data.Frames[frameIndex];
            UnregisterFrame(frame);

            m_Data.Frames.RemoveAt(frameIndex);
            
            if(!silent)
                OnChanged?.Invoke(this);
        }

        public void AddEntity(EntityID entityID, int numJoints, int numBodies)
        {
            // Ensure it is not empty
            if (FramesCount == 0)
                AddFrame();

            foreach (var frame in m_Data.Frames)
            {
                if (frame.TryGetModel(entityID, out var frameModel))
                {
                    Assert.AreEqual(frameModel.NumBodies, numBodies);
                    Assert.AreEqual(frameModel.NumJoints, numJoints);
                    continue;
                }

                frame.AddEntity(entityID, numJoints, numBodies);
            }
        }

        public void RemoveEntity(EntityID entityID)
        {
            foreach (var frame in m_Data.Frames)
            {
                if (!frame.HasEntity(entityID))
                    continue;

                frame.RemoveEntity(entityID);
            }
        }

        public void GetAllEntities(HashSet<EntityID> set)
        {
            foreach (var frame in m_Data.Frames)
            {
                frame.GetAllEntities(set);
            }
        }

        void RegisterEvents()
        {
            foreach (var frame in m_Data.Frames)
            {
                RegisterFrame(frame);
            }
        }

        void RegisterFrame(BakedFrameModel frame)
        {
            frame.OnEntityPoseChanged += OnBakedFrameModelPoseChanged;
        }

        void UnregisterFrame(BakedFrameModel frame)
        {
            frame.OnEntityPoseChanged -= OnBakedFrameModelPoseChanged;
        }

        void OnBakedFrameModelPoseChanged(BakedFrameModel model, EntityID entityID)
        {
            OnChanged?.Invoke(this);
        }
        
        public void CopyTo(BakedTimelineModel target)
        {
            CopyTo(target, false);
        }
        
        public void CopyTo(BakedTimelineModel target, bool silent)
        {
            target.Clear();
            
            for (var i = 0; i < FramesCount; i++)
            {
                var frame = GetFrame(i);
                var otherFrame = target.AddFrame(silent);
                frame.CopyTo(otherFrame, silent);
            }

            if (!silent)
                target.OnChanged?.Invoke(target);
        }

        public BakedTimelineModel Clone()
        {
            var clone = new BakedTimelineModel();
            CopyTo(clone);
            return clone;
        }

        public Bounds GetWorldBounds()
        {
            var bounds = new Bounds();
            var first = true;
            foreach (var frame in m_Data.Frames)
            {
                var frameBounds = frame.GetWorldBounds();
                if (first)
                {
                    bounds = frameBounds;
                    first = false;
                }
                else
                {
                    bounds.Encapsulate(frameBounds);
                }
            }

            return bounds;
        }

        /// <summary>
        /// Get the root-centered bounds of the timeline, ignoring the root motion and global positions.
        /// </summary>
        public Bounds GetLocalBounds()
        {
            var bounds = new Bounds();
            var first = true;
            foreach (var frame in m_Data.Frames)
            {
                var frameBounds = frame.GetLocalBounds();
                if (first)
                {
                    bounds = frameBounds;
                    first = false;
                }
                else
                {
                    bounds.Encapsulate(frameBounds);
                }
            }

            return bounds;
        }

    }
}
