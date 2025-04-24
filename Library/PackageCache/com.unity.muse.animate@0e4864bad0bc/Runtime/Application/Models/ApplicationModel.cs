using System;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents the base Model of the Application itself.
    /// </summary>
    /// <remarks>
    /// Handles requests for actions available at the base level of the application.
    /// </remarks>
    class ApplicationModel
    {
        AuthoringModel AuthoringModel => Context.AuthorContext.Authoring;
        internal ApplicationLibraryModel ApplicationLibraryModel { get; }
        ApplicationContext m_Context;

        ApplicationContext Context => m_Context;

        public ApplicationModel(SelectionModel<LibraryItemAsset> itemsSelection)
        {
            DevLogger.LogInfo("Created ApplicationModel");
            ApplicationLibraryModel = new ApplicationLibraryModel(itemsSelection);
        }

        internal void SetContext(ApplicationContext context)
        {
            if (m_Context != null)
            {
                Unsubscribe();
            }

            m_Context = context;

            if (m_Context != null)
            {
                Subscribe();
            }
        }

        void Subscribe()
        {
            ApplicationLibraryModel.Context = Context;

            // Takes UI Requests
            Context.TakesUIModel.OnRequestedGenerate += OnTakesUIRequestedGenerate;

            // Video to Motion requests
            Context.AuthorContext.Authoring.VideoToMotionAuthoringModel.OnGenerateRequested += DoVideoToMotionGenerate;

            // Return to library button
            Context.ReturnToLibraryButton.clicked += OnReturnToLibraryRequested;
        }

        void Unsubscribe()
        {
            ApplicationLibraryModel.Context = null;

            // Takes UI Requests
            Context.TakesUIModel.OnRequestedGenerate -= OnTakesUIRequestedGenerate;

            // Video to Motion requests
            AuthoringModel.VideoToMotionAuthoringModel.OnGenerateRequested -= DoVideoToMotionGenerate;

            // Return to library button
            Context.ReturnToLibraryButton.clicked -= OnReturnToLibraryRequested;
        }

        void OnReturnToLibraryRequested()
        {
            var canExit = true;
            if (Context.AuthorContext.TimelineContext.TimelineBakingLogic.IsRunning)
            {
                canExit = EditorUtility.DisplayDialog("Motion Completion in progress",
                    "The motion completion operation is in progress. If you exit now, the animation will be incomplete until you edit it again.\n\nContinue?",
                    "Yes", "No");
            }
            if (canExit)
            {
                DoGoToLibrary();
            }
        }

        // [Section] Do Methods

        internal void DoGoToLibrary()
        {
            // Save any ongoing work
            if (Context.ApplicationLibraryModel.ActiveLibraryItem != null)
                Application.Instance.PublishMessage(new SaveLibraryItemAssetMessage(Context.ApplicationLibraryModel.ActiveLibraryItem));

            Application.Instance.IsAuthoringAssetLoaded = false;
            AuthoringModel.Mode = AuthoringModel.AuthoringMode.Unknown;
        }

        void DoTextToMotionGenerate(string prompt, int? seed, int takesAmount, float duration, ITimelineBakerTextToMotion.Model model)
        {
            var textToMotionAuthoring = Context.AuthorContext.TextToMotionTakeContext.Model;

            textToMotionAuthoring.RequestPrompt = prompt;
            textToMotionAuthoring.RequestTakesCounter = 0;
            textToMotionAuthoring.RequestTakesAmount = takesAmount;
            textToMotionAuthoring.RequestDuration = duration;
            textToMotionAuthoring.RequestModel = model;

            while (textToMotionAuthoring.RequestTakesCounter <
                   textToMotionAuthoring.RequestTakesAmount)
            {
                var numFrames = (int)(textToMotionAuthoring.RequestDuration * ApplicationConstants.FramesPerSecond);
                numFrames = Mathf.Clamp(numFrames - numFrames % 8, 16, 300);

                // Queue the Text to Motion solve
                RequestTextToMotionSolve(textToMotionAuthoring.RequestPrompt,
                    seed + textToMotionAuthoring.RequestTakesCounter,
                    null,
                    numFrames,
                    textToMotionAuthoring.RequestModel
                );

                textToMotionAuthoring.RequestTakesCounter++;
            }
        }

        void DoTextToMotionSolve(string prompt, int? seed, float? temperature, int length, ITimelineBakerTextToMotion.Model model)
        {
            var take = new TextToMotionTake(
                prompt,
                seed,
                LibraryRegistry.GetNewT2MTakeTitle(),
                prompt,
                temperature,
                length,
                model);

            // We want new T2M items to start with the default camera viewpoint, not the current working
            // camera viewpoint, but it can create some confusing thumbnails.
            var stage = new StageModel(Context.Stage);
            stage.RestoreDefaultCameraViewpoint();
            var newItem = ApplicationLibraryModel.AskAddLibraryItem(take, stage);

            if (newItem.Data.Model is TextToMotionTake itemTake)
            {
                DevLogger.LogInfo($"ApplicationModel -> DoTextToMotionSolve({itemTake}) -> Start a Text to Motion Request");
                Context.TextToMotionService.Request(take);
            }
        }

        void DoVideoToMotionGenerate(string videoPath, int startFrame, int frameCount)
        {
            var take = new VideoToMotionTake(LibraryRegistry.GetNewV2MTakeTitle(), videoPath, startFrame, frameCount);
            var request = Context.VideoToMotionService.CreateRequest(take, videoPath, startFrame, frameCount);
            take.TrackRequest(request);

            Common.Model.SendAnalytics(new VideoToMotionAnalytic(request.FilePath, request.StartFrame, request.FrameCount, request.Model));

            var newItem = ApplicationLibraryModel.AskAddLibraryItem(take, Context.Stage);
            Context.VideoToMotionService.QueueRequest(request);
        }

        void DoMotionToTimelineSolve(float sensitivity, bool useMotionCompletion)
        {
            Context.AuthorContext.TimelineContext.TimelineBakingLogic.Cancel();
            Context.AuthorContext.MotionToTimelineContext.OutputTimelineBaking.Cancel();
            Context.AuthorContext.MotionToTimelineContext.MotionToKeysSampling.Cancel();
            Context.AuthorContext.MotionToTimelineContext.MotionToKeysSampling.QueueBaking(sensitivity, useMotionCompletion);
        }

        void DoConvertMotionToKeys(DenseTake source)
        {
            source.BakedTimelineModel.CopyTo(Context.AuthorContext.MotionToTimelineContext.InputBakedTimeline);
            AuthoringModel.Mode = AuthoringModel.AuthoringMode.ConvertMotionToTimeline;
            Context.AuthorContext.MotionToTimelineContext.Model.Step = MotionToTimelineAuthoringModel.AuthoringStep.NoPreview;
        }

        void DoOpenSidePanelPage(SidePanelUtils.PageType type)
        {
            Context.SidePanelUIModel.SelectedPageIndex = (int)type;
        }



        // [Section] Request Methods

        internal void RequestTextToMotionGenerate(string prompt, int? seed, int takesAmount, float duration, ITimelineBakerTextToMotion.Model model)
        {
            DoTextToMotionGenerate(prompt, seed, takesAmount, duration, model);
        }

        void RequestTextToMotionSolve(string prompt, int? seed, float? temperature, int length, ITimelineBakerTextToMotion.Model model)
        {
            DoTextToMotionSolve(prompt, seed, temperature, length, model);
        }

        public void RequestMotionToTimelineSolve(float sensitivity, bool useMotionCompletion)
        {
            DoMotionToTimelineSolve(sensitivity, useMotionCompletion);
        }

        public void RequestVideoToMotionGenerate(string videoPath, int startFrame = -1, int frameCount = -1)
        {
            DoVideoToMotionGenerate(videoPath, startFrame, frameCount);
        }

        public void RequestConvertMotionToKeys(DenseTake target)
        {
            DoConvertMotionToKeys(target);
        }

        public void RequestSetPrompt(string prompt)
        {
            DoSetPrompt(prompt);
        }

        void DoSetPrompt(string prompt)
        {
            Context.TakesUIModel.Prompt = prompt;
        }

        public void RequestOpenSidePanelPage(SidePanelUtils.PageType page)
        {
            DoOpenSidePanelPage(page);
        }



        // [Section] UI Events Handlers

        // Takes UI Events Handlers

        void OnTakesUIRequestedGenerate()
        {
            RequestTextToMotionGenerate(Context.TakesUIModel.Prompt, Context.TakesUIModel.Seed, Context.TakesUIModel.TakesAmount, Context.TakesUIModel.Duration, Context.TakesUIModel.InferenceModel);
        }


    }
}
