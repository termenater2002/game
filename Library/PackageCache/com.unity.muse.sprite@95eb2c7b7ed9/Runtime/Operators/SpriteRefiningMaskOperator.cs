using System;
using System.Collections.Generic;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.Sprite.Data;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.Muse.Sprite.Operators
{
    [Preserve]
    [Serializable]
    internal class SpriteRefiningMaskOperator : IMaskOperator
    {
        enum ESettings
        {
            Mask,
            SourceJobID,
            SourceArtifactID,
            Refined,
            IsMaskClear
        }

        Model m_Model;
        OperatorOverridePublishers m_Publishers;

        public SpriteRefiningMaskOperator()
        {
            m_OperatorData = new OperatorData(OperatorName, "Unity.Muse.Sprite", "0.0.1", new[] { "", "", "", "", "1" }, false);
        }

        public Action onMaskUpdated;

        OperatorData m_OperatorData;
        public const string baseUssClassName = "appui-sprite-refining-mask";
        public string OperatorName => "Unity.Muse.Sprite.Operators.SpriteRefiningMaskOperator";

        /// <summary>
        /// Human-readable label for the operator.
        /// </summary>
        public string Label => "Sprite Refining Mask";

        public bool Enabled() => m_OperatorData.enabled;

        public void Enable(bool enable) => m_OperatorData.enabled = enable;
        public bool Hidden { get; set; }

        public VisualElement GetCanvasView() => new VisualElement();

        public VisualElement GetOperatorView(Model model) => new VisualElement();

        public OperatorData GetOperatorData() => m_OperatorData;

        public void SetOperatorData(OperatorData data)
        {
            m_OperatorData.enabled = data.enabled;
            if (data.settings == null || data.settings.Length == 0)
                return;
            for (int i = 0; i < m_OperatorData.settings.Length && i < data.settings.Length; i++)
                m_OperatorData.settings[i] = data.settings[i];
        }

        public IOperator Clone()
        {
            var result = new SpriteRefiningMaskOperator();
            var operatorData = new OperatorData();
            operatorData.FromJson(GetOperatorData().ToJson());
            result.SetOperatorData(operatorData);
            return result;
        }

        public void RegisterToEvents(Model model)
        {
            if (!model.CurrentOperators.Contains(this))
                return; // Only register to paint event for the current operator and not the selected artifact's operator

            m_Model = model;
            if (Enabled())
            {
                model.OnMaskPaintDone += OnMaskPaintDone;
            }
            RegisterToOverrideEvents();
        }

        void UnregisterFromOverrideEvents()
        {
            if (m_Publishers != null)
            {
                m_Publishers.UnregisterFromPublisher<OperatorMaskImageOverride>(OnMaskImageOverride);
                m_Publishers.OnModified -= OnPublishersModified;
            }
        }

        public void RegisterToOverrideEvents()
        {
            var dataPublisher = m_Model.GetData<OperatorOverridePublishers>();
            UnregisterFromOverrideEvents();

            m_Publishers = dataPublisher;
            var havePublisher = m_Publishers != null;
            bool hasOverride = PublisherHasOverrides();
            if (havePublisher)
            {
                m_Publishers.OnModified += OnPublishersModified;
                if (hasOverride)
                {
                    m_Publishers.RegisterToPublisher<OperatorMaskImageOverride>(OnMaskImageOverride);
                    OnMaskImageOverride(m_Publishers.RequestCurrentPublisherData<OperatorMaskImageOverride>());
                }
            }
        }

        void OnMaskImageOverride(OperatorMaskImageOverride arg0)
        {
            if (Enabled())
            {
                var haveData = arg0.bytes != null && arg0.bytes.Length != 0;
                refined = haveData;
                SetMask(haveData ? Convert.ToBase64String(arg0.bytes) : string.Empty);
            }
        }

        void OnPublishersModified()
        {
            RegisterToOverrideEvents();
        }

        bool PublisherHasOverrides()
        {
            return m_Publishers?.HavePublisher<OperatorMaskImageOverride>() == true;
        }

        public void UnregisterFromEvents(Model model)
        {
            UnregisterFromOverrideEvents();
            if (Enabled())
                model.OnMaskPaintDone -= OnMaskPaintDone;
        }

        public bool IsSavable() => true;

        void OnMaskPaintDone(Texture2D texture, bool isClear)
        {
            m_OperatorData.settings[(int)ESettings.IsMaskClear] = isClear ? "1" : "0";
            SetMask(Convert.ToBase64String(texture.EncodeToPNG()));
        }

        public string GetMask()
        {
            return m_OperatorData.settings[(int)ESettings.Mask];
        }

        public bool IsClear()
        {
            return m_OperatorData.settings[(int)ESettings.IsMaskClear] == "1";
        }

        public void SetMask(string mask)
        {
            m_OperatorData.settings[(int)ESettings.Mask] = mask;

            onMaskUpdated?.Invoke();
        }

        void ValidateSettings() { }

        public string sourceJobID
        {
            get => m_OperatorData.settings[(int)ESettings.SourceJobID];
            set => m_OperatorData.settings[(int)ESettings.SourceJobID] = value;
        }

        public string sourceArtifactID
        {
            get => m_OperatorData.settings[(int)ESettings.SourceArtifactID];
            set => m_OperatorData.settings[(int)ESettings.SourceArtifactID] = value;
        }

        public void InitFromJobInfo(JobInfoResponse jobInfoResponse)
        {
            Guid guid;
            if (System.Guid.TryParse(jobInfoResponse.request.mask0Guid, out guid) && guid != Guid.Empty)
            {
                var getArtifact = new GetArtifactRestCall(ServerConfig.serverConfig, jobInfoResponse.request.mask0Guid);
                getArtifact.RegisterOnSuccess(OnGetMaskArtifactSuccess);
                getArtifact.RegisterOnFailure(OnGetMaskArtifactFailed);
                getArtifact.SendRequest();
            }
        }

        public bool refined
        {
            get => m_OperatorData.settings[(int)ESettings.Refined] == "1";
            set => m_OperatorData.settings[(int)ESettings.Refined] = value ? "1" : "0";
        }

        void OnGetMaskArtifactSuccess(GetArtifactRestCall request, byte[] data)
        {
            SetMask(Convert.ToBase64String(data));
        }

        void OnGetMaskArtifactFailed(GetArtifactRestCall request)
        {
            Debug.Log($"Failed to get refine image mask artifact: {request.requestError} {request.requestResult}");
        }

        Texture2D GetTexture()
        {
            Texture2D texture = null;

            var raw = GetMask();
            if (!string.IsNullOrEmpty(raw))
            {
                texture = new Texture2D(2, 2);
                texture.LoadImage(Convert.FromBase64String(raw));
                texture.Apply();
            }

            return texture;
        }

        /// <summary>
        /// Get the settings view for this operator.
        /// </summary>
        /// <returns> UI for the operator. Set to Null if the operator should not be displayed in the settings view. Disable the returned VisualElement if you want it to be displayed but not usable.</returns>
        public VisualElement GetSettingsView(Model model, ref bool customSection, Action dismissAction)
        {
            var mask = GetTexture();
            if (mask is null)
                return null;
            var ui = new Image { image = mask };
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

        public void OnOperatorRemoved(List<IOperator> removedInOperators)
        {
            var keyImageOperator = removedInOperators.Find(x => x is KeyImageOperator) as KeyImageOperator;
            if (keyImageOperator != null)
            {
                keyImageOperator.Enable(true);
                return;
            }

            removedInOperators.Add(new KeyImageOperator());
        }
    }
}