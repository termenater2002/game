using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    static class BakingUtils
    {
        public static void RetrieveFrames(this BakedTimelineModel bakedTimeline, List<int> indices, int frameIndexOffset, List<BakedFrameModel> frames)
        {
            frames.Clear();

            foreach (var idx in indices)
            {
                var frameIdx = idx + frameIndexOffset;

                var bakedFrame = bakedTimeline.GetFrame(frameIdx);
                frames.Add(bakedFrame);
            }
        }

        public static void TransferKeysToBakedTimeline(this TimelineModel sourceTimeline, HashSet<EntityID> entityIDs, BakedTimelineMappingModel sourceMapping, BakedTimelineModel destinationBakedTimeline)
        {
            for (var keyIndex = 0; keyIndex < sourceTimeline.KeyCount; keyIndex++)
            {
                if (!sourceMapping.TryGetBakedKeyIndex(keyIndex, out var bakedKeyIndex))
                    throw new Exception($"Could not find mapping for key: {keyIndex}");

                var sequenceKey = sourceTimeline.GetKey(keyIndex);

                // Only transfer full poses
                if (sequenceKey.Key.Type != KeyData.KeyType.FullPose)
                    continue;

                // Copy key pose to baked timeline
                var bakedFrame = destinationBakedTimeline.GetFrame(bakedKeyIndex);
                TransferKeyToBakedFrame(entityIDs, sequenceKey, bakedFrame);
            }
        }

        /// <summary>
        /// Make sure the baked timeline only contains the entities in the source timeline, nothing more or less.
        /// Usefull when re-using the same BakedTimeline instance
        /// </summary>
        /// <param name="sourceTimeline">The source timeline from which to get teh entities</param>
        /// <param name="destinationBakedTimeline">The destination BakedTimeline that will have its entities synced with the source timeline ones</param>
        public static void SyncTimelineAndBakedTimelineEntities(TimelineModel sourceTimeline, BakedTimelineModel destinationBakedTimeline)
        {
            // Get all keyed actors
            using var keyedEntityIDs = TempHashSet<EntityID>.Allocate();
            sourceTimeline.GetAllEntities(keyedEntityIDs.Set);

            // Get all baked actors
            using var bakedEntityIDs = TempHashSet<EntityID>.Allocate();
            destinationBakedTimeline.GetAllEntities(bakedEntityIDs.Set);

            // Remove baked actors that are not keyed
            foreach (var entityID in bakedEntityIDs)
            {
                if (keyedEntityIDs.Contains(entityID))
                    continue;

                sourceTimeline.RemoveEntity(entityID);
            }

            // Add actors that are keyed but not baked
            foreach (var entityID in keyedEntityIDs)
            {
                if (bakedEntityIDs.Contains(entityID))
                    continue;

                var actorKey = sourceTimeline.GetFirstKey(entityID);
                actorKey.Key.TryGetKey(entityID, out var keyframeModel);

                var numJoints = keyframeModel.NumJoints;
                var numBodies = 0;  // TODO: physics body support
                destinationBakedTimeline.AddEntity(entityID, numJoints, numBodies);
            }
        }

        public static void SyncTimelineAndBakedTimelineEntities(
            IEnumerable<KeyValuePair<EntityID, EntityBakingContext>> entityBakingContexts,
            BakedTimelineModel destinationBakedTimeline)
        {
            // Get all baked actors
            using var bakedEntityIDs = TempHashSet<EntityID>.Allocate();
            destinationBakedTimeline.GetAllEntities(bakedEntityIDs.Set);

            // Get all actors in the backing context and add them to the baked timeline
            foreach (var (entityID, context) in entityBakingContexts)
            {
                if (bakedEntityIDs.Contains(entityID))
                    continue;

                var numBodies = 0;  // TODO: physics body support
                var numJoints = context.MotionArmature.ArmatureDefinition.NumJoints;
                destinationBakedTimeline.AddEntity(entityID, numJoints, numBodies);
            }
        }
        
        /// <summary>
        /// Computes the total duration of a timeline once baked
        /// </summary>
        /// <param name="timeline">The timeline for which to compute the duration</param>
        /// <returns>The number of frames that the baked timeline will have</returns>
        public static int ComputeAnimationLength(this TimelineModel timeline)
        {
            var sequenceKey = timeline.GetKey(0);
            if (sequenceKey == null)
                return 0;

            var bakedLength = 1;
            while (sequenceKey != null)
            {
                var sequenceTransition = sequenceKey.OutTransition;
                if (sequenceTransition != null)
                    bakedLength += sequenceTransition.Transition.Duration;

                sequenceKey = sequenceKey.NextKey;
            }

            return bakedLength;
        }

        /// <summary>
        /// Updates the mapping between a timeline and its baked timeline
        /// </summary>
        /// <param name="sourceTimeline">The source timeline</param>
        /// <param name="destinationMapping">The destination mapping that will contain the mapping info between the source timeline and its baked version</param>
        public static void UpdateBakingMapping(this TimelineModel sourceTimeline, BakedTimelineMappingModel destinationMapping)
        {
            destinationMapping.Clear();

            var currentIndex = 0;
            var sequenceKey = sourceTimeline.GetKey(0);
            while (sequenceKey != null)
            {
                var keyIndex = sourceTimeline.IndexOf(sequenceKey);
                Assert.IsTrue(keyIndex >= 0, "Invalid key index");

                destinationMapping.AddKey(keyIndex, currentIndex);

                var sequenceTransition = sequenceKey.OutTransition;
                if (sequenceTransition == null)
                    break;

                var transitionIndex = sourceTimeline.IndexOf(sequenceTransition);
                Assert.IsTrue(transitionIndex >= 0, "Invalid transition index");

                var transitionLength = sequenceTransition.Transition.Duration;
                destinationMapping.AddTransition(transitionIndex, currentIndex, currentIndex + transitionLength);
                currentIndex += transitionLength;

                sequenceKey = sequenceKey.NextKey;
            }
        }

        public static void TransferKeyToBakedFrame(HashSet<EntityID> entityIDs, TimelineModel.SequenceKey fromKey, BakedFrameModel toBakedFrame)
        {
            foreach (var entityID in entityIDs)
            {
                // Get the first valid key for that actor, first looking in the past
                var entityKey = fromKey.GetPreviousKey(entityID, true);

                // Then in the future
                if (entityKey == null)
                    entityKey = fromKey.GetNextKey(entityID, false);

                // This should not happen
                if (entityKey == null)
                    throw new ArgumentOutOfRangeException($"Could not find a key containing entity: {entityID}");

                // Get source actor key
                var found = entityKey.Key.TryGetKey(entityID, out var keyframeModel);

                // This should not happen
                if (!found)
                    AssertUtils.Fail($"Could not find an entity in the key for ID: {entityID}");

                // Get the baked key for that actor
                toBakedFrame.TryGetModel(entityID, out var bakedArmaturePoseModel);

                // This should not happen
                if (bakedArmaturePoseModel == null)
                    throw new ArgumentOutOfRangeException($"Could not find baked frame for entity: {entityID}");

                // Transfer the pose in the key to the baked timeline
                keyframeModel.LocalPose.CopyTo(bakedArmaturePoseModel.LocalPose);
                keyframeModel.GlobalPose.CopyTo(bakedArmaturePoseModel.GlobalPose);
            }
        }

        public static AnimationClip CloneAnimationClip(AnimationClip source)
        {
            return UnityEngine.Object.Instantiate(source);
        }
    }
}
