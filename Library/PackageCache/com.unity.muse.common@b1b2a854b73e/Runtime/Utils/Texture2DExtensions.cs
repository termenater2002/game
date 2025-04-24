using UnityEngine;

namespace Unity.Muse.Common
{
    //Add unit tests

    static class Texture2DExtensions
    {
        /// <summary>
        /// Verify if the texture is a square
        /// </summary>
        /// <param name="texture2D">Texture to verify if it's a square</param>
        /// <returns><c>true</c> If the texture is a square, <c>false</c> otherwise</returns>
        internal static bool IsSquare(this Texture2D texture2D)
        {
            if (texture2D.width == 0 || texture2D.height == 0)
                return false;

            return texture2D.width == texture2D.height;
        }

        /// <summary>
        /// Creates a copy of a texture and resize it to target width and target height
        /// </summary>
        /// <param name="texture2D">Texture to resize</param>
        /// <param name="targetWidth">Target width</param>
        /// <param name="targetHeight">Target height</param>
        /// <returns>Resized Texture</returns>
        // The method is not called "Resize" because it would conflict with
        // the method of the same name in the Texture2D class
        internal static Texture2D ResizeTexture(this Texture2D texture2D, int targetWidth, int targetHeight)
        {
            var renderTexture = new RenderTexture(targetWidth, targetHeight, 24);
            RenderTexture.active = renderTexture;
            Graphics.Blit(texture2D, renderTexture);
            var result = new Texture2D(targetWidth, targetHeight);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();
            return result;
        }

        /// <summary>
        /// Creates a copy of a texture, crop it at the center using target width and target height
        /// </summary>
        /// <param name="texture2D">Texture to resize</param>
        /// <param name="cropWidth">Crop width</param>
        /// <param name="cropHeight">Crop height</param>
        /// <returns>Resized Texture</returns>
        internal static Texture2D CropTextureCenter(this Texture2D texture2D, int cropWidth, int cropHeight)
        {
            if (texture2D.width < cropWidth || texture2D.height < cropHeight)
            {
                Debug.LogError("Crop size must be smaller than texture size");
                return null;
            }

            var renderTexture = new RenderTexture(texture2D.width, texture2D.height, 24);
            RenderTexture.active = renderTexture;
            Graphics.Blit(texture2D, renderTexture);
            var result = new Texture2D(cropWidth, cropHeight);

            var ratio = (float)texture2D.width / texture2D.height;
            var startHeight = 0f;
            var startWidth = 0f;

            if (ratio < 1)
            {
                var middleHeight = (float)texture2D.height / 2;
                var middleWidth = (float)cropWidth / 2;
                startHeight = middleHeight - middleWidth;
            }
            else
            {
                var middleWidth = (float)texture2D.width / 2;
                var middleHeight = (float)cropHeight / 2;
                startWidth = middleWidth - middleHeight;
            }

            result.ReadPixels(new Rect(startWidth, startHeight, cropWidth, cropHeight), 0, 0);
            result.Apply();
            return result;
        }
    }
}
