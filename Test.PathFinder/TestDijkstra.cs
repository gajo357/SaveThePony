using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathFinder.Models;
using PathFinder.Services;

namespace Test.PathFinder
{
    [TestClass]
    public class TestDijkstra
    {
        [TestMethod]
        public void TestDijkstra_NetworkWithWeights()
        {
            var network = new Network();
            var nodes = new[]
            {
                new Node(0),
                new Node(1),
                new Node(2),
                new Node(3),
                new Node(4),
                new Node(5),
                new Node(6)
            };

            network.MakeLink(nodes[0], nodes[2], 3);
            network.MakeLink(nodes[1], nodes[2], 10);
            network.MakeLink(nodes[0], nodes[1], 15);
            network.MakeLink(nodes[1], nodes[3], 9);
            network.MakeLink(nodes[0], nodes[3], 4);
            network.MakeLink(nodes[3], nodes[5], 7);
            network.MakeLink(nodes[3], nodes[4], 3);
            network.MakeLink(nodes[4], nodes[6], 1);
            network.MakeLink(nodes[4], nodes[5], 5);
            network.MakeLink(nodes[5], nodes[6], 2);
            network.MakeLink(nodes[1], nodes[5], 1);

            var service = new DijsktraService();

            var path06 = service.FindShortestPath(network, nodes[0], nodes[6]);
            Assert.AreEqual(nodes[0].Id, path06[0].Id);
            Assert.AreEqual(nodes[3].Id, path06[1].Id);
            Assert.AreEqual(nodes[4].Id, path06[2].Id);
            Assert.AreEqual(nodes[6].Id, path06[3].Id);

            // clean nodes up
            foreach (var n in nodes)
            {
                n.Index = -1;
                n.Value = 0;
            }

            var path01 = service.FindShortestPath(network, nodes[0], nodes[1]);
            Assert.AreEqual(nodes[0].Id, path01[0].Id);
            Assert.AreEqual(nodes[3].Id, path01[1].Id);
            Assert.AreEqual(nodes[4].Id, path01[2].Id);
            Assert.AreEqual(nodes[6].Id, path01[3].Id);
            Assert.AreEqual(nodes[5].Id, path01[4].Id);
            Assert.AreEqual(nodes[1].Id, path01[5].Id);
        }
    }
}
