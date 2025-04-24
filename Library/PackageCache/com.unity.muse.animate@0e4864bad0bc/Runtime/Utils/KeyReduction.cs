using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Muse.Animate
{
    static class KeyReduction
    {
        /// <summary>
        /// An implementation of the Ramer-Douglas-Peucker algorithm for curve simplification.
        /// </summary>
        /// <param name="keys">A list of keypoints that defines the curve.</param>
        /// <param name="startIndex">The index of the start of the range to simplify.</param>
        /// <param name="endIndex">The index of the end of the range to simplify (inclusive). </param>
        /// <param name="getDistance">A delegate that computes computes the difference between the
        /// first argument and some interpolated value derived from the second and third arguments.
        /// </param>
        /// <param name="tolerance">The minimum distance required to keep a keypoint.</param>
        /// <param name="result">Holds the indices of the keypoints in the simplified curve.</param>
        /// <typeparam name="T">Datatype that represents a point on the curve.</typeparam>
        public static void Reduce<T>(ReadOnlySpan<T> keys,
            int startIndex,
            int endIndex,
            Func<T, T, T, float> getDistance,
            float tolerance,
            List<int> result)
        {
            endIndex = endIndex >= 0 ? endIndex : keys.Length - 1;
            
            var start = keys[startIndex];
            var end = keys[endIndex];
            var maxDistance = 0f;
            var maxDistanceIndex = 0;
            for (var i = startIndex + 1; i < endIndex; ++i)
            {
                var distance = getDistance(keys[i], start, end);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxDistanceIndex = i;
                }
            }

            if (maxDistance > tolerance)
            {
                Reduce(keys, startIndex, maxDistanceIndex, getDistance, tolerance, result);
                Reduce(keys, maxDistanceIndex, endIndex, getDistance, tolerance, result);
            }
            else
            {
                if (startIndex == 0)
                    result.Add(startIndex);
                result.Add(endIndex);
            }
        }
        
        /// <summary>
        /// An async implementation of <see cref="Reduce{T}"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="getDistanceAsync"/> must be a thread-safe method. Pass in a <see cref="CancellationToken"/>
        /// if you want to be able to cancel the operation.
        /// The indices stored in <paramref name="result"/> are not guaranteed to be in order.
        /// </remarks>
        public static async Task ReduceAsync<T>(ReadOnlyMemory<T> keys,
            int startIndex,
            int endIndex,
            Func<T, T, T, ValueTask<float>> getDistanceAsync,
            float tolerance,
            ConcurrentBag<int> result,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var start = keys.Span[startIndex];
            var end = keys.Span[endIndex];
            var maxDistance = 0f;
            var maxDistanceIndex = 0;
            for (var i = startIndex + 1; i < endIndex; ++i)
            {
                var distance = await getDistanceAsync(keys.Span[i], start, end);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxDistanceIndex = i;
                }
            }

            if (maxDistance > tolerance)
            {
                var task1 = ReduceAsync(keys, startIndex, maxDistanceIndex, getDistanceAsync, tolerance, result,
                    cancellationToken);
                var task2 = ReduceAsync(keys, maxDistanceIndex, endIndex, getDistanceAsync, tolerance, result,
                    cancellationToken);
                await Task.WhenAll(task1, task2);
            }
            else
            {
                if (startIndex == 0)
                    result.Add(startIndex);
                result.Add(endIndex);
            }
        }
    }
}
