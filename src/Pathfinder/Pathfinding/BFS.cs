using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Pathfinder.Pathfinding;

public class BFS
{
    public static List<Node> Search(int[,] map, Node start, Node goal, Action<int[,], ICollection<Node>, ICollection<Node>, Node, ICollection<Node>?> callbackFunc, int callBackInterval, TimeSpan stepDelay)
    {
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        var cameFrom = new Dictionary<Node, Node>();

        queue.Enqueue(start);

        var timingStopwatch = Stopwatch.StartNew();
        long steps = 0;

        var counter = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            visited.Add(current);

            if (current == goal)
            {
                var path = Helpers.ReconstructPath(cameFrom, current);

                callbackFunc(map, visited, queue.ToImmutableArray(), current, path);

                return path;
            }

            if (counter % callBackInterval == 0)
            {
                callbackFunc(map, visited, queue.ToImmutableArray(), current, null);
            }

            var neighbors = Helpers.GetNeighbors(map, current);

            foreach (var neighbor in neighbors)
            {
                if (visited.Contains(neighbor) || queue.Contains(neighbor))
                {
                    continue;
                }

                queue.Enqueue(neighbor);
                cameFrom[neighbor] = current;
            }

            if (stepDelay.TotalMilliseconds > 0)
            {
                steps++;
                double elapsedMs = timingStopwatch.Elapsed.TotalMilliseconds;
                double targetDelay = steps * stepDelay.TotalMilliseconds;
                if (elapsedMs < targetDelay)
                {
                    Thread.Sleep((int)Math.Max(1, targetDelay - elapsedMs));
                }
            }

            counter++;
        }

        return new List<Node>();
    }
}
