using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Muse.Chat.BackendApi;
using UnityEngine;

namespace Unity.Muse.Chat.FunctionCalling
{
    internal abstract class FunctionToolbox
    {
        protected readonly Dictionary<string, CachedFunction> k_Tools = new();

        public FunctionToolbox(FunctionCache functionCache, params string[] tags)
        {
            // Build list of available tool methods:
            k_Tools.Clear();

            foreach (CachedFunction cachedFunction in functionCache.GetFunctionsByTags(tags))
            {
                // Silent fail if multiple functions with same key are added
                k_Tools.TryAdd(cachedFunction.FunctionDefinition.Name, cachedFunction);
            }
        }

        public bool TryGetSelectorAndConvertArgs(string name, string[] args, out CachedFunction function,
            out object[] convertedArgs)
        {
            convertedArgs = null;

            if (!k_Tools.TryGetValue(name, out function))
                return false;

            if (function.FunctionDefinition.Parameters == null || function.FunctionDefinition.Parameters.Count == 0)
            {
                convertedArgs = Array.Empty<object>();
                return true;
            }

            // Check what parameters are required:
            var requiredArgCount = function.FunctionDefinition?.Parameters?.Count(parameter => !parameter.Optional) ?? 0;

            if (args.Length < requiredArgCount)
            {
                InternalLog.LogError($"Incorrect function call: {name} args: {string.Join(",", args)}");
                throw new ArgumentException("The incorrect number of args were provided");
            }

            convertedArgs = new object[function.FunctionDefinition.Parameters.Count];

            string[] argNames = function
                .FunctionDefinition
                .Parameters
                .Select(param => param.Name)
                .ToArray();

            Func<string, object>[] converters = function
                .FunctionDefinition
                .Parameters
                .Select(param => FunctionCallingUtilities.GetConverter(param.Type))
                .ToArray();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (!arg.Contains(":"))
                {
                    InternalLog.LogWarning(
                        $"SmartContextError: The LLM did not return an arg as a named arg. Assuming it is a positional arg");

                    try
                    {
                        convertedArgs[i] = converters[i](arg);
                        continue;
                    }
                    catch (Exception)
                    {
                        InternalLog.LogWarning(
                            $"SmartContextError: The LLM did not return an arg that was a valid positional arg");
                    }
                }

                try
                {
                    // Split arg at first ":":
                    var splitIndex = arg.IndexOf(":", StringComparison.Ordinal);
                    var argName = arg[..splitIndex];
                    var argValue = arg[(splitIndex + 1)..];

                    var namedindex = Array.IndexOf(argNames, argName);

                    convertedArgs[namedindex] = converters[namedindex](argValue);

                    // TODO: This is a temporary fix for a backend issue, remove this when the backend is fixed:
                    if (convertedArgs[namedindex] as string == "AttributedDict()")
                    {
                        convertedArgs[namedindex] = "";
                    }

                    if (convertedArgs[namedindex] is object[] objArray)
                    {
                        for (var objArrayIdx = 0; objArrayIdx < objArray.Length; objArrayIdx++)
                        {
                            var o = objArray[objArrayIdx];
                            if (o is string s && s.Trim() == "AttributedDict()")
                            {
                                objArray[objArrayIdx] = "";
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    InternalLog.LogWarning(
                        $"SmartContextError: The LLM did not return an arg that was a valid named arg");
                    return false;
                }
            }

            return true;
        }
    }
}
