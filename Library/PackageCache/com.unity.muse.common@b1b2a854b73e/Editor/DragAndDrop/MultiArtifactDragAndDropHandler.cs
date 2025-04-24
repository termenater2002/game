using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Muse.Common.Editor
{
    [Preserve]
    internal sealed class MultiArtifactDragAndDropHandler : IArtifactDragAndDropHandler
    {
        readonly IList<IArtifactDragAndDropHandler> m_Handlers;

#pragma warning disable 67
        public event Action<string, Artifact> ArtifactDropped;
#pragma warning restore 67

        public MultiArtifactDragAndDropHandler(IList<IArtifactDragAndDropHandler> handlers)
        {
            m_Handlers = handlers;

            Debug.Assert(m_Handlers != null);
            Debug.Assert(m_Handlers.Count > 0);

            foreach (var handler in m_Handlers)
            {
                Debug.Assert(handler != null);
            }
        }

        public bool CanDropSceneView(GameObject dropUpon, Vector3 worldPosition) => m_Handlers.Any(h => h.CanDropSceneView(dropUpon, worldPosition));

        public void HandleDropSceneView(GameObject dropUpon, Vector3 worldPosition)
        {
            foreach (var handler in m_Handlers)
            {
                handler.HandleDropSceneView(dropUpon, worldPosition);
            }
        }

        public bool CanDropHierarchy(GameObject dropUpon) => m_Handlers.Any(h => h.CanDropHierarchy(dropUpon));

        public void HandleDropHierarchy(GameObject dropUpon)
        {
            foreach (var handler in m_Handlers)
            {
                handler.HandleDropHierarchy(dropUpon);
            }
        }

        public bool CanDropProject(string path) => m_Handlers.Any(h => h.CanDropProject(path));

        public void HandleDropProject(string path)
        {
            foreach (var handler in m_Handlers)
            {
                handler.HandleDropProject(path);
            }
        }
    }
}