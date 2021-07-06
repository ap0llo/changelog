using System;
using System.Linq;
using Grynwald.ChangeLog.Pipeline;
using Xunit;

namespace Grynwald.ChangeLog.Test.Pipeline
{
    /// <summary>
    /// Tests for <see cref="Graph{T}"/>
    /// </summary>
    public class GraphTest
    {
        public class Nodes
        {
            [Fact]
            public void Nodes_is_initially_empty()
            {
                // ARRANGE
                var sut = new Graph<object>();

                // ACT 

                // ASSERT
                Assert.NotNull(sut.Nodes);
                Assert.Empty(sut.Nodes);
            }
        }

        public class AddNode
        {
            [Fact]
            public void Node_must_not_be_null()
            {
                // ARRANGE
                var sut = new Graph<object>();

                // ACT 
                var ex = Record.Exception(() => sut.AddNode(null!));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("node", argumentNullException.ParamName);
            }

            [Fact]
            public void AddNode_adds_node()
            {
                // ARRANGE
                var node1 = new object();
                var node2 = new object();

                var sut = new Graph<object>();

                // ACT 
                sut.AddNode(node1);
                sut.AddNode(node2);

                // ASSERT
                Assert.Equal(2, sut.Nodes.Count);
                Assert.Contains(sut.Nodes, x => x == node1);
                Assert.Contains(sut.Nodes, x => x == node2);
            }

            [Fact]
            public void A_node_cannot_be_added_twice()
            {
                // ARRANGE
                var node = new object();

                var sut = new Graph<object>();

                // ACT 
                var added1 = sut.AddNode(node);
                var added2 = sut.AddNode(node);

                // ASSERT
                Assert.True(added1);
                Assert.False(added2);
                var addedNode = Assert.Single(sut.Nodes);
                Assert.Same(node, addedNode);
            }
        }

        public class Edges
        {
            [Fact]
            public void Edges_is_initially_empty()
            {
                // ARRANGE
                var sut = new Graph<object>();

                // ACT 

                // ASSERT
                Assert.NotNull(sut.Edges);
                Assert.Empty(sut.Edges);
            }
        }

        public class AddEdge
        {
            [Fact]
            public void Nodes_must_not_be_null()
            {
                // ARRANGE
                var sut = new Graph<object>();

                // ACT / ASSERT
                Assert.Throws<ArgumentNullException>(() => sut.AddEdge(new object(), null!));
                Assert.Throws<ArgumentNullException>(() => sut.AddEdge(null!, new object()));
            }

            [Fact]
            public void AddEdge_implicitly_adds_nodes()
            {
                // ARRANGE
                var node1 = new object();
                var node2 = new object();

                var sut = new Graph<object>();

                // ACT 
                sut.AddEdge(node1, node2);

                // ASSERT
                Assert.Equal(2, sut.Nodes.Count);
                Assert.Contains(sut.Nodes, x => x == node1);
                Assert.Contains(sut.Nodes, x => x == node2);
            }

            [Fact]
            public void Adds_expected_edges()
            {
                // ARRANGE
                var node1 = new object();
                var node2 = new object();
                var node3 = new object();

                var sut = new Graph<object>();

                // ACT 
                var added1 = sut.AddEdge(node1, node2);
                var added2 = sut.AddEdge(node1, node3);

                // ASSERT
                Assert.True(added1);
                Assert.True(added2);
                Assert.Equal(2, sut.Edges.Count());
                Assert.Collection(
                    sut.Edges,
                    e => Assert.Equal(new Graph<object>.Edge(node1, node2), e),
                    e => Assert.Equal(new Graph<object>.Edge(node1, node3), e)
                );
            }

            [Fact]
            public void Edges_cannot_be_added_twice()
            {
                // ARRANGE
                var node1 = new object();
                var node2 = new object();

                var sut = new Graph<object>();

                // ACT 
                var added1 = sut.AddEdge(node1, node2);
                var added2 = sut.AddEdge(node1, node2);

                // ASSERT
                Assert.True(added1);
                Assert.False(added2);
                var edge = Assert.Single(sut.Edges);
                Assert.Same(node1, edge.From);
                Assert.Same(node2, edge.To);
            }
        }

        public class GetAdjacentNodes
        {
            [Fact]
            public void Throws_ArgumentNullException_is_node_is_null()
            {
                // ARRANGE
                var sut = new Graph<object>();

                // ACT 
                var ex = Record.Exception(() => sut.GetAdjacentNodes(null!));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("node", argumentNullException.ParamName);
            }

            [Fact]
            public void Throws_ArgumentNullException_if_not_is_not_found()
            {
                // ARRANGE
                var node = new object();

                var sut = new Graph<object>();

                // ACT 
                var ex = Record.Exception(() => sut.GetAdjacentNodes(node));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentException>(ex);
            }

            [Fact]
            public void Returns_empty_collection_if_node_has_no_neighbours()
            {
                // ARRANGE
                var node = new object();

                var sut = new Graph<object>();
                sut.AddNode(node);

                // ACT 
                var adjacentNodes = sut.GetAdjacentNodes(node);

                // ASSERT
                Assert.Empty(adjacentNodes);
            }

            [Fact]
            public void Returns_adjacent_nodes()
            {
                // ARRANGE
                var node1 = new object();
                var node2 = new object();
                var node3 = new object();

                var sut = new Graph<object>();
                sut.AddEdge(node1, node2);
                sut.AddEdge(node1, node3);

                // ACT 
                var adjacentNodes = sut.GetAdjacentNodes(node1);

                // ASSERT
                Assert.Equal(2, adjacentNodes.Count);
                Assert.Contains(adjacentNodes, x => x == node2);
                Assert.Contains(adjacentNodes, x => x == node3);
            }
        }

    }
}
