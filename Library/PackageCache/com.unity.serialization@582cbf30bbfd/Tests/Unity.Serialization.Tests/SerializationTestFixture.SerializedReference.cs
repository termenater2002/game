using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Properties;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        internal class WithGenericsBase {}
        [GeneratePropertyBag]
        internal class WithGenerics<T> : WithGenericsBase
        {
            public T Value;
        }
        
        [GeneratePropertyBag]
        internal class Node : IEnumerable<Node>
        {
            [CreateProperty] string m_Name;
            [CreateProperty] Node m_Parent;
            [CreateProperty] List<Node> m_Children = new List<Node>();
            
            public Node() { }
            public Node(string name) => m_Name = name;

            public Node Parent => m_Parent;
            public List<Node> Children => m_Children;

            public void Add(Node child)
            {
                m_Children.Add(child);
                child.m_Parent = this;
            }

            public IEnumerator<Node> GetEnumerator()
                => m_Children.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => m_Children.GetEnumerator();
        }
        
        [Test]
        public void ClassWithSerializedReferences_ReferencesAreMaintained()
        {
            var node = new Node("node");
            
            var src = new List<Node>
            {
                node,
                node
            };

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst[0], Is.SameAs(dst[1]));
        }
        
        [Test]
        public void ClassWithSerializedReferences_WithDisableSerializedReferencesSet_ReferencesAreMaintained()
        {
            var node = new Node("node");
            
            var src = new List<Node>
            {
                node,
                node
            };

            var parameters = new CommonSerializationParameters {DisableSerializedReferences = true};

            var dst = SerializeAndDeserialize(src, parameters);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst[0], Is.Not.SameAs(dst[1]));
        }
        
        [Test]
        public void ClassWithRecursiveReferences_CanBeSerializedAndDeserialized()
        {
            var src = new Node("root")
            {
                new Node("a"),
                new Node("b"),
                new Node("c")
            };

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));

            AssertThatParentReferencesAreSet(dst);
            
            void AssertThatParentReferencesAreSet(Node node)
            {
                foreach (var child in node)
                {
                    Assert.That(child.Parent, Is.EqualTo(node));
                    AssertThatParentReferencesAreSet(child);
                }
            }
        }
        
        [Test]
        public void AbstractClassWithNonPrimitiveGenericsInheritor_CanBeSerializedAndDeserialized()
        {
            // The downcast is important to repro the bug
            // downcasting means JsonPropertyWriter will add a $type field to the json as the actual type differs from
            // the static type
            // The generic argument is a non-primitive type, as those require an assembly name to be deserialized (when
            // Type.GetType() is used, "UnityEngine.Vector3" won't work, but
            // "UnityEngine.Vector3, UnityEngine. CoreModule" will). The previous behaviour only qualified the root type
            // (here, WithGenerics), but not the (nested or not) generic arguments.
            WithGenericsBase src = new WithGenerics<UnityEngine.Vector3> {Value = new(1, 2, 3)};

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(((WithGenerics<UnityEngine.Vector3>)dst).Value, Is.EqualTo(new UnityEngine.Vector3(1,2,3)));
        }
        
        [Test]
        public void ClassWithReferenceToSelf_CanBeSerializedAndDeserialized()
        {
            var src = new Node("root");
            
            src.Add(src);

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            
            Assert.That(dst, Is.SameAs(dst.Parent));
            Assert.That(dst, Is.SameAs(dst.Children[0]));
        }
        
        [Test]
        public void ClassWithCircularReferences_CanBeSerializedAndDeserialized()
        {
            var a = new Node("a");
            
            var src = new Node("root")
            {
                a
            };

            a.Add(src);

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Parent, Is.SameAs(dst.Children[0]));
            Assert.That(dst.Children[0].Parent, Is.SameAs(dst));
        }
    }
}