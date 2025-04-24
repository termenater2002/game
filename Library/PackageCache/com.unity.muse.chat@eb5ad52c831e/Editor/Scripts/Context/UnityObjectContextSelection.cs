using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Chat
{
    /// <summary>
    /// Allows a Unity object or asset to be sent to the LLM for evaluation
    /// </summary>
    internal class UnityObjectContextSelection : IContextSelection
    {
        Object m_Target;

        static readonly List<string> k_ExtensionsToExtract = new() { ".cs", ".json", ".shader", ".uxml" };

        public bool IncludeObjectName = true;

        public GameObject GameObject => m_Target as GameObject;
        public Object Target => m_Target;

        public int MaxObjectDepth = 1;
        public bool IncludeFileContents = true;

        // The serialized json string will be limited to this length:
        public int SerializationLimit = MuseChatConstants.PromptContextLimit;

        public void SetTarget(Object target)
        {
            m_Target = target;
        }

        string IContextSelection.Classifier
        {
            get
            {
                if (m_Target == null)
                    return "Null";

                // We might want to special path for gameobjects to include all their components
                return $"UnityEngine.Object, {m_Target.GetType().Name}";
            }
        }

        string IContextSelection.Description
        {
            get
            {
                if (m_Target == null)
                    return "No object selected";

                return $"{m_Target.name} - {m_Target.GetType().Name}";
            }
        }

        string IContextSelection.Payload
        {
            get
            {
                if (m_Target == null)
                    return null;

                string path = AssetDatabase.GetAssetPath(m_Target);
                string fileContents = null;
                if (IncludeFileContents)
                {
                    if (k_ExtensionsToExtract.Contains(Path.GetExtension(path)))
                    {
                        fileContents = File.ReadAllText(path);
                    }
                    else if (m_Target is MonoBehaviour mono)
                    {
                        var monoScript = MonoScript.FromMonoBehaviour(mono);
                        fileContents = monoScript.text;
                    }
                }

                var json = UnityDataUtils.OutputUnityObject(m_Target, false, false, MaxObjectDepth,
                    outputDirectory: true, includeObjectName: IncludeObjectName,
                    jsonLengthLimit: SerializationLimit);

                if (fileContents != null)
                {
                    return
                        $"\n{json}" +
                        $"\n\nFile contents:\"\n{fileContents}\"";
                }

                return $"\n{json}";
            }
        }

        string IContextSelection.DownsizedPayload
        {
            get
            {
                if (m_Target == null)
                    return null;

                return $"\n{UnityDataUtils.OutputUnityObject(m_Target, false, false, 0, includeObjectName: IncludeObjectName, jsonLengthLimit: SerializationLimit)}";
            }
        }

        string IContextSelection.ContextType
        {
            get
            {
                if (m_Target == null)
                    return null;

                string path = AssetDatabase.GetAssetPath(m_Target);
                if (path.EndsWith(".cs"))
                {
                    return "monoscript";
                }

                if (path.EndsWith(".json"))
                {
                    return "json";
                }
                return m_Target.GetType().Name;
            }
        }

        string IContextSelection.TargetName => $"{m_Target.name}";

        bool? IContextSelection.Truncated => null;

        bool System.IEquatable<IContextSelection>.Equals(IContextSelection other)
        {
            if (other is not UnityObjectContextSelection otherSelection)
                return false;

            return otherSelection.m_Target == m_Target;
        }
    }
}
