using System;
using System.Collections.Generic;

namespace Pathfinder.Pathfinding;

public interface IPathFindingAlgorithm
{
    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal, Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc, StepDelay? stepDelay);

    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal);
}
