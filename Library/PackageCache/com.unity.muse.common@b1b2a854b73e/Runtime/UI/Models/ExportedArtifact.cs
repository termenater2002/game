using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class ExportedArtifact
    {
        [SerializeField]
        private string m_UnityGuid;
        [SerializeField]
        private string m_MuseGuid;

        public string UnityGuid => m_UnityGuid;
        public string MuseGuid => m_MuseGuid;

        public ExportedArtifact(string unityGuid, string museGuid)
        {
            m_UnityGuid = unityGuid;
            m_MuseGuid = museGuid;
        }
    }
}