using System;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Common.Tools
{
    class PaintCanvasToolManipulatorUndo : ScriptableObject, IDisposable
    {
        public Action onUndoRedo;

        [SerializeField]
        byte[] m_RawTextureData = Array.Empty<byte>();

        [SerializeField]
        int m_Version = 0;

        public byte[] rawTextureData => m_RawTextureData;

        public int version => m_Version;

        public static PaintCanvasToolManipulatorUndo Get()
        {
            var instance = CreateInstance<PaintCanvasToolManipulatorUndo>();
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.Init();
            return instance;
        }

        void Init()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.undoRedoPerformed += UndoRedoPerformed;
#endif
        }

        public void SetData(byte[] doodle, int ver)
        {
#if UNITY_EDITOR
            if(m_Version != ver)
                UnityEditor.Undo.RegisterCompleteObjectUndo(this, "Modified Mask");
            m_RawTextureData = doodle;
            m_Version = ver;
#endif
        }

        void UndoRedoPerformed()
        {
#if UNITY_EDITOR
            onUndoRedo?.Invoke();
#endif
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.ClearUndo(this);
            ObjectHelper.SafeDestroy(this);
#endif
        }
    }
}