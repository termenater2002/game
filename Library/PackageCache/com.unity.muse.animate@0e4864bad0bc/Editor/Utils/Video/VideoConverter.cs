using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Media;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Unity.Muse.Animate.Editor
{
    /// <summary>
    /// Converts a video file to a different format.
    /// </summary>
    class VideoConverter : IVideoConverter
    {
        /// <summary>
        /// The maximum resolution of the output video.
        /// </summary>
        /// <remarks>
        /// The output video will have maximum dimensions of WxH or HxW, whichever is larger, while maintaining the
        /// aspect ratio.
        /// </remarks>
        public Vector2Int MaxResolution { get; set; } = new(1920, 1080);

        /// <summary>
        /// Whether to include audio in the output video.
        /// </summary>
        public bool IncludeAudio { get; set; } = false;

        /// <summary>
        /// The video encoding profile to use if the codec is H.264.
        /// </summary>
        public VideoEncodingProfile VideoProfile { get; set; } = VideoEncodingProfile.H264High;

        /// <summary>
        /// The target bitrate in Mbps.
        /// </summary>
        public float BitRateMbps { get; set; } = 8f;

        /// <summary>
        /// The codec to use for encoding the video.
        /// </summary>
        public IVideoConverter.Format OutputFormat { get; set; } = IVideoConverter.Format.MP4;

        /// <summary>
        /// Whether the component is currently converting a video.
        /// </summary>
        public bool IsConverting { get; private set; }

        /// <summary>
        /// Event that is raised when the conversion is completed.
        /// </summary>
        public event Action conversionCompleted;

        /// <summary>
        /// Event that is raised when the conversion fails.
        /// </summary>
        public event Action<string> conversionFailed;

        VideoPlayer m_VideoPlayer;

        readonly Queue<Texture2D> m_InputFrames = new();

        public VideoConverter(VideoPlayer videoPlayer)
        {
            m_VideoPlayer = videoPlayer;

            m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            m_VideoPlayer.sendFrameReadyEvents = true;
            m_VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            m_VideoPlayer.isLooping = false;
            m_VideoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;

            m_VideoPlayer.skipOnDrop = false;
            m_VideoPlayer.playbackSpeed = 10f;
        }

        /// <summary>
        /// Perform the conversion where the caller controls the co-routine. Useful to chaining another operation
        /// or for testing.
        /// </summary>
        /// <param name="inputPath">Path to the input video file.</param>
        /// <param name="outputPath">Path to the output video file.</param>
        /// <param name="startFrame">Starting frame number (0-indexed).</param>
        /// <param name="endFrame">Ending frame number (exclusive). If argument is -1, end on the final frame.</param>
        /// <returns>The enumerator.</returns>
        public IEnumerator Convert(string inputPath, string outputPath, int startFrame = 0, int endFrame = -1)
        {
            if (IsConverting)
                throw new InvalidOperationException("Already converting a video.");

            IsConverting = true;

            yield return LoadVideo(inputPath);

            if (!m_VideoPlayer.isPrepared)
            {
                // We failed to prepare the video for playback. Probably encountered and error.
                IsConverting = false;
                yield break;
            }

            var adjustedEndFrame = endFrame < 0 ? (int)m_VideoPlayer.frameCount : endFrame;
            if (startFrame < 0 || startFrame >= adjustedEndFrame)
            {
                IsConverting = false;
                throw new ArgumentOutOfRangeException(nameof(startFrame), "Invalid frame range specified");
            }

            if (adjustedEndFrame > (int)m_VideoPlayer.frameCount)
            {
                adjustedEndFrame = (int)m_VideoPlayer.frameCount;
                Debug.LogWarning("End frame exceeds the total number of frames. Adjusting to the last frame.");
            }

            var framesToConvert = adjustedEndFrame - startFrame;

            // Create a render texture of the desired resolution to hold the decoded frames.
            var videoSize = new Vector2Int((int)m_VideoPlayer.width, (int)m_VideoPlayer.height);
            var outputSize = GetScaledResolution(videoSize, MaxResolution);
            var videoRenderTarget = new RenderTexture(outputSize.x, outputSize.y, 0, RenderTextureFormat.ARGB32)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            m_VideoPlayer.targetTexture = videoRenderTarget;

            VideoTrackEncoderAttributes videoTrackAttributes = OutputFormat switch
            {
                IVideoConverter.Format.MP4 => new VideoTrackEncoderAttributes(new H264EncoderAttributes
                {
                    gopSize = 25,
                    numConsecutiveBFrames = 2,
                    profile = VideoProfile
                }),
                IVideoConverter.Format.WEBM => new VideoTrackEncoderAttributes(new VP8EncoderAttributes
                {
                    keyframeDistance = 25
                }),
                _ => throw new ArgumentOutOfRangeException(nameof(OutputFormat), OutputFormat, "Unsupported codec")
            };

            videoTrackAttributes.frameRate = GetRationalFrameRate(m_VideoPlayer.frameRate);
            videoTrackAttributes.width = (uint)outputSize.x;
            videoTrackAttributes.height = (uint)outputSize.y;
            videoTrackAttributes.includeAlpha = false;
            videoTrackAttributes.bitRateMode = VideoBitrateMode.High;
            videoTrackAttributes.targetBitRate = (uint)(BitRateMbps * 1_000_000);

            if (IncludeAudio)
            {
                Debug.LogWarning("Audio is not supported at this time.");
            }

            using var encoder = new MediaEncoder(outputPath, videoTrackAttributes);

            m_InputFrames.Clear();

            int progressId = Progress.Start("Converting video");

            var grabFramesIter = GrabFrames(m_InputFrames, m_VideoPlayer, startFrame, adjustedEndFrame);
            var writeFramesIter = WriteFrames(m_InputFrames, encoder);

            bool keepGrabbing = true;
            bool keepWriting = true;
            while (keepGrabbing || keepWriting)
            {
                if (keepGrabbing)
                {
                    keepGrabbing = grabFramesIter.MoveNext();
                }
                if (keepWriting)
                {
                    keepWriting = writeFramesIter.MoveNext();
                }
                Progress.Report(progressId, (float)(m_VideoPlayer.frame - startFrame) / framesToConvert, "Converting video");

                yield return null;
            }

            Progress.Remove(progressId);

            videoRenderTarget.Release();
            m_VideoPlayer.targetTexture = null;
            IsConverting = false;
            conversionCompleted?.Invoke();
        }

        public string GetTempOutputPath() => FileUtil.GetUniqueTempPathInProject();

        static Vector2Int GetScaledResolution(Vector2Int original, Vector2Int max)
        {
            var aspect = (float)original.x / original.y;
            if (aspect < 1f && max.x > max.y)
            {
                max = new Vector2Int(max.y, max.x);
            }

            if (original.x <= max.x && original.y <= max.y)
            {
                return original;
            }

            var scale = Mathf.Min(max.x / (float)original.x, max.y / (float)original.y);
            return Vector2Int.RoundToInt(scale * new Vector2(original.x, original.y));
        }

        static MediaRational GetRationalFrameRate(float value)
        {
            var numerator = Mathf.RoundToInt(value * 1000);
            return new MediaRational(numerator, 1000);
        }

        IEnumerator LoadVideo(string path)
        {
            m_VideoPlayer.url = path;
            m_VideoPlayer.errorReceived += VideoLoadFailed;
            m_VideoPlayer.Play();
            bool hasError = false;

            void VideoLoadFailed(VideoPlayer vp, string error)
            {
                Debug.LogError(error);
                hasError = true;
                conversionFailed?.Invoke(error);
            }

            while (!m_VideoPlayer.isPrepared && !hasError)
            {
                if (!EditorApplication.isPlaying)
                {
                    // Force a redraw of the scene. The video player will not advance otherwise.
                    EditorApplication.QueuePlayerLoopUpdate();
                }
                yield return null;
            }

            m_VideoPlayer.errorReceived -= VideoLoadFailed;
        }

        static IEnumerator GrabFrames(Queue<Texture2D> queue,
            VideoPlayer player,
            int startFrame, int endFrame)
        {
            // the endFrame parameter is exclusive
            Debug.Assert(startFrame >= 0 && startFrame < endFrame && endFrame <= (int)player.frameCount);
            long currentFrame = startFrame;

            player.frameReady += FrameReady;
            player.Pause();
            player.frame = startFrame;
            player.Play();

            void FrameReady(VideoPlayer vp, long frameIndex)
            {
                currentFrame = frameIndex;
                if (frameIndex >= endFrame)
                {
                    queue.Enqueue(null);
                    vp.frameReady -= FrameReady;
                    return;
                }

                // Copy the contents of the render texture to a Texture2D. The data must be available on the CPU.
                // This is a requirement for the MediaEncoder.
                var frameTexture = vp.targetTexture;

                var tex = new Texture2D(frameTexture.width, frameTexture.height, TextureFormat.RGBA32, false);

                RenderTexture.active = frameTexture;
                tex.ReadPixels(new Rect(0, 0, frameTexture.width, frameTexture.height), 0, 0);
                tex.Apply();

                queue.Enqueue(tex);
            }

            while (currentFrame < endFrame)
            {
                if (!EditorApplication.isPlaying)
                {
                    // Force a redraw of the scene. The video player will not advance otherwise.
                    EditorApplication.QueuePlayerLoopUpdate();
                }

                yield return null;
            }

            player.frameReady -= FrameReady;
        }

        IEnumerator WriteFrames(Queue<Texture2D> queue, MediaEncoder encoder)
        {
            while (true)
            {
                if (queue.TryDequeue(out var tex))
                {
                    if (tex == null)
                    {
                        break;
                    }

                    encoder.AddFrame(tex);
                    Object.DestroyImmediate(tex);
                }
                yield return null;
            }
        }
    }
}
