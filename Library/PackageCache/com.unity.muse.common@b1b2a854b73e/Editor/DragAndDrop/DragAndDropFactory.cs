using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    internal class DragAndDropFactory
    {
        static Dictionary<string, Type> s_ArtifactTypes;

        public static bool SetHandlerForArtifact(string dragAndDropInteractionType, Type dragAndDropHandlerType)
        {
            s_ArtifactTypes ??= new Dictionary<string, Type>();
            return s_ArtifactTypes.TryAdd(dragAndDropInteractionType, dragAndDropHandlerType);
        }

        public static IArtifactDragAndDropHandler CreateHandler(string dragAndDropInteractionType, IList<Artifact> artifacts)
        {
            if (artifacts == null || artifacts.Count == 0)
                return null;

            if (s_ArtifactTypes == null || !s_ArtifactTypes.TryGetValue(dragAndDropInteractionType, out var type))
                return null;

            return (IArtifactDragAndDropHandler)Activator.CreateInstance(type, artifacts);
        }

        public static IArtifactDragAndDropHandler CreateMultiHandler(IList<IArtifactDragAndDropHandler> handlers)
        {
            if (handlers == null || handlers.Count == 0)
                return null;

            return new MultiArtifactDragAndDropHandler(handlers);
        }
    }
}
