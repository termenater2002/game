using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.Search;
using System.Reflection;
using Unity.Muse.Common.Editor.Integration;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Unity.Muse.Chat.Context.SmartContext
{
    internal static partial class ContextRetrievalTools
    {
        private const int k_FinalResultLimit = 2;

        private static readonly string[] k_RootFields = new string[1];

        // Find actual setting name, ignore names that don't have details:
        private static readonly string[] k_IgnoredNames =
            new[] { "UnityEditor", "UnityEngine", "settings", "setting" }
                .Select(nameSpace => nameSpace.ToLowerInvariant()).ToArray();

        // Cache types for performance:
        private static List<Type> s_AllTypes;


        class FuzzySearchResult
        {
            public class FuzzySearchResultPropertyDetails
            {
                public string Name;
                public object Value;
                public long Score;
            }

            public enum Source
            {
                StaticProperty,
                SerializedProperty
            }

            public FuzzySearchResult(
                string typeName,
                long score,
                Func<FuzzySearchResultPropertyDetails> propDetailsGetter,
                Source source)
            {
                TypeName = typeName;
                Score = score;
                PropertySource = source;
                k_PropDetailsGetter = propDetailsGetter;
            }

            public string TypeName { get; }
            public long Score { get; private set; }
            public Source PropertySource { get; }

            private readonly Func<FuzzySearchResultPropertyDetails> k_PropDetailsGetter;

            string m_PropertyName;
            object m_PropertyValue;

            void GetPropDetails()
            {
                if (m_PropertyName == null)
                {
                    try
                    {
                        var propDetails = k_PropDetailsGetter();
                        if (propDetails != null)
                        {
                            m_PropertyName = propDetails.Name;
                            m_PropertyValue = propDetails.Value;

                            if (propDetails.Score > Score)
                            {
                                Score = propDetails.Score;
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            public string PropertyName
            {
                get
                {
                    GetPropDetails();
                    return m_PropertyName;
                }
            }

            string PropertyValueString
            {
                get
                {
                    GetPropDetails();
                    return m_PropertyValue != null ? m_PropertyValue.ToString() : "<null>";
                }
            }

            public string ToFullString() =>
                PropertySource == Source.SerializedProperty
                    ? PropertyValueString
                    : $"{TypeName}.{PropertyName}: {PropertyValueString}\n";

            public string ToPartialString() =>
                PropertySource == Source.SerializedProperty
                    ? PropertyValueString
                    : $"{PropertyName}: {PropertyValueString}";
        }

        static FuzzySearchResult FuzzyMatch(
            string typeName,
            string propertyName,
            string pattern,
            Func<FuzzySearchResult.FuzzySearchResultPropertyDetails> propDetailsGetter,
            FuzzySearchResult.Source source)
        {
            long score = 0;

            var isMatch = FuzzySearch.FuzzyMatch(
                pattern,
                propertyName != null ? $"{typeName}.{propertyName}" : typeName,
                ref score);

            if (isMatch && score > 100)
            {
                return new FuzzySearchResult(typeName, score, propDetailsGetter, source);
            }

            return null; // Return null for non-matching or low-score results
        }

        static IEnumerable<SerializedProperty> AsEnumerable(SerializedProperty property)
        {
            var current = property.Copy();
            if (current.Next(true))
            {
                do
                {
                    yield return current;
                } while (current.Next(false));
            }
        }

        private static bool FuzzySearchUnityObjectFieldName(
            Object targetObject,
            string rootField,
            out string propertyName,
            out object propertyValue,
            out long score)
        {
            if (targetObject == null || rootField == null)
            {
                propertyName = null;
                propertyValue = null;
                score = 0;
                return false;
            }

            var targetSerializedObject = new SerializedObject(targetObject);

            long outScore = 0;

            var matchedResult = AsEnumerable(targetSerializedObject.GetIterator())
                .Where(prop => FuzzySearch.FuzzyMatch(rootField, prop.name, ref outScore))
                .Select(prop => new Tuple<string, long>(prop.name, outScore))
                .OrderByDescending(result => result.Item2).FirstOrDefault();

            if (matchedResult == default)
            {
                propertyName = null;
                propertyValue = null;
                score = 0;
                return false;
            }

            propertyName = matchedResult.Item1;
            score = matchedResult.Item2;

            if (propertyName == default)
            {
                propertyValue = null;
                return false;
            }

            // Find the property:
            var prop = targetSerializedObject.FindProperty(propertyName);

            propertyName = prop.name;

            k_RootFields[0] = propertyName;
            propertyValue = UnityDataUtils.OutputUnityObject(
                targetObject,
                false, false, useDisplayName: true, includeInstanceID: false,
                rootFields: k_RootFields);

            return true;
        }

        static readonly Dictionary<Type, string> s_typeNames = new();
        static readonly Dictionary<Type, PropertyInfo[]> s_typeProps = new();

        /// <summary>
        /// Calling type.FullName is slow, use this instead.
        /// </summary>
        internal static string GetFullName(Type type)
        {
            if (s_typeNames.TryGetValue(type, out var name))
                return name;

            name = type.FullName;
            name = name?.Replace('+', '.');
            s_typeNames[type] = name;
            return name;
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            if (s_typeProps.TryGetValue(type, out var props))
                return props;

            props = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            s_typeProps[type] = props;
            return props;
        }

        [ContextProvider("ProjectSettingExtractor function exclusively extracts the following Unity project settings: " +
                         "AudioManager, PhysicsManager, NavMeshAreas, MemorySettings, Physics2DSettings, EditorSettings, GraphicsSettings, " +
                         "ShaderGraphSettings, UnityConnectSettings, VFXManager, XRSettings, PresetManager, TagManager, " +
                         "TimeManager, VersionControlSettings, InputManager, PlayerSettings, QualitySettings.")]
        internal static SmartContextToolbox.ExtractedContext ProjectSettingExtractor(
            [Parameter("The specific Unity project setting to extract.")]
            string projectSettingName)
        {
            if (string.IsNullOrEmpty(projectSettingName))
            {
                return null;
            }

            var staticPropertyNameInput = projectSettingName;

            // We're fuzzy matching, avoid lots of ToLower() calls later:
            staticPropertyNameInput = staticPropertyNameInput.ToLowerInvariant();

            // Remove all non-alphanumeric characters except _ from the start and end of the string:
            staticPropertyNameInput = Regex.Replace(staticPropertyNameInput, @"^[^a-zA-Z0-9_]+|[^a-zA-Z0-9_]+$", "");

            // Replace all non-alpha numeric characters except _ with . for fuzzy search of separators:
            staticPropertyNameInput = Regex.Replace(staticPropertyNameInput, @"[^a-zA-Z0-9_]+", ".");

            // Don't call properties that return certain types that should never get sent:
            var propertyReturnTypesToIgnore = new List<Type>
            {
                typeof(Texture2D),
                typeof(Texture),
                typeof(Material),
                typeof(Shader),
                typeof(AssetBundle)
            };

            if (s_AllTypes == null)
            {
                // Search for the class name in all available assemblies:
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // Extract each type from s_AllTypes with exception handling:
                s_AllTypes = assemblies.SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (Exception)
                    {
                        return Enumerable.Empty<Type>();
                    }
                }).ToList();
            }

            // First, try to find matches in static properties of classes:
            var allResults = s_AllTypes
                .SelectMany(type =>
                {
                    // Make sure we should and can call the property:
                    if (type.IsNotPublic || type.IsGenericType || type.ContainsGenericParameters)
                    {
                        return Enumerable.Empty<FuzzySearchResult>();
                    }

                    var typeName = GetFullName(type);

                    // Some types with static constructors cause error logs when trying to get their properties, we should not need these anyway, skip them:
                    if (typeName == "Unity.VisualScripting.UnitBase")
                    {
                        return Enumerable.Empty<FuzzySearchResult>();
                    }

                    return GetProperties(type)
                        .Select(property =>
                            FuzzyMatch(
                                typeName,
                                property.Name,
                                staticPropertyNameInput,
                                delegate // Using reflection to get the property values is slow, only do it if we need to:
                                {
                                    var returnType = property.PropertyType;
                                    if (propertyReturnTypesToIgnore.Contains(returnType))
                                    {
                                        return null;
                                    }

                                    // Don't call methods that return Lists, we don't have any settings that need this, yet.
                                    if (returnType.IsGenericType &&
                                        returnType.GetGenericTypeDefinition() == typeof(List<>))
                                    {
                                        return null;
                                    }

                                    try
                                    {
                                        return new FuzzySearchResult.FuzzySearchResultPropertyDetails
                                        {
                                            Name = property.Name, Value = property.GetValue(null)
                                        };
                                    }
                                    catch (Exception)
                                    {
                                        return null;
                                    }
                                },
                                FuzzySearchResult.Source.StaticProperty))
                        .Where(result => result != null); // Filter out non-matching or low-score results
                }).ToList();

            var settingsResults =
                ProjectSettingExtractor(staticPropertyNameInput, projectSettingName, true);

            allResults.AddRange(settingsResults);

            // Deduplicate by type name, prefer sources from static properties and
            // Sort all results by score and return best:

            allResults = allResults.OrderByDescending(r => r.Score).ToList();

            var stringBuilder = new StringBuilder();
            var result = allResults
                .Where(result => result.PropertyName != null)
                .GroupBy(result => new { result.TypeName, result.PropertyName }) // Group by all relevant properties
                .Select(group =>
                    group.OrderBy(result => result.PropertySource)
                        .First()) // Select the one with the highest Source value from each group
                .OrderByDescending(result => result.Score)
                .ThenByDescending(result => result.PropertySource)
                .GroupBy(result => result.TypeName)
                .Take(k_FinalResultLimit) // Limit number of results to show
                .Aggregate(stringBuilder, (sb, group) =>
                {
                    string resultAsString;
                    if (group.Count() > 1) // Check if there is more than one property in the group
                    {
                        var sortedProperties = group
                            .OrderByDescending(result => result.Score) // Sort properties within each group by score
                            .Select(result => result.ToPartialString());

                        resultAsString =
                            $"{group.Key}:\n{string.Join("\n", sortedProperties)}\n\n"; // Combine type name and its properties into a single string
                    }
                    else
                    {
                        resultAsString = group.First().ToFullString();
                    }

                    return sb.Append(resultAsString);
                });

            return new SmartContextToolbox.ExtractedContext()
            {
                Payload = result.ToString().TrimEnd('\n'),
                ContextType = "project setting"
            };
        }

        private static List<FuzzySearchResult> ProjectSettingExtractor(
            string staticPropertyNameInput,
            string staticPropertyName,
            bool showExpandedSettings)
        {
            // Some settings come from the serialized fields of .asset files found in the ProjectSettings/ folder.
            // Below we assume the setting is this type of setting, and attempt to extract it from the serialized
            // properties of those assets in the asset database
            var settingFiles = UnityDataUtils.GetSettingsAssets();

            // Get settings name from given string, using first non-alphanumeric character as separator:
            var stringParts = Regex.Split(staticPropertyNameInput, @"[^a-zA-Z0-9_]+");

            // Prepare a property backup name to check if the property was not found,
            // this checks against settings with the full given name after the last separator,
            // to handle cases like "Editor Settings/Enter Play Mode Options"

            // Split staticPropertyName on non-alphanumeric characters but not spaces:
            var staticPropertyNameParts = Regex.Split(staticPropertyName, @"[^a-zA-Z0-9_ ]+");

            // The last part could be the property name, if we have a setting name with multiple components:
            string fullPropNameToCheck = null;
            if (staticPropertyNameParts.Count(part => !k_IgnoredNames.Contains(part)) > 1)
            {
                fullPropNameToCheck = staticPropertyNameParts[^1].Replace(" ", ""); // remove spaces
            }


            // Create list for all results:
            var settingsResults = new List<FuzzySearchResult>();

            for (var i = stringParts.Length - 1; i >= 0; i--)
            {
                var potentialSettingName = stringParts[i];

                if (k_IgnoredNames.Contains(potentialSettingName))
                {
                    continue;
                }

                var projectSettingPropertyName = stringParts.Length > i + 1 ? stringParts[i + 1] : null;

                // Check for settings with the given projectSettingName:
                settingsResults.AddRange(
                    settingFiles.Select(settingFile =>
                            new Tuple<Object, string>(
                                settingFile.Item1,
                                settingFile.Item1.GetType() !=
                                typeof(Object) // If the type is Object (because there is no C# type), use its name instead
                                    ? GetFullName(settingFile.Item1.GetType())
                                    : settingFile.Item1.name +
                                      settingFile
                                          .Item2)) // Append the setting file name, this gives a fuzzy match from one string for class name and setting file name
                        .Select(fileToName => FuzzyMatch(
                            fileToName.Item2,
                            null,
                            potentialSettingName,
                            () =>
                            {
                                try
                                {
                                    var settingFile = fileToName.Item1;
                                    var settingsFileName = fileToName.Item2;

                                    var propExists =
                                        FuzzySearchUnityObjectFieldName(
                                            settingFile,
                                            projectSettingPropertyName,
                                            out var propName,
                                            out var propValue,
                                            out var score);

                                    if (!propExists && fullPropNameToCheck != null)
                                    {
                                        propExists =
                                            FuzzySearchUnityObjectFieldName(
                                                settingFile,
                                                fullPropNameToCheck,
                                                out propName,
                                                out propValue,
                                                out score);
                                    }

                                    if (propExists)
                                    {
                                        // Found a property that exists, don't ever show expanded settings:
                                        showExpandedSettings = false;

                                        return new FuzzySearchResult.FuzzySearchResultPropertyDetails
                                        {
                                            Name = propName, Value = propValue, Score = score
                                        };
                                    }

                                    if (showExpandedSettings)
                                    {
                                        return new FuzzySearchResult.FuzzySearchResultPropertyDetails
                                        {
                                            Name = settingsFileName,
                                            Value = UnityDataUtils.OutputUnityObject(settingFile,
                                                false, false, useDisplayName: true, includeInstanceID: false)
                                        };
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError("Failed to get property value.");
                                    Debug.LogException(e);
                                }

                                return null;
                            }, FuzzySearchResult.Source.SerializedProperty))
                        .Where(result => result != null));
            }

            return settingsResults;
        }
    }
}
