using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{
    class SequenceAddKeyButtonView : Button
    {
        const string k_UssClassName = "deeppose-sequence-add-key-button";
        const string k_IconName = "plus";

        SequenceAddKeyButtonViewModel m_Model;

        public SequenceAddKeyButtonView(string name)
        {
            this.name = name;
            tooltip = "Add a key at the end of the sequence";
            leadingIcon = k_IconName;

            AddToClassList(k_UssClassName);

            RegisterCallback<ClickEvent>(OnButtonClicked);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            Update();
        }

        public void SetModel(SequenceAddKeyButtonViewModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged += OnChanged;
            Update();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnChanged;
            m_Model = null;
        }

        void Update()
        {
            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnChanged()
        {
            Update();
        }

        void OnButtonClicked(ClickEvent evt)
        {
            m_Model?.AddKey();
        }
    }
}
