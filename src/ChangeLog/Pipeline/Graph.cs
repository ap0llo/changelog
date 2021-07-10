using System;
using System.Collections.Generic;
using System.Linq;

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

            var added = m_Edges.Add(edge);

            return added;
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

        /// <summary>
        /// Enumerates the graph's cycles
        /// </summary>
        public IEnumerable<IReadOnlyList<T>> GetCycles()
        {
            // No need to check for cycles if there aren't any edges
            if (m_Edges.Count == 0)
                yield break;


            bool CheckForCycles(T currentNode, HashSet<T> allVisitedNodes, Stack<T> currentPath)
            {
                allVisitedNodes.Add(currentNode);
                currentPath.Push(currentNode);

                // Recurse for all outoging edges
                foreach (var edge in GetOutgoingEdges(currentNode))
                {
                    // We reached a node already on the current path => we found a cycle
                    if (currentPath.Contains(edge.To))
                    {
                        // Add the current node to the path, so it the last node is in the path twice
                        // e.g. node1 -> node2 -> node1
                        currentPath.Push(edge.To);
                        return true;
                    }

                    // check for paths recursively, abort when the first cycle is found
                    if (CheckForCycles(edge.To, allVisitedNodes, currentPath))
                        return true;
                }

                // Remove the current node from the path,
                // there are no cycles that go through the current node
                currentPath.Pop();
                return false;
            }


            // The graph might contain nodes without incoming edges
            // so we need to consider all nodes as a possible start point for a cycle
            var startNodes = new HashSet<T>(m_Nodes);
            while (startNodes.Count > 0)
            {
                // Start Searching at the first unprocessed node
                var currentNode = startNodes.First();

                // The set of all nodes visited when doing a depth-first search starting at 'currentNode' 
                var visitedNodes = new HashSet<T>();

                // The stack keeps track of the path currently being visited
                var cycle = new Stack<T>();

                // Visit the graph and search for cycles
                if (CheckForCycles(currentNode, visitedNodes, cycle))
                {
                    // The stack contains the path in reverse order
                    // => return the reversed stack to the returned cycle is in the order in which the nodes were visited
                    yield return cycle.Reverse().ToList();
                }

                // Remove all nodes visited from the set of start nodes
                // There is no need to start searching for cycles beginning at these nodes again
                // as they already were visited when starting from 'currentNode'
                startNodes.ExceptWith(visitedNodes);
            }

        }
    }
}
