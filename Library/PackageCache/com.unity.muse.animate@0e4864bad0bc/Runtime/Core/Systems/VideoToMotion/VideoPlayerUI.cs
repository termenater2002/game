using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// A UI component for playing videos.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class VideoPlayerUI : VisualElement
    {
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string Source
        {
            get => m_SourceUrl;
            set
            {
                if (value == m_SourceUrl) return;

                m_SourceUrl = value;
                LoadVideo();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool AutoPlay
        {
            get => m_AutoPlay;
            set => m_AutoPlay = value;
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool Loop
        {
            get => m_VideoPlayer.isLooping;
            set => m_VideoPlayer.isLooping = value;
        }

        public bool IsReady => m_VideoPlayer != null && m_VideoPlayer.isPrepared;

        /// <summary>
        /// The maximum width of the video texture. Set to 0 to use the video's original width.
        /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int MaxWidth
        {
            get => m_MaxWidth;
            set
            {
                m_MaxWidth = value;
                InitializeTexture();
            }
        }

        /// <summary>
        /// The maximum height of the video texture. Set to 0 to use the video's original height.
        /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int MaxHeight
        {
            get => m_MaxHeight;
            set
            {
                m_MaxHeight = value;
                InitializeTexture();
            }
        }

        /// <summary>
        /// The current time being rendered, in seconds.
        /// </summary>
        /// <remarks>
        /// This property is read-only. To seek the video to a specific time, use <see cref="SeekToTime"/>.
        /// </remarks>
        public float Time { get; private set; }

        /// <summary>
        /// The current frame number being rendered.
        /// </summary>
        public int Frame { get; private set; }

        /// <summary>
        /// Whether the video is currently playing.
        /// </summary>
        public bool IsPlaying => m_VideoPlayer != null && m_VideoPlayer.isPlaying;

        /// <summary>
        /// The total number of frames in the video.
        /// </summary>
        public int NumFrames => m_VideoPlayer != null && m_VideoPlayer.isPrepared ? (int)m_VideoPlayer.frameCount : 0;

        public float FrameRate => m_VideoPlayer != null && m_VideoPlayer.isPrepared ? m_VideoPlayer.frameRate : 0;

        public bool CanSeek => m_VideoPlayer != null && m_VideoPlayer.canSetTime && !IsSeeking;

        public bool IsSeeking => m_ProcessingSeeks || m_SeekQueue.Count > 0;

        public bool IsLoaded => m_VideoPlayer != null && !string.IsNullOrEmpty(m_VideoPlayer.url) && m_VideoPlayer.isPrepared;

        readonly Queue<int> m_SeekQueue = new();

        /// <summary>
        /// Invoked when the video is playing and <see cref="Time"/> changed.
        /// </summary>
        public event Action timeChanged;

        /// <summary>
        /// Invoked when the <see cref="IsPlaying"/> property changes.
        /// </summary>
        public event Action playStateChanged;

        /// <summary>
        /// Invoked when the <see cref="IsReady"/> property changes.
        /// </summary>
        public event Action readyStateChanged;

        public event Action<string> errorReceived;

        const string k_ClassName = "deeppose-video-player";

        Image m_VideoPlaybackImage;
        RenderTexture m_VideoTexture;
        VideoPlayer m_VideoPlayer;

        int m_MaxWidth;
        int m_MaxHeight;
        bool m_AutoPlay;
        bool m_ProcessingSeeks;
        bool m_IsSeeking;
        bool m_SeekCompletedEventReceived;
        string m_SourceUrl;
        
        const int k_MaxFramesToWaitForSeek = 60;

#if ENABLE_UXML_TRAITS
        public new class UxmlTraits : UITemplateContainer.UxmlTraits
        {
            readonly UxmlIntAttributeDescription m_MaxWidth = new() { name = "max-width", defaultValue = 0 };
            readonly UxmlIntAttributeDescription m_MaxHeight = new() { name = "max-height", defaultValue = 0 };
            readonly UxmlStringAttributeDescription m_Source = new() { name = "source" };
            readonly UxmlBoolAttributeDescription m_AutoPlay = new() { name = "auto-play", defaultValue = false };
            readonly UxmlBoolAttributeDescription m_Loop = new() { name = "loop", defaultValue = false };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var element = (VideoPlayerUI)ve;
                element.MaxWidth = m_MaxWidth.GetValueFromBag(bag, cc);
                element.MaxHeight = m_MaxHeight.GetValueFromBag(bag, cc);
                element.Source = m_Source.GetValueFromBag(bag, cc);
                element.AutoPlay = m_AutoPlay.GetValueFromBag(bag, cc);
                element.Loop = m_Loop.GetValueFromBag(bag, cc);
            }
        }

        public new class UxmlFactory : UxmlFactory<VideoPlayerUI, UxmlTraits> { }
#endif

        public VideoPlayerUI()
        {
            InitializePlayer();

            AddToClassList(k_ClassName);

            m_VideoPlaybackImage = new Image
            {
                name = "deeppose-video-player-image",
                style =
                {
                    backgroundColor = style.backgroundColor,
                }
            };
            Add(m_VideoPlaybackImage);
            style.overflow = Overflow.Hidden;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (m_VideoPlayer == null)
                InitializePlayer();
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (m_VideoPlayer != null)
            {
                GameObjectUtils.Destroy(m_VideoPlayer.gameObject);
                m_VideoPlayer = null;
            }

            if (m_VideoTexture != null)
            {
                m_VideoTexture.Release();
                m_VideoTexture = null;
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            InitializeTexture();
        }

        public void Play()
        {
            Debug.Assert(m_VideoPlayer != null, "Video player is not initialized.");

            // HACK: The player skips ahead a few frames when playing from paused.
            // This fixes it somehow.
            m_VideoPlayer.time = Time;
            m_VideoPlayer.Play();
        }

        public void Pause()
        {
            Debug.Assert(m_VideoPlayer != null, "Video player is not initialized.");
            m_VideoPlayer.Pause();

            playStateChanged?.Invoke();
        }

        void OnPlayerStarted(VideoPlayer _)
        {
            playStateChanged?.Invoke();
            if (Locator.TryGet(out ICoroutineRunner coroutineRunner))
                coroutineRunner.StartCoroutine(Tick());
        }

        IEnumerator Tick()
        {
            while (IsPlaying)
            {
                if (Frame != m_VideoPlayer.frame)
                {
                    Frame = (int)m_VideoPlayer.frame;
                    Time = (float)m_VideoPlayer.time;
                    timeChanged?.Invoke();
                }

                MarkDirtyRepaint();
                yield return null;
            }
        }

        void InitializePlayer()
        {
            if (m_VideoPlayer != null) return;

            if (!Locator.TryGet(out IRootObjectSpawner<GameObject> spawner))
                throw new Exception("Cannot instantiate game object. Is the application initialized?");

            m_VideoPlayer = spawner.CreateGameObject($"VideoPlayer ({name})")
                .AddComponent<VideoPlayer>();

            m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            m_VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            m_VideoPlayer.prepareCompleted += OnVideoPrepared;
            m_VideoPlayer.errorReceived += OnVideoError;
            m_VideoPlayer.seekCompleted += OnSeekCompleted;
            m_VideoPlayer.loopPointReached += OnLoopPointReached;
            m_VideoPlayer.started += OnPlayerStarted;
            m_VideoPlayer.waitForFirstFrame = true;
            m_VideoPlayer.timeUpdateMode = VideoTimeUpdateMode.GameTime;

            LoadVideo();
        }

        void OnVideoError(VideoPlayer _, string error)
        {
            errorReceived?.Invoke(error);
            readyStateChanged?.Invoke();
        }

        void InitializeTexture()
        {
            if (m_VideoPlayer == null || !m_VideoPlayer.isPrepared)
                return;

            if (m_VideoTexture != null)
                m_VideoTexture.Release();

            var textureSize = ImageUtils.FitSize(
                new Vector2Int((int)m_VideoPlayer.width, (int)m_VideoPlayer.height),
                new Vector2Int(m_MaxWidth, m_MaxHeight));

            m_VideoTexture = new RenderTexture(textureSize.x, textureSize.y, 24);
            m_VideoTexture.hideFlags = HideFlags.HideAndDontSave;
            m_VideoPlayer.targetTexture = m_VideoTexture;
            m_VideoPlaybackImage.image = m_VideoTexture;
        }

        void LoadVideo()
        {
            if (!m_VideoPlayer)
                return;

            if (string.IsNullOrEmpty(m_SourceUrl))
            {
                m_VideoPlayer.Stop();
                m_VideoPlayer.url = null;
                return;
            }

            m_VideoPlayer.url = m_SourceUrl;
            m_VideoPlayer.Pause();
        }

        void OnVideoPrepared(VideoPlayer source)
        {
            InitializeTexture();
            readyStateChanged?.Invoke();
            if (AutoPlay)
            {
                m_VideoPlayer.Play();
            }
        }

        /// <summary>
        /// Seek the video to the given time. It is not guaranteed to complete immediately.
        /// </summary>
        /// <param name="time">Desired time in seconds.</param>
        public void SeekToTime(float time)
        {
            m_SeekQueue.Clear();
            m_SeekQueue.Enqueue(Mathf.RoundToInt(time * FrameRate));

            if (m_ProcessingSeeks || m_IsSeeking)
                return;

            m_ProcessingSeeks = true;
            if (Locator.TryGet(out ICoroutineRunner coroutineRunner))
                coroutineRunner.StartCoroutine(PerformSeek());
        }

        /// <summary>
        /// Seek the video to the given frame. It is not guaranteed to complete immediately.
        /// </summary>
        /// <param name="frame"></param>
        public void SeekToFrame(int frame)
        {
            m_SeekQueue.Clear();
            m_SeekQueue.Enqueue(frame);

            if (m_ProcessingSeeks || m_IsSeeking)
                return;

            m_ProcessingSeeks = true;
            if (Locator.TryGet(out ICoroutineRunner coroutineRunner))
                coroutineRunner.StartCoroutine(PerformSeek());
        }

        IEnumerator PerformSeek()
        {
            m_ProcessingSeeks = true;
            while (m_SeekQueue.Count > 0)
            {
                var frame = m_SeekQueue.Dequeue();
                yield return SeekToFrameAsync(frame);
                MarkDirtyRepaint();
            }

            m_ProcessingSeeks = false;
        }

        /// <summary>
        /// Seek the video to the given frame. This operation is asynchronous and may not complete immediately.
        /// </summary>
        /// <param name="frame">The desired frame number.</param>
        /// <returns></returns>
        /// <remarks>
        /// Only one seek operation can be in progress at a time. If a seek operation is already in progress,
        /// this method will do nothing.
        /// </remarks>
        IEnumerator SeekToFrameAsync(int frame)
        {
            Debug.Assert(m_VideoPlayer != null, "Video player is not initialized.");

            // Seeking can take a few frames to complete.
            // If we are in the middle of a seek operation, we should not start another one.
            if (m_IsSeeking)
            {
                Debug.LogWarning("Seeking in progress");
                yield break;
            }

            // Pause for better stability
            bool wasPlaying = m_VideoPlayer.isPlaying;
            Pause();

            m_IsSeeking = true;
            m_SeekCompletedEventReceived = false;

            m_VideoPlayer.frame = frame;
            MarkDirtyRepaint();

            yield return WaitForSeekCompletionEvent();

            // The video player properties may take several frames to update, even after we're received the
            // seek completion event.
            int framesWaited = 0;
            while (m_VideoPlayer.frame != frame && framesWaited < k_MaxFramesToWaitForSeek)
            {
                MarkDirtyRepaint();
                yield return null;
                framesWaited++;
            }

            Time = (float)m_VideoPlayer.time;
            Frame = (int)m_VideoPlayer.frame;

            if (wasPlaying)
                Play();

            m_IsSeeking = false;
        }

        void OnSeekCompleted(VideoPlayer source)
        {
            m_SeekCompletedEventReceived = true;
        }

        IEnumerator WaitForSeekCompletionEvent()
        {
            while (!m_SeekCompletedEventReceived)
            {
                MarkDirtyRepaint();
                yield return null;
            }
        }

        void OnLoopPointReached(VideoPlayer source)
        {
            if (!Loop)
            {
                // If not looping, the video has stopped.
                playStateChanged?.Invoke();
            }
        }
    }
}
