using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

// ReSharper disable MethodTooLong

namespace ReflectionExtended
{
    /// <summary>
    /// 
    /// </summary>
    [PublicAPI]
    public sealed class TypeTree
    {
        /// <summary>
        /// 
        /// </summary>
        [NotNull]
        public Type Type => Root.Type;
        /// <summary>
        /// 
        /// </summary>
        [NotNull]
        public Node Root { get; }

        private TypeTree([NotNull] Node root)
        {
            Root = root;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        [NotNull]
        public static TypeTree Create([NotNull] Type root)
        {
            return new TypeTree( Node.FromType( root ) );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromBase"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromBase"></param>
        /// <param name="removeDuplicates"></param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
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
                foreach (Node i in top.Interfaces) { s.Push( i ); }

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

        /// <summary>
        /// 
        /// </summary>
        [PublicAPI]
        [DebuggerDisplay( "Node: {Type.Name}" )]
        public sealed class Node
        {
            [NotNull, ItemNotNull] private readonly List<Node> _interfaces;
            /// <summary>
            /// 
            /// </summary>
            [NotNull]
            public Type Type { get; }
            /// <summary>
            /// 
            /// </summary>
            [PublicAPI, NotNull, ItemNotNull]
            public IReadOnlyList<Node> Interfaces => _interfaces;
            /// <summary>
            /// 
            /// </summary>
            [CanBeNull]
            public Node Ancestor { get; private set; }
            /// <summary>
            /// 
            /// </summary>
            [PublicAPI, CanBeNull]
            public Node Child { get; }

            private Node([NotNull] Type type, [NotNull] Node child) : this( type )
            {
                Child = child;
            }

            private Node([NotNull] Type type)
            {
                Type        = type;
                _interfaces = new List<Node>();
            }

            internal static Node FromType([NotNull] Type type, [CanBeNull] Node child = null)
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

            private void AddInterfaceAncestor([NotNull] Node interfaceNode)
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

            private void AddDirectAncestor([NotNull] Node node)
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
