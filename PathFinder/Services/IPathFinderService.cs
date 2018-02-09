using PathFinder.Models;
using System.Collections.Generic;

namespace PathFinder.Services
{
    public interface IPathFinderService
    {
        IList<Node> FindShortestPath(Network network, Node startNode, Node endNode);
    }
}
