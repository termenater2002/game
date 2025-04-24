using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;
using Unity.Muse.Common.Editor.Integration;
using UnityEngine.SceneManagement;

namespace Unity.Muse.Chat.Context.SmartContext
{
    internal static partial class ContextRetrievalTools
    {
        internal interface IParentable<T>
        {
            T Parent { get; }
        }

        internal struct ComponentTypeScores
        {
            public Type Type;
            public long Score;
        }

        /// <summary>
        /// Stores object hierarchy in a tree structure.
        /// </summary>
        internal abstract class HierarchyMapEntry<T> where T : IParentable<T>
        {
            internal static int SmartContextLimit { get; set; }

            internal static int EstimatedSerializedLength { get; set; }

            internal static void Reset()
            {
                EstimatedSerializedLength = 0;
            }

            protected readonly T k_ObjectRef;
            public readonly List<HierarchyMapEntry<T>> Children = new();

            public bool Truncated { get; protected set; }

            public abstract string ObjectName { get; }

            protected HierarchyMapEntry(T obj)
            {
                k_ObjectRef = obj;
            }

            protected abstract HierarchyMapEntry<T> CreateInstance(T obj, HierarchyMapEntry<T> parent);

            public override bool Equals(object obj)
            {
                if (obj is not HierarchyMapEntry<T> other)
                    return false;

                if (ObjectName != other.ObjectName)
                    return false;

                if (Children.Count != other.Children.Count)
                    return false;

                for (var i = 0; i < Children.Count; i++)
                {
                    var child1 = Children[i];
                    var child2 = other.Children[i];
                    if (!child1.Equals(child2))
                        return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(k_ObjectRef, Children);
            }

            protected virtual bool Matches(T obj)
            {
                if (obj == null && ObjectName == null)
                {
                    return true;
                }

                return k_ObjectRef != null && k_ObjectRef.Equals(obj);
            }

            HierarchyMapEntry<T> InsertHere(T obj, int depth)
            {
                if (Matches(obj))
                {
                    return this;
                }

                foreach (var childMap in Children)
                {
                    if (childMap.Matches(obj))
                    {
                        return childMap;
                    }
                }

                var container = CreateInstance(obj, this);
                Children.Add(container);
                EstimatedSerializedLength +=
                    container.ObjectName.Length + depth + 1 +
                    2; // depth+1 for `-` prefix, 1 for space and 2 for line break

                return container;
            }

            readonly List<T> m_ParentsList = new();

            public void Insert(T obj)
            {
                // Build a hierarchy of parents:
                m_ParentsList.Clear();

                var parent = obj;
                m_ParentsList.Add(obj);
                while (true)
                {
                    parent = parent.Parent;
                    if (parent == null)
                    {
                        break;
                    }

                    m_ParentsList.Add(parent);
                }

                // Now we know all parents, insert the parents starting at the top of the hierarchy and then the given object:
                var parentEntry = this;
                for (int i = m_ParentsList.Count - 1; i >= 0; i--)
                {
                    parentEntry = parentEntry.InsertHere(m_ParentsList[i], m_ParentsList.Count - i - 1);
                }
            }

            private void Serialize(StringBuilder sb, int depth)
            {
                if (!string.IsNullOrEmpty(ObjectName))
                {
                    sb.Append('-', depth);
                    sb.Append(' ');
                    sb.Append(ObjectName);
                    sb.Append('\n');
                }

                Children.Sort((a, b) => string.Compare(a.ObjectName, b.ObjectName, StringComparison.Ordinal));

                foreach (var child in Children)
                {
                    child.Serialize(sb, depth + 1);
                }
            }

            private void PruneAndSerialize(StringBuilder sb)
            {
                while (true)
                {
                    sb.Clear();

                    // Check how long the resulting string would be and remove children if it's too long:
                    Serialize(sb, 0);
                    if (sb.Length > 0 && sb.Length > SmartContextLimit)
                    {
                        var prunedChildren = false;
                        Prune(ref prunedChildren, 1, GetDepth(0));
                    }
                    else
                    {
                        break;
                    }
                }

                // After pruning, there may be duplicates because of truncated children, try a final collapse and serialize again if needed:
                if (Collapse())
                {
                    sb.Clear();
                    Serialize(sb, 0);
                }
            }

            /// <summary>
            /// Remove duplicates entries in the hierarchy.
            /// </summary>
            protected virtual bool Collapse()
            {
                return false;
            }

            public int GetDepth(int depth)
            {
                return Children.Select(child => child.GetDepth(depth + 1)).Prepend(depth).Max();
            }

            private void Prune(ref bool prunedChildren, int depth, int pruneDepth)
            {
                Truncated = true;

                // Find first child node at pruneDepth that has no children and remove it:
                for (var i = 0; i < Children.Count; i++)
                {
                    var child = Children[i];

                    child.Prune(ref prunedChildren, depth + 1, pruneDepth);

                    if (depth >= pruneDepth)
                    {
                        if (child.Children.Count == 0)
                        {
                            Children.RemoveAt(i);
                            prunedChildren = true;
                        }
                    }

                    if (prunedChildren)
                    {
                        break;
                    }
                }
            }

            public string Serialized()
            {
                var sb = new StringBuilder();

                PruneAndSerialize(sb);

                return sb.ToString();
            }
        }

