using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;
using TextField = Unity.Muse.AppUI.UI.TextField;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class TextToMotionTakeUI : UITemplateContainer, IUITemplate
    {
        const string k_StyleName = "deeppose-text-to-motion-take-info";
        const string k_PromptTextName = "t2m-take-info-prompt";
        const string k_NameTextName = "t2m-take-info-name-field";
        
        TextToMotionTakeUIModel m_Model;
        
        Text m_PromptText;
        TextField m_NameText;
        
        public TextToMotionTakeUI()
            : base(k_StyleName) { }

        public void FindComponents()
        {
            m_PromptText = this.Q<Text>(k_PromptTextName);
            m_NameText = this.Q<TextField>(k_NameTextName);
        }

        public void RegisterComponents()
        {
            m_NameText.RegisterValueChangedCallback(OnNameChanged);
        }

        void OnNameChanged(ChangeEvent<string> evt)
        {
            m_Model.Title = evt.newValue;
        }

        public void UnregisterComponents()
        {
            m_NameText.UnregisterValueChangedCallback(OnNameChanged);
        }

        public void SetModel(TextToMotionTakeUIModel model)
        {
            if (m_Model != null)
                UnregisterModel();
            
            m_Model = model;
            
            RegisterModel();
            Update();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;
            
            m_Model.OnTitleChanged += Update;
        }
        
        void UnregisterModel()
        {
            if (m_Model == null)
                return;
            
            m_Model.OnTitleChanged -= Update;
        }

        public void Update()
        {
            if (m_Model == null)
                return;
            
            m_PromptText.text = m_Model.Description;
            m_NameText.value = m_Model.Title;
        }
    }
}
