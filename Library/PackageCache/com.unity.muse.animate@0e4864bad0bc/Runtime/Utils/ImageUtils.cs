using UnityEngine;

namespace Unity.Muse.Animate
{
    static class ImageUtils
    {
        /// <summary>
        /// Compute the size of an image that fits within a maximum size while maintaining aspect ratio.
        /// </summary>
        /// <param name="size">The original size.</param>
        /// <param name="maxSize">The maximum size.</param>
        /// <returns></returns>
        /// <remarks>Set a dimension of <paramref name="maxSize"/> to 0 if you want it to be unlimited.</remarks>
        public static Vector2Int FitSize(Vector2Int size, Vector2Int maxSize)
        {
            if (maxSize is { x: <= 0, y: <= 0 })
                return size;
            
            var aspect = (float)size.x / size.y;
            var maxAspect = maxSize switch
            {
                { y: <= 0 } => 0,
                { x: <= 0 } => float.PositiveInfinity,
                _ => (float)maxSize.x / maxSize.y
            };

            if (aspect > maxAspect)
            {
                size.x = maxSize.x;
                size.y = Mathf.RoundToInt(size.x / aspect);
            }
            else
            {
                size.y = maxSize.y;
                size.x = Mathf.RoundToInt(size.y * aspect);
            }

            return size;
        }
    }
}