        internal class GameObjectInfo : IParentable<GameObjectInfo>
        {
            public readonly GameObject Object;
            public GameObjectInfo Parent { get; }
            public readonly int SceneObjectCount;

            public readonly string Name;

            internal GameObjectInfo(GameObject obj, Scene scene = default, string nameSuffix = null)
            {
                Object = obj;

                if (obj != null)
                {
                    Name = obj.name;
                    if (!string.IsNullOrEmpty(nameSuffix))
                    {
                        Name += nameSuffix;
                    }
                }
                else if (scene.IsValid())
                {
                    Name = $"{scene.name} (This is a SubScene)";
                    SceneObjectCount = scene.GetRootGameObjects().Length;
                }

                if (obj == null)
                    return;

                if (obj.transform.parent == null)
                {
                    // Check if obj is in the current scene or a sub scene:
                    if (obj.scene != SceneManager.GetActiveScene())
                    {
                        Parent = new GameObjectInfo(null, obj.scene);
                    }
                }
                else
                {
                    Parent = new GameObjectInfo(obj.transform.parent?.gameObject);
                }
            }
        }

        internal class GameObjectHierarchyMapEntry : HierarchyMapEntry<GameObjectInfo>
        {
            readonly HashSet<GameObject> m_Copies = new(1);

            private GameObject Original { get; }

            private readonly GameObjectHierarchyMapEntry m_Parent;

            public GameObjectHierarchyMapEntry() : this(null, null)
            {
            }

            private GameObjectHierarchyMapEntry(GameObjectInfo obj, GameObjectHierarchyMapEntry parent) : base(obj)
            {
                m_Parent = parent;
                m_Copies.Add(obj?.Object);
                Original = obj?.Object;
            }

            public override bool Equals(object obj)
            {
                if (obj is not GameObjectHierarchyMapEntry other)
                {
                    return false;
                }

                return k_ObjectRef?.Name == other.k_ObjectRef?.Name && TruncatedChildCount > 0 &&
                       other.TruncatedChildCount > 0;
            }

            private bool Equals(GameObjectHierarchyMapEntry other)
            {
                return base.Equals(other) && Equals(m_Copies, other.m_Copies) && Equals(m_Parent, other.m_Parent) && Equals(Original, other.Original);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(base.GetHashCode(), m_Copies, m_Parent, Original);
            }

            public override string ObjectName
            {
                get
                {
                    var result = k_ObjectRef?.Name;

                    if (m_Copies.Count > 1)
                    {
                        result += $" - There are {m_Copies.Count} objects with this name";
                    }

                    // Check for truncated children:
                    var truncatedChildren = TruncatedChildCount;
                    if (truncatedChildren > 0)
                    {
                        result += " - There are truncated children";
                        Truncated = true;
                    }

                    return result;
                }
            }

            private int TruncatedChildCount
            {
                get
                {
                    if (k_ObjectRef == null)
                    {
                        return -1;
                    }

                    if (!k_ObjectRef.Object)
                    {
                        return k_ObjectRef.SceneObjectCount - Children.Count;
                    }

                    var childCopyCount = Children.Sum(child => ((GameObjectHierarchyMapEntry)child).m_Copies.Count);

                    return k_ObjectRef.Object.transform.childCount - childCopyCount;
                }
            }

            protected override HierarchyMapEntry<GameObjectInfo> CreateInstance(GameObjectInfo obj,
                HierarchyMapEntry<GameObjectInfo> parent)
            {
                return new GameObjectHierarchyMapEntry(obj, parent as GameObjectHierarchyMapEntry);
            }

            static bool DoNamesMatch(Transform a, Transform b)
            {
                return a?.name.GetHashCode() == b?.name.GetHashCode();
            }

