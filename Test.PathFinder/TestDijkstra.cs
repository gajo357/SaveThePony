using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathFinder.Models;
using PathFinder.Services;

namespace Test.PathFinder
{
    [TestClass]
    public class TestDijkstra
    {
        [TestMethod]
        public void TestDijkstra_NetworkWithWeights0to1()
        {
            var network = new Network();
            var node0 = network.AddNode(0);
            var node1 = network.AddNode(1);
            network.AddNode(2);
            network.AddNode(3);
            network.AddNode(4);
            network.AddNode(5);
            network.AddNode(6);

            network.MakeLink(0, 2, 3);
            network.MakeLink(1, 2, 10);
            network.MakeLink(0, 1, 15);
            network.MakeLink(1, 3, 9);
            network.MakeLink(0, 3, 4);
            network.MakeLink(3, 5, 7);
            network.MakeLink(3, 4, 3);
            network.MakeLink(4, 6, 1);
            network.MakeLink(4, 5, 5);
            network.MakeLink(5, 6, 2);
            network.MakeLink(1, 5, 1);

            var service = new DijsktraService();

            var path01 = service.FindShortestPath(network, node0, node1);
            Assert.AreEqual(0, path01[0].Id);
            Assert.AreEqual(3, path01[1].Id);
            Assert.AreEqual(4, path01[2].Id);
            Assert.AreEqual(6, path01[3].Id);
            Assert.AreEqual(5, path01[4].Id);
            Assert.AreEqual(1, path01[5].Id);
        }

        [TestMethod]
        public void TestDijkstra_NetworkWithWeights0to6()
        {
            var network = new Network();
            var node0 = network.AddNode(0);
            network.AddNode(1);
            network.AddNode(2);
            network.AddNode(3);
            network.AddNode(4);
            network.AddNode(5);
            var node6 = network.AddNode(6);

            network.MakeLink(0, 2, 3);
            network.MakeLink(1, 2, 10);
            network.MakeLink(0, 1, 15);
            network.MakeLink(1, 3, 9);
            network.MakeLink(0, 3, 4);
            network.MakeLink(3, 5, 7);
            network.MakeLink(3, 4, 3);
            network.MakeLink(4, 6, 1);
            network.MakeLink(4, 5, 5);
            network.MakeLink(5, 6, 2);
            network.MakeLink(1, 5, 1);

            var service = new DijsktraService();

            var path06 = service.FindShortestPath(network, node0, node6);
            Assert.AreEqual(0, path06[0].Id);
            Assert.AreEqual(3, path06[1].Id);
            Assert.AreEqual(4, path06[2].Id);
            Assert.AreEqual(6, path06[3].Id);
        }
    }
}
