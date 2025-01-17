using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Pathfinder.Pathfinding;

public class BFS
{
    public static void Search(int[,] map, Node start, Node goal, Action<int[,], HashSet<Node>, Queue<Node>, Node> callbackFunc, int callBackInterval, TimeSpan stepDelay)
    {
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(start);

        var timingStopwatch = Stopwatch.StartNew();
        long steps = 0;

        var counter = 0;

        while (queue.Count > 0)
        {
            counter++;

            var current = queue.Dequeue();
            visited.Add(current);

            if (current == goal)
            {
                callbackFunc(map, visited, queue, current);
                break;
            }

            if (counter % callBackInterval == 0)
            {
                callbackFunc(map, visited, queue, current);
            }

            var neighbors = Helpers.GetNeighbors(map, current);

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor) && !queue.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
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
        }
    }
}
