using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Muse.Chat.BackendApi.Model;
using UnityEngine;

namespace Unity.Muse.Chat
{
    internal static class ContextUtils
    {
        public static void MergeSceneHierarchyExtractorCalls(List<FunctionCall> deduplicatedCalls)
        {
            var sceneHierarchyCalls = deduplicatedCalls.Where(call => call.Function == "SceneHierarchyExtractor").ToList();

            if (sceneHierarchyCalls.Count == 2)
            {
                var sceneHierarchyCall1 = sceneHierarchyCalls[0];
                var sceneHierarchyCall2 = sceneHierarchyCalls[1];

                var sceneHierarchyCall1Params = sceneHierarchyCall1.Parameters;
                var sceneHierarchyCall2Params = sceneHierarchyCall2.Parameters;

                var params1 = sceneHierarchyCall1Params;
                var params2 = sceneHierarchyCall2Params;

                var params1Objects = params1.FirstOrDefault(s => s.Contains("gameObjectNameFilters"));
                var params2Objects = params2.FirstOrDefault(s => s.Contains("gameObjectNameFilters"));

                var params1ObjectsAsList = GetExtractorFunctionParamsAsList(params1Objects);
                var params2ObjectsAsList = GetExtractorFunctionParamsAsList(params2Objects);

                // merge params1ObjectsAsList and params2ObjectsAsList into a new List<string> variable
                var mergedObjectParamsList = new List<string>();
                mergedObjectParamsList.AddRange(params1ObjectsAsList);
                mergedObjectParamsList.AddRange(params2ObjectsAsList);

                var allObjectParams = "gameObjectNameFilters:" + ConvertListExtractorFunctionParams(mergedObjectParamsList);

                var finalParams = new List<string> {allObjectParams};

                sceneHierarchyCall1.Parameters = finalParams;

#if MUSE_INTERNAL
                Debug.Log($"Trying merged params for {sceneHierarchyCall1.Function}: " +
                          $"{string.Join(", ", sceneHierarchyCall1.Parameters)}");
#endif

                deduplicatedCalls.Remove(sceneHierarchyCall2);
            }
        }

        private static List<string> GetExtractorFunctionParamsAsList(string input)
        {
            if (input == null)
                return new List<string>();

            // Trim the input string to remove unwanted parts
            string trimmedInput = input.Substring(input.IndexOf('[') + 1, input.LastIndexOf(']') - input.IndexOf('[') - 1);

            // Split the entries by comma
            string[] entries = trimmedInput.Split(',');

            // Create a list to store the results
            List<string> result = new List<string>();

            // Iterate through each entry, remove quotes and add to the list
            foreach (string entry in entries)
            {
                string trimmedEntry = entry.Trim().Trim('\''); // Trim whitespace and quotes
                result.Add(trimmedEntry);
            }

            return result;
        }

        private static string ConvertListExtractorFunctionParams(List<string> inputList)
        {
            // Check if the list is empty
            if (inputList == null || inputList.Count == 0)
            {
                return "[]";
            }

            var result = new StringBuilder("[");

            for (int i = 0; i < inputList.Count; i++)
            {
                result.Append('\'').Append(inputList[i]).Append('\'');

                if (i < inputList.Count - 1)
                {
                    result.Append(',');
                }
            }

            result.Append(']');
            return result.ToString();
        }
    }
}
