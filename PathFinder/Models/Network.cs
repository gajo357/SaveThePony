using System.Collections.Generic;
using System.Linq;

namespace PathFinder.Models
{
    public class Network
    {
        private IDictionary<Node, IDictionary<Node, int>> _network = new Dictionary<Node, IDictionary<Node, int>>();

        public void MakeLink(Node node1, Node node2, int weight = 1)
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
    }
}
