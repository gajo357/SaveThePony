using System;
using System.Collections.Generic;
using PathFinder.Models;

namespace PathFinder.Services
{
    /// <summary>
    /// Extended Dijkstra, keeps the path to each node
    /// </summary>
    public class DijsktraService : IPathFinderService
    {
        public IList<Node> FindShortestPath(Network network, Node startNode, Node endNode)
        {
            // the heap
            var distancesSoFar = new PathHeap();
            // dictionary to track nodes and distances to them
            var finalDistances = new Dictionary<Node, int>();
            // paths from start node to every other
            var paths = new Dictionary<Node, IList<Node>>();

            // add the start node to the heap
            distancesSoFar.AddToHeap(startNode);

            while (distancesSoFar.Any())
            {
                // find the closes node
                var minNode = distancesSoFar.RemoveMin();

                // lock it down, we have been here
                finalDistances.Add(minNode, minNode.Value);

                // add it to the list of paths
                if (!paths.ContainsKey(minNode))
                {
                    paths.Add(minNode, new List<Node>() { minNode });
                }

                // we have found the shortest path to the end node
                if (minNode == endNode)
                {
                    return paths[endNode];
                }

                foreach (var node in network.GetNodeConnections(minNode))
                {
                    // only need the nodes that are not over and done
                    if (finalDistances.ContainsKey(node))
                        continue;

                    var value = Math.Max(finalDistances[minNode], network.GetDistance(minNode, node));

                    if (node.Index < 0)
                    {
                        // if the node is not in the heap, add it to both heap and dictionary
                        node.Value = value;
                        distancesSoFar.AddToHeap(node);

                        paths.Add(node, new List<Node>(paths[minNode]));
                        paths[node].Add(node);
                    }
                    else if (value < node.Value)
                    {
                        // if it is in the heap and the new value is smaller
                        // change it's value and balance the heap
                        node.Value = value;
                        distancesSoFar.UpHeapify(node.Index);

                        paths[node] = new List<Node>(paths[minNode]);
                        paths[node].Add(node);
                    }
                }
            }

            if (!paths.ContainsKey(endNode))
                return new List<Node>();

            return paths[endNode];
        }
    }
}
