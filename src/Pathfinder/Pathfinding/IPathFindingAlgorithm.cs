using System;
using System.Collections.Generic;

namespace Pathfinder.Pathfinding;

public interface IPathFindingAlgorithm
{
    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal, Action<int[,], IEnumerable<Node>, IEnumerable<Node>, Node>? callbackFunc, TimeSpan stepDelay);

    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal);
}
