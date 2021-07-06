using System;
using System.Collections.Generic;
using Grynwald.Utilities.Collections;

namespace Grynwald.ChangeLog.Pipeline
{
    /// <summary>
    /// Represents a directed graph
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph.</typeparam>
    internal class Graph<T> where T : notnull
    {
        public sealed class Edge : IEquatable<Edge>
        {
            public T From { get; }

            public T To { get; }


            public Edge(T from, T to)
            {
                From = from ?? throw new ArgumentNullException(nameof(from));
                To = to ?? throw new ArgumentNullException(nameof(to));
            }

            public override int GetHashCode() => HashCode.Combine(To, From);

            public override bool Equals(object? obj) => Equals(obj as Edge);

            public bool Equals(Edge? other)
            {
                return other is not null &&
                       From.Equals(other.From) &&
                       To.Equals(other.To);
            }
        }


        private readonly Dictionary<T, HashSet<T>> m_Edges = new Dictionary<T, HashSet<T>>();

        /// <summary>
        /// Gets all the graph's nodes
        /// </summary>
        public IReadOnlyCollection<T> Nodes { get; }

        /// <summary>
        /// Gets all the graph's edges
        /// </summary>
        public IEnumerable<Edge> Edges
        {
            get
            {
                foreach (var (fromNode, to) in m_Edges)
                {
                    foreach (var toNode in to)
                    {
                        yield return new(fromNode, toNode);
                    }
                }
            }
        }


        public Graph()
        {
            Nodes = ReadOnlyCollectionAdapter.Create(m_Edges.Keys);
        }


        /// <summary>
        /// Adds the specified node to the graph.
        /// </summary>
        /// <param name="node">The node to add</param>
        /// <returns>Returns <c>true</c> is the node was added or <c>false</c> if the graph already contained the node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="node"/> is <c>null</c>.</exception>
        public bool AddNode(T node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            if (m_Edges.ContainsKey(node))
                return false;

            m_Edges.Add(node, new HashSet<T>());
            return true;
        }

        /// <summary>
        /// Adds the specified edge to the graph.
        /// </summary>
        /// <remarks>
        /// Adding an edge will implicitly add <paramref name="to"/> and <paramref name="from"/> as nodes to the graph if the graph does not yet contain these nodes.
        /// </remarks>
        /// <param name="from">The edge's start node</param>
        /// <param name="to">The edge's end node</param>
        /// <returns>Returns <c>true</c> is the edge was added or <c>false</c> if the graph already contained the edge.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="from"/> of <see cref="to"/> is <c>null</c>.</exception>
        public bool AddEdge(T from, T to)
        {
            if (from is null)
                throw new ArgumentNullException(nameof(from));

            if (to is null)
                throw new ArgumentNullException(nameof(to));

            AddNode(from);
            AddNode(to);

            return m_Edges[from].Add(to);
        }

        /// <summary>
        /// Gets the adjacent nodes of the specified nodes.
        /// </summary>
        /// <remarks>
        /// Gets all the nodes for which an edge from <see cref="node"/> to the node exists.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="node"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the graph does not contain <see cref="node"/>.</exception>
        public IReadOnlyCollection<T> GetAdjacentNodes(T node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            if (!m_Edges.ContainsKey(node))
                throw new ArgumentException("Specified node was not found in the graph", nameof(node));

            return m_Edges[node];

        }
    }
}
