using System;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class ExperimentalProgramChangedTracker : Manipulator
    {
        Action m_Callback;

        public ExperimentalProgramChangedTracker(Action callback, bool callImmediately = true)
        {
            m_Callback = callback;
            if (callImmediately)
                Refresh();
        }

        protected override void RegisterCallbacksOnTarget() => ExperimentalProgram.Changed += Refresh;
        
        protected override void UnregisterCallbacksFromTarget() => ExperimentalProgram.Changed -= Refresh;

        void Refresh() => m_Callback?.Invoke();
    }
}
