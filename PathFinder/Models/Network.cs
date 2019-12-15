using System;
using System.Collections.Generic;
using System.Linq;

namespace PathFinder.Models
{
    public class Network
    {
        private readonly IDictionary<int, Node> _nodesDictionary = new Dictionary<int, Node>();
        private readonly IDictionary<Node, IDictionary<Node, int>> _network = new Dictionary<Node, IDictionary<Node, int>>();

        private void MakeLink(Node node1, Node node2, int weight = 1)
        {
            if (!_network.ContainsKey(node1))
                _network.Add(node1, new Dictionary<Node, int>());

            if (!_network[node1].ContainsKey(node2))
                _network[node1].Add(node2, 0);

            _network[node1][node2] += weight;

            if (!_network.ContainsKey(node2))
                _network.Add(node2, new Dictionary<Node, int>());

            if (!_network[node2].ContainsKey(node1))
                _network[node2].Add(node1, 0);

            _network[node2][node1] += weight;
        }

        public IEnumerable<Node> GetNodeConnections(Node node)
        {
            if (!_network.ContainsKey(node))
                return Enumerable.Empty<Node>();

            return _network[node].Keys;
        }

        public int GetDistance(Node node1, Node node2)
        {
            if (!_network.ContainsKey(node1))
                return 0;

            if (!_network[node1].ContainsKey(node2))
                return 0;

            return _network[node1][node2];
        }

        public Node AddNode(int id)
        {
            var node = new Node(id);

            _nodesDictionary.Add(id, node);

            return node;
        }

        public void MakeLink(int index, int otherIndex, int weight = 1)
        {
            var node1 = _nodesDictionary[index];
            var node2 = _nodesDictionary[otherIndex];

            MakeLink(node1, node2, weight);
        }
    }
}
