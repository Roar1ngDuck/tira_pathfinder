using System;
using System.Collections.Generic;

namespace Pathfinder.Pathfinding;

public interface IPathFindingAlgorithm
{
    public List<Node> Search(int[,] map, Node start, Node goal, Action<int[,], ICollection<Node>, ICollection<Node>, Node, ICollection<Node>?>? callbackFunc, int callBackInterval, TimeSpan stepDelay);

    public List<Node> Search(int[,] map, Node start, Node goal);
}
