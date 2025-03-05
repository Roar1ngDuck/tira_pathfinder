using System;
using System.Collections.Generic;

namespace Pathfinder.Pathfinding.Algorithms;

public abstract class PathFindingAlgorithm
{
    public abstract PathFindingResult Search(Node start, Node goal, bool allowDiagonal, Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc, StepDelay? stepDelay);

    public abstract PathFindingResult Search(Node start, Node goal, bool allowDiagonal);

    public CallbackInterval CallbackInterval { get; set; } = new CallbackInterval(TimeSpan.FromMilliseconds(32));
}
