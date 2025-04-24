using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Muse.Chat.Commands;
using Unity.Muse.Chat.BackendApi.Model;
using UnityEngine;
using Unity.Muse.Common.Editor.Integration;

namespace Unity.Muse.Chat.FunctionCalling
{
    static class FunctionCallingUtilities
    {
        internal const string k_PluginTag = "plugin";
        internal const string k_SmartContextTag = "smart-context";

        /// <summary>
        /// Returns a function that parses a string and returns an object of a given type.
        /// </summary>
        /// <param name="csharpType">The string name of the type to convert</param>
        /// <returns></returns>
        internal static Func<string, object> GetConverter(string csharpType)
        {
            // Get parser for array types, format should be: List[<type>]:
            if (csharpType.StartsWith("List["))
            {
                var elementType = csharpType.Substring(5, csharpType.Length - 6);
                var elementConverter = GetConverter(elementType);

                // Make converter that parses an array:
                return ConverterFactory(s =>
                {
                    s = s.TrimStart('[').TrimEnd(']');

                    // s is a comma separated string:
                    var elements = s.Replace("[", "").Replace("]", "").Replace("'", "").Split(',');

                    // Trim " " and " ' " from the elements:
                    for (int i = 0; i < elements.Length; i++)
                    {
                        elements[i] = elements[i].Trim(' ').Trim('\'');
                    }

                    // Remove empty elements:
                    elements = elements.Where(e => !string.IsNullOrEmpty(e)).ToArray();

                    // We cannot just return an array of type object[], we need to create an array of the correct type:
                    var result = Array.CreateInstance(GetArrayType(), elements.Length);
                    for (var i = 0; i < elements.Length; i++)
                    {
                        result.SetValue(elementConverter(elements[i]), i);
                    }

                    return result;

                    Type GetArrayType()
                    {
                        return elementType switch
                        {
                            "String" => typeof(string),
                            "Int32" => typeof(int),
                            "Int64" => typeof(long),
                            "Single" => typeof(float),
                            "Boolean" => typeof(bool),
                            _ => null
                        };
                    }
                });
            }

            return csharpType switch
            {
                "String" => s =>
                {
                    var output = s.TrimStart('\'').TrimEnd('\'');

                    // If the string is an empty array, treat it as an empty string:
                    if (output == "[]")
                    {
                        output = string.Empty;
                    }

                    return output;
                },
                "Int32" => ConverterFactory(int.Parse),
                "Int64" => ConverterFactory(long.Parse),
                "Single" => ConverterFactory(float.Parse),
                "Boolean" => ConverterFactory(bool.Parse),
                _ => s => null
            };

            Func<string, object> ConverterFactory<T>(Func<string, T> converter)
            {
                return s =>
                {
                    try
                    {
                        return converter(s);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                };
            }
        }

        /// <summary>
        /// Get a <see cref="ParameterDefinition"/> used to serialize parameters and send them to the server.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="toolMethod"></param>
        /// <returns></returns>
        internal static ParameterDefinition GetParameterDefinition(ParameterInfo parameter, MethodInfo toolMethod)
        {
            var parameterAttribute = parameter.GetCustomAttribute<ParameterAttribute>();
            if (parameterAttribute == null)
            {
                Debug.LogWarning(
                    $"Method \"{toolMethod.Name}\" in \"{toolMethod.DeclaringType?.FullName}\" contains the parameter \"{parameter.Name}\" that must marked with the {nameof(ParameterAttribute)} attribute. This method will be ignored.");
                return null;
            }

            var parameterType = parameter.ParameterType.Name;

            if (parameterType.EndsWith("[]"))
            {
                var elementType = parameterType.Substring(0, parameterType.Length - 2); // Remove "[]"
                parameterType = $"List[{elementType}]"; // Convert to Python list
            }

            var def = new ParameterDefinition(parameterAttribute.Description, parameter.Name, parameterType)
            {
                Optional = parameter.IsDefined(typeof(ParamArrayAttribute), false) || parameter.HasDefaultValue
            };

            return def;
        }

        internal static string GetTagForAttribute(Attribute attribute)
        {
            switch (attribute)
            {
                case PluginAttribute:
                    return k_PluginTag;
                case ContextProviderAttribute:
                    return k_SmartContextTag;
            }

            return string.Empty;
        }

        internal static string GetTagForCommandAttribute(Attribute attribute, ChatCommandHandler handler)
        {
            if (handler == null)
                return string.Empty;

            switch (attribute)
            {
                case ContextProviderAttribute:
                    return $"command_{handler.Command}";
            }

            return string.Empty;
        }

        /// <summary>
        /// Transforms a MethodInfo and a description into a FunctionDefinition that can be sent to the server.
        /// </summary>
        /// <param name="method">The method info this definition should define</param>
        /// <param name="description">The user written description destined for the LLM</param>
        /// <param name="tags">Any tags associated with the function</param>
        /// <returns></returns>
        internal static FunctionDefinition GetFunctionDefinition(MethodInfo method, string description,
            params string[] tags)
        {
            var parameters = method.GetParameters();

            bool valid = true;

            // Create parameter info list:
            var toolParameters = new List<ParameterDefinition>(parameters.Length);
            for (var parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                var parameter = parameters[parameterIndex];
                var parameterInfo = GetParameterDefinition(parameter, method);
                if (parameterInfo == null)
                {
                    valid = false;
                    break;
                }

                toolParameters.Add(parameterInfo);
            }

            if (!valid)
            {
                return null;
            }

            return new FunctionDefinition(description, method.Name)
            {
                Parameters = toolParameters,
                Tags = tags.ToList()
            };
        }
    }
}
