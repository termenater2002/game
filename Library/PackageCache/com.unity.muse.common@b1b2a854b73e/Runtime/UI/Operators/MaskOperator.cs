using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;
using Text = Unity.Muse.AppUI.UI.Text;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class MaskOperator : IMaskOperator
    {
        public string OperatorName => "MaskOperator";

        /// <summary>
        /// Human-readable label for the operator.
        /// </summary>
        public string Label => "Mask Image";

        [SerializeField]
        OperatorData m_OperatorData;

        const int k_MaskIndex = 0;
        const int k_SeamlessIndex = 1;
        const int k_ClearIndex = 2;

        event Action OnDataUpdate;

        public Texture2D GetMaskTexture()
        {
            var b64String = m_OperatorData.settings[k_MaskIndex];
            var bytes = Convert.FromBase64String(b64String);
            var maskTexture = TextureUtils.Create();
            maskTexture.LoadImage(bytes);
            return maskTexture;
        }

        public string GetMask()
        {
            return m_OperatorData.settings[k_MaskIndex];
        }

        public bool IsClear()
        {
            return m_OperatorData.settings[k_ClearIndex] == "1";
        }

        public MaskOperator()
        {
            m_OperatorData = new OperatorData("MaskOperator", "0.0.1", new[] { "", "True", "1" }, false);
        }

        public bool IsSavable()
        {
            return true;
        }

        public bool GetSeamless()
        {
            return bool.Parse(m_OperatorData.settings[k_SeamlessIndex]);
        }

        public VisualElement GetCanvasView()
        {
            Debug.Log("MaskOperator.GetCanvasView()");
            return new VisualElement();
        }

        public VisualElement GetOperatorView(Model model)
        {
            var UI = new ExVisualElement
            {
                passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows | ExVisualElement.Passes.BackgroundColor
            };
            UI.AddToClassList("muse-node");
            UI.AddToClassList("appui-elevation-8");
            UI.name = "mask-node";

            //title
            var text = new Text();
            text.text = Label;
            text.AddToClassList("muse-node__title");
            text.AddToClassList("bottom-gap");
            UI.Add(text);

            //Probably need to create a new class for Mask
            var image = GetImageUI();

            var imageText = new Text();
            imageText.text = "No Mask";
            imageText.AddToClassList("muse-ref-image__text");
            UpdateMaskImage(image, imageText);

            image.Add(imageText);
            UI.Add(image);

            m_OperatorData.settings[k_SeamlessIndex] = "True";

            OnDataUpdate += () =>
            {
                UpdateMaskImage(image, imageText);
            };

            return UI;
        }

        void UpdateMaskImage(Image image, Text imageText)
        {
            if (m_OperatorData.settings[k_MaskIndex] != "" && m_OperatorData.settings[k_ClearIndex] == "0")
            {
                image.image = GetMaskTexture();
                imageText.text = "";
            }
            else if (m_OperatorData.settings[k_ClearIndex] == "1") // If clear, show no mask
            {
                image.image = null;
                imageText.text = "No Mask";
            }
        }

        Image GetImageUI()
        {
            var image = new Image();
            image.AddToClassList("muse-ref-image");
            image.name = "muse-reference-image-field";

            if (m_OperatorData.settings[k_MaskIndex] != "")
                image.image = GetMaskTexture();

            image.AddToClassList("bottom-gap");

            return image;
        }

        public OperatorData GetOperatorData()
        {
            return m_OperatorData;
        }

        public void SetOperatorData(OperatorData data)
        {
            m_OperatorData.enabled = data.enabled;
            if (data.settings == null || data.settings.Length < 2)
                return;
            m_OperatorData.settings = data.settings;
            OnDataUpdate?.Invoke();
        }

        void SetSettings(IReadOnlyList<string> settings)
        {
            m_OperatorData.settings[k_MaskIndex] = settings[k_MaskIndex];
            m_OperatorData.settings[k_SeamlessIndex] = settings[k_SeamlessIndex];
            m_OperatorData.settings[k_ClearIndex] = settings[k_ClearIndex];
            OnDataUpdate?.Invoke();
        }

        string[] GetSettings()
        {
            return new[] { m_OperatorData.settings[k_MaskIndex], m_OperatorData.settings[k_SeamlessIndex], m_OperatorData.settings[k_ClearIndex] };
        }

        public bool Enabled()
        {
            return m_OperatorData.enabled;
        }

        public void Enable(bool enable)
        {
            m_OperatorData.enabled = enable;
        }

        public bool Hidden { get; set; }

        public IOperator Clone()
        {
            var result = new MaskOperator();
            var operatorData = new OperatorData();
            operatorData.FromJson(GetOperatorData().ToJson());
            result.SetOperatorData(operatorData);
            return result;
        }

        static TextureFormat FindEquivalentTextureFormat(GraphicsFormat graphicsFormat)
        {
            // Perform mapping to find equivalent TextureFormat
            return graphicsFormat switch
            {
                GraphicsFormat.R8_UNorm => TextureFormat.R8,
                GraphicsFormat.R8G8B8A8_UNorm => TextureFormat.RGBA32,
                _ => TextureFormat.RGBA32
            };
        }

        void OnMaskPaintDone(Texture2D texture, bool isClear)
        {
            m_OperatorData.settings[k_ClearIndex] = isClear ? "1" : "0";
            if (isClear)
            {
                m_OperatorData.settings[k_MaskIndex] = "";
            }
            else
            {
                m_OperatorData.settings[k_MaskIndex] = Convert.ToBase64String(texture.EncodeToPNG());
            }

            OnDataUpdate?.Invoke();
        }

        public void RegisterToEvents(Model model)
        {
            if (!model.CurrentOperators.Contains(this))
                return; // Only register to paint event for the current operator and not the selected artifact's operator

            model.OnMaskPaintDone += OnMaskPaintDone;
        }

        public void UnregisterFromEvents(Model model)
        {
            model.OnMaskPaintDone -= OnMaskPaintDone;
        }

        /// <summary>
        /// Get the settings view for this operator.
        /// </summary>
        /// <param name="model">Current Model</param>
        /// <param name="isCustomSection">This VisualElement will override the whole operator section used by the generation settings</param>
        /// <param name="dismissAction">Action to trigger on dismiss</param>
        /// <returns> UI for the operator. Set to Null if the operator should not be displayed in the settings view. Disable the returned VisualElement if you want it to be displayed but not usable.</returns>
        public VisualElement GetSettingsView(Model model, ref bool isCustomSection, Action dismissAction)
        {
            if (string.IsNullOrEmpty(m_OperatorData.settings[k_MaskIndex]))
                return null;

            var ui = GetImageUI();

            if(!model.isRefineMode)
            {
                ui.userData = GenerationSettingsView.HideUse;
            }

            return ui;
        }

        public bool EvaluateAddOperator(Model model)
        {
            return model.isRefineMode;
        }

        //When removed from an operator's list, we want to enable the reference operator that was disabled when this operator was added
        public void OnOperatorRemoved(List<IOperator> removedInOperators)
        {
            var referenceOps = removedInOperators.FindAll(x => x is ReferenceOperator);
            foreach (var referenceOperator in referenceOps)
            {
                referenceOperator.Enable(true);
            }
        }
    }
}