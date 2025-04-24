using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.DeepPose.Cloud;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class MotionCompletionCloudInterpolator
    {
        readonly struct IndexablePair<T>
        {
            public readonly T First;
            public readonly T Second;

            public IndexablePair(T first, T second)
            {
                First = first;
                Second = second;
            }

            public T this[int i] => i == 0 ? First : Second;
        }

        const string k_CharacterId = "biped_v0";
        readonly WebAPI m_MotionCompletionAPI;
        readonly ArmatureStaticPoseModel[] m_CachedFrames;
        readonly HashSet<IndexablePair<int>> m_ValidRanges = new();
        readonly object m_CacheLock = new();

        /// <summary>
        /// Mapping from the armature definition of the source poses to the armature definition of the motion completion
        /// model.
        /// </summary>
        readonly ArmatureToArmatureMapping m_SourceToMotionArmatureMapping;

#if UNITY_EDITOR
        int m_ApiCallCount;
#endif

        public MotionCompletionCloudInterpolator(int numFrames, ArmatureToArmatureMapping modelToSourceToArmatureMapping)
        {
            Assert.IsTrue(ApplicationConstants.UseMotionCloudInference,
                "The application must have cloud motion inference enabled.");

            m_MotionCompletionAPI = new WebAPI(WebUtils.BackendUrl, MotionCompletionAPI.ApiName);

            m_CachedFrames = new ArmatureStaticPoseModel[numFrames];
            m_SourceToMotionArmatureMapping = modelToSourceToArmatureMapping;
        }

        /// <summary>
        /// Use the motion completion cloud model to interpolate between two poses. Returns synchronously if the
        /// requested frame is already cached.
        /// </summary>
        /// <param name="targetIndex">The frame index that we want to interpolate for.</param>
        /// <param name="startPose">The starting pose.</param>
        /// <param name="startIndex">The frame index of the starting pose.</param>
        /// <param name="endPose">The ending pose.</param>
        /// <param name="endIndex">The frame index of the ending pose.</param>
        /// <param name="result">Holds the result of the interpolation. Must be pre-allocated with the
        /// same number of joints as the starting and end poses.</param>
        /// <returns>A task the represents the asynchronous operation.</returns>
        /// <remarks>
        /// DO NOT await this method from the main thread. This is because UnityWebRequest needs to respond on the main
        /// thread. Make sure that <see cref="ValueTask.IsCompleted"/> is <c>true</c> before accessing the result.
        /// </remarks>
        public async ValueTask InterpolateAsync(int targetIndex,
            ArmatureStaticPoseModel startPose,
            int startIndex,
            ArmatureStaticPoseModel endPose,
            int endIndex,
            ArmatureStaticPoseModel result)
        {
            await ComputeAllInBetweens(startPose, startIndex, endPose, endIndex);

            ArmatureStaticPoseModel interpolatedPose;
            lock (m_CacheLock)
            {
                interpolatedPose = m_CachedFrames[targetIndex];
            }

            // Motion completion results are in a different armature space; we need to map them back.
            interpolatedPose.CopyTo(result, m_SourceToMotionArmatureMapping, inverseMapping: true);
        }

        async ValueTask ComputeAllInBetweens(ArmatureStaticPoseModel startPose,
            int startIndex,
            ArmatureStaticPoseModel endPose,
            int endIndex)
        {
            var range = new IndexablePair<int>(startIndex, endIndex);

            lock (m_CacheLock)
            {
                if (m_ValidRanges.Contains(range))
                {
                    // Nothing to do. The computed range is in the cache.
                    return;
                }
            }

            var request = new MotionCompletionAPI.Request
            {
                CharacterID = k_CharacterId
            };

            var poses = new IndexablePair<ArmatureStaticPoseModel>(startPose, endPose);
            var numRotations = m_SourceToMotionArmatureMapping.NumTargetJoints;

            for (int i = 0; i < 2; i++)
            {
                var keyIndex = range[i];
                var pose = poses[i];

                var requestKey = new MotionCompletionAPI.Key(1, numRotations)
                {
                    Index = keyIndex - startIndex,
                    Type = MotionCompletionAPI.Key.KeyType.FullPose
                };

                requestKey.Positions[0] = pose.GetPosition(0);
                
                // API keys use the motion armature definition, so they need to be mapped. 
                pose.CopyTo(requestKey.Rotations, m_SourceToMotionArmatureMapping);

                request.Keys.Add(requestKey);
            }

#if UNITY_EDITOR
            var callCount = Interlocked.Increment(ref m_ApiCallCount);

            // Debug.Log($"Calling motion completion API: {callCount}");
#endif

            var tcs = new TaskCompletionSource<bool>();
            m_MotionCompletionAPI.SendJobRequestWithAuthHeaders<MotionCompletionAPI.Request, MotionCompletionAPI.Response>(request,
                response => OnRequestSuccess(response, range, tcs),
                error => OnRequestFail(error.Message, tcs));

            await tcs.Task;
        }

        static void OnRequestFail(string msg, TaskCompletionSource<bool> tcs)
        {
            tcs.SetException(new Exception(msg));
        }

        void OnRequestSuccess(MotionCompletionAPI.Response response, IndexablePair<int> range, TaskCompletionSource<bool> tcs)
        {
            lock (m_CacheLock)
            {
                m_ValidRanges.RemoveWhere(r => Overlaps(r, range));
                m_ValidRanges.Add(range);

                for (int i = 0; i < response.Frames.Count; i++)
                {
                    var responseFrame = response.Frames[i];
                    var targetPose = new ArmatureStaticPoseModel(responseFrame.Rotations.Length, ArmatureStaticPoseData.PoseType.Local);
                    targetPose.SetPosition(0, responseFrame.Positions[0]);
                    for (int j = 0; j < targetPose.NumJoints; j++)
                    {
                        targetPose.SetRotation(j, responseFrame.Rotations[j]);
                    }

                    m_CachedFrames[range.First + i] = targetPose;
                }

                tcs.SetResult(true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Overlaps(IndexablePair<int> a, IndexablePair<int> b)
        {
            return a.First < b.Second || b.First < a.Second;
        }
    }
}
