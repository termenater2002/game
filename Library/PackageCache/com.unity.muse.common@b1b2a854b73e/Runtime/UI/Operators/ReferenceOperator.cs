using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class ReferenceOperator : IOperator
    {
        internal enum Mode
        {
            Color = 0,
            Shape
        }

        internal enum Setting
        {
            Guid = 0,
            Image,
            Mode,
            Strength,
            Color,

            SettingsLength
        }

        public string OperatorName  => "ReferenceOperator";

        public string Label => "Input Image";
        
        [SerializeField]
        OperatorData m_OperatorData;

        ReferenceOperatorView m_View;

        event Action OnDataUpdate;

        public ReferenceOperator()
        {
            m_OperatorData = new OperatorData("ReferenceOperator", "0.0.2", new[]
            {
                "",   // Guid
                "",   // Image
                "0",  // Mode
                "20", // Strength
                "" // Color
            }, false);
            OnDataUpdate += UpdateView;
        }

        public bool IsSavable() => true;

        public VisualElement GetCanvasView() => new VisualElement();

        public VisualElement GetOperatorView(Model model)
        {
            if (m_View != null)
            {
                m_View.dataChanged -= OnViewDataChanged;
                m_View.closeButtonClicked -= OnCloseButtonClicked;
            }

            m_View = new ReferenceOperatorView(model);
            m_View.dataChanged += OnViewDataChanged;
            m_View.closeButtonClicked += OnCloseButtonClicked;
            UpdateView();
            return m_View;
            
            void OnCloseButtonClicked()
            {
                model.SetOperatorVisibility(this, false);
            }
        }

        void UpdateView()
        {
            if (m_View == null)
                return;

            var guid = GetSettingString(Setting.Guid);
            var img = GetSettingTex(Setting.Image);
            var mode = GetSettingEnum<Mode>(Setting.Mode);
            var strength = GetSettingInt(Setting.Strength);
            var color = GetSettingString(Setting.Color);

            m_View.SetGuidWithoutNotify(guid);
            m_View.SetModeWithoutNotify(mode);
            m_View.SetColorWithoutNotify(ColorUtility.TryParseHtmlString(color, out var c) ? c : null); 
            if (mode == Mode.Color)
                m_View.SetColorImageWithoutNotify(img);
            else
                m_View.SetShapeImageWithoutNotify(img);
            m_View.SetStrengthWithoutNotify(strength);
            m_View.SetVisibility(!Hidden);
            m_View.SetCloseButtonVisibility(GetOperatorData().hideable);
        }
        
        void OnViewDataChanged()
        {
            var guid = m_View.GetGuid();
            var mode = m_View.GetMode();
            var img = mode == Mode.Color ? m_View.GetColorImage() : m_View.GetShapeImage();
            var strength = m_View.GetStrength();
            var color = m_View.GetColor();

            var imgPng = img ? img.EncodeToPNG() : null;

            SetSettingWithoutNotify(Setting.Guid, guid);
            SetSettingWithoutNotify(Setting.Image, imgPng != null ? Convert.ToBase64String(imgPng) : "");
            SetSettingWithoutNotify(Setting.Mode, ((int)mode).ToString());
            SetSettingWithoutNotify(Setting.Strength, strength.ToString());
            SetSettingWithoutNotify(Setting.Color, color != null ? $"#{ColorUtility.ToHtmlStringRGB(color.Value)}": string.Empty);
        }

        public OperatorData GetOperatorData()
        {
            return m_OperatorData;
        }

        public int GetSettingInt(Setting setting)
        {
            EnsureSetting(setting);
            return int.Parse(m_OperatorData.settings[(int) setting]);
        }

        public string GetSettingString(Setting setting)
        {
            EnsureSetting(setting);
            return m_OperatorData.settings[(int) setting];
        }

        public T GetSettingEnum<T>(Setting setting)
            where T : Enum
        {
            EnsureSetting(setting);
            return (T)Enum.Parse(typeof(T), m_OperatorData.settings[(int) setting]);
        }

        public Texture2D GetSettingTex(Setting setting)
        {
            EnsureSetting(setting);
            var b64String = m_OperatorData.settings[(int) setting];

            if (string.IsNullOrEmpty(b64String))
                return null;

            var bytes = Convert.FromBase64String(b64String);
            var img = new Texture2D(2, 2);
            img.LoadImage(bytes);
            return img;
        }

        public void SetOperatorData(OperatorData data)
        {
            m_OperatorData.enabled = data.enabled;
            if (data.settings == null || data.settings.Length == 0 || data.settings.Length > (int)Setting.SettingsLength)
                return;
            m_OperatorData.settings = data.settings;
            OnDataUpdate?.Invoke();
        }

        public void SetColorImage(Artifact<Texture2D> artifact)
        {
            if (artifact is null)
                return;

            m_OperatorData.enabled = true;
            SetSettingWithoutNotify(Setting.Guid, artifact.Guid);
            SetSettingWithoutNotify(Setting.Mode, ((int)Mode.Color).ToString());

            artifact.GetArtifact((artifactInstance, data, message) =>
            {
                SetSettingWithoutNotify(Setting.Image, Convert.ToBase64String(artifactInstance.EncodeToPNG()));
                OnDataUpdate?.Invoke();
            }, true);
        }

        void SetSettingWithoutNotify(Setting setting, string value)
        {
            EnsureSetting(setting);
            m_OperatorData.settings[(int)setting] = value;
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
            var result = new ReferenceOperator();
            var operatorData = new OperatorData();
            operatorData.FromJson(GetOperatorData().ToJson());
            result.SetOperatorData(operatorData);
            return result;
        }

        public void RegisterToEvents(Model model)
        {
        }

        public void UnregisterFromEvents(Model model)
        {
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
            return GetSettingTex(Setting.Image) == null ? null : GetImageUI();
        }

        Image GetImageUI()
        {
            var image = new Image();
            image.AddToClassList("muse-ref-image");
            image.name = "muse-reference-image-field";
            image.image = GetSettingTex(Setting.Image);

            return image;
        }

        void EnsureSetting(Setting setting)
        {
            if(m_OperatorData.settings.Length <= (int)setting)
            {
                Array.Resize(ref m_OperatorData.settings, (int)setting + 1);
            }
        }
    }
}