using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.Muse.Common.Editor.Integration;

namespace Unity.Muse.Chat.Context.SmartContext
{
    internal static partial class ContextRetrievalTools
    {
        // Description strings that are used in multiple methods:
        const string k_NameFilter = "Name of the gameObject.";

        private static string GetComponents(GameObject gameObject)
        {
            return $"GameObject {gameObject.name} has the following components attached to it: " + string.Join("\n",
                gameObject.GetComponents<Component>().Select(c => c != null ? c.GetType().Name : "<Missing Script>"));
        }

        [ContextProvider("Returns the serialized data of the object or asset (GameObject, prefab, script, etc.). Only to be used with a non-empty objectName.")]
        internal static SmartContextToolbox.ExtractedContext ObjectDataExtractor(
            [Parameter("Name of the object or asset to extract data from.")]
            string objectName,
            [Parameter(
                "Optional: Filter to specify the NAME of a particular component or script on the object IF it’s a GameObject or prefab. If we use this filter we don't filter for any optional assetType.")]
            string componentFilter = null,
            [Parameter(
                "Optional: Filter to specify an asset type, from the following asset types: script, mesh, texture, material, audioclip, sprite, model, and prefab. If we use this filter we don't filter for any optional componentFilter.")]
            string assetType = null)
        {
            if (string.IsNullOrEmpty(objectName))
                return null;

            var selectedContext = new ContextBuilder();
            Assistant.instance.GetAttachedContextString(ref selectedContext);

            Object matchingAsset = null;

            if (!string.IsNullOrEmpty(assetType))
            {
                // TODO: Generally ensure we use a known and valid type
                assetType = assetType.ToLower();

                // Assets
                matchingAsset = TryFindAssets(objectName, assetType);
            }

            // Objects
            if (matchingAsset == null)
                matchingAsset = ContextRetrievalHelpers.FindObject<Object>(objectName);

            var objectContext = new UnityObjectContextSelection();

            if (matchingAsset == null)
            {
                return null;
            }

            var prefix = $"Contents of asset {matchingAsset.name}:\n";

            if (matchingAsset is GameObject gameObject && !string.IsNullOrEmpty(componentFilter))
            {
                // Find matching component:
                var components = ContextRetrievalHelpers.FindComponents(gameObject, componentFilter).ToList();

                if (components.Count > 0)
                {
                    // In first iteration of the loop, try to use full context,
                    // if that becomes too large, use downsized payload in second iteration:
                    for (var fullOrDownsizedPayLoad = 0; fullOrDownsizedPayLoad < 2; fullOrDownsizedPayLoad++)
                    {
                        var componentsResult = string.Empty;
                        foreach (var component in components)
                        {
                            prefix =
                                $"Contents of component {component.GetType().Name} on GameObject {matchingAsset.name}:\n";

                            objectContext.SetTarget(component);

                            var payload = fullOrDownsizedPayLoad == 0
                                ? ((IContextSelection)objectContext).Payload
                                : ((IContextSelection)objectContext).DownsizedPayload;

                            var componentPayload = ((IContextSelection)objectContext).Payload;

                            // If the payload is already in the selected context, do not include this component:
                            if (selectedContext.Contains(componentPayload))
                            {
                                continue;
                            }

                            componentsResult += prefix + payload + "\n";

                            // If the context is going to get too big, exit early and try again with downsized payload:
                            if (componentsResult.Length > SmartContextToolbox.SmartContextLimit &&
                                fullOrDownsizedPayLoad == 0)
                            {
                                break;
                            }
                        }

                        if (componentsResult.Length <= SmartContextToolbox.SmartContextLimit ||
                            fullOrDownsizedPayLoad == 1)
                        {
                            return new SmartContextToolbox.ExtractedContext
                            {
                                Payload = componentsResult,
                                ContextType = "component data"
                            };
                        }
                    }
                }
            }

            var path = AssetDatabase.GetAssetPath(matchingAsset);
            if (!string.IsNullOrEmpty(path))
            {
                prefix = $"Contents of asset at path '{path}':";
            }

            objectContext.SetTarget(matchingAsset);
            var objectPayload = ((IContextSelection)objectContext).Payload;

            // If the payload is already in the selected context, do not return anything:
            if (selectedContext.Contains(objectPayload))
            {
                return null;
            }

            var result = prefix + objectPayload;
            if (result.Length <= SmartContextToolbox.SmartContextLimit)
            {
                return new SmartContextToolbox.ExtractedContext
                {
                    Payload = result,
                    ContextType = "object data"
                };
            }

            objectPayload = ((IContextSelection)objectContext).DownsizedPayload;
            // If the payload is already in the selected context, do not return anything:
            if (selectedContext.Contains(objectPayload))
            {
                return null;
            }

            return new SmartContextToolbox.ExtractedContext
            {
                Payload = prefix + objectPayload,
                ContextType = "object data",
                Truncated = true
            };
        }
    }
}
