#if UNITY_EDITOR

using System;
using Unity.Muse.Common;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Sprite
{
    internal class KeyImageUndo : ScriptableObject, IDisposable
    {
        public Action onUndoRedo;
        
        [SerializeField]
        byte[] m_RawTextureData = Array.Empty<byte>();

        [SerializeField]
        Texture2D m_ReferenceImage;

        [SerializeField]
        bool m_IsClear;

        [SerializeField]
        int m_Version = 0;

        public byte[] rawTextureData => m_RawTextureData;
        public Texture2D referenceImage => m_ReferenceImage;
        public bool isClear => m_IsClear;
        
        public int version => m_Version;

        public static KeyImageUndo Get()
        {
            var instance = CreateInstance<KeyImageUndo>();
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.Init();
            return instance;
        }

        void Init()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        public void SetData(byte[] doodle, bool clear, Texture2D refImage, int ver)
        {
            if(m_Version != ver)
                Undo.RegisterCompleteObjectUndo(this, "Modified Input Image Operator");
            m_RawTextureData = doodle;
            m_IsClear = clear;
            m_ReferenceImage = refImage;
            m_Version = ver;
        }

        void UndoRedoPerformed()
        {
            onUndoRedo?.Invoke();
        }

        public void Dispose()
        {
            Undo.ClearUndo(this);
            ObjectHelper.SafeDestroy(this);
        }
    }
}
#endif
