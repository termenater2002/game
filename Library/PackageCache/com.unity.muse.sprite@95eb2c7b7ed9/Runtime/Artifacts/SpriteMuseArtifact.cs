using System;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Analytics;
using Unity.Muse.Sprite.Backend;
using Unity.Muse.Sprite.Common;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.Sprite.Common.DebugConfig;
using Unity.Muse.Sprite.Data;
using Unity.Muse.Sprite.Operators;
using Unity.Muse.Sprite.UIComponents;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.Muse.Sprite.Artifacts
{
    [Serializable]
    internal class SpriteMuseArtifact : Artifact<Texture2D>, IGenerateArtifact, IDisposable
    {
        public override bool isSafe => !m_UnsafeContent;

        public override string FileExtension => "png";
        GetJobRestCall m_GetJobRestCall;
        GetArtifactRestCall m_GetArtifactRestCall;
        SpriteGenerateRestCall m_SpriteGenerateCall;
        string m_JobStatus = string.Empty;
        ArtifactCreationDelegate m_ArtifactCreationDelegate;
        Model m_Model;
        bool m_Disposing = false;

        [SerializeField]
        bool m_UnsafeContent;

        public SpriteMuseArtifact(string guid, uint seed)
            : base(guid, seed)
        { }
        public SpriteMuseArtifact()
            : base(string.Empty, 0)
        { }


        public void Dispose()
        {
            m_Disposing = true;
            m_GetJobRestCall?.Dispose();
            m_GetArtifactRestCall?.Dispose();
            m_SpriteGenerateCall?.Dispose();
        }

        public string jobStatus => m_JobStatus;

        Texture2D spriteGeneratorErrorTexture => ResourceManager.Load<Texture2D>(PackageResources.generateErrorTexture);

        public override void GetArtifact(ArtifactCreationDelegate onReceived, bool useCache)
        {
            if (Guid.StartsWith("Error-"))
            {
                onReceived?.Invoke(spriteGeneratorErrorTexture, Array.Empty<byte>(), "Failed to generate artifact");
                return;
            }

            if (useCache && IsCached)
            {
                var artifact = ReadFromCache(out var rawData);
                onReceived?.Invoke(artifact, rawData, string.Empty);

                return;
            }

            m_ArtifactCreationDelegate += onReceived;

            var request = new ServerRequest<JobInfoRequest>();
            request.guid = Guid;
            request.data = new JobInfoRequest() { jobID = Guid, assetID = this.GetOperator<SessionOperator>().GetSessionID() };
            request.access_token = serverConfig.accessToken;
            m_GetJobRestCall = new GetJobRestCall(serverConfig, request);
            m_GetJobRestCall.RegisterOnSuccess(OnGetJobSuccess);
            m_GetJobRestCall.RegisterOnFailure(OnGetJobFailed);
            m_GetJobRestCall.SendRequest();
        }

        void OnGetJobFailed(GetJobRestCall obj)
        {
            if (m_GetJobRestCall.retriesFailed)
            {
                Debug.LogError($"TextToSpriteController.OnGetJobFailed: Result: {obj.requestResult} Error:{obj.requestError} Retry {m_GetJobRestCall.retries} Endpoint {obj.info}");
                NotifyErrorGettingArtifact();
            }
            else if (m_Disposing)
                m_GetJobRestCall.maxRetries = 0;
        }

        static ServerConfig serverConfig => ServerConfig.serverConfig;

        void OnGetJobSuccess(GetJobRestCall arg1, JobInfoResponse arg2)
        {
            m_JobStatus = arg2.status;
            if (arg2.status == SpriteGenerateRestCall.Status.completed)
            {
                m_GetArtifactRestCall = new GetArtifactRestCall(serverConfig, Guid);
                m_GetArtifactRestCall.RegisterOnSuccess(OnGetArtifactSuccess);
                m_GetArtifactRestCall.RegisterOnFailure(OnGetArtifactFailed);
                m_GetArtifactRestCall.SendRequest();
            }
            else if (arg2.status == SpriteGenerateRestCall.Status.failed)
            {
                Debug.LogError($"TextToSpriteController.OnGetJobFailed: Result: {arg1.requestResult} Error:{arg1.requestError} Retry {m_GetJobRestCall.retries} Endpoint: {arg1.info}");
                NotifyErrorGettingArtifact();
            }
            else if (!m_Disposing)
                Scheduler.ScheduleCallback(serverConfig.webRequestPollRate, () => m_GetJobRestCall.SendRequest());
        }

        void OnGetArtifactFailed(GetArtifactRestCall request)
        {
            if (m_GetArtifactRestCall.retriesFailed)
            {
                Debug.LogError($"TextToSpriteController.OnGetArtifactFailed: Result: {request.requestResult} Error:{request.requestError} Retry {m_GetArtifactRestCall.retries} Endpoint: {request.info}");
                NotifyErrorGettingArtifact();
            }
            else if (m_Disposing)
                m_GetArtifactRestCall.maxRetries = 0;
        }

        void OnGetArtifactSuccess(GetArtifactRestCall request, byte[] bytes)
        {
            var deserializedArtifact = CreateFromData(bytes, true);

            if (m_UnsafeContent)
                m_ArtifactCreationDelegate?.Invoke(deserializedArtifact, Array.Empty<byte>(), "Potentially inappropriate content was detected.");
            else
                m_ArtifactCreationDelegate?.Invoke(deserializedArtifact, bytes, string.Empty);
            m_ArtifactCreationDelegate = null;
        }

        void NotifyErrorGettingArtifact()
        {
            m_ArtifactCreationDelegate?.Invoke(spriteGeneratorErrorTexture, Array.Empty<byte>(), "Failed to generate artifact");
        }

        public override Texture2D ConstructFromData(byte[] data)
        {
            Texture2D tex = TextureUtils.Create();
            tex.LoadImage(data);
            tex.Apply();

            return tex;
        }

        /// <inheritdoc cref="Artifact{T}"/>
        protected override Texture2D CreateFromData(byte[] data, bool updateCache)
        {
            Texture2D tex = ConstructFromData(data);

            var hasDifferentPixels = false;
            var pixels = tex.GetPixels();
            var pixel = pixels[0];
            for (var i = 1; i < pixels.Length; i++)
            {
                if (pixels[i] != pixel)
                {
                    hasDifferentPixels = true;
                    break;
                }
            }

            m_UnsafeContent = !hasDifferentPixels;
            if (m_UnsafeContent)
                return null;

            if (updateCache)
            {
                WriteToCache(data);
            }

            return tex;
        }

        protected override void WriteToCache(byte[] value)
        {
            ArtifactCache.Write(this, value);
        }

        protected override Texture2D ReadFromCache(out byte[] rawData)
        {
            if (ArtifactCache.Read(this) is Texture2D tex)
            {
                rawData = ArtifactCache.ReadRawData(this);
                tex.hideFlags = HideFlags.HideAndDontSave;
                return tex;
            }

            rawData = null;
            return null;
        }

        protected override byte[] ReadFromCacheRaw()
        {
            ReadFromCache(out var raw);
            return raw;
        }

        public override void GetPreview(ArtifactPreviewDelegate onDoneCallback, bool useCache)
        {
            GetArtifact((instance, data, message) =>
            {
                onDoneCallback?.Invoke(instance, data, message);
            }, true);
        }

        /// <summary>
        /// Only called once for a generation group
        /// </summary>
        /// <param name="model">Model used.</param>
        public override void StartGenerate(Model model)
        {
            try
            {
                m_Model = model;
                Model.SendAnalytics(new GenerateAnalytic(
                    this.GetOperator<PromptOperator>()?.GetPrompt(),
                    this.GetOperator<PromptOperator>()?.GetNegativePrompt(),
                    this.GetOperator<SpriteRefiningMaskOperator>()?.Enabled() ?? false,
                    m_Operators.GetOperator<GenerateOperator>()?.GetCount() ?? 0,
                    m_Operators.GetOperator<KeyImageOperator>()?.HasReference() ?? false,
                    m_Operators.GetOperator<KeyImageOperator>()?.HasDoodle() ?? false
                ));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public override void Generate(Model model)
        {
            m_Model = model;
            if (model.isRefineMode)
            {
                var refineArtifact = new SpriteRefinerArtifact();
                refineArtifact.SetOperators(GetOperators());
                refineArtifact.RefineSprite(model, RefineSpriteDone);
                SetOperators(refineArtifact.GetOperators());
            }
            else
            {
                Generate(model.GetData<GenerateCountData>());
            }
        }

        public override void RetryGenerate(Model model)
        {
            if (m_UnsafeContent)
            {
                Seed = (uint)UnityEngine.Random.Range(0, 65535);
                m_UnsafeContent = false;
            }
            Generate(model);
        }

        void RefineSpriteDone(string id, uint seed)
        {
            Guid = id;
            var sgo = m_Operators.GetOperator<SpriteGeneratorSettingsOperator>();
            sgo.jobID = id;
            Seed = seed;
            OnGenerationDone?.Invoke(this, null);
        }

        public void Generate(GenerateCountData generateCountData)
        {
            var request = new GeneratorRequest();
            request.image_count = 1;
            var operators = GetOperators();
            SpriteGeneratorSettingsOperator spriteGenerationOperator = null;
            string sessionId = "dummy-session-id";
            for (int i = 0; i < operators.Count; ++i)
            {
                if (operators[i] is PromptOperator promptOperator)
                {
                    request.prompt = promptOperator.GetPrompt();
                    request.settings.negative_prompt = promptOperator.GetNegativePrompt();
                }
                else if (operators[i] is SpriteGeneratorSettingsOperator sgo)
                {
                    spriteGenerationOperator = sgo;
                    var checkPoint = spriteGenerationOperator.GetSelectedStyleCheckpointGuid();
                    request.checkpoint_id = checkPoint.guid;
                    if (spriteGenerationOperator.seedUserSpecified)
                        spriteGenerationOperator.SetSeed(spriteGenerationOperator.seed + generateCountData.GetAndIncrementCount());
                    else
                        spriteGenerationOperator.RandomSeed();
                    spriteGenerationOperator.SetCheckPointUsed(checkPoint);
                    request.removeBackground = spriteGenerationOperator.removeBackground;
                    request.styleStrength = spriteGenerationOperator.styleStrength;
                    var imageSize = spriteGenerationOperator.imageSize;
                    request.settings.width = imageSize.x;
                    request.settings.height = imageSize.y;
                    request.settings.seed = spriteGenerationOperator.seed;
                    request.settings.model = serverConfig.model;
                    request.settings.strength = spriteGenerationOperator.styleStrength;
                    request.settings.seamless = false;
                    spriteGenerationOperator.Enable(true);
                }
                else if (operators[i] is KeyImageOperator keyImageOperator)
                {
                    request.scribble = !keyImageOperator.isClear && string.IsNullOrEmpty(keyImageOperator.GetReferenceImage());
                    var srcImage = GetKeyImageOperatorTexture(keyImageOperator);
                    request.maskStrength = keyImageOperator.maskStrength;
                    if (srcImage != null)
                    {
                        var raw = srcImage.EncodeToPNG();
                        request.base64Image = Convert.ToBase64String(raw);
                        DebugConfig.DebugGenerateImage(raw);
                    }
                    keyImageOperator.Enable(true);
                }
                else if (operators[i] is SpriteRefiningMaskOperator rmo)
                {
                    rmo.refined = false;
                }
                else if (operators[i] is SessionOperator sessionOperator)
                {
                    sessionId = sessionOperator.GetSessionID();
                }

            }

            request.simulate = serverConfig.simulate;
            if (spriteGenerationOperator != null)
            {
                if (string.IsNullOrEmpty(request.base64Image))
                    m_SpriteGenerateCall = new SpriteGenerateRestCall(serverConfig, request, sessionId);
                else
                {
                    if (request.scribble)
                        m_SpriteGenerateCall = new SpriteScribbleRestCall(serverConfig, request, sessionId);
                    else
                        m_SpriteGenerateCall = new SpriteVariantRestCall(serverConfig, request, sessionId);
                }

                m_SpriteGenerateCall.RegisterOnSuccess(OnSpriteGenerateSuccess);
                m_SpriteGenerateCall.RegisterOnFailure(OnSpriteGenerateFailed);
                m_SpriteGenerateCall.SendRequest();
            }
            else
            {
                Debug.LogError($"Unable to generate. SpriteGeneration operator not found");
            }
        }

        public override ArtifactView CreateView()
        {
            return new SpriteMuseArtifactResultView(this);
        }

        public override ArtifactView CreateCanvasView()
        {
            return new ResultItemVisualElement(this);
        }

        Texture2D GetKeyImageOperatorTexture(KeyImageOperator opr)
        {
            Texture2D keyImageTexture = null;
            var referenceImageRaw = opr.GetReferenceImage();
            var doodleRaw = opr.GetDoodle();
            Texture2D referenceImage = null;
            if (!string.IsNullOrEmpty(referenceImageRaw))
            {
                referenceImage = new Texture2D(2, 2);
                referenceImage.LoadImage(Convert.FromBase64String(referenceImageRaw));
                referenceImage.Apply();
            }

            Texture2D doodleImage = null;
            if (!string.IsNullOrEmpty(doodleRaw) && !opr.isClear)
            {
                doodleImage = new Texture2D(2, 2);
                doodleImage.LoadImage(Convert.FromBase64String(doodleRaw));
                doodleImage.Apply();
            }

            if (referenceImage != null)
            {
                var referenceImagePixels = referenceImage.GetPixels32(0);
                for (var j = 0; j < referenceImage.width * referenceImage.height; ++j)
                    referenceImagePixels[j] = (referenceImagePixels[j].a < 1) ? new Color32(0, 0, 0, 255) : referenceImagePixels[j];

                keyImageTexture = new Texture2D(referenceImage.width, referenceImage.height, TextureFormat.RGBA32, false);
                keyImageTexture.SetPixels32(referenceImagePixels);
                keyImageTexture.Apply();
            }
            else if (doodleImage != null)
            {
                keyImageTexture = doodleImage;
            }

            return keyImageTexture;
        }

        void OnSpriteGenerateSuccess(SpriteGenerateRestCall arg1, GenerateResponse arg2)
        {
            var operators = GetOperators();
            for (int i = 0; i < operators.Count; ++i)
            {
                if (operators[i] is SpriteGeneratorSettingsOperator sgo)
                {
                    sgo.jobID = arg2.jobID;
                    break;
                }
            }

            Guid = arg2.jobID;
            Seed = (uint)arg1.request.settings.seed;
            OnGenerationDone?.Invoke(this, null);
        }

        void OnSpriteGenerateFailed(SpriteGenerateRestCall obj)
        {
            if (m_SpriteGenerateCall.retries == 1 && obj.requestError.Contains("HTTP/1.1 403 Forbidden") && m_Model != null)
            {
                m_Model.ServerError(obj.responseCode, obj.requestError);
            }

            if (m_SpriteGenerateCall.retriesFailed)
            {
                Debug.LogError($"TextToSpriteController.OnSpriteGenerateFailed: Result: {obj.requestResult} Error:{obj.requestError} Retry:{m_SpriteGenerateCall.retries} {obj.errorMessage}");
                Guid = "Error-" + System.Guid.NewGuid().ToString();
                OnGenerationDone?.Invoke(this, obj.requestError);
            }
            else if (m_Disposing)
                m_SpriteGenerateCall.maxRetries = 0;
        }

        public void SetOperators(JobInfoResponse jobInfoResponse)
        {
            //todo putting jobInfoResponse back to operatorData
        }

        public void Generate(string prompt, TextToImageRequest settings, Action<TextToImageResponse, string> onDone)
        {
            throw new NotImplementedException();
        }
    }
}
