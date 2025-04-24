using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using AppUI = Unity.Muse.AppUI.UI;

namespace Unity.Muse.Common
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class GenericLoader : VisualElement
    {
#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<GenericLoader, UxmlTraits> { }
#endif
        const string k_GradientLoaderClass = "genai-loader-gradient";
        const string k_HiddenStateClass = "genai-loader-state-none";

        readonly AppUI.UI.CircularProgress m_Progress;
        readonly AppUI.UI.Text m_ProgressLabel;
        internal ErrorView m_ErrorView;

        internal State LoadingState { get; private set; }
        internal event Action<State> OnLoadingStateChanged;
        internal event Action OnRetry;
        internal event Action OnDelete;

        public GenericLoader()
            : this(State.Loading)
        {
        }

        public GenericLoader(State state, bool withProgress = false, ErrorView errorView = null)
        {
            m_Progress = new AppUI.UI.CircularProgress
            {
                size = Size.L
            };
            if (withProgress)
            {
                m_Progress.variant = AppUI.UI.Progress.Variant.Determinate;

                m_ProgressLabel = new AppUI.UI.Text
                {
                    size = AppUI.UI.TextSize.XS
                };

                m_Progress.Add(m_ProgressLabel);
            }

            Add(m_Progress);

            m_ErrorView = (errorView == null) ? new ErrorView() : errorView;
            Add(m_ErrorView);

            m_ErrorView.OnDelete += OnDeleteClicked;
            m_ErrorView.OnRetry += OnRetryClicked;

            InitializeStyle();

            SetState(state);
        }

        private void OnRetryClicked()
        {
            OnRetry?.Invoke();
        }

        private void OnDeleteClicked()
        {
            OnDelete?.Invoke();
        }

        void InitializeStyle()
        {
            var styleSheet = ResourceManager.Load<StyleSheet>(PackageResources.gradientLoaderStyleSheet);
            Debug.Assert(styleSheet, $"Could not find stylesheet at path: {PackageResources.gradientLoaderStyleSheet}");

            styleSheets.Add(styleSheet);

            AddToClassList(k_GradientLoaderClass);
        }

        public void SetState(State state, string errorMessage = null)
        {
            m_ErrorView.SetError(errorMessage);

            EnableInClassList(k_HiddenStateClass, state == State.None);
            m_Progress.EnableInClassList(k_HiddenStateClass, state != State.Loading);
            m_ErrorView.EnableInClassList(k_HiddenStateClass, state != State.Error);

            var preChangedState = LoadingState;

            LoadingState = state;

            if (preChangedState != state)
                OnLoadingStateChanged?.Invoke(state);
        }

        public void SetProgress(float progress)
        {
            progress /= 100f;
            m_Progress.value = progress;

            if (m_ProgressLabel != null)
                m_ProgressLabel.text = $"{Mathf.RoundToInt(progress * 100f)}%";
        }

        internal enum State
        {
            None,
            Loading,
            Error
        }
    }
}