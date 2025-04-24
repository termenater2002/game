using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.Sprite.Data;
using Unity.Muse.Sprite.Operators;
using UnityEngine;

namespace Unity.Muse.Sprite.UIMode
{
    class UIMode : IUIMode
    {
        public const string modeKey = "TextToSprite";

        MainUI m_MainUI;
        Model m_Model;

        SpriteRefiningMaskOperator m_MaskOperator;
        string m_Prompt;
        bool m_HasMask;

        public void Activate(MainUI mainUI, string modeKey1)
        {
            m_MainUI = mainUI;
            m_Model = m_MainUI.model;

            AddListeners();

            m_Model.GetData<DefaultStyleData>().Reset();

            UpdateMaskOperator();
        }

        void UpdateMaskOperator()
        {
            if (m_MaskOperator != null)
                m_MaskOperator.onMaskUpdated -= OnMaskUpdated;

            m_HasMask = false;
            m_MaskOperator = m_Model.CurrentOperators.GetOperator<SpriteRefiningMaskOperator>();
            if (m_MaskOperator != null)
            {
                m_HasMask = !m_MaskOperator.IsClear();
                m_MaskOperator.onMaskUpdated += OnMaskUpdated;
            }
            else
            {
                m_HasMask = false;
            }

            UpdateEnableGeneration();
        }

        public void Deactivate()
        {
            RemoveListeners();

            m_Model.GetData<DefaultStyleData>().Reset();
        }

        void AddListeners()
        {
            m_Model.OnCurrentPromptChanged += OnPromptChanged;
            m_Model.OnOperatorUpdated += OnOperatorUpdated;
            m_Model.OnGenerateButtonClicked += OnGenerateButtonClicked;
            m_Model.OnSetOperatorDefaults += OnSetOperatorDefault;
            m_Model.GetData<FeedbackManager>().OnDislike += OnDislike;
        }

        void RemoveListeners()
        {
            m_Model.OnCurrentPromptChanged -= OnPromptChanged;
            m_Model.OnOperatorUpdated -= OnOperatorUpdated;
            m_Model.OnGenerateButtonClicked -= OnGenerateButtonClicked;
            m_Model.OnSetOperatorDefaults -= OnSetOperatorDefault;
            m_Model.GetData<FeedbackManager>().OnDislike -= OnDislike;
        }

        void OnOperatorUpdated(IEnumerable<IOperator> operators, bool set)
        {
            UpdateMaskOperator();
        }

        void OnGenerateButtonClicked()
        {
            var countData = m_Model.GetData<GenerateCountData>();
            countData.ResetCounter();
        }

        void OnMaskUpdated()
        {
            m_HasMask = !m_MaskOperator?.IsClear() ?? false;

            UpdateEnableGeneration();
        }

        IEnumerable<IOperator> OnSetOperatorDefault(IEnumerable<IOperator> currentOperators)
        {
            if (m_Model.isRefineMode)
            {
                // Keep these operators from the original artifact as they are not displayed but still needed for refinement requests.
                currentOperators = currentOperators.Select(op =>
                {
                    var newOp = op switch
                    {
                        SpriteGeneratorSettingsOperator => m_Model.SelectedArtifact?.GetOperator<SpriteGeneratorSettingsOperator>().Clone() ?? op,
                        SessionOperator => m_Model.SelectedArtifact?.GetOperator<SessionOperator>().Clone() ?? op,
                        _ => op
                    };

                    return newOp;
                }).Where(op => op is not KeyImageOperator);
            }

            return currentOperators;
        }

        void OnDislike(Artifact artifact)
        {
            var feedbackData = new FeedbackData
            {
                version = FeedbackData.currentVersion,
                disliked = m_Model.GetData<FeedbackManager>().IsDisliked(artifact)
            };

            var requestData = new FeedbackRequest
            {
                guid = artifact.Guid,
                feedback_flags = feedbackData.GetFlags(),
                feedback_comment = feedbackData.ToString()
            };

            var request = new SubmitFeedbackRestCall(ServerConfig.serverConfig, requestData);
            request.SendRequest();
        }

        void OnPromptChanged(string prompt)
        {
            m_Prompt = prompt;

            UpdateEnableGeneration();
        }

        void UpdateEnableGeneration()
        {
            string tooltip;
            var hasValidPrompt = !string.IsNullOrWhiteSpace(m_Prompt) && m_Prompt.Length >= PromptOperator.MinimumPromptLength;

            var canGenerate = hasValidPrompt;
            if (m_Model.isRefineMode)
            {
                if (hasValidPrompt)
                    tooltip = m_HasMask ? null : TextContent.generateButtonPaintMaskTooltip;
                else
                    tooltip = m_HasMask ? TextContent.generateButtonEnterPromptTooltip : TextContent.generateButtonEnterPromptAndPaintMaskTooltip;

                canGenerate &= m_HasMask;
            }
            else
            {
                tooltip = hasValidPrompt ? null : TextContent.generateButtonEnterPromptTooltip;
            }

            m_Model.GetData<GenerateButtonData>().SetGenerateButtonData(canGenerate, tooltip);
        }
    }
}
