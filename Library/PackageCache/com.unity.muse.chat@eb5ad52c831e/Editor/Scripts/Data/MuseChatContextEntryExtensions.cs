using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Muse.Chat
{
    internal static class MuseChatContextEntryExtensions
    {
        public static void Activate(this MuseChatContextEntry entry)
        {
            switch (entry.EntryType)
            {
                case MuseChatContextType.Component:
                case MuseChatContextType.HierarchyObject:
                case MuseChatContextType.SceneObject:
                {
                    var targetObject = entry.GetTargetObject();
                    if (targetObject != null)
                    {
                        Selection.activeObject = targetObject;
                        EditorGUIUtility.PingObject(targetObject);
                    }

                    break;
                }
            }
        }

        public static MuseChatContextEntry GetContextEntry(this LogData logData)
        {
            var result = new MuseChatContextEntry
            {
                Value = logData.Message,
                ValueType = logData.Type.ToString(),
                EntryType = MuseChatContextType.ConsoleMessage
            };

            return result;
        }

        public static MuseChatContextEntry GetContextEntry(this Object source)
        {
            if (AssetDatabase.Contains(source))
            {
                var result = new MuseChatContextEntry
                {
                    DisplayValue = source.name,
                    Value = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(source)).ToString(),
                    ValueType = source.GetType().FullName,
                    EntryType = MuseChatContextType.HierarchyObject
                };

                return result;
            }

            if (source is Component component)
            {
                var result = new MuseChatContextEntry
                {
                    DisplayValue = source.name,
                    Value = component.gameObject.GetObjectHierarchy(),
                    ValueType = component.GetType().FullName,
                    ValueIndex = component.GetComponentIndex(),
                    EntryType = MuseChatContextType.Component
                };

                return result;
            }

            if (source is GameObject gameObject)
            {
                var result = new MuseChatContextEntry
                {
                    DisplayValue = source.name,
                    Value = gameObject.GetObjectHierarchy(),
                    ValueType = source.GetType().FullName,
                    EntryType = MuseChatContextType.SceneObject
                };

                return result;
            }

            throw new InvalidDataException("Source is not a valid Object for " + typeof(MuseChatContextEntry));
        }

        public static Component GetComponent(this MuseChatContextEntry entry)
        {
            switch (entry.EntryType)
            {
                case MuseChatContextType.Component:
                {
                    var host = GameObject.Find(entry.Value);
                    if (host == null)
                    {
                        return null;
                    }

                    Component candidate = null;
                    var components = host.GetComponents<Component>();
                    for (var i = 0; i < components.Length; i++)
                    {
                        if (components[i] == null || components[i].GetType().FullName != entry.ValueType)
                        {
                            continue;
                        }

                        if (candidate == null)
                        {
                            candidate = components[i];
                        }

                        if (i == entry.ValueIndex)
                        {
                            // We found the exact component we want
                            candidate = components[i];
                            break;
                        }
                    }

                    return candidate;
                }

                default:
                {
                    throw new InvalidOperationException("Invalid Type for GetComponent: " + entry.EntryType);
                }
            }
        }

        public static LogData GetLogData(this MuseChatContextEntry entry)
        {
            switch (entry.EntryType)
            {
                case MuseChatContextType.ConsoleMessage:
                {
                    var result = new LogData
                    {
                        Message = entry.Value,
                        Type = Enum.Parse<LogDataType>(entry.ValueType)
                    };

                    return result;
                }

                default:
                {
                    throw new InvalidOperationException("Invalid Type for GetLogData: " + entry.EntryType);
                }
            }
        }

        public static Object GetTargetObject(this MuseChatContextEntry entry)
        {
            switch (entry.EntryType)
            {
                case MuseChatContextType.Component:
                case MuseChatContextType.SceneObject:
                {
                    return GameObject.Find(entry.Value);
                }

                case MuseChatContextType.HierarchyObject:
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(entry.Value);
                    if (string.IsNullOrEmpty(assetPath))
                    {
                        return null;
                    }

                    var type = typeof(Object);

                    if (AssetDatabase.GetImporterType(assetPath) == typeof(ModelImporter))
                    {
                        type = typeof(Mesh);
                    }

                    return AssetDatabase.LoadAssetAtPath(assetPath, type);
                }

                default:
                {
                    throw new InvalidOperationException("Invalid Type for GetTargetObject: " + entry.EntryType);
                }
            }
        }
    }
}
