using System;
using System.Diagnostics;
using static Unity.Muse.Animate.DevLogger;

namespace Hsm
{
    class StateMachineLogger
    {
        readonly object m_Owner;
        
        public StateMachineLogger(object aOwner = null)
        {
            m_Owner = aOwner;
        }

        [Conditional("UNITY_MUSE_DEV")]
        public void LogTransition(TraceLevel aTraceLevel, int aDepth, string aTransitionName, Type aTargetStateType)
        {
            var s = $"HSM [{m_Owner ?? "NoOwner"}]:{new string(' ', aDepth)}{aTransitionName,-11}{aTargetStateType}";

            LogSeverity(aTraceLevel, s);
        }
    }
}
