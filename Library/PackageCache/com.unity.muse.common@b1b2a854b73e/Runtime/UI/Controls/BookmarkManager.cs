using System;
using Unity.Muse.Common.Utils;
using UnityEngine;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class BookmarkManager : IModelData
    {
        public event Action OnModified;
        public event Action OnSaveRequested;

        [SerializeField]
        SerializedHashSet<string> m_BookmarkedArtifacts;

        [SerializeField]
        bool m_IsFilterEnabled;

        public bool isFilterEnabled => m_IsFilterEnabled;

        public BookmarkManager()
        {
            InitializeBookmarks();
        }

        public bool IsBookmarked(Artifact artifact) => artifact != null && m_BookmarkedArtifacts.Contains(artifact.Guid);

        public void Bookmark(Artifact artifact, bool bookmark = true)
        {
            if (artifact == null)
                return;

            var guid = artifact.Guid;
            if (bookmark)
                m_BookmarkedArtifacts.Add(guid);
            else
                m_BookmarkedArtifacts.Remove(guid);

            OnModified?.Invoke();
            OnSaveRequested?.Invoke();
        }

        public void SetFilter(bool enabled)
        {
            m_IsFilterEnabled = enabled;

            OnModified?.Invoke();
        }

        void InitializeBookmarks()
        {
            m_BookmarkedArtifacts = new SerializedHashSet<string>();
        }
    }
}