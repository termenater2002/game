using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// A tree structure defined by a collection of '/'-separated paths.
    /// </summary>
    /// <example>
    /// Let's say you have the following entries:
    /// <code>
    /// 
    /// "Assets/My Folder/My File 1"
    /// "Library/My File 2"
    /// "Library/My File 3"
    /// "Assets/My Folder 2/My File 4"
    /// "My File 5"
    ///
    /// </code>
    ///
    /// The <see cref="PathTree{T}"/> will look like this (top-level nodes are children of the root node):
    /// <code>
    ///
    /// Assets
    ///   |- My Folder
    ///   |   |- My File 1
    ///   |- My Folder 2
    ///       |- My File 4
    /// Library
    ///   |- My File 2
    ///   |- My File 3
    /// My File 5
    ///  
    /// </code>
    /// Note that "Assets", "Assets/My Folder 1", "Assets/My Folder 2", and "Library" are not explicitly
    /// in the list of entries, so their respective nodes are created implicitly. The user data in this case
    /// will be initialized to the default value of <typeparamref name="T"/>.
    /// 
    /// </example>
    /// <typeparam name="T">Type of data contained within each node.</typeparam>
    class PathTree<T>
    {
        public Node Root { get; } = new(default);

        public class Node
        {
            readonly Dictionary<string, Node> m_Children;

            public IEnumerable<Node> ChildNodes => m_Children.Values;

            public T Data { get; }

            public Node(T data)
            {
                Data = data;
                m_Children = new Dictionary<string, Node>();
            }

            public void AddChild(string path, Node childNode) => m_Children.Add(path, childNode);
            
            public bool TryGetChild(string path, out Node child) => m_Children.TryGetValue(path, out child);
        }

        public static PathTree<T> Build(Dictionary<string, T> entries)
        {
            var tree = new PathTree<T>();
            foreach (var (path, _) in entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(path));
                
                var current = tree.Root;
                var index = 0;
                var nextIndex = 0;
                while (nextIndex < path.Length && index > -1)
                {
                    index = path.IndexOf('/', nextIndex);
                    nextIndex = index + 1;

                    var partialPath = index >= 0 ? path.Substring(0, index) : path;
                    
                    if (!current.TryGetChild(partialPath, out var childNode))
                    {
                        childNode = new Node(entries.TryGetValue(partialPath, out var childData)
                            ? childData
                            : default);
                        current.AddChild(partialPath, childNode);
                    }

                    current = childNode;
                }
            }

            return tree;
        }
    }
}
