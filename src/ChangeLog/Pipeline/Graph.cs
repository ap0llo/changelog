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


        private readonly Dictionary<T, HashSet<Edge>> m_OutgoingEdges = new Dictionary<T, HashSet<Edge>>();
        private readonly Dictionary<T, HashSet<Edge>> m_IncomingEdges = new Dictionary<T, HashSet<Edge>>();
        private readonly HashSet<T> m_Nodes = new HashSet<T>();
        private readonly HashSet<Edge> m_Edges = new HashSet<Edge>();


        /// <summary>
        /// Gets all the graph's nodes
        /// </summary>
        public IReadOnlyCollection<T> Nodes => m_Nodes;

        /// <summary>
        /// Gets all the graph's edges
        /// </summary>
        public IReadOnlyCollection<Edge> Edges => m_Edges;


        public Graph()
        { }


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

            if (!m_Nodes.Add(node))
                return false;

            m_OutgoingEdges.Add(node, new HashSet<Edge>());
            m_IncomingEdges.Add(node, new HashSet<Edge>());
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

            var edge = new Edge(from, to);

            m_OutgoingEdges[from].Add(edge);
            m_IncomingEdges[to].Add(edge);
            return m_Edges.Add(edge);
        }

        /// <summary>
        /// Gets all edges that start at the specified node
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="node"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the graph does not contain <see cref="node"/>.</exception>
        public IReadOnlyCollection<Edge> GetOutgoingEdges(T node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            if (!m_Nodes.Contains(node))
                throw new ArgumentException("Specified node was not found in the graph", nameof(node));

            return m_OutgoingEdges[node];
        }

        /// <summary>
        /// Gets all edges that end at the specified node
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="node"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the graph does not contain <see cref="node"/>.</exception>
        public IReadOnlyCollection<Edge> GetIncomingEdges(T node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            if (!m_Nodes.Contains(node))
                throw new ArgumentException("Specified node was not found in the graph", nameof(node));

            return m_IncomingEdges[node];
        }

    }
}
