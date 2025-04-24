using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common;
using Unity.Muse.StyleTrainer;

#pragma warning disable 0067

namespace Unity.Muse.Sprite.Data
{
    class DefaultStyleData : IModelData
    {
        public event Action OnModified;
        public event Action OnSaveRequested;

        IReadOnlyList<StyleData> m_DefaultStyles;
        bool m_Loading = false;
        bool m_Failed = false;

        public IReadOnlyList<StyleData> GetBuiltInStyle()
        {
            if (!m_Loading && (m_DefaultStyles == null || m_Failed))
            {
                m_DefaultStyles = StyleTrainerProjectData.instance.GetDefaultStyles(OnGetDefaultStyleDone, OnGetDefaultStyleFailed, false);
                m_Loading = true;
            }

            return m_DefaultStyles;
        }

        public bool loading => m_Loading;
        public bool failedToLoad => m_Failed;

        void OnGetDefaultStyleDone(IReadOnlyList<StyleData> obj)
        {
            var newDefaultStyles = new List<StyleData>(obj.Where(s => s.state == EState.Loaded && s.visible && s.checkPoints is { Count: > 0 }));
            m_DefaultStyles = newDefaultStyles.Count == 0 ? m_DefaultStyles : newDefaultStyles;
            m_Loading = false;
            m_Failed = obj.Any(s => s.state == EState.Error || s.checkPoints.Any(c => c.state == EState.Error));
            OnModified?.Invoke();
        }

        void OnGetDefaultStyleFailed()
        {
            m_Loading = false;
            m_Failed = true;
            OnModified?.Invoke();
        }

        public void Reset()
        {
            m_DefaultStyles = null;
            m_Loading = false;
            m_Failed = false;
        }
    }
}