            protected override bool Matches(GameObjectInfo obj)
            {
                // If either of the object infos has no game object, check if both have none and their names match:
                if (k_ObjectRef?.Object == null || obj.Object == null)
                {
                    return k_ObjectRef?.Object == null && obj.Object == null && k_ObjectRef?.Name == obj.Name;
                }

                if (obj == k_ObjectRef || obj.Object == k_ObjectRef?.Object)
                {
                    return true;
                }

                if (DoNamesMatch(k_ObjectRef.Object.transform, obj.Object.transform))
                {
                    if (DoChildrenMatch(k_ObjectRef.Object.transform, obj.Object.transform))
                    {
                        // Only add the object to the copies list if the parent is the first copy of the object:
                        if (m_Parent == null || m_Parent.Original == obj.Object.transform.parent?.gameObject)
                        {
                            m_Copies.Add(obj.Object);
                        }

                        return true;
                    }

                    return false;

                    static bool DoChildrenMatch(Transform a, Transform b)
                    {
                        if (a.childCount != b.childCount)
                        {
                            return false;
                        }

                        if (!DoNamesMatch(a, b))
                        {
                            return false;
                        }

                        for (int childIdx = 0; childIdx < a.transform.childCount; childIdx++)
                        {
                            if (!DoChildrenMatch(a.GetChild(childIdx), b.GetChild(childIdx)))
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }

                return false;
            }

            protected override bool Collapse()
            {
                bool collapsed = false;

                // Remove children on the same level that have the same name and child hierarchy:
                for (var i = 0; i < Children.Count; i++)
                {
                    var child = (GameObjectHierarchyMapEntry)Children[i];
                    collapsed |= child.Collapse();

                    for (var j = i + 1; j < Children.Count; j++)
                    {
                        var otherChild = (GameObjectHierarchyMapEntry)Children[j];
                        collapsed |= otherChild.Collapse();

                        if (child.Equals(otherChild))
                        {
                            Children.RemoveAt(j);
                            j--;
                            collapsed = true;

                            child.m_Copies.UnionWith(otherChild.m_Copies);
                        }
                    }
                }

                return collapsed;
            }
        }

        [ContextProvider(
            "Returns the hierarchy of gameObjects in the scene matching the given name filter." +
            "If no name filter is provided, all gameObjects in the scene are returned.")]
        internal static SmartContextToolbox.ExtractedContext SceneHierarchyExtractor(
            [Parameter(
                "Filters to specify which gameObjects' hierarchies to return. Use an empty list if the full scene hierarchy is needed. " +
                "Optional: Add one component type per gameObject only where we want the component to exist or be absent, by adding a suffix in the name starting with '|type:' followed by the component type name. " +
                "To search for a component on any gameObject a '*' wildcard instead of an object name works, e.g. the parameter '*|type:Light' searches all Light components without checking object names.")]
            params string[] gameObjectNameFilters)
        {
            GameObjectHierarchyMapEntry.SmartContextLimit = SmartContextToolbox.SmartContextLimit;

            // Store all objects in a tree structure first, then serialize it:
            var hierarchyMap = new GameObjectHierarchyMapEntry();

            // Get all gameObjects:
            var allObjects = Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID);

            // Loop through all GameObjects and if their names are in the list of args, add them to the hierarchy map:
            ICollection<GameObject> objectsToSearch;

            GameObjectHierarchyMapEntry.Reset();

            var finalGameObjectFilters = new List<string>();
            var finalComponentFilters = new List<string>();

            // For each filtered name create a matching assetTypeFilters entry, an empty string or a given type
            string[] componentFilters = null;
            if (gameObjectNameFilters is { Length: > 0 })
            {
                for (int i = 0; i < gameObjectNameFilters.Length; i++)
                {
                    var filter = gameObjectNameFilters[i];

#if MUSE_INTERNAL
                    Debug.Log($"SceneHierarchyExtractor - Checking '{gameObjectNameFilters[i]}' for any type filters...");
#endif

                    // See if "|type:" or "|~type:" is in the string, and extract the two strings before and after that
                    var split = Regex.Split(filter, @"\|type:|\|~type:");
                    if (split.Length == 2)
                    {
                        var name = split[0];

                        // Note: Smart Context thinks '!' is a valid negation operator
                        if (name is "*" or "!" or "!*")
                        {
                            name = "";
                        }

                        // Has another type at the start
                        if (name.StartsWith("type:"))
                        {
                            var firstType = name.Substring("type:".Length);

                            name = "";

                            if (!NameAndTypeExist(finalGameObjectFilters, finalComponentFilters, name, firstType))
                            {
                                finalGameObjectFilters.Add(name);
                                finalComponentFilters.Add(firstType);

#if MUSE_INTERNAL
                                Debug.Log(
                                    $"Found 1st additional object name '{name}' with component filter: '{firstType}'");
#endif
                            }
                        }

                        if (!NameAndTypeExist(finalGameObjectFilters, finalComponentFilters, name, split[1]))
                        {
                            finalGameObjectFilters.Add(name);
                            finalComponentFilters.Add(split[1]);

#if MUSE_INTERNAL
                            Debug.Log($"Found object name '{name}' with component filter: '{split[1]}'");
#endif
                        }
                    }
                    else
                    {
                        if (!NameAndTypeExist(finalGameObjectFilters, finalComponentFilters, gameObjectNameFilters[i], string.Empty))
                        {
                            finalGameObjectFilters.Add(gameObjectNameFilters[i]);
                            finalComponentFilters.Add(string.Empty);
                        }
                    }
                }
            }

            gameObjectNameFilters = finalGameObjectFilters.ToArray();
            componentFilters = finalComponentFilters.ToArray();

            Dictionary<GameObject, List<ContextRetrievalHelpers.ObjectAndScore>> componentScores = new();
            Dictionary<Type, long> componentTypeScores = new();

            if (gameObjectNameFilters == null || gameObjectNameFilters.Length == 0 || gameObjectNameFilters[0] == "*")
            {
                if (componentFilters is { Length: > 0 })
                {
                    // No object names provided, just filter all objects by types
                    objectsToSearch = new HashSet<GameObject>();

                    foreach (var obj in allObjects)
                    {
                        var scores = GetComponentTypeScores(obj, componentFilters,
                            o => o is Component c ? c.GetType().Name : o.name);

                        if (scores?.Count > 0)
                        {
                            objectsToSearch.Add(obj);

                            foreach (var score in scores)
                            {
                                if (!componentTypeScores.ContainsKey(score.Type))
                                {
                                    componentTypeScores[score.Type] = score.Score;
                                }
                            }
                        }
                    }
                }
                else
                {
                    objectsToSearch = allObjects;
                }
            }
            else
            {
                objectsToSearch = new HashSet<GameObject>();
                for (int i = 0; i < gameObjectNameFilters.Length; ++i)
                {
                    var filter = gameObjectNameFilters[i];
                    var componentType = componentFilters?.Length > i ? componentFilters[i] : null;

                    foreach (var obj in ContextRetrievalHelpers.FuzzyObjectSearch(filter, allObjects))
                    {
                        if (!string.IsNullOrEmpty(componentType))
                        {
                            var scores = GetComponentTypeScores(obj, componentFilters,
                                o => o is Component c ? c.GetType().Name : o.name);

                            if (scores?.Count > 0)
                            {
                                objectsToSearch.Add(obj);

                                foreach (var score in scores)
                                {
                                    if (!componentTypeScores.ContainsKey(score.Type))
                                    {
                                        componentTypeScores[score.Type] = score.Score;
                                    }
                                }
                            }
                        }
                        else
                        {
                            objectsToSearch.Add(obj);
                        }
                    }
                }
            }

#if MUSE_INTERNAL
            if (componentTypeScores.Count > 0)
            {
                var deb = "Scores for found component types:\n";
                componentTypeScores.ToList().OrderByDescending(e => e.Value)
                    .ToList().ForEach(e => deb += $"- Type '{e.Key.Name}': Score {e.Value}\n");
                Debug.Log(deb);
            }
#endif

            // Make sort faster by caching depth instead of recalculating every time:
            Dictionary<GameObject, int> depthCache = new();
            objectsToSearch = new List<GameObject>(objectsToSearch.OrderBy(GetDepth));

            // Build the final list of types we found
            if (componentTypeScores.Count > 0)
            {
                var finalComponentTypes = new List<string>();

                foreach (var e in componentTypeScores)
                {
                    finalComponentTypes.Add(e.Key.Name);
                }

                componentFilters = finalComponentTypes.ToArray();
            }

            var sortedComponents = componentFilters.OrderBy(s => s).ToArray();

            HashSet<int> foundSortedComponents = new();

            foreach (var obj in objectsToSearch)
            {
                string suffix = "";
                var matchingComponents = TryFindComponentTypes(obj, sortedComponents);
                if (matchingComponents?.Count > 0)
                {
                    for (int i = 0; i < sortedComponents.Length; i++)
                    {
                        if (suffix.Length == 0)
                            suffix = " : ";

                        var name = sortedComponents[i];

                        if (matchingComponents.Contains(i))
                        {
                            foundSortedComponents.Add(i);
                            suffix += "Has " + name + ". ";
                        }
                        else
                        {
                            suffix += "Has NO " + name + ". ";
                        }
                    }
                }

                hierarchyMap.Insert(new GameObjectInfo(obj, default, suffix));

                if (GameObjectHierarchyMapEntry.EstimatedSerializedLength >
                    GameObjectHierarchyMapEntry.SmartContextLimit)
                {
                    break;
                }
            }

            if (hierarchyMap.Children.Count == 0)
            {
                return null;
            }

            string componentDescription = "";
            if (foundSortedComponents.Count > 0)
            {
                componentDescription +=
                    "We searched in the scene hierarchy for all objects with any of those components the user wanted to filter:\n";

                foreach (var componentIdx in foundSortedComponents)
                {
                    componentDescription += $"- {sortedComponents[componentIdx]}\n";
                }

                componentDescription +=
                    "\nWe only list scene objects with components we searched for, others are not shown in the hierarchy below because the user implicitly ignored them in their prompt." +
                    $"\nIf any of the following object names in the hierarchy is followed by a note like \"Has {componentFilters[0]}.\", this means this component was found on the object, otherwise is was missing and has a note like \"Has NO {componentFilters[0]}.\".";

                componentDescription += "\n\n";
            }

            return new SmartContextToolbox.ExtractedContext
            {
                Payload = componentDescription + hierarchyMap.Serialized(),
                ContextType = "scene hierarchy",
                Truncated = hierarchyMap.Truncated
            };

            // Sort objects by their depth in the hierarchy:
            int GetDepth(GameObject obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                if (!depthCache.TryGetValue(obj, out var depth))
                {
                    depth = GetDepth(obj.transform.parent?.gameObject) + 1;
                    depthCache[obj] = depth;
                }

                return depth;
            }

            bool NameAndTypeExist(List<string> finalGameObjectFilters, List<string> finalComponentFilters, string name, string component)
            {
                for (int i = 0; i < finalGameObjectFilters.Count; i++)
                {
                    if (finalGameObjectFilters[i] == name && finalComponentFilters[i] == component)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static List<ComponentTypeScores> GetComponentTypeScores(GameObject gameObject, string[] componentFilters,
            Func<Object, string> customNameFunc = null)
        {
            // fuzzy match componentFilters strings to any components on the gameObject
            List<ComponentTypeScores> componentMatches = new();

            var objectMatches = GetComponentMatches(gameObject, componentFilters, customNameFunc);

            foreach (var objectMatch in objectMatches)
            {
                var t = objectMatch.Object.GetType();
                if (componentMatches.All(e => e.Type != t))
                {
                    componentMatches.Add(new ComponentTypeScores { Type = t, Score = objectMatch.Score });
                }
            }

            return componentMatches;
        }

        private static List<ContextRetrievalHelpers.ObjectAndScore> GetComponentMatches(GameObject gameObject,
            string[] componentFilters, Func<Object, string> customNameFunc = null)
        {
            // fuzzy match componentFilters strings to any components on the gameObject
            List<ContextRetrievalHelpers.ObjectAndScore> componentMatches = new();

            var objectsToSearch = gameObject.GetComponents<Component>();

            foreach (var componentName in componentFilters)
            {
                if (string.IsNullOrEmpty(componentName))
                    continue;

                var matches = ContextRetrievalHelpers.FuzzyObjectSearchWithScore(componentName, objectsToSearch, customNameFunc);
                if (matches.Count() > 0)
                {
                    componentMatches.AddRange(matches);
                }
            }

            return componentMatches;
        }


        private static List<int> TryFindComponentTypes(GameObject obj, string[] componentTypes)
        {
            if (obj == null || componentTypes == null || componentTypes.Length == 0)
                return null;

            List<int> foundTypes = new();

            for (int i = 0; i < componentTypes.Length; ++i)
            {
                var componentType = componentTypes[i];

                if (obj.GetComponent(componentType))
                {
                    foundTypes.Add(i);
                }
                // TODO: Maybe better to not use/allow "UnityEngine." prefix in the first place
                else if (componentType.Contains("UnityEngine.", StringComparison.OrdinalIgnoreCase))
                {
                    var shortType = componentType.Substring("UnityEngine.".Length);

                    if (obj.GetComponent(shortType))
                    {
                        foundTypes.Add(i);
                    }
                }
            }

            return foundTypes;
        }
    }
}
