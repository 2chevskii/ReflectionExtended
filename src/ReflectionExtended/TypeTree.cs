using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReflectionExtended
{
    public sealed class TypeTree
    {
        public Type Type => Root.Type;

        public Node Root { get; }

        private TypeTree(Node root)
        {
            Root = root;
        }

        public static TypeTree Create(Type root)
        {
            return new TypeTree( Node.FromType( root ) );
        }

        public IEnumerable<Node> GetDirectInheritanceChain(bool fromBase = false)
        {
            List<Node> nodes = new List<Node>();
            Node       node  = Root;

            while (node != null)
            {
                nodes.Add( node );
                node = node.Ancestor;
            }

            if (fromBase)
                nodes.Reverse();

            return nodes;
        }

        public IEnumerable<Node> GetFullInheritanceChain(
            bool fromBase         = false,
            bool removeDuplicates = false
        )
        {
            List<Node>  l = new List<Node>();
            Stack<Node> s = new Stack<Node>();

            s.Push( Root );

            while (s.Count != 0)
            {
                Node top = s.Pop();
                foreach (var i in top.Interfaces) { s.Push( i ); }

                if (top.Ancestor != null)
                    s.Push( top.Ancestor );

                if (removeDuplicates && l.Any( node => node.Type == top.Type ))
                    continue;
                l.Add( top );
            }

            if (fromBase)
                l.Reverse();

            return l;
        }

        [DebuggerDisplay( "Node: {Type.Name}" )]
        public sealed class Node
        {
            private List<Node> _interfaces;

            public Type Type { get; }

            public IReadOnlyList<Node> Interfaces => _interfaces;
            public Node Ancestor { get; private set; }
            public Node Child { get; }

            private Node(Type type, Node child) : this( type )
            {
                Child = child;
            }

            private Node(Type type)
            {
                Type        = type;
                _interfaces = new List<Node>();
            }

            internal static Node FromType(Type type, Node child = null)
            {
                Node node = child != null ? new Node( type, child ) : new Node( type );

                foreach (Node interfaceNode in from @interface in type.GetInterfaces()
                                               select FromType( @interface, node ))
                {
                    node.AddInterfaceAncestor( interfaceNode );
                }

                Type baseType = type.BaseType;

                if (baseType != null)
                    node.AddDirectAncestor( FromType( baseType, node ) );

                return node;
            }

            private void AddInterfaceAncestor(Node interfaceNode)
            {
                if (_interfaces.Any( node => node.Type == interfaceNode.Type ))
                    throw new InvalidOperationException(
                        $"Node with type {interfaceNode.Type.Name} is already present in the ancestor list"
                    );

                if (!interfaceNode.Type.IsInterface)
                    throw new ArgumentException( "Given node is not an interface node" );

                if (interfaceNode.Child != this)
                    throw new InvalidOperationException(
                        "Given node's child does not match this node"
                    );

                _interfaces.Add( interfaceNode );
            }

            private void AddDirectAncestor(Node node)
            {
                if (Ancestor != null)
                    throw new InvalidOperationException( "Node already has an ancestor" );

                if (node.Type.IsInterface)
                    throw new ArgumentException( "Interface node cannot be a direct ancestor" );

                if (node.Child != this)
                    throw new InvalidOperationException(
                        "Given node's child does not match this node"
                    );

                Ancestor = node;
            }
        }
    }
}
