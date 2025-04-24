using System.Collections;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class ScopeToolbar : VisualElement, IControl, INotifyValueChanged<int>
    {
        const string ussClassName = "muse-scopetoolbar";

        static readonly string titleUssClassName = ussClassName + "__title";

        static readonly string scopesContainerUssClassName = ussClassName + "__scopes";

        static readonly string linkUssClassName = ussClassName + "__link";

        int m_Value;

        IList m_SourceItems;

        Model m_Model;

#if ENABLE_UXML_TRAITS
        internal new class UxmlFactory : UxmlFactory<ScopeToolbar, UxmlTraits> { }
#endif

        public ScopeToolbar()
        {
            this.RegisterContextChangedCallback<Model>(context => SetModel(context.context));
        }

        public void SetModel(Model model)
        {
            Unbind();
            m_Model = model;
            Bind();
        }

        void Bind()
        {
            // TODO: Bind to m_Model if it's not null
            this.RegisterValueChangedCallback(OnValueChanged);
        }

        void Unbind()
        {
            // TODO: Unbind from m_Model if it's not null
            this.UnregisterValueChangedCallback(OnValueChanged);
        }

        void OnValueChanged(ChangeEvent<int> evt)
        {
            var scope = sourceItems[evt.newValue];
            // todo send m_Model new scope
        }

        public void UpdateView()
        {
            var scopesContainer = this.Q<VisualElement>(classes: scopesContainerUssClassName);
            scopesContainer.Clear();

            for (var i = 0; i < m_SourceItems.Count; i++)
            {
                var scope = MakeItem(i);
                scopesContainer.Add(scope);
            }
        }

        VisualElement MakeItem(int index)
        {
            var link = new Link {text = sourceItems[index].ToString()};
            link.AddToClassList(linkUssClassName);
            link.clickable.clickedWithEventInfo += OnLinkClicked;

            return link;
        }

        void OnLinkClicked(EventBase evt)
        {
            var link = (Link)evt.target;
            value = link.parent.IndexOf(link);
        }

        public void SetValueWithoutNotify(int newValue)
        {
            var scopesContainer = this.Q<VisualElement>(classes: scopesContainerUssClassName);
            var scope = scopesContainer.ElementAt(m_Value);
            scope.RemoveFromClassList(Styles.selectedUssClassName);
            m_Value = newValue;
            scope = scopesContainer.ElementAt(m_Value);
            scope.AddToClassList(Styles.selectedUssClassName);
        }

        public int value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;

                using var evt = ChangeEvent<int>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        public string title
        {
            get => this.Q<Text>(classes: titleUssClassName).text;
            set => this.Q<Text>(classes: titleUssClassName).text = value;
        }

        public IList sourceItems
        {
            get => m_SourceItems;
            set
            {
                m_SourceItems = value;
                UpdateView();
            }
        }
    }
}
