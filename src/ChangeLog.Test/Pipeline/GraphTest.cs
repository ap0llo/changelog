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

        public class GetOutgoingEdges
        {
            [Fact]
            public void Throws_ArgumentNullException_is_node_is_null()
            {
                // ARRANGE
                var sut = new Graph<object>();

                // ACT 
                var ex = Record.Exception(() => sut.GetOutgoingEdges(null!));

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
                var ex = Record.Exception(() => sut.GetOutgoingEdges(node));

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
                var edges = sut.GetOutgoingEdges(node);

                // ASSERT
                Assert.Empty(edges);
            }

            [Fact]
            public void Returns_expected_edges()
            {
                // ARRANGE
                var node1 = new object();
                var node2 = new object();
                var node3 = new object();

                var sut = new Graph<object>();
                sut.AddEdge(node1, node2);
                sut.AddEdge(node1, node3);

                // ACT 
                var edges = sut.GetOutgoingEdges(node1);

                // ASSERT
                Assert.Equal(2, edges.Count());
                Assert.All(edges, e => Assert.Equal(node1, e.From));
                Assert.Contains(edges, e => e.To == node2);
                Assert.Contains(edges, e => e.To == node3);
            }
        }

        public class GetIncomingEdges
        {
            [Fact]
            public void Throws_ArgumentNullException_is_node_is_null()
            {
                // ARRANGE
                var sut = new Graph<object>();

                // ACT 
                var ex = Record.Exception(() => sut.GetIncomingEdges(null!));

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
                var ex = Record.Exception(() => sut.GetIncomingEdges(node));

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
                var edges = sut.GetIncomingEdges(node);

                // ASSERT
                Assert.Empty(edges);
            }

            [Fact]
            public void Returns_expected_edges()
            {
                // ARRANGE
                var node1 = new object();
                var node2 = new object();
                var node3 = new object();

                var sut = new Graph<object>();
                sut.AddEdge(node2, node1);
                sut.AddEdge(node3, node1);

                // ACT 
                var edges = sut.GetIncomingEdges(node1);

                // ASSERT
                Assert.Equal(2, edges.Count());
                Assert.All(edges, e => Assert.Equal(node1, e.To));
                Assert.Contains(edges, e => e.From == node2);
                Assert.Contains(edges, e => e.From == node3);
            }
        }

        public class GetCycles
        {
            [Fact]
            public void Returns_empty_enumerable_for_an_empty_graph()
            {
                // ARRANGE
                var sut = new Graph<object>();

                // ACT 
                var cycles = sut.GetCycles();

                // ASSERT
                Assert.Empty(cycles);
            }

            [Fact]
            public void Returns_empty_enumerable_for_a_graph_without_edges()
            {
                // ARRANGE
                var node1 = new object();
                var node2 = new object();
                var sut = new Graph<object>();

                sut.AddNode(node1);
                sut.AddNode(node2);

                // ACT 
                var cycles = sut.GetCycles();

                // ASSERT
                Assert.Empty(cycles);
            }

            [Fact]
            public void Returns_empty_enumerable_if_graph_is_acyclic()
            {
                // ARRANGE
                var sut = new Graph<string>();

                sut.AddNode("node1");
                sut.AddNode("node2");
                sut.AddNode("node3");

                sut.AddEdge("node1", "node2");
                sut.AddEdge("node2", "node3");
                sut.AddEdge("node1", "node3");

                // ACT 
                var cycles = sut.GetCycles();

                // ASSERT
                Assert.Empty(cycles);
            }



            [Fact]
            public void Returns_empty_enumerable_if_graph_contains_multiple_acyclic_components()
            {
                // ARRANGE
                var sut = new Graph<string>();

                sut.AddNode("node10");
                sut.AddNode("node11");

                sut.AddNode("node20");
                sut.AddNode("node21");

                sut.AddEdge("node10", "node11");
                sut.AddEdge("node20", "node21");

                // ACT 
                var cycles = sut.GetCycles();

                // ASSERT
                Assert.Empty(cycles);
            }



            [Fact]
            public void Returns_expected_cycles_for_a_cyclic_graph()
            {
                // ARRANGE
                var sut = new Graph<string>();

                sut.AddNode("node1");
                sut.AddNode("node2");

                sut.AddEdge("node1", "node2");
                sut.AddEdge("node2", "node1");

                // ACT 
                var cycles = sut.GetCycles();

                // ASSERT
                var cycle = Assert.Single(cycles);
                Assert.Collection(
                    cycle,
                    x => Assert.Equal("node1", x),
                    x => Assert.Equal("node2", x),
                    x => Assert.Equal("node1", x)
                );
            }

            [Fact]
            public void Returns_expected_cycles_if_graph_contains_a_self_loop()
            {
                // ARRANGE
                var sut = new Graph<string>();

                sut.AddNode("node1");
                sut.AddEdge("node1", "node1");

                // ACT 
                var cycles = sut.GetCycles();

                // ASSERT
                var cycle = Assert.Single(cycles);
                Assert.Collection(
                     cycle,
                     x => Assert.Equal("node1", x),
                     x => Assert.Equal("node1", x)
                 );
            }
            [Fact]
            public void Returns_expected_cycles_if_graph_contains_multiple_cycles()
            {
                // ARRANGE
                var sut = new Graph<string>();

                sut.AddNode("node1");
                sut.AddNode("node2");
                sut.AddNode("node3");
                sut.AddNode("node4");

                // Cycle1 : node1 -> node2 -> node3 -> node1
                sut.AddEdge("node1", "node2");
                sut.AddEdge("node2", "node3");
                sut.AddEdge("node3", "node1");

                // Cycle2: node4 -> node5 -> node4
                sut.AddEdge("node4", "node5");
                sut.AddEdge("node5", "node4");

                // ACT 
                var cycles = sut.GetCycles();

                // ASSERT
                Assert.Collection(
                     cycles,
                     cycle1 =>
                        Assert.Collection(
                            cycle1,
                            x => Assert.Equal("node1", x),
                            x => Assert.Equal("node2", x),
                            x => Assert.Equal("node3", x),
                            x => Assert.Equal("node1", x)
                        ),
                     cycle2 => Assert.Collection(
                            cycle2,
                            x => Assert.Equal("node4", x),
                            x => Assert.Equal("node5", x),
                            x => Assert.Equal("node4", x)
                        )
                 );
            }
        }
    }
}
