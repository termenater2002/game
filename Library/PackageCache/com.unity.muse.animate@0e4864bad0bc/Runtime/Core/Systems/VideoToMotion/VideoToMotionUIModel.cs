using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// The ViewModel providing the UI for video-to-motion generation.
    /// </summary>
    /// <remarks>
    /// This UI allows us to select a section of a video to extract motion from.
    /// </remarks>
    class VideoToMotionUIModel
    {
        public enum Property
        {
            VideoPath,
            FrameRange
        }

        public string VideoPath => m_VideoToMotionAuthoringModel?.VideoPath ?? "";

        /// <summary>
        /// The frame of the video that we should start extracting motion from.
        /// </summary>
        public int StartFrame
        {
            get => m_VideoToMotionAuthoringModel?.StartFrame ?? 0;
            set
            {
                if (m_VideoToMotionAuthoringModel != null)
                {
                    m_VideoToMotionAuthoringModel.StartFrame = value;
                }
            }
        }

        /// <summary>
        /// Duration of the take we want to generate in seconds.
        /// </summary>
        public float Duration
        {
            get => m_Duration;
            set
            {
                m_Duration = value;
                if (m_VideoToMotionAuthoringModel != null)
                {
                    m_VideoToMotionAuthoringModel.FrameCount = Mathf.RoundToInt(m_Duration * m_VideoFrameRate);
                }
            }
        }

        /// <summary>
        /// Gets or sets the frame rate of the video.
        /// </summary>
        public float VideoFrameRate
        {
            get => m_VideoFrameRate;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Frame rate must be greater than zero.");
                }

                m_VideoFrameRate = value;
                if (m_VideoToMotionAuthoringModel != null)
                {
                    m_VideoToMotionAuthoringModel.FrameCount = Mathf.RoundToInt(m_Duration * m_VideoFrameRate);
                }
            }
        }

        /// <summary>
        /// The frame number of the last frame of the video section that we are extracting.
        /// </summary>
        public int EndFrame =>
            m_VideoToMotionAuthoringModel == null
                ? 0
                : m_VideoToMotionAuthoringModel.StartFrame + m_VideoToMotionAuthoringModel.FrameCount - 1;

        public bool IsFrameRangeValid =>
            StartFrame >= 0 &&
            Duration is > 0 and <= ApplicationConstants.VideoToMotionMaxDuration;

        public bool CanGenerate => !string.IsNullOrEmpty(VideoPath) && IsFrameRangeValid;

        public event Action<Property> changed;

        VideoToMotionAuthoringModel m_VideoToMotionAuthoringModel;
        float m_VideoFrameRate = 30f;
        float m_Duration;

        public void SetAuthoringModel(VideoToMotionAuthoringModel authoringModel)
        {
            UnsubscribeFromAuthoringModel();
            m_VideoToMotionAuthoringModel = authoringModel;
            SubscribeToAuthoringModel();
        }

        void SubscribeToAuthoringModel()
        {
            if (m_VideoToMotionAuthoringModel == null)
                return;

            m_VideoToMotionAuthoringModel.OnChanged += OnModelChanged;
        }

        void UnsubscribeFromAuthoringModel()
        {
            if (m_VideoToMotionAuthoringModel == null)
                return;

            m_VideoToMotionAuthoringModel.OnChanged -= OnModelChanged;
        }

        /// <summary>
        /// Shows a file dialog to select a video file.
        /// </summary>
        public void BrowseFile(IEnumerable<string> extensions, float maxFileSizeMb)
        {
            if (m_VideoToMotionAuthoringModel == null)
                return;

            // Open a file dialog to select a video file
            var filePath = EditorUtility.OpenFilePanelWithFilters("Select a video file", "",
                new[]
                {
                    "Video files",
                    string.Join(",", extensions).ToLower()
                });

            if (string.IsNullOrEmpty(filePath))
                return;

            if (maxFileSizeMb > 0 && new System.IO.FileInfo(filePath).Length > maxFileSizeMb * 1024 * 1024)
            {
                EditorUtility.DisplayDialog("File too large",
                    $"File size exceeds the maximum allowed size of {maxFileSizeMb} MB.",
                    "OK");
                return;
            }

            m_VideoToMotionAuthoringModel.VideoPath = filePath;
        }

        void OnModelChanged(VideoToMotionAuthoringModel.Property property)
        {
            switch (property)
            {
                case VideoToMotionAuthoringModel.Property.RequestFilePath:
                    OnVideoPathChanged();
                    break;
                case VideoToMotionAuthoringModel.Property.RequestStartFrame:
                case VideoToMotionAuthoringModel.Property.RequestFrameCount:
                    Debug.Assert(m_VideoFrameRate > 0);
                    m_Duration = m_VideoToMotionAuthoringModel.FrameCount / m_VideoFrameRate;
                    changed?.Invoke(Property.FrameRange);
                    break;
            }
        }

        void OnVideoPathChanged()
        {
            DevLogger.LogInfo($"Opening video file: {m_VideoToMotionAuthoringModel.VideoPath}");
            changed?.Invoke(Property.VideoPath);
        }

        public void Generate()
        {
            m_VideoToMotionAuthoringModel?.RequestGenerate();
        }

        public void UnloadVideo()
        {
            if (m_VideoToMotionAuthoringModel != null)
            {
                m_VideoToMotionAuthoringModel.VideoPath = null;
            }
        }
    }
}
