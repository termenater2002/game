using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Services
{
    static class GenerationService
    {
        public static void GenerateImage(string text,
            Action<GenerationResult> callback,
            ImageModel imageModel = ImageModel.Texture,
            Action<Model> onModelCreated = null)
        {
            var modeIndex = (int) imageModel;
            if (imageModel == ImageModel.Texture)
                modeIndex = ModesFactory.GetModeIndexFromKey("TextToImage");
            else if (imageModel == ImageModel.Sprite)
                modeIndex = ModesFactory.GetModeIndexFromKey("TextToSprite");

            var assetPath = $"Assets/Muse Chat {imageModel}.asset";
            var model = AssetDatabase.LoadAssetAtPath<Model>(assetPath);
            if (model is null)
            {
                model = ScriptableObject.CreateInstance<Model>();
                AssetDatabase.CreateAsset(model, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                model.Initialize();
                model.ModeChanged(modeIndex);
            }

            var operators = model.SetOperatorDefaults();
            var promptOperator = operators.GetOperator<PromptOperator>();
            promptOperator.SetPrompt(text);

            var generateOperator = operators.GetOperator<GenerateOperator>();
            generateOperator?.SetSettings(new List<string> {generateOperator.GetSettings()[0], "1"});

            // Since logic is often coupled with UI, generating UI is necessary to get the correct settings
            foreach (var op in operators)
            {
                op.GetOperatorView(model);
            }

            model.OnArtifactAdded += artifact =>
            {
                artifact.OnGenerationDone += (_, _) =>
                {
                    artifact.GetPreview((texture, _, _) =>
                    {
                        callback(new GenerationResult
                        {
                            image = texture,
                            model = model
                        });
                    }, true);
                };
            };

            GenerateImage(model, operators, modeIndex);
            onModelCreated?.Invoke(model);
        }

        public static void GenerateImage(Model model, IEnumerable<IOperator> operators, int mode)
        {
            var promptOperator = operators.GetOperator<PromptOperator>();
            var referenceOperator = operators.GetOperator<ReferenceOperator>();
            var isReferenceOperatorEnabled = referenceOperator != null && referenceOperator.Enabled();
            var isShape = isReferenceOperatorEnabled &&
                referenceOperator.GetSettingTex(ReferenceOperator.Setting.Image) &&
                referenceOperator.GetSettingEnum<ReferenceOperator.Mode>(ReferenceOperator.Setting.Mode) ==
                ReferenceOperator.Mode.Shape;
            var isVariation = IsVariation(operators);

            if (!promptOperator.IsPromptValid())
                return;

            var generationOperators = operators.Select(x => x.Clone()).ToList();

            var modeType = ModesFactory.GetModeKeyFromIndex(mode);
            var groupArtifact = ArtifactFactory.CreateArtifact(modeType);
            groupArtifact.ProcessArtifactOperations(modeType, generationOperators, model, isVariation, isShape);

            // Cancel inpainting mode after generation settings have been sent, otherwise the inpainting mask would not be part of the generation
            model.SetActiveTool(null);
        }

        public static bool IsVariation(IEnumerable<IOperator> operators)
        {
            var referenceOperator = operators.GetOperator<ReferenceOperator>();
            var isReferenceOperatorEnabled = referenceOperator != null && referenceOperator.Enabled();
            var isColorMode = isReferenceOperatorEnabled &&
                referenceOperator.GetSettingEnum<ReferenceOperator.Mode>(ReferenceOperator.Setting.Mode) == ReferenceOperator.Mode.Color;
            var isVariationFromArtifact =
                isColorMode && !string.IsNullOrEmpty(referenceOperator.GetSettingString(ReferenceOperator.Setting.Guid));
            var isVariationFromTexture = isColorMode && referenceOperator.GetSettingTex(ReferenceOperator.Setting.Image);

            return isVariationFromArtifact || isVariationFromTexture;
        }
    }
}
