using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Chat.BackendApi.Model;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Chat
{
    /// <summary>
    /// Context builder is used to build a context string from a list of context selections.
    /// </summary>
    public class ContextBuilder
    {
        private readonly EditorContextReport m_ContextList = new(
            attachedContext: new List<ContextItem>(),
            extractedContext: new List<ContextItem>(),
            characterLimit: MuseChatConstants.PromptContextLimit);

        internal int PredictedLength { get; private set; }

        /// <summary>
        /// Adds the given piece of context to the context list.
        /// </summary>
        /// <param name="contextSelection">The context to add.</param>
        /// <param name="userSelected">True if this context was added by the user.</param>
        /// <param name="priority">Higher priority values make it more likely to be included when the limit is reached.</param>
        internal ContextItem InjectContext(IContextSelection contextSelection, bool userSelected, int priority = 0)
        {
            var payload = contextSelection?.Payload;

            if (string.IsNullOrEmpty(payload))
            {
                return null;
            }

            if (m_ContextList.AllContext.Any(existingContext => existingContext.Context == contextSelection))
            {
                return null;
            }

            if (contextSelection is UnityObjectContextSelection contextSelectionAsUnityObjectContextSelection)
            {
                foreach (var existingContext in m_ContextList.AllContext)
                {
                    if (existingContext.Context is not UnityObjectContextSelection
                        existingAsUnityObjectContextSelection) continue;

                    if (existingAsUnityObjectContextSelection.Target ==
                        contextSelectionAsUnityObjectContextSelection.Target)
                        return null;
                }
            }

            // Find if the context is already in the list and get that element:
            var existingContextItem =
                m_ContextList.AllContext.FirstOrDefault(existingContext => existingContext.Payload == payload);

            if (existingContextItem != null)
            {
                // TODO: Count copies here:
                // existingContextItem.NumberOfCopies += 1;
                return null;
            }

            var contextItem = new ContextItem(payload, false, contextSelection.ContextType)
            {
                Context = contextSelection
            };

            PredictedLength += contextItem.Payload.Length;

            if (userSelected)
            {
                m_ContextList.AttachedContext.Add(contextItem);
            }
            else
            {
                m_ContextList.ExtractedContext.Add(contextItem);
            }

            return contextItem;
        }

        static bool AreObjectsEqual(Object a, Object b)
        {
            if (a.GetType() != b.GetType())
            {
                return false;
            }

            var correspondingSourceA = PrefabUtility.GetCorrespondingObjectFromSource(a);
            var correspondingSourceB = PrefabUtility.GetCorrespondingObjectFromSource(b);

            var sA = new SerializedObject(a);
            var sB = new SerializedObject(b);

            var propA = sA.GetIterator();

            while (propA.Next(true))
            {
                // Stop at depth that extracted context would stop at:
                if (propA.depth > 0)
                {
                    continue;
                }

                // Ignore fields that are not serialized on components:
                if (SerializationObjectJsonAdapter.ComponentFieldsToIgnore.Contains(propA.propertyPath))
                {
                    continue;
                }

                var propB = sB.FindProperty(propA.propertyPath);
                if (propB == null)
                {
                    continue;
                }

                // Ignore self references:
                if (propA.propertyType is SerializedPropertyType.ObjectReference
                        or SerializedPropertyType.ExposedReference &&
                    propA.objectReferenceValue != null &&
                    propB.objectReferenceValue != null)
                {
                    if ((propA.objectReferenceValue == a || propA.objectReferenceValue == correspondingSourceA) &&
                        (propB.objectReferenceValue == b || propB.objectReferenceValue == correspondingSourceB))
                    {
                        continue;
                    }
                }

                if (propA.contentHash != propB.contentHash)
                {
                    return false;
                }
            }

            return true;
        }

        private void IndexContextList()
        {
            SerializationObjectJsonAdapter.ClearDeduplicatedObjects();

            var allComponents = new List<Component>();
            var copiedComponents = new HashSet<Component>();

            // var allGameObjects = new List<GameObject>();
            // var copiedGameObjects = new HashSet<GameObject>();

            // Collect all components on UnityObjectContextSelection:
            foreach (var contextItem in m_ContextList.AllContext)
            {
                var unityObjectContextSelection = contextItem.Context as UnityObjectContextSelection;
                if (unityObjectContextSelection?.GameObject is null)
                    continue;

                var gameObject = unityObjectContextSelection.GameObject;

                foreach (var componentToCheck in gameObject.GetComponents<Component>())
                {
                    if (componentToCheck == null)
                        continue;

                    // Check if there is another component that would serialize to the same json:
                    var isDuplicateComponent = false;
                    foreach (var previouslyVisitedComponent in allComponents)
                    {
                        if (AreObjectsEqual(previouslyVisitedComponent, componentToCheck))
                        {
                            if (copiedComponents.Add(previouslyVisitedComponent))
                            {
                                // Deduplicated context items should not go deeper than they would have if they were not deduplicated:
                                var componentSelection =
                                    new UnityObjectContextSelection { MaxObjectDepth = 0, IncludeFileContents = false };
                                componentSelection.SetTarget(previouslyVisitedComponent);
                                var deduplicatedContext = InjectContext(componentSelection, false);
                                if (deduplicatedContext != null)
                                {
                                    deduplicatedContext.DeduplicationID = previouslyVisitedComponent.GetInstanceID();
                                }
                            }

                            SerializationObjectJsonAdapter.AddDeduplicatedObject(
                                previouslyVisitedComponent.GetInstanceID(), componentToCheck.GetInstanceID());

                            isDuplicateComponent = true;
                            break;
                        }
                    }

                    if (!isDuplicateComponent)
                    {
                        allComponents.Add(componentToCheck);
                    }
                }

                // var isDuplicateGameObject = false;
                // foreach (var previouslyVisitedGameObject in allGameObjects)
                // {
                //     if (AreObjectsEqual(previouslyVisitedGameObject, gameObject))
                //     {
                //         if (copiedGameObjects.Add(previouslyVisitedGameObject))
                //         {
                //             var gameObjectSelection = new UnityObjectContextSelection();
                //             gameObjectSelection.SetTarget(previouslyVisitedGameObject);
                //             InjectContext(gameObjectSelection, false);
                //         }
                //
                //         SerializationObjectJsonAdapter.AddDeduplicatedObject(
                //             previouslyVisitedGameObject.GetInstanceID(), gameObject.GetInstanceID());
                //
                //         isDuplicateGameObject = true;
                //         break;
                //     }
                // }
                //
                // if (!isDuplicateGameObject)
                // {
                //     allGameObjects.Add(gameObject);
                // }
            }
        }

        internal bool Contains(string contextString)
        {
            return m_ContextList.AllContext.Any(contextPiece =>
                contextPiece.Payload.Contains(contextString));
        }

        private void ProcessContextList(List<ContextItem> contextList, int contextLimit, bool forceRecomputePayloads,
            out bool removedDeduplicatedPieces)
        {
            removedDeduplicatedPieces = false;

            // Make sure we got space for everything:
            for (var i = 0; i < contextList.Count; i++)
            {
                var contextPiece = contextList[i];

                // Update payload if context was duplicated, otherwise it will not have the shortened payload:
                if (forceRecomputePayloads || SerializationObjectJsonAdapter.HasDeduplicatedObjects)
                {
                    contextPiece.Payload = ((IContextSelection)contextPiece.Context).Payload;
                }

                // If the new length would exceed the limit, try using the downsized payload:
                if (PredictedLength + contextPiece.Payload.Length > contextLimit)
                {
                    contextPiece.Payload = ((IContextSelection)contextPiece.Context).DownsizedPayload;
                    contextPiece.Truncated = true;

                    // If the new length still exceeds the limit, remove this piece:
                    if (string.IsNullOrEmpty(contextPiece.Payload) ||
                        PredictedLength + contextPiece.Payload.Length > contextLimit)
                    {
                        contextList.RemoveAt(i--);

                        // If this was a deduplicated object, remove the deduplication ID and exit, because we will have to recompute all the payloads:
                        if (contextPiece.DeduplicationID.HasValue)
                        {
                            SerializationObjectJsonAdapter.RemoveDeduplicatedObject(contextPiece.DeduplicationID.Value);
                            removedDeduplicatedPieces = true;
                            return;
                        }

                        continue;
                    }
                }

                PredictedLength += contextPiece.Payload.Length;
            }
        }

        internal EditorContextReport BuildContext(int contextLimit)
        {
            IndexContextList();

            m_ContextList.Sort();

            bool removedDeduplicatedPieces;
            var forceRecomputePayloads = false;
            do
            {
                PredictedLength = 0;

                ProcessContextList(m_ContextList.AttachedContext,
                    contextLimit,
                    forceRecomputePayloads,
                    out _);

                ProcessContextList(m_ContextList.ExtractedContext,
                    contextLimit,
                    forceRecomputePayloads,
                    out removedDeduplicatedPieces);

                // If the loop is repeated, we will have to recompute the payloads, because extracted context were removed:
                forceRecomputePayloads = true;
            } while (removedDeduplicatedPieces);

            PredictedLength = m_ContextList.AllContext.Sum(contextItem => contextItem.Payload.Length);

            return m_ContextList;
        }
    }
}
