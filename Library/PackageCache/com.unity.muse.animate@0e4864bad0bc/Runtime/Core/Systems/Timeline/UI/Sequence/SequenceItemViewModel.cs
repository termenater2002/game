using System;

namespace Unity.Muse.Animate
{
    abstract class SequenceItemViewModel<T>
    {
        public delegate void ItemClicked(SequenceItemViewModel<T> item);
        public delegate void ItemChanged(SequenceItemViewModel<T> item);

        public event ItemClicked OnLeftClicked;
        public event ItemClicked OnRightClicked;
        public event ItemChanged OnChanged;

        [Flags]
        enum ItemState
        {
            Visible = 1,
            Selected = 2,
            Editing = 4,
            Highlighted = 8
        }

        public T Target
        {
            get => m_Target;
            set => SetTarget(value);
        }
        
        
        public bool IsHighlighted
        {
            get => m_State.HasFlag(ItemState.Highlighted);
            set => SetState(ItemState.Highlighted, value);
        }

        public bool IsVisible
        {
            get => m_State.HasFlag(ItemState.Visible);
            set => SetState(ItemState.Visible, value);
        }

        public bool IsSelected
        {
            get => m_State.HasFlag(ItemState.Selected);
            set => SetState(ItemState.Selected, value);
        }

        public bool IsEditing
        {
            get => m_State.HasFlag(ItemState.Editing);
            set => SetState(ItemState.Editing, value);
        }

        T m_Target;
        ItemState m_State = ItemState.Visible;

        protected SequenceItemViewModel(T sequenceTarget)
        {
            m_Target = sequenceTarget;
        }
        
        void SetState(ItemState state, bool value)
        {
            if (value == m_State.HasFlag(state))
                return;

            if (value)
            {
                m_State |= state;
            }
            else
            {
                m_State &= ~state;
            }

            OnChanged?.Invoke(this);
        }
        
        void SetTarget(T value)
        {
            if (m_Target != null)
                UnregisterCallbacks();
            
            m_Target = value;

            if (m_Target != null)
                RegisterCallbacks();

            InvokeChanged();
        }
        
        public void Clicked(int button = 0)
        {
            if (button == 0)
            {
                OnLeftClicked?.Invoke(this);
                return;
            }
            
            OnRightClicked?.Invoke(this);
        }

        protected void InvokeChanged()
        {
            OnChanged?.Invoke(this);
        }
        
        protected virtual void UnregisterCallbacks()
        {
            // Override to register/unregister to the item's specific models.
            // See SequenceKeyViewModel.cs for an example
        }
        
        protected virtual void RegisterCallbacks()
        {
            // Override to register/unregister to the item's specific models.
            // See SequenceKeyViewModel.cs for an example
        }
    }
}
