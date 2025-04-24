using System;
using Unity.Muse.Common.Utils;
using UnityEngine;

#pragma warning disable 0067

namespace Unity.Muse.Common
{
    [Serializable]
    internal class FeedbackManager : IModelData
    {
        public event Action OnModified;
        public event Action OnSaveRequested;

        public event Action<Artifact> OnLiked;
        public event Action<Artifact> OnDislike;

        [SerializeField]
        SerializedHashSet<string> m_Liked;
        [SerializeField]
        SerializedHashSet<string> m_Disliked;

        public FeedbackManager()
        {
            m_Liked = new SerializedHashSet<string>();
            m_Disliked = new SerializedHashSet<string>();
        }

        public void Like(Artifact artifact)
        {
            m_Liked.Add(artifact.Guid);

            OnLiked?.Invoke(artifact);

            OnModified?.Invoke();
        }

        public void Dislike(Artifact artifact)
        {
            m_Disliked.Add(artifact.Guid);

            OnDislike?.Invoke(artifact);

            OnModified?.Invoke();
        }

        public void ToggleLike(Artifact artifact)
        {
            var guid = artifact.Guid;
            if (m_Liked.Contains(guid))
                m_Liked.Remove(guid);
            else
                m_Liked.Add(guid);

            OnLiked?.Invoke(artifact);

            OnModified?.Invoke();
        }

        public void ToggleDislike(Artifact artifact)
        {
            var guid = artifact.Guid;
            if (m_Disliked.Contains(guid))
                m_Disliked.Remove(guid);
            else
                m_Disliked.Add(guid);

            OnDislike?.Invoke(artifact);

            OnModified?.Invoke();
        }

        public bool IsLiked(Artifact artifact) => m_Liked.Contains(artifact.Guid);
        public bool IsDisliked(Artifact artifact) => m_Disliked.Contains(artifact.Guid);
    }
}
