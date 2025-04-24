using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Defines a class that can convert a video file to a different format.
    /// </summary>
    interface IVideoConverter
    {
        /// <summary>
        /// Available codecs.
        /// </summary>
        public enum Format
        {
            [InspectorName("H.264 MP4")] MP4,
            [InspectorName("VP8 WEBM")] WEBM,
        }

        /// <summary>
        /// The maximum resolution of the output video.
        /// </summary>
        /// <remarks>
        /// The output video will have maximum dimensions of WxH or HxW, whichever is larger, while maintaining the
        /// aspect ratio.
        /// </remarks>
        Vector2Int MaxResolution { get; set; }

        /// <summary>
        /// Whether to include audio in the output video.
        /// </summary>
        bool IncludeAudio { get; set; }

        /// <summary>
        /// The video encoding profile to use if the codec is H.264.
        /// </summary>
        VideoEncodingProfile VideoProfile { get; set; }

        /// <summary>
        /// The target bitrate in Mbps.
        /// </summary>
        float BitRateMbps { get; set; }

        /// <summary>
        /// The codec to use for encoding the video.
        /// </summary>
        Format OutputFormat { get; set; }

        /// <summary>
        /// Whether the component is currently converting a video.
        /// </summary>
        bool IsConverting { get; }

        /// <summary>
        /// Event that is raised when the conversion is completed.
        /// </summary>
        event Action conversionCompleted;

        /// <summary>
        /// Event that is raised when the conversion fails.
        /// </summary>
        event Action<string> conversionFailed;

        /// <summary>
        /// Perform the conversion. The caller is responsible for launching the coroutine.
        /// </summary>
        /// <param name="inputPath">Path to the input video file.</param>
        /// <param name="outputPath">Path to the output video file.</param>
        /// <param name="startFrame">Starting frame number (0-indexed).</param>
        /// <param name="endFrame">Ending frame number (exclusive). If argument is -1, end on the final frame.</param>
        /// <returns>The enumerator.</returns>
        IEnumerator Convert(string inputPath, string outputPath, int startFrame = 0, int endFrame = -1);

        string GetTempOutputPath();
    }
}
