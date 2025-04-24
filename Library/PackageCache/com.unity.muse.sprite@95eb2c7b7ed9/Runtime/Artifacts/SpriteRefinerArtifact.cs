using System;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.Sprite.Common.DebugConfig;
using Unity.Muse.Sprite.Data;
using Unity.Muse.Sprite.Operators;
using Unity.Muse.Sprite.UIComponents;
using UnityEngine;

namespace Unity.Muse.Sprite.Artifacts
{
    internal class SpriteRefinerArtifact : SpriteMuseArtifact
    {
        public override ArtifactView CreateView()
        {
            return new ResultItemVisualElement(this);
        }

        public void RefineSprite(Model model, Action<string, uint> onDone)
        {
            var request = new SpriteRefinerRequest();
            request.image_count = 1;
            var operators = GetOperators();
            SpriteGeneratorSettingsOperator spriteGenerationOperator = null;
            SpriteRefiningMaskOperator refineMaskOperator = null;
            var counter = model.GetData<GenerateCountData>();
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
                    var checkPointUsed = spriteGenerationOperator.GetSelectedStyleCheckpointGuid();
                    request.checkpoint_id = checkPointUsed.guid;
                    if (spriteGenerationOperator.seedUserSpecified)
                        spriteGenerationOperator.SetSeed(spriteGenerationOperator.seed + counter.GetAndIncrementCount());
                    else
                        spriteGenerationOperator.RandomSeed();
                    spriteGenerationOperator.SetCheckPointUsed(checkPointUsed);
                    spriteGenerationOperator.seedUserSpecified = false;
                    request.removeBackground = spriteGenerationOperator.removeBackground ? 1 : 0;
                    request.maskStrength = 1;
                    request.settings.strength = spriteGenerationOperator.styleStrength;
                    var imageSize = spriteGenerationOperator.imageSize;
                    request.settings.width = imageSize.x;
                    request.settings.height = imageSize.y;
                    request.settings.seed = spriteGenerationOperator.seed;
                    request.scribble = 0;
                    request.settings.seamless = false;
                    Guid = sgo.jobID;
                    sgo.artifactID = string.Empty;
                    sgo.Enable(true);
                }
                else if(operators[i] is KeyImageOperator keyImageOperator)
                {
                    keyImageOperator.Enable(true);
                }
                else if (operators[i] is SpriteRefiningMaskOperator rmo)
                {
                    refineMaskOperator = rmo;
                    refineMaskOperator.refined = true;
                    var doodle = refineMaskOperator.GetMask();
                    if (doodle.Length > 0)
                    {
                        var texture2D = new Texture2D(2,2);
                        var bytes = Convert.FromBase64String(doodle);
                        texture2D.LoadImage(bytes);
                        texture2D.Apply();
                        var raw = texture2D.EncodeToPNG();
                        DebugConfig.DebugRefineDoodleImage(raw);
                        //BackendUtilities.SaveTexture2DToFile("Assets/maskDoodle.png", texture2D);
                        request.mask64Image = Convert.ToBase64String(raw);
                    }
                }
                else if (operators[i] is SessionOperator sessionOperator)
                {
                    sessionId = sessionOperator.GetSessionID();
                }
            }

            if (spriteGenerationOperator != null && refineMaskOperator != null)
            {
                refineMaskOperator.sourceJobID = spriteGenerationOperator.jobID;
                refineMaskOperator.sourceArtifactID = spriteGenerationOperator.artifactID;
                GetArtifact((x,y,z) => GetSourceImageDone(x,y,z, request, onDone, sessionId), true);
            }
            else
            {
                Debug.LogError($"Unable to refine. One of the operators is not found. spriteGenerationOperator: {spriteGenerationOperator} refineMaskOperator: {refineMaskOperator}");
            }
        }

        static ServerConfig serverConfig => ServerConfig.serverConfig;

        void GetSourceImageDone(Texture2D artifactinstance, byte[] rawdata, string errormessage, SpriteRefinerRequest request, Action<string, uint> onDone, string sessionid)
        {
            request.simulate = serverConfig.simulate;
            DebugConfig.DebugRefineSrcImage(rawdata);
            //BackendUtilities.SaveBytesToFile("Assets/srcImage.png", rawdata);
            request.base64Image = Convert.ToBase64String(rawdata);
            if(string.IsNullOrEmpty(request.mask64Image))
            {
                Debug.LogWarning("Refine without mask");
                request.mask64Image = request.base64Image;
            }
            var generateCall = new SpriteRefineRestCall(serverConfig, request, sessionid);
            generateCall.maxRetries = 0;
            generateCall.RegisterOnSuccess((restCall, response) => OnRefineSpriteSuccess(restCall, response, onDone));
            generateCall.RegisterOnFailure(obj => OnRefineSpriteFailed(obj, onDone));
            generateCall.SendRequest();
        }

        void OnRefineSpriteSuccess(SpriteRefineRestCall arg1, GenerateResponse arg2, Action<string, uint> onDone)
        {
            //Debug.Log($"TextToSpriteController.OnSpriteGenerateSuccess {arg2.jobID}");
            Guid = arg2.jobID;
            Seed = (uint)arg1.request.settings.seed;
            //OnGenerationDone?.Invoke();
            onDone?.Invoke(Guid, Seed);
        }

        void OnRefineSpriteFailed(SpriteRefineRestCall obj, Action<string, uint> onDone)
        {
            Debug.LogError($"TextToSpriteController.OnRefineSpriteFailed:  Result: {obj.requestResult} Error:{obj.requestError} ");
            Guid = "Error-" + System.Guid.NewGuid().ToString();
            onDone?.Invoke(Guid, Seed);
        }

        public override Artifact Clone(string mode)
        {
            var artifact = new SpriteRefinerArtifact();
            artifact.Guid = Guid;
            artifact.Seed = Seed;

            artifact.SetOperators(m_Operators);

            return artifact;
        }
    }
}