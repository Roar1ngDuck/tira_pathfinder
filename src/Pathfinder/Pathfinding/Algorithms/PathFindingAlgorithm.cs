using System;
using System.Collections.Generic;

namespace Pathfinder.Pathfinding.Algorithms;

public abstract class PathFindingAlgorithm
{
    public abstract PathFindingResult Search(Node start, Node goal, bool allowDiagonal, Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc, StepDelay? stepDelay);

    public abstract PathFindingResult Search(Node start, Node goal, bool allowDiagonal);

    public CallbackInterval CallbackInterval { get; set; } = new CallbackInterval(TimeSpan.FromMilliseconds(16.66));

    protected void CallCallback(IEnumerable<Node> visitedNodes, List<Node> path, Node goal, Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc)
    {
        if (!CallbackInterval.ShouldCallCallback())
        {
            return;
        }

        callbackFunc?.Invoke(visitedNodes, path, goal);
    }
}
